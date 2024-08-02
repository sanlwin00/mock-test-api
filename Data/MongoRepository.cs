using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MockTestApi.Data
{
    public class MongoRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<T>(collectionName);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<T> GetByIdAsync(string id)
        {
            return await _collection.Find(entity => entity.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            // Initialize Id if it's null or empty
            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = ObjectId.GenerateNewId().ToString();
            }
            await _collection.InsertOneAsync(entity);
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            var result = await _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(entity => entity.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}
