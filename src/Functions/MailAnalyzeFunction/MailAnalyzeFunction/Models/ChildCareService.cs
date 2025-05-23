using System;

namespace MailAnalyzeFunction.Models 
{
    public class ChildCareService
    {
        public string Id { get; set; }
        public string? MailAddress { get; set; }
        public string? MailText { get; set; }
        public DateTime MailReceiveTime { get; set; }
    }
}
