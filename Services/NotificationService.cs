using Microsoft.Extensions.Options;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using SendGrid.Helpers.Mail;
using System.Net.Mail;
using System.Net.Mime;

namespace MockTestApi.Services
{
    /// <summary>
    /// This service orchestrates which template and notfication handler to use based on the notification type.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly MailTemplateSettings _templateSettings;
        private readonly SmtpEmailServiceHandler _generalNotificationService;
        private readonly SendGridEmailServiceHandler _transactionalNotificationService;
        public NotificationService(IOptions<MailTemplateSettings> templateSettings,
            SmtpEmailServiceHandler generalNotificationService,
            SendGridEmailServiceHandler transactionalNotificationService)
        {
            _templateSettings = templateSettings.Value;
            _generalNotificationService = generalNotificationService;
            _transactionalNotificationService = transactionalNotificationService;
        }

        public async Task<bool> SendThankYouEmailAsync(string toEmail, string name, string validUntil)
        {
            var template = await LoadTemplateAsync(_templateSettings.ThankYouTemplate);
            var emailBody = template
                .Replace("{{Name}}", name)
                .Replace("{{ValidUntil}}", validUntil);

            var emailMessage = new EmailMessage
            {
                To = toEmail,
                Subject = $"[{_templateSettings.AppName}] Thank you!",
                Body = emailBody,
                BodyPlainText = "Thanks for the membership purchase!\r\nYou now have access to over 500 test questions to practice to ensure you pass with confidence.",
                Bcc = _templateSettings.CcEmail,
            };

            return await _transactionalNotificationService.SendEmailAsync(emailMessage);
        }

        public async Task<bool> SendContactFormEmailAsync(string toEmail, string firstName, string lastName, string phone, string message, List<IFormFile>? attachments = null)
        {
            var template = await LoadTemplateAsync(_templateSettings.EnquiryTemplate);
            var emailBody = template
                .Replace("{{First}}", firstName)
                .Replace("{{Last}}", lastName)
                .Replace("{{Phone}}", phone)
                .Replace("{{Email}}", toEmail)
                .Replace("{{Message}}", message)
                .Replace("{{AttachmentCount}}", (attachments == null ? "0" : attachments?.Count().ToString()) + " file(s)");

            var emailMessage = new EmailMessage
            {
                To = toEmail,
                Subject = $"[{_templateSettings.AppName}] Message received",
                Body = emailBody,
                BodyPlainText = "This email to acknowledge that your message has been received. We will get back to you as soon as we can.",
                Cc = _templateSettings.CcEmail,
                Attachments = attachments
            };

            return await _transactionalNotificationService.SendEmailAsync(emailMessage);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var template = await LoadTemplateAsync(_templateSettings.PasswordResetTemplate);
            var emailBody = template.Replace("{{reset_link}}", resetLink);

            var emailMessage = new EmailMessage
            {
                To = toEmail,
                Subject = $"[{_templateSettings.AppName}] Password Reset Link",
                Body = emailBody,
                BodyPlainText = $"Click the link below to reset the password. Please take note that the link will expire in 24 hours.\r\n{resetLink}",
                Bcc = _templateSettings.CcEmail,
            };

            return await _transactionalNotificationService.SendEmailAsync(emailMessage);
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string name)
        {
            var template = await LoadTemplateAsync(_templateSettings.SignUpTemplate);
            var emailBody = template.Replace("{{Name}}", name);

            var emailMessage = new EmailMessage
            {
                To = toEmail,
                Subject = $"[{_templateSettings.AppName}] Sign up successful",
                Body = emailBody,
                BodyPlainText = "Nice to have you onboard! Sign in and take the first step towards passing your test with confidence!",
                Bcc = _templateSettings.CcEmail,
            };

            return await _generalNotificationService.SendEmailAsync(emailMessage);
        }

        private async Task<string> LoadTemplateAsync(string templateFileName)
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), _templateSettings.TemplateFolder, templateFileName);
            var templateContent = await File.ReadAllTextAsync(templatePath);
            templateContent = templateContent
               .Replace("{{AppName}}", _templateSettings.AppName)
               .Replace("{{StartYear}}", _templateSettings.StartYear)
               .Replace("{{CurrentYear}}", DateTime.UtcNow.Year.ToString())
               .Replace("{{ContactEmail}}", _templateSettings.ContactEmail)
               .Replace("{{HeaderImageUrl}}", _templateSettings.HeaderImageUrl)
               .Replace("{{LoginUrl}}", _templateSettings.LoginUrl)
               .Replace("{{PrivacyPolicyUrl}}", _templateSettings.PrivacyPolicyUrl);
            return templateContent;
        }

    }
}
