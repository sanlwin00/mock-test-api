using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Driver;

namespace MockTestApi.Data
{
    public class MongoUserStore : IUserStore
    {
        private readonly IMongoCollection<User> _collection;

        public MongoUserStore(IMongoDatabase database)
        {
            _collection = database.GetCollection<User>("users");
        }

        public Task<User> GetByAccessCodeAsync(string accessCode)
        {
            return _collection.Find(user => user.Subscription.AccessCode == accessCode).FirstOrDefaultAsync();
        }

        public Task<User> GetByUsernameAsync(string username)
        {
            return _collection.Find(user => user.Email == username).FirstOrDefaultAsync();
        }
    }
}
