using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MailAnalyzeFunction.Models;
using MailAnalyzeFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Extensions.CosmosDB;
using System.Text.Json;

namespace MailAnalyzeFunction
{
    public class ReceivedEmailsFunction
    {
        private readonly ILogger _logger;
        private readonly EmailContentAnalyzer _emailAnalyzer;
        private readonly VendorComparisonService _vendorComparisonService;

        public ReceivedEmailsFunction(
            ILoggerFactory loggerFactory, 
            EmailContentAnalyzer emailAnalyzer,
            VendorComparisonService vendorComparisonService)
        {
            _logger = loggerFactory.CreateLogger<ReceivedEmailsFunction>();
            _emailAnalyzer = emailAnalyzer;
            _vendorComparisonService = vendorComparisonService;
        }

        [Function("ReceivedEmailsFunction")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "%COSMOSDB_DATABASE%",
            containerName: "%COSMOSDB_CONTAINER%",
            Connection = "COSMOSDB_CONNECTION_STRING",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true,
            FeedPollDelay = 1000,  // ポーリング間隔を1秒に短縮
            MaxItemsPerInvocation = 100,  // バッチサイズを調整
            StartFromBeginning = false
            )] IReadOnlyList<ReceivedEmail> input,
            FunctionContext context)
        {
            if (input is not null && input.Any())
            {
                _logger.LogInformation("Processing {Count} documents at {Timestamp}", 
                    input.Count, DateTime.UtcNow);
                
                foreach (var document in input)
                {
                    _logger.LogInformation("ReceivedEmail: {Id}, UserEmail: {userEmail}, ProcessedAt: {ProcessedAt}", 
                        document.Id, document.UserEMailAddress, DateTime.UtcNow);
                    
                    // 各託児サービス業者のメールを処理
                    foreach (var service in document.ChildCareServices)
                    {
                        try
                        {
                            _logger.LogInformation($"業者メールアドレス: {service.MailAddress}, 受信日時: {service.MailReceiveTime}");
                            
                            // メール本文を分析
                            var analysisResults = await _emailAnalyzer.AnalyzeEmailContentAsync(service.MailText);
                            
                            // 分析結果をCosmosDBに保存
                            var comparisonDocument = new VendorComparisonDocument
                            {
                                UserEmailAddress = document.UserEMailAddress,
                                SourceEmailId = document.Id,
                                VendorEmail = service.MailAddress,
                                AnalysisResults = analysisResults,
                                AnalyzedAt = DateTime.UtcNow
                            };
                            
                            await _vendorComparisonService.SaveVendorComparisonAsync(comparisonDocument);
                            
                            _logger.LogInformation($"業者メール分析完了: {service.MailAddress}, 分析結果ID: {comparisonDocument.Id}");
                        }
                        catch (Exception ex)
                        {
                            // 一つのメール処理で例外が発生してもほかのメールは処理を続行
                            _logger.LogError(ex, $"業者メールの処理中にエラーが発生しました: {service.MailAddress}");
                        }
                    }
                }
                
                _logger.LogInformation("Completed processing {Count} documents", input.Count);
            }
        }
    }
}
