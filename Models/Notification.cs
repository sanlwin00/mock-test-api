namespace MockTestApi.Models
{
    public class Notification : IEntity
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
