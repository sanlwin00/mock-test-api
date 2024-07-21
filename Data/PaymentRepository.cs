using MockTestApi.Data.Interfaces;
using MockTestApi.Models;

namespace MockTestApi.Data
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IRepository<Payment> _repository;

        public PaymentRepository(IRepository<Payment> repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Payment>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<Payment> GetByIdAsync(string id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task CreateAsync(Payment payment)
        {
            return _repository.CreateAsync(payment);
        }

        public Task<bool> UpdateAsync(Payment payment)
        {
            return _repository.UpdateAsync(payment);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return _repository.DeleteAsync(id);
        }
    }
}
