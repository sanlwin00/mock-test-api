using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace MockTestApi.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(IAuditLogRepository auditLogRepository, IHttpContextAccessor httpContextAccessor)
        {
            _auditLogRepository = auditLogRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync()
        {
            return await _auditLogRepository.GetAllAsync();
        }

        public async Task CreateAuditLogAsync(
            string action,
            string entityType,
            string entityId,
            string? userId = null,
            string? userName = null,
            object? newValues = null,
            object? oldValues = null,
            string? details = null,
            bool isSuccess = true)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
            var correlationId = httpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();

            var _userId = userId 
                ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? "Unknown";

            var _userName = userName 
                ?? user?.FindFirst(ClaimTypes.Email)?.Value 
                ?? "Unknown";

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid().ToString(),
                UserId = _userId,
                UserName = _userName,
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

            await _auditLogRepository.CreateAsync(auditLog);
        }

        public async Task<MockTestUserCount> GetMockTestUserCountAsync()
        {
            return await _auditLogRepository.GetMockTestStartedEventCountAsync();
        }

        public async Task<PagedResult<AuditLog>> GetAuditLogsAsync(int pageNumber, int pageSize, string? searchKeyword = null)
        {
            return await _auditLogRepository.GetAuditLogsAsync(pageNumber, pageSize, searchKeyword);
        }
    }
}
