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
            databaseName: "%COSMOSDB_DATABASE%",
            containerName: "%COSMOSDB_CONTAINER%",
            Connection = "COSMOSDB_CONNECTION_STRING",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)] IReadOnlyList<ReceivedEmail> input,
            FunctionContext context)
        {
            if (input is not null && input.Any())
            {
                foreach (var doc in input)
                {
                    _logger.LogInformation("ReceivedEmail: {Id}, UserEmail: {userEmail}", doc.id, doc.userEMailAddress);
                }
            }

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
}

