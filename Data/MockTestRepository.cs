using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MockTestApi.Data
{
    public class MockTestRepository : IMockTestRepository
    {
        private readonly IMongoCollection<MockTest> _collection;

        public MockTestRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<MockTest>("mock_tests");
        }

        public async Task<IEnumerable<MockTest>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<MockTest> GetByIdAsync(string id)
        {
            return await _collection.Find(mt => mt.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(MockTest mockTest)
        {
            if (mockTest == null)
            {
                throw new ArgumentNullException(nameof(mockTest));
            }

            if (string.IsNullOrEmpty(mockTest.Id))
            {
                mockTest.Id = ObjectId.GenerateNewId().ToString();
            }

            await _collection.InsertOneAsync(mockTest);
        }

        public async Task<bool> UpdateAsync(MockTest mockTest)
        {
            if (mockTest == null || string.IsNullOrEmpty(mockTest.Id))
            {
                throw new ArgumentNullException(nameof(mockTest));
            }

            var result = await _collection.ReplaceOneAsync(mt => mt.Id == mockTest.Id, mockTest);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(mt => mt.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
