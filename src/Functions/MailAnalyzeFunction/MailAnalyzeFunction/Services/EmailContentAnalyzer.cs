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

namespace MailAnalyzeFunction.Services
{
    /// <summary>
    /// Azure OpenAIを利用してメール内容を分析するサービス
    /// </summary>
    public class EmailContentAnalyzer
    {
        private readonly OpenAIClient _openAIClient;
        private readonly ILogger<EmailContentAnalyzer> _logger;
        private readonly string _deploymentName;

        public EmailContentAnalyzer(IConfiguration configuration, ILogger<EmailContentAnalyzer> logger)
        {
            var endpoint = configuration["OpenAI:Endpoint"] ?? throw new ArgumentNullException("OpenAI:Endpoint");
            var key = configuration["OpenAI:Key"] ?? throw new ArgumentNullException("OpenAI:Key");
            _deploymentName = configuration["OpenAI:DeploymentName"] ?? throw new ArgumentNullException("OpenAI:DeploymentName");
            
            _openAIClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            _logger = logger;
        }

        /// <summary>
        /// メール本文を分析して結果を返す
        /// </summary>
        /// <param name="mailText">メール本文</param>
        /// <returns>分析結果</returns>
        public async Task<AnalysisResults> AnalyzeEmailContentAsync(string mailText)
        {
            try
            {
                // HTMLからテキストを抽出
                var plainText = ExtractTextFromHtml(mailText);
                
                // OpenAIにリクエストを作成
                var chatCompletionsOptions = new ChatCompletionsOptions
                {
                    DeploymentName = _deploymentName,
                    Messages =
                    {
                        new ChatRequestSystemMessage(@"あなたはイベント託児サービス業者からのメールを分析する専門家です。
与えられたメール本文から次の4つの情報を抽出してください：
1. 金額：サービスの料金情報
2. 条件：サービス提供の条件や制約
3. 対応年齢：対応可能な子供の年齢範囲
4. 付加価値：基本サービス以外に提供される追加サービスや特徴

各項目についてできるだけ具体的に抽出し、情報がない場合は「情報なし」と記載してください。
次のJSON形式で返してください:
{
  ""price"": ""金額情報"",
  ""conditions"": ""条件情報"",
  ""ageRange"": ""対応年齢情報"",
  ""addedValue"": ""付加価値情報""
}"),
                        new ChatRequestUserMessage(plainText)
                    },
                    Temperature = 0.1f,
                    MaxTokens = 800,
                    ResponseFormat = ChatCompletionsResponseFormat.Json
                };

                // OpenAIに送信
                var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
                var analysisContent = response.Value.Choices[0].Message.Content;
                
                _logger.LogInformation($"AI分析結果: {analysisContent}");

                // 分析結果をパース（最もシンプルな実装として文字列で各項目を取得）
                var analysis = new AnalysisResults();
                
                // 簡易的にJSON文字列から各項目を抽出
                // Note: 実際の環境では、JSON.NETでデシリアライズするほうが望ましい
                try 
                {
                    // ここでは非常にシンプルな文字列処理を行い、レスポンスから各項目を抽出
                    // 「金額」の抽出
                    if (analysisContent.Contains("\"price\""))
                    {
                        var match = Regex.Match(analysisContent, "\"price\"\\s*:\\s*\"([^\"]*)\"");
                        if (match.Success)
                        {
                            analysis.Price = match.Groups[1].Value;
                        }
                    }

                    // 「条件」の抽出
                    if (analysisContent.Contains("\"conditions\""))
                    {
                        var match = Regex.Match(analysisContent, "\"conditions\"\\s*:\\s*\"([^\"]*)\"");
                        if (match.Success)
                        {
                            analysis.Conditions = match.Groups[1].Value;
                        }
                    }

                    // 「対応年齢」の抽出
                    if (analysisContent.Contains("\"ageRange\""))
                    {
                        var match = Regex.Match(analysisContent, "\"ageRange\"\\s*:\\s*\"([^\"]*)\"");
                        if (match.Success)
                        {
                            analysis.AgeRange = match.Groups[1].Value;
                        }
                    }

                    // 「付加価値」の抽出
                    if (analysisContent.Contains("\"addedValue\""))
                    {
                        var match = Regex.Match(analysisContent, "\"addedValue\"\\s*:\\s*\"([^\"]*)\"");
                        if (match.Success)
                        {
                            analysis.AddedValue = match.Groups[1].Value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "分析結果のパースに失敗しました");
                    // 分析が失敗した場合はデフォルト値を返す
                    analysis = new AnalysisResults
                    {
                        Price = "分析エラー",
                        Conditions = "分析エラー",
                        AgeRange = "分析エラー",
                        AddedValue = "分析エラー"
                    };
                }

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "メール内容の分析に失敗しました");
                throw;
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
                _logger.LogWarning(ex, "HTMLからのテキスト抽出に失敗しました。オリジナルのテキストを使用します。");
                return html;
            }
        }
    }
}