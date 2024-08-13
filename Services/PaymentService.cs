using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using MockTestApi.Utils;
using MongoDB.Bson.IO;
using Serilog;
using Stripe;
using Stripe.Checkout;

namespace MockTestApi.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        public PaymentService(IPaymentRepository paymentRepository, IMapper mapper, IUserService userService)
        {
            _paymentRepository = paymentRepository;
            _mapper = mapper;
            _userService = userService;
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

        public async Task<StripeRequestDto> CreateSession(StripeRequestDto stripeRequestDto, string userId)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    AutomaticTax = new SessionAutomaticTaxOptions { Enabled = true },
                };

                var stripeLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long?)(stripeRequestDto.Product.Price * 100),
                        Currency = stripeRequestDto.Currency.ToLower(),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = stripeRequestDto.Product.Name
                        },
                    },
                    Quantity = stripeRequestDto.Product.Qty
                };
                options.LineItems.Add(stripeLineItem);

                Log.Information("## Creating stripe session...");
                Log.Debug("Stripe options: {@Options}", options);
                var service = new SessionService();
                Session session = service.Create(options);
                Log.Information("## Stripe session created. SessionID: {Id}", session.Id);

                stripeRequestDto.SessionUrl = session.Url;
                stripeRequestDto.SessionId = session.Id;

                Payment payment = new Payment
                {
                    Id = session.Id,
                    Customer = userId,
                    Amount = stripeRequestDto.Product.Price * stripeRequestDto.Product.Qty,
                    Currency = stripeRequestDto.Currency,
                    Status = "pending",
                    PaymentMethod = "stripe",
                    Description = stripeRequestDto.Product.Name,
                    Created = DateTime.UtcNow,
                };
                await _paymentRepository.CreateAsync(payment);

                Log.Information("## Payment {Id} updated.", payment.Id);

                return stripeRequestDto;
            }
            catch (Exception ex)
            {
                Log.Error("Exception occured: {@ex}", ex);
                throw ex;
            }
            return null;
        }


        public async Task<PaymentDto> ValidateSession(string paymentId, string userId)
        {
            try
            {
                Log.Information("## Retrieving payment {id} ...", paymentId);
                var payment = await _paymentRepository.GetByIdAsync(paymentId);
                if (payment.Customer != userId)
                {
                    throw new InvalidOperationException("Not authorized.");
                }

                var service = new SessionService();
                Session session = service.Get(paymentId);

                var paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                if (paymentIntent != null)
                {                    
                    if (paymentIntent.Status.ToLower() == payment.Status.ToLower())
                    {
                        // no update required, just return the payment object
                        return _mapper.Map<PaymentDto>(payment); 
                    }
                    Log.Information("## Updating status and paymentIntent : {PaymentIntentId} ...", paymentIntent.Id);
                    payment.Status = paymentIntent.Status; 
                    payment.PaymentRef = paymentIntent.Id;

                    PaymentMethodService paymentMethodService = new PaymentMethodService();
                    PaymentMethod paymentMethod = paymentMethodService.Get(paymentIntent.PaymentMethodId);

                    payment.BillingDetails = new BillingDetails
                    {
                        Name = paymentMethod.BillingDetails.Name,
                        Email = paymentMethod.BillingDetails.Email,
                        Phone = paymentMethod.BillingDetails.Phone,
                        Address = new Models.Address
                        {
                            City = paymentMethod.BillingDetails.Address.City,
                            Country = paymentMethod.BillingDetails.Address.Country,
                            State = paymentMethod.BillingDetails.Address.State,
                            PostalCode = paymentMethod.BillingDetails.Address.PostalCode
                        }
                    };

                    await _paymentRepository.UpdateAsync(payment);

                    // update subscription
                    if (paymentIntent.Status.ToLower() == "succeeded")
                    {
                        User user = await _userService.GetUserByIdAsync(userId);
                        if(user != null)
                        {
                            user.Subscription.Plan = "premium";
                            user.Subscription.AccessCode = Utility.GetRandomString(8);
                            user.Subscription.StartDate = DateTime.UtcNow;
                            user.Subscription.EndDate = DateTime.UtcNow.AddDays(30);
                            await _userService.UpdateUserAsync(user);
                        }
                    }

                    return _mapper.Map<PaymentDto>(payment);
                }
                else
                {
                    Log.Warning("Unable to retrieve payment intent. {@PaymentIntent}", paymentIntent);
                    throw new Exception("Unable to retrieve payment intent.");
                }
            }
            catch (Exception ex)
            {               
                Log.Error("Exception occured: {@ex}", ex);
                throw ex;
            }
            return null;
        }
    }
}
