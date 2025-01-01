namespace MockTestApi.Models
{
    public class Notification : IEntity
    { 
        public string Id { get; set; } 
        public EmailMessage Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public string Status { get; set; } 
        public string SentVia { get; set; }
        public string? ErrorMessage { get; set; } 
    }

}
