using AutoMapper;
using Carter;
using MockTestApi.Models;
using MockTestApi.Services;
using MockTestApi.Services.Interfaces;
using MockTestApi.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MockTestApi.Modules
{
    public class UserModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/users/profile", async (HttpContext httpContext, IMapper mapper, IUserService userService, IAuditLogService auditLogService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {                    
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    
                    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

                    var user = await userService.GetUserByIdAsync(userId);
                    if (user == null) return Results.NotFound();

                    var userDto = mapper.Map<UserDto>(user);

                    await auditLogService.CreateAuditLogAsync("Fetch", "User", userId);

                    return Results.Ok(userDto);
                });
            }).RequireAuthorization();

            // Update user partially
            app.MapPatch("/users/{id}", async (IUserService userService, UpdateUserDto updateUserDto, string id, IAuditLogService auditLogService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var user = await userService.GetUserByIdAsync(id);
                    var sanitizedUser = MyUtility.KeepTheseProperties(user, "DisplayName", "PhoneNumber", "TimeZone");

                    var success = await userService.UpdateUserAsync(id, updateUserDto);

                    await auditLogService.CreateAuditLogAsync("Update", "User", id, newValues: updateUserDto, oldValues: sanitizedUser, isSuccess: success);

                    return success ? Results.NoContent() : Results.NotFound();
                });
            }).RequireAuthorization();
        }
    }
}