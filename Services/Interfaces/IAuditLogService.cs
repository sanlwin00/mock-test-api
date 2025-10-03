using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync();
        Task CreateAuditLogAsync(string action, string entityType, string entityId, string? userId = null, string? userName = null, object? newValues = null, object? oldValues = null, string? details = null, bool isSuccess = true);
        Task<MockTestUserCount> GetMockTestUserCountAsync();
        Task<PagedResult<AuditLog>> GetAuditLogsAsync(int pageNumber, int pageSize, string? searchKeyword = null);
    }
}
