namespace MockTestApi.Models
{
    public class SendGridApiSettings
    {
        public string ApiKey { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
    }
    public class BrevoApiSettings
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
    }
}
