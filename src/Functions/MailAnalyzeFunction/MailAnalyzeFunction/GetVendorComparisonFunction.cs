using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MailAnalyzeFunction.Services;
using MailAnalyzeFunction.Models;
using System.Net.Http;

namespace MailAnalyzeFunction
{
    public class GetVendorComparisonFunction
    {
        private readonly CosmosDbService _cosmosDbService;
        private readonly ILogger<GetVendorComparisonFunction> _logger;

        public GetVendorComparisonFunction(CosmosDbService cosmosDbService, ILoggerFactory loggerFactory)
        {
            _cosmosDbService = cosmosDbService;
            _logger = loggerFactory.CreateLogger<GetVendorComparisonFunction>();
        }

        [Function("GetVendorComparison")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vendor-comparison/{userEmail}")] HttpRequestData req,
            string userEmail)
        {
            _logger.LogInformation($"Getting vendor comparison for user: {userEmail}");

            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("User email is empty");
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("User email is required");
                return badResponse;
            }

            try
            {
                // サニタイズ処理
                var sanitizedEmail = SanitizeString(userEmail);
                
                // Cosmos DBからデータを取得
                var document = await _cosmosDbService.GetLatestDocumentByUserEmailAsync(sanitizedEmail);

                if (document == null)
                {
                    _logger.LogWarning($"No document found for user: {sanitizedEmail}");
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"No vendor comparison data found for user: {sanitizedEmail}");
                    return notFoundResponse;
                }

                // 正常なレスポンスを返す
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(document);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vendor comparison data for user: {userEmail}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An internal server error occurred");
                return errorResponse;
            }
        }

        // 安全な文字列処理のためのユーティリティメソッド
        private static string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // 危険な文字を取り除く簡易的なサニタイズ
            return input.Trim();
        }
    }
}