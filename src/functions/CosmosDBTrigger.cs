using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventChildcare
{
    public static class CosmosDBTrigger
    {
        /// <summary>
        /// Azure Function triggered by changes in Cosmos DB Change Feed
        /// </summary>
        /// <param name="input">The documents from the change feed</param>
        /// <param name="log">The logger instance</param>
        [FunctionName("CosmosDBTrigger")]
        public static void Run(
            [CosmosDBTrigger(
                databaseName: "%COSMOS_DB_DATABASE_NAME%",
                containerName: "%COSMOS_DB_COLLECTION_NAME_RECEIVED_EMAILS%",
                Connection = "COSMOS_DB_CONNECTION_STRING",
                LeaseContainerName = "leases",
                CreateLeaseContainerIfNotExists = true)]
            IReadOnlyList<dynamic> input,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation($"Documents modified: {input.Count}");
                
                foreach (var document in input)
                {
                    // Log the document as a JSON string
                    log.LogInformation($"Document: {JsonConvert.SerializeObject(document)}");
                    
                    // You can add more processing logic here as needed
                }
            }
        }
    }
}