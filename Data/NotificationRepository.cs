using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MockTestApi.Data
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification> _collection;

        public NotificationRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Notification>("notifications");
        }

        public async Task<Notification> GetByIdAsync(string id)
        {
            return await _collection.Find(n => n.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            if (string.IsNullOrEmpty(notification.Id))
            {
                notification.Id = ObjectId.GenerateNewId().ToString();
            }

            await _collection.InsertOneAsync(notification);
        }

        public async Task<bool> UpdateAsync(Notification notification)
        {
            var result = await _collection.ReplaceOneAsync(n => n.Id == notification.Id, notification);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}
