namespace MockTestApi.Models
{
    public class EmailMessage
    {
        public string To { get; set; }
        public string ToName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string BodyPlainText { get; set; }
        public List<IFormFile>? Attachments { get; set; } = new List<IFormFile>();
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
    }

}
