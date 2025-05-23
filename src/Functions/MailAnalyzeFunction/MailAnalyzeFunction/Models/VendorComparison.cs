using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MailAnalyzeFunction.Models
{
    /// <summary>
    /// 業者比較用の分析結果ドキュメントモデル
    /// </summary>
    public class VendorComparison
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("userEMailAddress")]
        public string UserEmailAddress { get; set; } = string.Empty;

        [JsonProperty("sourceEmailId")]
        public string SourceEmailId { get; set; } = string.Empty;

        [JsonProperty("vendorEmail")]
        public string VendorEmail { get; set; } = string.Empty;

        [JsonProperty("analysisResults")]
        public AnalysisResults AnalysisResults { get; set; } = new AnalysisResults();

        [JsonProperty("analyzedAt")]
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// AIによる分析結果
    /// </summary>
    public class AnalysisResults
    {
        [JsonProperty("price")]
        public string Price { get; set; } = string.Empty;

        [JsonProperty("conditions")]
        public string Conditions { get; set; } = string.Empty;

        [JsonProperty("ageRange")]
        public string AgeRange { get; set; } = string.Empty;

        [JsonProperty("addedValue")]
        public string AddedValue { get; set; } = string.Empty;
    }
}