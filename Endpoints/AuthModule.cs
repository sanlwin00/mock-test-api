using AutoMapper;
using Carter;
using Microsoft.AspNetCore.Antiforgery;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Endpoints
{
    public class AuthModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/auth/register", async (IUserService userService, RegisterRequest registerDto) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var loginResponse = await userService.RegisterUserAsync(registerDto);
                    return loginResponse == null
                        ? Results.Unauthorized()
                        : Results.Ok(new { loginResponse.Token, loginResponse.User });
                });
            });

            app.MapPost("/auth/login", async (IAuthenticationService authService, LoginRequest loginRequest) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var loginResponse = await authService.AuthenticateAsync(loginRequest);
                    return loginResponse == null
                        ? Results.Unauthorized()
                        : Results.Ok(new { loginResponse.Token, loginResponse.User });
                });
            });

            app.MapPost("/auth/login/accesscode", async (IAuthenticationService authService, AccessCodeRequest accessCodeRequest) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var loginResponse = await authService.AuthenticateWithAccessCodeAsync(accessCodeRequest.AccessCode);
                    if (loginResponse == null) throw new UnauthorizedAccessException("Invalid Access Code.");
                    return Results.Ok(new { loginResponse.Token, loginResponse.User });
                });
            });

            app.MapGet("/auth/profile", async (HttpContext httpContext, IMapper mapper, IUserService userService) =>
            {
                var userId = httpContext.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

                if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                var user = await userService.GetUserByIdAsync(userId);
                if (user == null) return Results.NotFound();

                var userDto = mapper.Map<UserDto>(user);
                return Results.Ok(userDto);
            }).RequireAuthorization();

            
            app.MapGet("/auth/csrf-token", (HttpContext context) =>
            {
                var tokens = context.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context);
                return Results.Ok(new { token = tokens.RequestToken });
            });
        }
    }
}