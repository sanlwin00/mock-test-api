using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> GetByIdAsync(string id);
        Task CreateAsync(Payment payment);
        Task<bool> UpdateAsync(Payment payment);
    }
}
