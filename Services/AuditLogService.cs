using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync()
        {
            return await _auditLogRepository.GetAllAsync();
        }

        public async Task<AuditLog> GetAuditLogByIdAsync(string id)
        {
            return await _auditLogRepository.GetByIdAsync(id);
        }

        public async Task CreateAuditLogAsync(AuditLog auditLog)
        {
            await _auditLogRepository.CreateAsync(auditLog);
        }

        public async Task<bool> UpdateAuditLogAsync(AuditLog auditLog)
        {
            return await _auditLogRepository.UpdateAsync(auditLog);
        }

        public async Task<bool> DeleteAuditLogAsync(string id)
        {
            return await _auditLogRepository.DeleteAsync(id);
        }
    }
}
