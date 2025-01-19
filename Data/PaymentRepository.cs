using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MockTestApi.Data
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IMongoCollection<Payment> _collection;

        public PaymentRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Payment>("payments");
        }

        public async Task<Payment> GetByIdAsync(string id)
        {
            return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            if (string.IsNullOrEmpty(payment.Id))
            {
                payment.Id = ObjectId.GenerateNewId().ToString();
            }

            await _collection.InsertOneAsync(payment);
        }

        public async Task<bool> UpdateAsync(Payment payment)
        {
            var result = await _collection.ReplaceOneAsync(p => p.Id == payment.Id, payment);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}
