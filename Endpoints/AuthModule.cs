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
            app.MapPatch("/users/{id}", async (IUserService userService, UpdateUserDto updateUserDto, string id) =>
            {
                return await HandleRequestAsync(async () =>
                {
                    var success = await userService.UpdateUserAsync(id, updateUserDto);
                    return success ? Results.NoContent() : Results.NotFound();
                });
            }).RequireAuthorization();

            // Auth endpoints
            app.MapPost("/auth/register", async (IUserService userService, RegisterRequest registerDto) =>
            {
                return await HandleRequestAsync(async () =>
                {
                    var loginResponse = await userService.RegisterUserAsync(registerDto);
                    return loginResponse == null
                        ? Results.Unauthorized()
                        : Results.Ok(new { loginResponse.Token, loginResponse.User });
                });
            });

            app.MapPost("/auth/login", async (IUserService userService, LoginRequest loginRequest) =>
            {
                return await HandleRequestAsync(async () =>
                {
                    var loginResponse = await userService.AuthenticateAsync(loginRequest);
                    return loginResponse == null
                        ? Results.Unauthorized()
                        : Results.Ok(new { loginResponse.Token, loginResponse.User });
                });
            });

            app.MapPost("/auth/login/accesscode", async (IUserService userService, AccessCodeRequest accessCodeRequest) =>
            {
                return await HandleRequestAsync(async () =>
                {
                    var loginResponse = await userService.AuthenticateWithAccessCodeAsync(accessCodeRequest.AccessCode);
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

            app.MapPost("/auth/request-password-reset", async (PasswordResetRequest passwordResetRequest, IUserService userService) =>
            {
                var success = await userService.RequestPasswordResetAsync(passwordResetRequest.Email, passwordResetRequest.PasswordResetUrl);
                return success ? Results.Ok("Password reset email sent") : Results.NotFound("User not found");
            });

            app.MapPost("/auth/reset-password", async (PasswordResetDto passwordResetDto, IUserService userService) =>
            {
                var success = await userService.ResetPasswordAsync(passwordResetDto.Token, passwordResetDto.Password);
                return success ? Results.Ok("Password has been reset") : Results.BadRequest("Invalid or expired link. Please request for a new reset link.");
            });

            app.MapGet("/auth/csrf-token", (HttpContext context) =>
            {
                var tokens = context.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context);
                return Results.Ok(new { token = tokens.RequestToken });
            });
        }

        private async Task<IResult> HandleRequestAsync(Func<Task<IResult>> action)
        {
            try
            {
                return await action();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(new { message = ex.Message }, statusCode: 401);
            }
            catch (Exception ex)
            {
                return Results.Json(new { message = ex.Message }, statusCode: 500);
            }
        }
    }
}