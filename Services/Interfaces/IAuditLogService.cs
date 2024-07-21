using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync();
        Task<AuditLog> GetAuditLogByIdAsync(string id);
        Task CreateAuditLogAsync(AuditLog auditLog);
        Task<bool> UpdateAuditLogAsync(AuditLog auditLog);
        Task<bool> DeleteAuditLogAsync(string id);
    }
}
