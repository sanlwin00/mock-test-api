using Carter;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Endpoints
{
    public class PasswordResetModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/request-password-reset", async (PasswordResetRequest passwordResetRequest, IPasswordResetService pwResetService, IAuditLogService auditLogService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    try
                    {
                        await pwResetService.RequestPasswordResetAsync(passwordResetRequest.Email, passwordResetRequest.PasswordResetUrl);

                        await auditLogService.CreateAuditLogAsync(
                            "RequestPasswordReset",
                            "User",
                            passwordResetRequest.Email,
                            newValues: passwordResetRequest,
                            isSuccess: true
                        );
                    }
                    catch (ArgumentException ex)
                    {
                        await auditLogService.CreateAuditLogAsync(
                            "RequestPasswordReset",
                            "User",
                            passwordResetRequest.Email,
                            newValues: passwordResetRequest,
                            isSuccess: false
                        );
                        return Results.NotFound(ex.Message);
                    }
                    return Results.Ok("Password reset email sent");
                });
            });

            app.MapPost("/auth/reset-password", async (PasswordResetDto passwordResetDto, IPasswordResetService pwResetService, IAuditLogService auditLogService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var success = await pwResetService.ResetPasswordAsync(passwordResetDto.Token, passwordResetDto.Password);

                    await auditLogService.CreateAuditLogAsync(
                        "ResetPassword",
                        "User",
                        passwordResetDto.Token,
                        isSuccess: success
                    );
                    return success ? Results.Ok("Password has been reset") : Results.BadRequest("Invalid or expired link. Please request for a new reset link.");
                });
            });
        }
    }
}