using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<IEnumerable<AuditLog>> GetAllAsync();
        Task CreateAsync(AuditLog auditLog);
        Task<MockTestUserCount> GetMockTestStartedEventCountAsync();
    }
}
