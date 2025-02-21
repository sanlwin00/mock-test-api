using AutoMapper;
using Carter;
using Microsoft.AspNetCore.Antiforgery;
using MockTestApi.Models;
using MockTestApi.Services;
using MockTestApi.Services.Interfaces;
using MockTestApi.Utils;

namespace MockTestApi.Endpoints
{
    public class AuthModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/register", async (IUserService userService, IAuditLogService auditLogService, IAuditLogFactory auditLogFactory, RegisterRequest registerDto) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var loginResponse = await userService.RegisterUserAsync(registerDto);
                    var userId = loginResponse?.User?.Id ?? "Unknown";
                    var userName = loginResponse?.User?.Email ?? "Unknown";

                    var sanitizedRegisterDto = MyUtility.RemoveSensitiveProperties(registerDto, "Password");
                    var auditLog = auditLogFactory.CreateAuditLog(userId, userName, "Register", "User", userId, newValues: sanitizedRegisterDto, isSuccess: loginResponse != null);
                    await auditLogService.CreateAuditLogAsync(auditLog);

                    return loginResponse == null
                        ? Results.Unauthorized()
                        : Results.Ok(new { loginResponse.Token, loginResponse.User });
                });
            });

            app.MapPost("/auth/login", async (IAuthenticationService authService, IAuditLogService auditLogService, IAuditLogFactory auditLogFactory, LoginRequest loginRequest) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var loginResponse = await authService.AuthenticateAsync(loginRequest);
                    var userId = loginResponse?.User?.Id ?? "Unknown";
                    var userName = loginResponse?.User?.Email ?? "Unknown";

                    var sanitizedLoginRequest = MyUtility.RemoveSensitiveProperties(loginRequest, "Password");
                    var auditLog = auditLogFactory.CreateAuditLog(userId, userName, "Login", "User", userId, newValues: sanitizedLoginRequest, isSuccess: loginResponse != null);
                    await auditLogService.CreateAuditLogAsync(auditLog);

                    return loginResponse == null
                        ? Results.Unauthorized()
                        : Results.Ok(new { loginResponse.Token, loginResponse.User });
                });
            });

            app.MapPost("/auth/login/accesscode", async (IAuthenticationService authService, IAuditLogService auditLogService, IAuditLogFactory auditLogFactory, AccessCodeRequest accessCodeRequest) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var loginResponse = await authService.AuthenticateWithAccessCodeAsync(accessCodeRequest.AccessCode);
                    var userId = loginResponse?.User?.Id ?? "Unknown";
                    var userName = loginResponse?.User?.Email ?? "Unknown";

                    var auditLog = auditLogFactory.CreateAuditLog(userId, userName, "LoginWithAccessCode", "User", userId, newValues: accessCodeRequest, isSuccess: loginResponse != null);
                    await auditLogService.CreateAuditLogAsync(auditLog);

                    if (loginResponse == null) throw new UnauthorizedAccessException("Invalid Access Code.");
                    return Results.Ok(new { loginResponse.Token, loginResponse.User });
                });
            });

            app.MapGet("/auth/profile", async (HttpContext httpContext, IMapper mapper, IUserService userService, IAuditLogService auditLogService, IAuditLogFactory auditLogFactory) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var userId = httpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

                    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                    var user = await userService.GetUserByIdAsync(userId);
                    if (user == null) return Results.NotFound();

                    var userDto = mapper.Map<UserDto>(user);

                    var auditLog = auditLogFactory.CreateAuditLog(userId, user.Email, "ViewProfile", "User", userId);
                    await auditLogService.CreateAuditLogAsync(auditLog);

                    return Results.Ok(userDto);
                });
            }).RequireAuthorization();

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
