using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MockTestApi.Data
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly IMongoCollection<PasswordResetToken> _collection;

        public PasswordResetTokenRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<PasswordResetToken>("password_reset_token");
        }

        public async Task<PasswordResetToken> GetByIdAsync(string token)
        {
            return await _collection.Find(t => t.Id == token).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(PasswordResetToken passwordResetToken)
        {
            if (passwordResetToken == null)
            {
                throw new ArgumentNullException(nameof(passwordResetToken));
            }

            if (string.IsNullOrEmpty(passwordResetToken.Id))
            {
                passwordResetToken.Id = ObjectId.GenerateNewId().ToString();
            }

            await _collection.InsertOneAsync(passwordResetToken);
        }

        public async Task<bool> DeleteAsync(string token)
        {
            var result = await _collection.DeleteOneAsync(t => t.Id == token);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}
