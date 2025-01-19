using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MockTestApi.Data
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly IMongoCollection<AuditLog> _collection;

        public AuditLogRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<AuditLog>("audit_logs");
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task CreateAsync(AuditLog auditLog)
        {
            if (auditLog == null)
            {
                throw new ArgumentNullException(nameof(auditLog));
            }

            if (string.IsNullOrEmpty(auditLog.Id))
            {
                auditLog.Id = ObjectId.GenerateNewId().ToString();
            }

            await _collection.InsertOneAsync(auditLog);
        }
    }
}
