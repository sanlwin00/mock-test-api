namespace MockTestApi.Models
{
    public class SmtpSetting
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public string DisplayName { get; set; }

        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }

        public bool EnableSsl { get; set; }

    }
}
