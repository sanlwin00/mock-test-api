using MockTestApi.Models;
using System.Text.Json;

namespace MockTestApi.Services
{
    public interface IAuditLogFactory
    {
        AuditLog CreateAuditLog(string userId, string userName, string action, string entityType, string entityId, object? newValues = null, object? oldValues = null, string? details = null, bool isSuccess = true);
    }

    public class AuditLogFactory : IAuditLogFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogFactory(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public AuditLog CreateAuditLog(string userId, string userName, string action, string entityType, string entityId, object? newValues = null, object? oldValues = null, string? details = null, bool isSuccess = true)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
            var correlationId = httpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();

            return new AuditLog
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                Details = details,
                Timestamp = DateTime.UtcNow,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                CorrelationId = correlationId,
                IsSuccess = isSuccess
            };
        }
    }

}
