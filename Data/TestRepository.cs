using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MockTestApi.Data
{
    public class TestRepository : ITestRepository
    {
        private readonly IMongoCollection<Test> _collection;

        public TestRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Test>("tests");
        }

        public async Task<IEnumerable<Test>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<Test> GetByIdAsync(string id)
        {
            return await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();
        }        
    }
}
