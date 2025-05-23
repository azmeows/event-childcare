using System;
using System.Threading.Tasks;
using MailAnalyzeFunction.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MailAnalyzeFunction.Services
{
    /// <summary>
    /// Cosmos DBに分析結果を保存するサービス
    /// </summary>
    public class VendorComparisonService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<VendorComparisonService> _logger;
        private readonly Container _container;

        public VendorComparisonService(IConfiguration configuration, ILogger<VendorComparisonService> logger)
        {
            var connectionString = configuration["CosmosDb:ConnectionString"] ?? throw new ArgumentNullException("CosmosDb:ConnectionString");
            var databaseName = configuration["CosmosDb:Database"] ?? throw new ArgumentNullException("CosmosDb:Database");
            var containerName = configuration["CosmosDb:VendorComparisonsContainer"] ?? throw new ArgumentNullException("CosmosDb:VendorComparisonsContainer");
            
            _cosmosClient = new CosmosClient(connectionString);
            _logger = logger;
            _container = _cosmosClient.GetContainer(databaseName, containerName);
        }

        /// <summary>
        /// 分析結果をCosmosDBに保存する
        /// </summary>
        /// <param name="document">保存するドキュメント</param>
        /// <returns>保存したドキュメントのID</returns>
        public async Task<string> SaveVendorComparisonAsync(VendorComparisonDocument document)
        {
            try
            {
                var response = await _container.CreateItemAsync(document, new PartitionKey(document.UserEmailAddress));
                _logger.LogInformation($"業者比較ドキュメントを保存しました。ドキュメントID: {document.Id}, RequestCharge: {response.RequestCharge}");
                return document.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "業者比較ドキュメントの保存に失敗しました");
                throw;
            }
        }
    }
}