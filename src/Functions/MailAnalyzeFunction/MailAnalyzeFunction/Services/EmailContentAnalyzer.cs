using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using MailAnalyzeFunction.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using OpenAI.Chat;
using Newtonsoft.Json.Schema.Generation;
using System.Text.Json;

namespace MailAnalyzeFunction.Services
{
    public class EmailContentAnalyzer
    {
        private readonly AzureOpenAIClient aoaiClient;
        private readonly ChatClient chatClient;
        private readonly ChatCompletionOptions options;
        private readonly ILogger<EmailContentAnalyzer> logger;
        private readonly string deploymentName;
        

        public EmailContentAnalyzer(IConfiguration configuration, ILogger<EmailContentAnalyzer> logger)
        {
            var endpoint = configuration["AZURE_OPENAI_ENDPOINT"] ?? throw new ArgumentNullException("AZURE_OPENAI_ENDPOINT");
            var key = configuration["AZURE_OPENAI_KEY"] ?? throw new ArgumentNullException("AZURE_OPENAI_KEY");
            this.deploymentName = configuration["AZURE_OPENAI_DEPLOYMENT_NAME"] ?? throw new ArgumentNullException("AZURE_OPENAI_DEPLOYMENT_NAME");
            this.aoaiClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            this.chatClient = aoaiClient.GetChatClient(deploymentName);
            this.logger = logger;

            JSchemaGenerator generator = new JSchemaGenerator();
            var jsonSchema = generator.Generate(typeof(AnalysisResult)).ToString();

            this.options = new ChatCompletionOptions
            {
                Temperature = 0.1f,
                MaxOutputTokenCount = 800,
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("AnalysisResult", BinaryData.FromString(jsonSchema))
            };
        }


        public async Task<AnalysisResult> AnalyzeEmailContentAsync(string mailText)
        {
            try
            {
                
                // HTMLからテキストを抽出
                var plainText = ExtractTextFromHtml(mailText);

                var response = await chatClient.CompleteChatAsync(
                    new ChatMessage[]
                    {
                        new SystemChatMessage(@"あなたはイベント託児サービス業者からのメールを分析する専門家です。
                            与えられたメール本文から次の5つの情報を抽出してください：
                            1. 金額：サービスの料金情報
                            2. 条件：サービス提供の条件や制約
                            3. 対応年齢：対応可能な子供の年齢範囲
                            4. 付加価値：基本サービス以外に提供される追加サービスや特徴
                            5. Next Action:ユーザーが取るべき次のステップやアクション（不足している情報や確認事項など）
                            各項目についてできるだけ具体的に抽出し、情報がない場合は「情報なし」と記載してください。"),
                        new UserChatMessage(plainText)
                    },
                    options
                );

                var analysisContent = response.Value.Content[0].Text;
                logger.LogInformation($"AI分析結果: {analysisContent}");

                // JSON形式でレスポンスが返されるため、直接デシリアライズ
                var analysis = JsonSerializer.Deserialize<AnalysisResult>(analysisContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return analysis ?? new AnalysisResult
                {
                    Price = "分析エラー",
                    Conditions = "分析エラー",
                    AgeRange = "分析エラー",
                    AddedValue = "分析エラー"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "メール内容の分析に失敗しました");
                return new AnalysisResult
                {
                    Price = "分析エラー",
                    Conditions = "分析エラー",
                    AgeRange = "分析エラー",
                    AddedValue = "分析エラー"
                };
            }
        }


        /// <summary>
        /// HTMLからテキストを抽出する
        /// </summary>
        private string ExtractTextFromHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            try
            {
                // シンプルなHTMLタグの除去（HtmlAgilityPackなどのライブラリを使う方が望ましい）
                var text = Regex.Replace(html, "<[^>]*>", " ");
                text = Regex.Replace(text, "\\s+", " ").Trim();
                return text;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "HTMLからのテキスト抽出に失敗しました。オリジナルのテキストを使用します。");
                return html;
            }
        }
    }
}