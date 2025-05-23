using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailAnalyzeFunction.Models;
using System;
using System.Threading.Tasks;
using System.Linq;


namespace MailAnalyzeFunction.Services
{
    public class CosmosDbService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly ILogger<CosmosDbService> _logger;
        private readonly string _databaseName;
        private readonly string _containerName;

        public CosmosDbService(IConfiguration configuration, ILogger<CosmosDbService> logger)
        {
            _logger = logger;
            
            var connectionString = configuration["COSMOSDB_CONNECTION_STRING"];
            _databaseName = configuration["COSMOSDB_DATABASE"] ?? throw new ArgumentException("COSMOSDB_DATABASE configuration is missing");
            _containerName = configuration["COSMOSDB_OUTPUT_CONTAINER"] ?? throw new ArgumentException("COSMOSDB_OUTPUT_CONTAINER configuration is missing");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("COSMOSDB_CONNECTION_STRING configuration is missing");
            }

            try
            {
                var cosmosClientOptions = new CosmosClientOptions
                {
                    MaxRetryAttemptsOnRateLimitedRequests = 3,
                    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(10),
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    }
                };

                _cosmosClient = new CosmosClient(connectionString, cosmosClientOptions);
                _container = _cosmosClient.GetContainer(_databaseName, _containerName);
                
                _logger.LogInformation("CosmosDbService initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize CosmosDbService");
                throw;
            }
        }

        public async Task<VendorComparisonDocument> CreateDocumentAsync(VendorComparisonDocument document)
        {
            try
            {
                _logger.LogInformation($"Creating document with ID: {document.Id}");

                var response = await _container.CreateItemAsync(
                    document,
                    new PartitionKey(document.UserEmailAddress)
                );

                _logger.LogInformation($"Document created successfully. RU consumed: {response.RequestCharge}");
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger.LogWarning($"Document with ID {document.Id} already exists. Attempting to replace.");
                return await ReplaceDocumentAsync(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create document with ID: {document.Id}");
                throw;
            }
        }

        public async Task<VendorComparisonDocument> ReplaceDocumentAsync(VendorComparisonDocument document)
        {
            try
            {
                _logger.LogInformation($"Replacing document with ID: {document.Id}");

                var response = await _container.ReplaceItemAsync(
                    document,
                    document.Id,
                    new PartitionKey(document.UserEmailAddress)
                );

                _logger.LogInformation($"Document replaced successfully. RU consumed: {response.RequestCharge}");
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to replace document with ID: {document.Id}");
                throw;
            }
        }

        public async Task<VendorComparisonDocument> UpsertDocumentAsync(VendorComparisonDocument document)
        {
            try
            {
                _logger.LogInformation($"Upserting document with ID: {document.Id}");

                var response = await _container.UpsertItemAsync(
                    document,
                    new PartitionKey(document.UserEmailAddress)
                );

                _logger.LogInformation($"Document upserted successfully. RU consumed: {response.RequestCharge}");
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to upsert document with ID: {document.Id}");
                throw;
            }
        }

        public async Task<VendorComparisonDocument?> GetLatestDocumentByUserEmailAsync(string userEmailAddress)
        {
            try
            {
                _logger.LogInformation($"Retrieving latest document for user: {userEmailAddress}");

                var query = new QueryDefinition(
                    "SELECT * FROM c WHERE c.userEMailAddress = @userEmailAddress ORDER BY c._ts DESC OFFSET 0 LIMIT 1")
                    .WithParameter("@userEmailAddress", userEmailAddress);

                var iterator = _container.GetItemQueryIterator<VendorComparisonDocument>(
                    query,
                    requestOptions: new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(userEmailAddress),
                        MaxItemCount = 1
                    });

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    var document = response.FirstOrDefault();
                    
                    if (document != null)
                    {
                        _logger.LogInformation($"Found existing document with ID: {document.Id}");
                        return document;
                    }
                }

                _logger.LogInformation($"No existing document found for user: {userEmailAddress}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve document for user: {userEmailAddress}");
                throw;
            }
        }

        public void Dispose()
        {
            _cosmosClient?.Dispose();
        }
    }
}
