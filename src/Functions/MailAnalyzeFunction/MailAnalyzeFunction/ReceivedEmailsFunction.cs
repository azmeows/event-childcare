using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Newtonsoft.Json;
using MailAnalyzeFunction.Services;

namespace MailAnalyzeFunction
{
    public class ReceivedEmailsFunction
    {
        private readonly ILogger _logger;
        private readonly EmailContentAnalyzer _emailAnalyzer;
        private readonly CosmosDbService _cosmosDbService;

        public ReceivedEmailsFunction(ILoggerFactory loggerFactory, EmailContentAnalyzer emailAnalyzer, CosmosDbService cosmosDbService)
        {
            _logger = loggerFactory.CreateLogger<ReceivedEmailsFunction>();
            _emailAnalyzer = emailAnalyzer;
            _cosmosDbService = cosmosDbService;
        }
        [Function("ReceivedEmailsFunction")]
        [ExponentialBackoffRetry(3, "00:00:05", "00:15:00")]
        public async Task Run(
            [CosmosDBTrigger(
                databaseName: "%COSMOSDB_DATABASE%",
                containerName: "%COSMOSDB_CONTAINER%",
                Connection = "COSMOSDB_CONNECTION_STRING",
                LeaseContainerName = "leases",
                CreateLeaseContainerIfNotExists = true)] IReadOnlyList<ReceivedEmailDocument> input,
            FunctionContext context)
        {
            var logger = context.GetLogger("ReceivedEmailsFunction"); try
            {
                if (input == null || !input.Any())
                {
                    logger.LogWarning("No input documents to process");
                    return;
                }

                logger.LogInformation($"Processing {input.Count} documents");

                // 最初のドキュメントからユーザーメールアドレスを取得し、検証
                var userEmailAddress = input[0]?.UserEmailAddress?.Trim();
                if (string.IsNullOrEmpty(userEmailAddress))
                {
                    logger.LogError("User email address is null or empty");
                    throw new ArgumentException("User email address is required");
                }
                var sanitizedUserEmail = SanitizeString(userEmailAddress);

                // 既存のドキュメントを確認
                var existingDocument = await _cosmosDbService.GetLatestDocumentByUserEmailAsync(sanitizedUserEmail);

                VendorComparisonDocument vendorComparisonDocument;
                if (existingDocument != null)
                {
                    // 既存のドキュメントがある場合はそれを使用
                    vendorComparisonDocument = existingDocument;
                    logger.LogInformation($"Using existing document with ID: {existingDocument.Id}");
                }
                else
                {
                    // 新しいドキュメントを作成
                    var uniqueId = $"{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}";
                    vendorComparisonDocument = new VendorComparisonDocument
                    {
                        Id = uniqueId,
                        UserEmailAddress = sanitizedUserEmail
                        // PartitionKeyはUserEmailAddressを返す計算プロパティとして定義されているため、明示的な設定は不要
                    };
                    logger.LogInformation($"Creating new document with ID: {uniqueId}");
                }

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

                            logger.LogInformation($"業者メールアドレス: {sanitizedMailAddress}, 受信日時: {service.MailReceiveTime}");                            // メール本文を分析
                            var analysisResult = await _emailAnalyzer.AnalyzeEmailContentAsync(sanitizedMailText);

                            // 既存のベンダーリストに同じメールアドレスが存在するかチェック
                            var existingVendor = vendorComparisonDocument.VendorList
                                .FirstOrDefault(v => v.VendorEmail.Equals(sanitizedMailAddress, StringComparison.OrdinalIgnoreCase));

                            if (existingVendor != null)
                            {
                                // 既存のベンダー情報を更新
                                existingVendor.AnalysisResult = analysisResult;
                                existingVendor.AnalyzedTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
                                logger.LogInformation($"Updated existing vendor: {sanitizedMailAddress}");
                            }
                            else
                            {
                                // 新しいベンダーを追加
                                vendorComparisonDocument.VendorList.Add(new Vendor
                                {
                                    VendorEmail = sanitizedMailAddress,
                                    AnalysisResult = analysisResult,
                                    AnalyzedTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"))
                                });
                                logger.LogInformation($"Added new vendor: {sanitizedMailAddress}");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Error processing service {service.MailAddress}");
                            // 個別のサービス処理エラーは継続
                        }
                    }                }
                logger.LogInformation($"Successfully processed documents, total vendors: {vendorComparisonDocument.VendorList.Count}");

                // 業者比較分析を実行
                if (vendorComparisonDocument.VendorList.Count > 0)
                {
                    try
                    {
                        logger.LogInformation($"Starting vendor comparison analysis for {vendorComparisonDocument.VendorList.Count} vendors");
                        
                        var comparisonResult = await _emailAnalyzer.CompareVendorsAsync(vendorComparisonDocument.VendorList);
                        vendorComparisonDocument.VendorComparisonResult = comparisonResult;
                        
                        logger.LogInformation("Vendor comparison analysis completed successfully");
                    }
                    catch (Exception analysisEx)
                    {
                        logger.LogError(analysisEx, "Failed to perform vendor comparison analysis");
                        vendorComparisonDocument.VendorComparisonResult = "業者比較分析中にエラーが発生しました。個別の業者情報をご確認ください。";
                    }
                }
                else
                {
                    logger.LogWarning("No vendors available for comparison analysis");
                    vendorComparisonDocument.VendorComparisonResult = "比較対象の業者がありません。";
                }

                // Cosmos DB SDK を使用してドキュメントを更新または作成
                try
                {
                    var savedDocument = await _cosmosDbService.UpsertDocumentAsync(vendorComparisonDocument);
                    logger.LogInformation($"Document successfully saved to Cosmos DB with ID: {savedDocument.Id}, Total vendors: {savedDocument.VendorList.Count}, Comparison result length: {savedDocument.VendorComparisonResult?.Length ?? 0}");
                }
                catch (Exception cosmosEx)
                {
                    logger.LogError(cosmosEx, "Failed to save document to Cosmos DB");
                    throw;
                }
                
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

            // 長すぎる文字列を制限（Cosmos DBの制限を考慮）
            //if (sanitized.Length > 2000)
            //{
            //    sanitized = sanitized.Substring(0, 2000) + "...";
            //    _logger.LogWarning($"String truncated to 2000 characters");
            //}

            return sanitized.Trim();
        }
    }
}

