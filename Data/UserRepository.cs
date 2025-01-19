using MockTestApi.Data.Interfaces;
using MockTestApi.Models;

using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MockTestApi.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _collection;

        public UserRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<User>("users");
        }
        public async Task<User> GetByIdAsync(string id)
        {
            return await _collection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }
        public Task<User> GetByAccessCodeAsync(string accessCode)
        {
            return _collection.Find(user => user.Subscription.AccessCode == accessCode).FirstOrDefaultAsync();
        }

        public Task<User> GetByUsernameAsync(string username)
        {
            return _collection.Find(user => user.Email == username).FirstOrDefaultAsync();
        }
        public async Task CreateAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = ObjectId.GenerateNewId().ToString();
            }

            await _collection.InsertOneAsync(user);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            var result = await _collection.ReplaceOneAsync(u => u.Id == user.Id, user);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}

