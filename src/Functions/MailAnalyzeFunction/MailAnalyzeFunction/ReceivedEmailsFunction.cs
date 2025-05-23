using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            databaseName: "%CosmosDb:Database%",
            containerName: "%CosmosDb:ReceivedEmailsContainer%",
            Connection = "CosmosDb:ConnectionString",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)] IReadOnlyList<ReceivedEmailDocument> input)
        {
            if (input != null && input.Count > 0)
            {
                _logger.LogInformation($"受信メール数: {input.Count}");
                
                foreach (var document in input)
                {
                    _logger.LogInformation($"メールID: {document.Id}, ユーザーメールアドレス: {document.UserEmailAddress}");
                    
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
                                UserEmailAddress = document.UserEmailAddress,
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
            }
        }
    }
}
