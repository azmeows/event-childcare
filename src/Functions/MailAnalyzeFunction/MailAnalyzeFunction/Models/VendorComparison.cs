using System;

namespace MailAnalyzeFunction.Models
{
    public class VendorComparison
    {
        public string Id { get; set; } = null!;
        public string? UserEMailAddress { get; set; }
        public string? VendorName { get; set; }
        public string? ServiceDescription { get; set; }
        public decimal? Price { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}