namespace MockTestApi.Models
{
    public class SmtpSettings
    {
        public string Email { get; set; }

        public string DisplayName { get; set; }

        public string SmtpHost { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }

        public int SmtpPort { get; set; }

        public bool EnableSsl { get; set; }
        public bool IsActive { get; set; }

    }
}
