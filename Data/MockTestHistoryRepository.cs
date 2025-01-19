using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MockTestApi.Data
{
    public class MockTestHistoryRepository : IMockTestHistoryRepository
    {
        private readonly IMongoCollection<MockTestHistory> _collection;

        public MockTestHistoryRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<MockTestHistory>("mock_test_histories");
        }

        public async Task<IEnumerable<MockTestHistory>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<MockTestHistory> GetByIdAsync(string id)
        {
            return await _collection.Find(mth => mth.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(MockTestHistory mockTestHistory)
        {
            if (mockTestHistory == null)
            {
                throw new ArgumentNullException(nameof(mockTestHistory));
            }

            if (string.IsNullOrEmpty(mockTestHistory.Id))
            {
                mockTestHistory.Id = ObjectId.GenerateNewId().ToString();
            }

            await _collection.InsertOneAsync(mockTestHistory);
        }

        public async Task<bool> UpdateAsync(MockTestHistory mockTestHistory)
        {
            if (mockTestHistory == null || string.IsNullOrEmpty(mockTestHistory.Id))
            {
                throw new ArgumentNullException(nameof(mockTestHistory));
            }

            var result = await _collection.ReplaceOneAsync(mth => mth.Id == mockTestHistory.Id, mockTestHistory);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(mth => mth.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
