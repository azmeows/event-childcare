using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Extensions.CosmosDB;

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
            databaseName: "%CosmosDb:Database%",
            containerName: "%CosmosDb:Container%",
            Connection = "CosmosDb:ConnectionString",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)] IReadOnlyList<MyDocument> input)
        {
            if (input != null && input.Count > 0)
            {
                _logger.LogInformation("Documents modified: " + input.Count);
                foreach (var document in input)
                {
                    _logger.LogInformation($"Document Id: {document.id}, Content: {document.Text}");
                }
            }
        }
    }

    public class MyDocument
    {
        public string id { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public int Number { get; set; }

        public bool Boolean { get; set; }
    }
}
