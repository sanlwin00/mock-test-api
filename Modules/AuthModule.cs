using AutoMapper;
using Carter;
using Microsoft.AspNetCore.Antiforgery;
using MockTestApi.Models;
using MockTestApi.Services;
using MockTestApi.Services.Interfaces;
using MockTestApi.Utils;

namespace MockTestApi.Modules
{
    public class AuthModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/register", async (IUserService userService, IAuditLogService auditLogService, RegisterRequest registerDto) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var loginResponse = await userService.RegisterUserAsync(registerDto);
                    var userId = loginResponse?.User?.Id ?? "Unknown";                    

                    var sanitizedRegisterDto = MyUtility.RemoveSensitiveProperties(registerDto, "Password");
                    await auditLogService.CreateAuditLogAsync("Register", "User", userId, newValues: sanitizedRegisterDto, isSuccess: loginResponse != null);

                    return loginResponse == null
                        ? Results.Unauthorized()
                        : Results.Ok(new { loginResponse.Token, loginResponse.User });
                });
            });

            app.MapPost("/auth/login", async (IAuthenticationService authService, IAuditLogService auditLogService, LoginRequest loginRequest) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var loginResponse = await authService.AuthenticateAsync(loginRequest);
                    var userId = loginResponse?.User?.Id ?? "Unknown";

                    var sanitizedLoginRequest = MyUtility.RemoveSensitiveProperties(loginRequest, "Password");
                    await auditLogService.CreateAuditLogAsync("Login", "User", userId, userName: loginRequest.Username, isSuccess: loginResponse != null);

                    return loginResponse == null
                        ? Results.Unauthorized()
                        : Results.Ok(new { loginResponse.Token, loginResponse.User });
                });
            });

            app.MapPost("/auth/login/accesscode", async (IAuthenticationService authService, IAuditLogService auditLogService, AccessCodeRequest accessCodeRequest) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var loginResponse = await authService.AuthenticateWithAccessCodeAsync(accessCodeRequest.AccessCode);
                    var userId = loginResponse?.User?.Id ?? "Unknown";

                    await auditLogService.CreateAuditLogAsync("LoginWithAccessCode", "User", userId, newValues: accessCodeRequest, isSuccess: loginResponse != null);

                    if (loginResponse == null) throw new UnauthorizedAccessException("Invalid Access Code.");
                    return Results.Ok(new { loginResponse.Token, loginResponse.User });
                });
            });            

            app.MapGet("/auth/csrf-token", async (HttpContext context) =>
            {
                return await RequestHandler.HandleRequestAsync(() =>
                {
                    var tokens = context.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context);
                    return Task.FromResult(Results.Ok(new { token = tokens.RequestToken }));
                });
            });
        }
    }
}
