using System.Text.Json.Serialization;

namespace MockTestApi.Models
{
    public class OpenApiSetting
    {
        public string ApiUrl { get; set; }
        public string ApiSecret { get; set; }
        public string Instruction { get; set; }
    }
    public class ChatRequest
    {
        public string LastPrompt { get; set; }
        public List<ChatMessage> ConversationHistory { get; set; }
        public string? Context { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
    public class ChatResponse
    {
        public bool Success { get; set; }
        public string Result { get; set; }
    }

    public class OpenAIResponse
    {
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        public Message Message { get; set; }
    }

    public class Message
    {
        public string Content { get; set; }
    }

    public class BotConfiguration
    {
        public int BotStatus { get; set; }
        public string StartUpMessage { get; set; }
        public string UserAvatarURL { get; set; }
        public string BotImageURL { get; set; }
        public List<CommonButton> CommonButtons { get; set; }
    }

    public class CommonButton
    {
        public string ButtonText { get; set; }
        public string ButtonPrompt { get; set; }
    }

    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public ErrorDetail Error { get; set; }

        public class ErrorDetail
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("param")]
            public string Param { get; set; }

            [JsonPropertyName("code")]
            public string Code { get; set; }
        }
    }
}
