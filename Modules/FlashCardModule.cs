using Carter;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using System.Security.Claims;

namespace MockTestApi.Modules
{
    public class FlashCardModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/flashcards/decks", async (HttpContext httpContext, IFlashCardService flashCardService, IWebHostEnvironment env) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var tenant = env.EnvironmentName.Split('.')[0];
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var decks = await flashCardService.GetDecksAsync(tenant, userId);
                    return Results.Ok(decks);
                });
            });

            app.MapGet("/flashcards/decks/{slug}", async (string slug, IFlashCardService flashCardService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var deck = await flashCardService.GetDeckWithCardsAsync(slug);
                    if (deck == null) return Results.NotFound();
                    return Results.Ok(deck);
                });
            });

            app.MapPost("/flashcards/sessions", async (HttpContext httpContext, CreateFlashCardSessionDto dto, IFlashCardService flashCardService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                        return Results.Unauthorized();

                    await flashCardService.SaveSessionAsync(userId, dto);
                    return Results.Created("/flashcards/sessions", null);
                });
            });
        }
    }
}
