using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using VendorComparisonFunction.Models;
using System.Linq;
using System.Text.Json;

namespace VendorComparisonFunction
{
    public class VendorComparisonFunction
    {
        private readonly ILogger _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseName;
        private readonly string _containerName;

        public VendorComparisonFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<VendorComparisonFunction>();
            
            // Get connection information from environment variables
            var connectionString = Environment.GetEnvironmentVariable("COSMOSDB_CONNECTION_STRING");
            _databaseName = Environment.GetEnvironmentVariable("COSMOSDB_DATABASE");
            _containerName = Environment.GetEnvironmentVariable("COSMOSDB_CONTAINER_VENDOR_COMPARISONS");
            
            if (string.IsNullOrEmpty(connectionString) || 
                string.IsNullOrEmpty(_databaseName) || 
                string.IsNullOrEmpty(_containerName))
            {
                _logger.LogError("One or more required environment variables are missing");
                throw new InvalidOperationException("Missing required environment configuration");
            }
            
            _cosmosClient = new CosmosClient(connectionString);
        }

        [Function("GetVendorComparisons")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "vendor-comparisons")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request for vendor comparisons");

            // Parse query parameter
            var userEmailAddress = req.Query["userEMailAddress"];
            
            if (string.IsNullOrEmpty(userEmailAddress))
            {
                _logger.LogWarning("Request received without userEMailAddress parameter");
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Please provide userEMailAddress query parameter");
                return badResponse;
            }

            try
            {
                _logger.LogInformation($"Querying vendor comparisons for email: {userEmailAddress}");
                
                var container = _cosmosClient.GetContainer(_databaseName, _containerName);
                var query = new QueryDefinition(
                    "SELECT * FROM c WHERE c.userEMailAddress = @userEMailAddress")
                    .WithParameter("@userEMailAddress", userEmailAddress);

                var results = new List<VendorComparison>();
                
                using (var resultSet = container.GetItemQueryIterator<VendorComparison>(query))
                {
                    while (resultSet.HasMoreResults)
                    {
                        var response = await resultSet.ReadNextAsync();
                        results.AddRange(response.ToList());
                    }
                }

                _logger.LogInformation($"Found {results.Count} vendor comparison records");
                
                var httpResponse = req.CreateResponse(HttpStatusCode.OK);
                await httpResponse.WriteAsJsonAsync(results);
                return httpResponse;
            }
            catch (CosmosException cosmosEx)
            {
                _logger.LogError(cosmosEx, $"Cosmos DB error: {cosmosEx.Message}");
                
                var errorResponse = req.CreateResponse(
                    cosmosEx.StatusCode == HttpStatusCode.NotFound ? 
                    HttpStatusCode.NotFound : 
                    HttpStatusCode.InternalServerError);
                
                await errorResponse.WriteStringAsync(
                    cosmosEx.StatusCode == HttpStatusCode.NotFound ? 
                    "No data found" : 
                    "An error occurred while retrieving data");
                    
                return errorResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving vendor comparisons: {ex.Message}");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while processing your request");
                return errorResponse;
            }
        }
    }
}