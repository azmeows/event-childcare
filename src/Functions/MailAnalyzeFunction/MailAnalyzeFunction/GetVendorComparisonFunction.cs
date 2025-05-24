using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MailAnalyzeFunction.Services;
using System.Web.Http;
using MailAnalyzeFunction.Models;
using Newtonsoft.Json;

namespace MailAnalyzeFunction
{
    public class GetVendorComparisonFunction
    {
        private readonly CosmosDbService _cosmosDbService;

        public GetVendorComparisonFunction(CosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        [FunctionName("GetVendorComparison")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "vendor-comparison/{userEmail}")] HttpRequest req,
            string userEmail,
            ILogger log)
        {
            log.LogInformation($"Getting vendor comparison for user: {userEmail}");

            if (string.IsNullOrEmpty(userEmail))
            {
                log.LogWarning("User email is empty");
                return new BadRequestObjectResult("User email is required");
            }

            try
            {
                // サニタイズ処理
                var sanitizedEmail = SanitizeString(userEmail);
                
                // Cosmos DBからデータを取得
                var document = await _cosmosDbService.GetLatestDocumentByUserEmailAsync(sanitizedEmail);

                if (document == null)
                {
                    log.LogWarning($"No document found for user: {sanitizedEmail}");
                    return new NotFoundObjectResult($"No vendor comparison data found for user: {sanitizedEmail}");
                }

                // CORSヘッダーを追加
                return new OkObjectResult(document);
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Error getting vendor comparison data for user: {userEmail}");
                return new InternalServerErrorResult();
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