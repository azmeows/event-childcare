using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbChangeFeedFunction
{
    public class CosmosDbChangeFeedToQueue
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private const string DatabaseName = "cosmos-event-childcare-dev"; // 本番DB名
        private const string ContainerName = "vendor-comparisons"; // 使用するコレクション名

        public CosmosDbChangeFeedToQueue(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CosmosDbChangeFeedToQueue>();
            _httpClient = new HttpClient();
        }

        [Function("CosmosDbChangeFeedToQueue")]
        public async Task Run([
            CosmosDBTrigger(
                databaseName: DatabaseName,
                containerName: ContainerName,
                Connection = "CosmosDbConnection", // local.settings.jsonのCosmosDbConnectionキーを参照
                LeaseContainerName = "leases",
                CreateLeaseContainerIfNotExists = true)
        ] IReadOnlyList<VendorComparisonDocument> input)
        {
            if (input != null && input.Count > 0)
            {
                _logger.LogInformation($"Documents modified: {input.Count}");
                _logger.LogInformation($"First document Id: {input[0]?.id}");

                // Logic Appのエンドポイントを取得
                string? logicAppEndpoint = Environment.GetEnvironmentVariable("LogicAppEndpoint");
                if (string.IsNullOrEmpty(logicAppEndpoint))
                {
                    _logger.LogError("Logic Appのエンドポイントが設定されていません。");
                    return;
                }

                foreach (var doc in input)
                {
                    try
                    {
                        // ドキュメントをJSON形式でシリアライズ
                        string jsonContent = JsonSerializer.Serialize(doc);
                        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        // Logic Appのエンドポイントにデータをポスト
                        var response = await _httpClient.PostAsync(logicAppEndpoint, content);
                        
                        // レスポンスの検証
                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation($"ドキュメント ID: {doc.id} のPOSTに成功しました。");
                        }
                        else
                        {
                            string errorResponse = await response.Content.ReadAsStringAsync();
                            _logger.LogError($"ドキュメント ID: {doc.id} のPOSTに失敗しました。ステータスコード: {response.StatusCode}, エラー: {errorResponse}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"ドキュメント ID: {doc.id} の処理中にエラーが発生しました: {ex.Message}");
                    }
                }
            }
        }
    }

    // 提供されたJSONサンプルに合わせてモデルを定義
    public class VendorComparisonDocument
    {
        public string? id { get; set; }
        public string? userEMailAddress { get; set; }
        public string? sourceEmailId { get; set; }
        public string? vendorEmail { get; set; }
        public AnalysisResults? analysisResults { get; set; }
        public DateTime analyzedAt { get; set; }
        public string? _rid { get; set; }
        public string? _self { get; set; }
        public string? _etag { get; set; }
        public string? _attachments { get; set; }
        public long _ts { get; set; }
    }

    public class AnalysisResults
    {
        public string? price { get; set; }
        public string? conditions { get; set; }
        public string? ageRange { get; set; }
        public string? addedValue { get; set; }
    }
}
