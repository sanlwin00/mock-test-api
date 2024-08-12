using MockTestApi.Models;
namespace MockTestApi.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<Payment> GetPaymentByIdAsync(string id);
        Task CreatePaymentAsync(Payment payment);
        Task<bool> UpdatePaymentAsync(Payment payment);
        Task<bool> DeletePaymentAsync(string id);
        Task<StripeRequestDto> CreateSession(StripeRequestDto stripeRequestDto, string userId);
        Task<PaymentDto> ValidateSession(string paymentId, string userId);
    }
}
