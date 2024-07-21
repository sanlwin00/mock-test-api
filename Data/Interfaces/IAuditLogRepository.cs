using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<bool> UpdateAsync(AuditLog auditLog);
    }
}
