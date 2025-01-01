using Carter;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Endpoints
{
    public class PasswordResetModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/request-password-reset", async (PasswordResetRequest passwordResetRequest, IPasswordResetService pwResetService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    try
                    {
                        await pwResetService.RequestPasswordResetAsync(passwordResetRequest.Email, passwordResetRequest.PasswordResetUrl);
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.NotFound(ex.Message);
                    }
                    return Results.Ok("Password reset email sent");
                });
            });

            app.MapPost("/auth/reset-password", async (PasswordResetDto passwordResetDto, IPasswordResetService pwResetService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var success = await pwResetService.ResetPasswordAsync(passwordResetDto.Token, passwordResetDto.Password);
                    return success ? Results.Ok("Password has been reset") : Results.BadRequest("Invalid or expired link. Please request for a new reset link.");
                });
            });
        }
    }
}