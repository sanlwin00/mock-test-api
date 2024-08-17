using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using Serilog;
using System.Text;
using System.Text.Json;

namespace MockTestApi.Services
{
    public class ChatService : IChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl = "https://api.openai.com/v1/chat/completions";
        private readonly string _apiKey = "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";
        public ChatService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public BotConfiguration LoadChatBotBaseConfiguration()
        {
            return new BotConfiguration
            {
                BotStatus = 1,
                StartUpMessage = "Hi, How can I help you?",
                UserAvatarURL = "https://www.citypost.com.sg/wp-content/uploads/2024/05/guestorange_avatar.png",
                BotImageURL = "https://www.citypost.com.sg/wp-content/uploads/2024/05/robored_avatar.png",
                CommonButtons = new List<CommonButton>
            {
                new CommonButton { ButtonText = "Hello!", ButtonPrompt = "Hello!" },
                new CommonButton { ButtonText = "I need help with PSLE Prep!", ButtonPrompt = "I need help with PSLE Preparation." }
            }
            };
        }

        public async Task<ChatResponse> GenerateChatResponseAsync(string lastPrompt, List<ChatMessage> conversationHistory, string context = null)
        {
            var systemPrompt = "You are a tutor assisting primary six students who are preparing for PSLE exam in Singapore."
                + " Answer the topics related to PSLE exam questions such as Math and Science in a simple and concise for children to understand. "
                + " If the user asks further questions related to the context, answer it. If it's out of context, say sorry I can't assist you with that. ";
            if (context != null)
            {
                systemPrompt += " Use this as a context: " + context;
            }

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                temperature = 0.7,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = lastPrompt }
                }
            };

            HttpClient client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_apiUrl, content);

            //HttpResponseMessage response = await client.PostAsJsonAsync(_apiUrl, requestBody);
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Failed to get response from OpenAI. Response:${response}", response);
                return new ChatResponse
                {
                    Success = false,
                    Result = $"Failed to get response from OpenAI. {response.ReasonPhrase}"
                };
            }

            var responseContent = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
            if (responseContent?.Choices?.FirstOrDefault()?.Message == null)
            {
                return new ChatResponse
                {
                    Success = false,
                    Result = $"Invalid response from OpenAI. {response.ReasonPhrase}"
                };
            }

            return new ChatResponse
            {
                Success = true,
                Result = responseContent.Choices.First().Message.Content
            };
        }
    }
}
