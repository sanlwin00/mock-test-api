namespace MockTestApi.Services.Interfaces
{
    public interface INotificationService
    {
        Task<bool> SendThankYouEmailAsync(string toEmail, string name, string validUntil);
        Task<bool> SendContactFormEmailAsync(string toEmail, string firstName, string lastName, string phone, string message, List<IFormFile>? attachments = null);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string name);
    }
}
