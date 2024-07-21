using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _paymentRepository.GetAllAsync();
        }

        public async Task<Payment> GetPaymentByIdAsync(string id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        public async Task CreatePaymentAsync(Payment payment)
        {
            await _paymentRepository.CreateAsync(payment);
        }

        public async Task<bool> UpdatePaymentAsync(Payment payment)
        {
            return await _paymentRepository.UpdateAsync(payment);
        }

        public async Task<bool> DeletePaymentAsync(string id)
        {
            return await _paymentRepository.DeleteAsync(id);
        }
    }
}
