using System.Text.Json.Serialization;

namespace MockTestApi.Models
{
    public class LoginRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("user")]
        public UserDto User { get; set; }
    }
}
