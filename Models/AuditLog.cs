namespace MockTestApi.Models
{
    public class AuditLog : IEntity
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string? Details { get; set; }
        public DateTime? Timestamp { get; set; }
    }

}
