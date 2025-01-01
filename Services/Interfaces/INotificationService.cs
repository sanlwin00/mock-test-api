namespace MockTestApi.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendThankYouEmailAsync(string toEmail, string toName, string validUntil);
        Task SendContactFormEmailAsync(string toEmail, string firstName, string lastName, string phone, string message, List<IFormFile>? attachments = null);
        Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink);
        Task SendWelcomeEmailAsync(string toEmail, string toName);
    }
}
