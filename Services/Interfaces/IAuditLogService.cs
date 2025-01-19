using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync();
        Task CreateAuditLogAsync(AuditLog auditLog);
    }
}
