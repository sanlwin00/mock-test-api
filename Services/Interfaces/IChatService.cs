using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IChatService
    {
        Task<ChatResponse> GenerateChatResponseAsync(string lastPrompt, List<ChatMessage> conversationHistory, string context = null);
        BotConfiguration LoadChatBotBaseConfiguration();
    }
}
