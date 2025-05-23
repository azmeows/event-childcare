using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Extensions.CosmosDB;
using MailAnalyzeFunction.Models;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.TextCompletion;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Azure;
using System.Text.RegularExpressions;
using MailAnalyzeFunction.Services;

namespace MailAnalyzeFunction
{
    public class ReceivedEmailsFunction
    {
        private readonly ILogger _logger;
        private readonly EmailContentAnalyzer _emailAnalyzer;

        public ReceivedEmailsFunction(ILoggerFactory loggerFactory, EmailContentAnalyzer emailAnalyzer)
        {
            _logger = loggerFactory.CreateLogger<ReceivedEmailsFunction>();
            _emailAnalyzer = emailAnalyzer;
        }

        [Function("ReceivedEmailsFunction")]
        [ExponentialBackoffRetry(3, "00:00:05", "00:15:00")]
        [CosmosDBOutput(
            databaseName: "%COSMOSDB_DATABASE%",
            containerName: "%COSMOSDB_OUTPUT_CONTAINER%",
            Connection = "COSMOSDB_CONNECTION_STRING",
            CreateIfNotExists = true,
            PartitionKey = "/userEMailAddress")]
        public async Task<VendorComparisonDocument?> Run(
            [CosmosDBTrigger(
                databaseName: "%COSMOSDB_DATABASE%",
                containerName: "%COSMOSDB_CONTAINER%",
                Connection = "COSMOSDB_CONNECTION_STRING",
                LeaseContainerName = "leases",
                CreateLeaseContainerIfNotExists = true)] IReadOnlyList<ReceivedEmailDocument> input,
            FunctionContext context)
        {
            var logger = context.GetLogger("ReceivedEmailsFunction");

            try
            {
                if (input == null || !input.Any())
                {
                    logger.LogWarning("No input documents to process");
                    return null;
                }

                logger.LogInformation($"Processing {input.Count} documents");

                // 最初のドキュメントからユーザーメールアドレスを取得し、検証
                var userEmailAddress = input[0]?.UserEmailAddress?.Trim();
                if (string.IsNullOrEmpty(userEmailAddress))
                {
                    logger.LogError("User email address is null or empty");
                    throw new ArgumentException("User email address is required");
                }

                var vendorComparisonDocument = new VendorComparisonDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    UserEmailAddress = SanitizeString(userEmailAddress)
                };

                foreach (var document in input)
                {
                    if (document?.ChildCareServices == null) continue;

                    foreach (ChildCareService service in document.ChildCareServices)
                    {
                        try
                        {
                            // データの検証とサニタイズ
                            if (string.IsNullOrEmpty(service.MailAddress))
                            {
                                logger.LogWarning("Skipping service with empty mail address");
                                continue;
                            }

                            var sanitizedMailAddress = SanitizeString(service.MailAddress);
                            var sanitizedMailText = SanitizeString(service.MailText ?? string.Empty);

                            logger.LogInformation($"業者メールアドレス: {sanitizedMailAddress}, 受信日時: {service.MailReceiveTime}");

                            // メール本文を分析
                            var analysisResult = await _emailAnalyzer.AnalyzeEmailContentAsync(sanitizedMailText);                            // 分析結果を業者比較ドキュメントに追加
                            vendorComparisonDocument.VendorList.Add(new Vendor
                            {
                                VendorEmail = sanitizedMailAddress,
                                AnalysisResult = analysisResult ?? new AnalysisResult(),
                                AnalyzedTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"))
                            });
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Error processing service {service.MailAddress}");
                            // 個別のサービス処理エラーは継続
                        }
                    }
                }

                logger.LogInformation($"Successfully processed documents, returning {vendorComparisonDocument.VendorList.Count} vendors");
                return vendorComparisonDocument;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error in ReceivedEmailsFunction");
                throw;
            }
        }

        /// <summary>
        /// 文字列をサニタイズして無効な文字を除去
        /// </summary>
        private string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // 制御文字やnull文字を除去
            var sanitized = Regex.Replace(input, @"[\x00-\x1F\x7F]", " ");
            
            // 連続する空白を単一の空白に置換
            sanitized = Regex.Replace(sanitized, @"\s+", " ");
            
            return sanitized.Trim();
        }

    }
}

