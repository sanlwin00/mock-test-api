using Microsoft.Extensions.Options;
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
        private readonly OpenApiSetting _apiSetting;
        public ChatService(IHttpClientFactory httpClientFactory, IOptions<OpenApiSetting> apiSetting)
        {
            _httpClientFactory = httpClientFactory;
            _apiSetting = apiSetting.Value;
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
                new CommonButton { ButtonText = "I need help with the question!", ButtonPrompt = "I need help with PSLE Preparation." }
            }
            };
        }

        public async Task<ChatResponse> GenerateChatResponseAsync(
            string lastPrompt,
            List<ChatMessage> conversationHistory,
            string? context = null,
            string? imageUrl = null)
        {
            var messages = CreateMessages(lastPrompt, imageUrl, context);

            var requestBody = new
            {
                model = "gpt-4o-mini", //"gpt-3.5-turbo",
                temperature = 0.5,
                messages = messages,
                max_tokens = 1000
            };

            return await SendRequestAsync(requestBody);
        }

        private string CreateSystemPrompt(string context)
        {
            var basePrompt = _apiSetting.Instruction;

            if (!string.IsNullOrEmpty(context))
            {
                basePrompt += " Use this as a context: " + context;
            }

            return basePrompt;
        }

        private List<object> CreateMessages(string lastPrompt, string? imageUrl, string? context)
        {
            var messages = new List<object>
            {
                new { role = "system", content = CreateSystemPrompt(context) }
            };

            var userMessage = new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "text", text = lastPrompt },
                        !string.IsNullOrEmpty(imageUrl) ? new
                        {
                            type = "image_url",
                            image_url = new
                            {
                                url = imageUrl,
                                detail = "high"
                            }
                        } : null
                }.Where(x => x != null).ToList() // Filter out null image values
            };

            messages.Add(userMessage);

            return messages;
        }

        private async Task<ChatResponse> SendRequestAsync(object requestBody)
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiSetting.ApiSecret);

            var jsonRequestBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_apiSetting.ApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Failed to get response from OpenAI. Response: {response}", response);
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                return new ChatResponse
                {
                    Success = false,
                    Result = $"Failed to get response from OpenAI. {errorResponse?.Error.Message}"
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
