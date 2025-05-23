using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MailAnalyzeFunction.Models 
{
    /// <summary>
    /// 受信したメールのドキュメントモデル
    /// </summary>
    public class ReceivedEmailDocument
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("userEMailAddress")]
        public string UserEmailAddress { get; set; } = string.Empty;

        [JsonProperty("childCareServices")]
        public List<ChildCareService> ChildCareServices { get; set; } = new List<ChildCareService>();
    }

    /// <summary>
    /// 託児サービス業者からのメール情報
    /// </summary>
    public class ChildCareService
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("MailAddress")]
        public string MailAddress { get; set; } = string.Empty;

        [JsonProperty("MailText")]
        public string MailText { get; set; } = string.Empty;

        [JsonProperty("MailReceiveTime")]
        public DateTime MailReceiveTime { get; set; }
    }

    /// <summary>
    /// 業者比較用の分析結果ドキュメントモデル
    /// </summary>
    public class VendorComparisonDocument
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("userEMailAddress")]
        public string UserEmailAddress { get; set; } = string.Empty;

        [JsonProperty("vendorAnalysisList")]
        public List<Vendor> VendorList { get; set; } = new List<Vendor>();

        // パーティションキー用プロパティを明示的に追加
        [JsonProperty("partitionKey")]
        public string PartitionKey => UserEmailAddress;
    }

    public class Vendor
    {
        [JsonProperty("vendorEmail")]
        public string VendorEmail { get; set; } = string.Empty;

        [JsonProperty("analysisResult")]
        public AnalysisResult AnalysisResult { get; set; } = new AnalysisResult();

        [JsonProperty("analyzedTime")]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.IsoDateTimeConverter))]
        public DateTime AnalyzedTime { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
    }


    /// <summary>
    /// AIによる分析結果
    /// </summary>
    public class AnalysisResult
    {
        [JsonProperty("price")]
        public string Price { get; set; } = string.Empty;

        [JsonProperty("conditions")]
        public string Conditions { get; set; } = string.Empty;

        [JsonProperty("ageRange")]
        public string AgeRange { get; set; } = string.Empty;

        [JsonProperty("addedValue")]
        public string AddedValue { get; set; } = string.Empty;

        [JsonProperty("nextAction")]
        public string NextAction { get; set; } = string.Empty;
    }
}
