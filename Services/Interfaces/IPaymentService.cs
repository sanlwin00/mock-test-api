using MockTestApi.Models;
namespace MockTestApi.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<StripeRequestDto> CreateSession(StripeRequestDto stripeRequestDto, string userId);
        Task<PaymentDto> ValidateSession(string paymentId, string userId);
    }
}
