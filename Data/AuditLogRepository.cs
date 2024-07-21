using MockTestApi.Data.Interfaces;
using MockTestApi.Models;

namespace MockTestApi.Data
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly IRepository<AuditLog> _repository;

        public AuditLogRepository(IRepository<AuditLog> repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<AuditLog> GetByIdAsync(string id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task CreateAsync(AuditLog auditLog)
        {
            return _repository.CreateAsync(auditLog);
        }

        public Task<bool> UpdateAsync(AuditLog auditLog)
        {
            return _repository.UpdateAsync(auditLog);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return _repository.DeleteAsync(id);
        }
    }
}
