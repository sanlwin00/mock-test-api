using Carter;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Endpoints
{
    public class UserModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Update user partially
            app.MapPatch("/users/{id}", async (IUserService userService, UpdateUserDto updateUserDto, string id) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var success = await userService.UpdateUserAsync(id, updateUserDto);
                    return success ? Results.NoContent() : Results.NotFound();
                });
            }).RequireAuthorization();
        }
    }
}