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
            app.MapGet("/payments/validate_session/{sessionId}", async (HttpContext httpContext, IPaymentService paymentService, IAuditLogService auditLogService, string sessionId) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {                    
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId is null)
                    {
                        await auditLogService.CreateAuditLogAsync("Validate", "Payment", null, newValues: new { sessionId }, isSuccess: false);
                        return Results.Unauthorized();
                    }
                    var payment = await paymentService.ValidateSession(sessionId, userId);

                    await auditLogService.CreateAuditLogAsync("Validate", "Payment", payment.Id, newValues: new { sessionId }, isSuccess: payment != null
        );
                    return payment is null
                        ? Results.NotFound("Payment not found or validation failed.")
                        : Results.Ok(payment);

                });
                
            }).RequireAuthorization(); 

            app.MapPost("/payments/create_session", async (HttpContext httpContext, IPaymentService paymentService, IAuditLogService auditLogService, StripeRequestDto stripeRequestDto) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {                    
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId == null)
                    {
                        await auditLogService.CreateAuditLogAsync("Create", "Payment", null, newValues: stripeRequestDto, isSuccess: false);                        
                        return Results.Unauthorized();
                    }

                    var result = await paymentService.CreateSession(stripeRequestDto, userId);

                    await auditLogService.CreateAuditLogAsync("Create", "Payment", result?.SessionId ?? "Unknown", newValues: stripeRequestDto);

                    return Results.Ok(result);
                });
            }).RequireAuthorization();
        }
    }
}
