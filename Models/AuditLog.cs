namespace MockTestApi.Models
{
    public class AuditLog : IEntity
    {
        public string Id { get; set; }  // Unique Identifier for the audit log
        public string UserId { get; set; }  // User who performed the action
        public string UserName { get; set; }  // Optional: Useful for readability
        public string Action { get; set; }  // CRUD, Login, Payment, etc.
        public string? Details { get; set; }  // Additional metadata
        public DateTime Timestamp { get; set; }  // When the action happened
        public string IPAddress { get; set; }  // Captures IP for security
        public string UserAgent { get; set; }  // Device/browser info
        public string EntityType { get; set; }  // Type of object affected (e.g., "Test", "User", "Payment")
        public string EntityId { get; set; }  // ID of the affected object
        public string OldValues { get; set; }  // JSON of old values before update
        public string NewValues { get; set; }  // JSON of new values after update
        public string CorrelationId { get; set; }  // For tracing requests across services
        public bool IsSuccess { get; set; }  // Indicates if the action was successful
    }
}
