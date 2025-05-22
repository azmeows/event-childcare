using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Text.Json;

namespace CosmosDbChangeFeedFunction
{
    public class CosmosDbChangeFeedToQueue
    {
        private readonly ILogger _logger;
        private const string DatabaseName = "cosmos-event-childcare-dev"; // 本番DB名
        private const string ContainerName = "vendor-comparisons"; // 使用するコレクション名

        public CosmosDbChangeFeedToQueue(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CosmosDbChangeFeedToQueue>();
        }

        [Function("CosmosDbChangeFeedToQueue")]
        public void Run([
            CosmosDBTrigger(
                databaseName: DatabaseName,
                containerName: ContainerName,
                Connection = "CosmosDbConnection", // local.settings.jsonのCosmosDbConnectionキーを参照
                LeaseContainerName = "leases",
                CreateLeaseContainerIfNotExists = true)
        ] IReadOnlyList<MyDocument> input)
        {
            if (input != null && input.Count > 0)
            {
                _logger.LogInformation($"Documents modified: {input.Count}");
                _logger.LogInformation($"First document Id: {input[0]?.id}");

                // Azure Queue Storage へ送信
                string? queueConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                if (string.IsNullOrEmpty(queueConnectionString))
                {
                    _logger.LogError("AzureWebJobsStorage の接続文字列が設定されていません。");
                    return;
                }
                string queueName = Environment.GetEnvironmentVariable("NotifyQueueName") ?? "notify-queue";
                var queueClient = new QueueClient(queueConnectionString, queueName);
                queueClient.CreateIfNotExists();

                foreach (var doc in input)
                {
                    string message = JsonSerializer.Serialize(doc);
                    queueClient.SendMessage(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message)));
                }
            }
        }
    }

    public class MyDocument
    {
        public string? id { get; set; }

        public string? Text { get; set; }

        public int Number { get; set; }

        public bool Boolean { get; set; }
    }
}
