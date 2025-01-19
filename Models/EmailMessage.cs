namespace MockTestApi.Models
{
    public class EmailMessage
    {
        public string To { get; set; }
        public string ToName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string BodyPlainText { get; set; }
        public List<FileAttachment>? Attachments { get; set; } = new List<FileAttachment>();
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public bool IsTransactional { get; set; }
    }

    public class FileAttachment
    {
        public string FileName { get; set; } // Name of the file
        public string ContentType { get; set; } // MIME type of the file
        public long FileSize { get; set; } // File size in bytes
        public string? Base64Content { get; set; } // File content as a base64 string
    }
}
