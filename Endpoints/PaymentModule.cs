using Carter;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MockTestApi.Endpoints
{
    public class PaymentModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/payments/validate_session/{sessionId}", async (HttpContext httpContext, IPaymentService paymentService, string sessionId) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {                    
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId is null)
                        return Results.Unauthorized();

                    var payment = await paymentService.ValidateSession(sessionId, userId);
                    return payment is null
                        ? Results.NotFound("Payment not found or validation failed.")
                        : Results.Ok(payment);

                });
                
            }).RequireAuthorization(); 

            app.MapPost("/payments/create_session", async (HttpContext httpContext, IPaymentService paymentService, StripeRequestDto stripeRequestDto) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {                    
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId == null)
                        return Results.Unauthorized();

                    var result = await paymentService.CreateSession(stripeRequestDto, userId);
                    return Results.Ok(result);
                });
            }).RequireAuthorization();
        }
    }
}
