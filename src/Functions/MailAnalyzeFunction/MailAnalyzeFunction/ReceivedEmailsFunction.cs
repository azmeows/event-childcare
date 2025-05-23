using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Extensions.CosmosDB;
using MailAnalyzeFunction.Models;


namespace MailAnalyzeFunction
{
    public class ReceivedEmailsFunction
    {
        private readonly ILogger _logger;

        public ReceivedEmailsFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ReceivedEmailsFunction>();
        }

        [Function("ReceivedEmailsFunction")]
        public void Run([CosmosDBTrigger(
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
                
                foreach (var doc in input)
                {
                    _logger.LogInformation("ReceivedEmail: {Id}, UserEmail: {userEmail}, ProcessedAt: {ProcessedAt}", 
                        doc.Id, doc.UserEMailAddress, DateTime.UtcNow);
                }
                
                _logger.LogInformation("Completed processing {Count} documents", input.Count);
            }
        }
    }

}
