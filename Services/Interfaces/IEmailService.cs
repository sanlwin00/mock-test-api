namespace MockTestApi.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, List<IFormFile>? attachments = null, string cc = null, string bcc = null);
        Task SendThankYouEmailAsync(string toEmail, string name, string validUntil);
        Task SendContactFormEmailAsync(string toEmail, string firstName, string lastName, string phone, string message, List<IFormFile>? attachments = null);
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
        Task SendWelcomeEmailAsync(string toEmail, string name);
    }
}
