using Carter;
using Microsoft.AspNetCore.Mvc;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Endpoints
{
    public class NotificationModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/emails/send-contact-form", async ([FromForm] ContactFormRequest request, INotificationService emailService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    await emailService.SendContactFormEmailAsync(
                        request.ToEmail,
                        request.FirstName,
                        request.LastName,
                        request.Phone,
                        request.Message,
                        request.Attachments
                    );

                    return Results.Ok("Email sent successfully");
                });
            });

        }
    }
}
