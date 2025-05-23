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
                            与えられたメール本文から次の5つの情報を抽出してください:
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


        /// <summary>
        /// 複数の業者を比較して、メリット・デメリット・選び方のポイントを分析
        /// </summary>
        public async Task<string> CompareVendorsAsync(List<Vendor> vendors)
        {
            try
            {
                if (vendors == null || vendors.Count == 0)
                {
                    return "比較対象の業者がありません。";
                }

                if (vendors.Count == 1)
                {
                    // 1社のみの場合：そのベンダーのメリット・デメリット・選び方のポイント
                    return await AnalyzeSingleVendorAsync(vendors[0]);
                }

                // 複数社の場合：比較分析
                return await AnalyzeMultipleVendorsAsync(vendors);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "業者比較分析に失敗しました");
                return "業者比較分析中にエラーが発生しました。";
            }
        }

        /// <summary>
        /// 単一業者のメリット・デメリット・選び方のポイントを分析
        /// </summary>
        private async Task<string> AnalyzeSingleVendorAsync(Vendor vendor)
        {
            try
            {
                var vendorInfo = $@"
                    業者メール: {vendor.VendorEmail}
                    料金: {vendor.AnalysisResult.Price}
                    条件: {vendor.AnalysisResult.Conditions}
                    対応年齢: {vendor.AnalysisResult.AgeRange}
                    付加価値: {vendor.AnalysisResult.AddedValue}
                    次のアクション: {vendor.AnalysisResult.NextAction}
                    分析日時: {vendor.AnalyzedTime:yyyy年MM月dd日 HH時mm分}";

                var response = await chatClient.CompleteChatAsync(
                    new ChatMessage[]
                    {
                        new SystemChatMessage(@"あなたはイベント託児サービス業者選定の専門コンサルタントです。
                            提供された業者情報を基に、以下の観点で詳細な分析を行ってください：

                            1. この業者のメリット・強み
                            2. この業者のデメリット・懸念点
                            3. この業者を選ぶべき場面・条件
                            4. 追加で確認すべき情報
                            5. 総合的な評価とおすすめ度

                            親しみやすく、実用的なアドバイスを心がけ、具体的な根拠を示して説明してください。"),
                        new UserChatMessage(vendorInfo)
                    },
                    new ChatCompletionOptions
                    {
                        Temperature = 0.3f,
                        MaxOutputTokenCount = 1200
                    }
                );

                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"単一業者分析に失敗しました: {vendor.VendorEmail}");
                return $"業者 {vendor.VendorEmail} の分析中にエラーが発生しました。";
            }
        }

        /// <summary>
        /// 複数業者の比較分析を実行
        /// </summary>
        private async Task<string> AnalyzeMultipleVendorsAsync(List<Vendor> vendors)
        {
            try
            {
                var vendorComparison = new StringBuilder();
                vendorComparison.AppendLine("業者比較情報:");

                for (int i = 0; i < vendors.Count; i++)
                {
                    var vendor = vendors[i];
                    vendorComparison.AppendLine($@"
                        【業者{i + 1}】{vendor.VendorEmail}
                        ・料金: {vendor.AnalysisResult.Price}
                        ・条件: {vendor.AnalysisResult.Conditions}
                        ・対応年齢: {vendor.AnalysisResult.AgeRange}
                        ・付加価値: {vendor.AnalysisResult.AddedValue}
                        ・次のアクション: {vendor.AnalysisResult.NextAction}
                        ・分析日時: {vendor.AnalyzedTime:yyyy年MM月dd日 HH時mm分}");
                }

                var response = await chatClient.CompleteChatAsync(
                    new ChatMessage[]
                    {
                        new SystemChatMessage(@"あなたはイベント託児サービス業者選定の専門コンサルタントです。
                            複数の業者を比較して、以下の観点で詳細な分析を行ってください：

                            1. 各業者の特徴とメリット・デメリット
                            2. 料金面での比較（コストパフォーマンス含む）
                            3. サービス内容・条件面での比較
                            4. 年齢対応範囲での比較
                            5. 付加価値・特徴での比較
                            6. どのような場面でどの業者を選ぶべきか（選び方のポイント）
                            7. 総合的なランキングとおすすめ度
                            8. 決定前に確認すべき追加情報

                            親しみやすく、実用的なアドバイスを心がけ、具体的な根拠を示して比較してください。
                            表形式やリスト形式を活用して見やすく整理してください。"),
                        new UserChatMessage(vendorComparison.ToString())
                    },
                    new ChatCompletionOptions
                    {
                        Temperature = 0.3f,
                        MaxOutputTokenCount = 2000
                    }
                );

                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "複数業者比較分析に失敗しました");
                return "複数業者の比較分析中にエラーが発生しました。";
            }
        }

    }
}