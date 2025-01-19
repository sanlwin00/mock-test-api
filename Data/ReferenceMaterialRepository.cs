using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MockTestApi.Data
{
    public class ReferenceMaterialRepository : IReferenceMaterialRepository
    {
        private readonly IMongoCollection<ReferenceMaterial> _collection;

        public ReferenceMaterialRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<ReferenceMaterial>("reference_materials");
        }

        public async Task<IEnumerable<ReferenceMaterial>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<ReferenceMaterial> GetByIdAsync(string id)
        {
            return await _collection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }
    }
}
