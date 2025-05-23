using System.Collections.Generic;

namespace MailAnalyzeFunction.Models
{
    public class ReceivedEmail
    {
        public string Id { get; set; }
        public string? UserEMailAddress { get; set; }
        public required List<ChildCareService> ChildCareServices { get; set; }
    }
}
