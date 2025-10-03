using Carter;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Modules
{
    public class ChatModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/chat/config", async (IChatService chatService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    return Results.Ok(chatService.LoadChatBotBaseConfiguration());
                });
            });

            app.MapPost("/chat/send", async (ChatRequest chatRequest, IChatService chatService) =>
            {
                return await RequestHandler.HandleRequestAsync(async () =>
                {
                    var chatResponse = await chatService.GenerateChatResponseAsync(chatRequest.LastPrompt, chatRequest.ConversationHistory, chatRequest.Context, chatRequest.ImageUrl);
                    return Results.Ok(chatResponse);
                });
            });
        }
    }
}
