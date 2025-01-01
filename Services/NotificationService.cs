using Hangfire;
using Microsoft.Extensions.Options;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    /// <summary>
    /// This service orchestrates which template and notfication handler to use based on the notification type.
    /// </summary>
    public class NotificationService(
            IOptions<MailTemplateSettings> templateSettings,
            BrevoEmailServiceHandler generalNotificationService,
            SendGridEmailServiceHandler transactionalNotificationService,
            INotificationRepository notificationRepository,
            IBackgroundJobClient backgroundJobClient
        ) : INotificationService
    {
        private readonly MailTemplateSettings _templateSettings = templateSettings.Value;
        private readonly BrevoEmailServiceHandler _generalNotificationService = generalNotificationService;
        private readonly SendGridEmailServiceHandler _transactionalNotificationService = transactionalNotificationService;
        private readonly INotificationRepository _notificationRepository = notificationRepository;
        private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

        public async Task SendThankYouEmailAsync(string toEmail, string toName, string validUntil)
        {
            var template = await LoadTemplateAsync(_templateSettings.ThankYouTemplate);
            var emailBody = template
                .Replace("{{Name}}", toName)
                .Replace("{{ValidUntil}}", validUntil);

            var emailMessage = new EmailMessage
            {
                To = toEmail,
                ToName = toName,
                Subject = $"Thank you for the purchase!",
                Body = emailBody,
                BodyPlainText = "Thanks for the membership purchase!\r\nYou now have access to over 500 test questions to practice to ensure you pass with confidence.",
                Bcc = _templateSettings.CcEmail,
                IsTransactional = true
            };
            
            await QueueEmailMessageAsync(emailMessage);
        }

        public async Task SendContactFormEmailAsync(string toEmail, string firstName, string lastName, string phone, string message, List<IFormFile>? attachments = null)
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
                ToName = firstName,
                Subject = $"Message received",
                Body = emailBody,
                BodyPlainText = "This email to acknowledge that your message has been received. We will get back to you as soon as we can.",
                Cc = _templateSettings.CcEmail,
                Attachments = attachments,
                IsTransactional=true
            };

            await QueueEmailMessageAsync(emailMessage);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink)
        {
            var template = await LoadTemplateAsync(_templateSettings.PasswordResetTemplate);
            var emailBody = template.Replace("{{reset_link}}", resetLink);

            var emailMessage = new EmailMessage
            {
                To = toEmail,
                ToName = toName,
                Subject = $"Password Reset Link",
                Body = emailBody,
                BodyPlainText = $"Click the link below to reset the password. Please take note that the link will expire in 24 hours.\r\n{resetLink}",
                Bcc = _templateSettings.CcEmail,
                IsTransactional = true
            };

            await QueueEmailMessageAsync(emailMessage);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string toName)
        {
            var template = await LoadTemplateAsync(_templateSettings.SignUpTemplate);
            var emailBody = template.Replace("{{Name}}", toName);

            var emailMessage = new EmailMessage
            {
                To = toEmail,
                ToName = toName,
                Subject = $"Sign up successful",
                Body = emailBody,
                BodyPlainText = "Nice to have you onboard! Sign in and take the first step towards passing your test with confidence!",
                Bcc = _templateSettings.CcEmail,
            };

            await QueueEmailMessageAsync(emailMessage);
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

        private async Task QueueEmailMessageAsync(EmailMessage message)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                Message = message,
                CreatedAt = DateTime.UtcNow,
                Status = "Queued"
            };

            await _notificationRepository.CreateAsync(notification);

            BackgroundJob.Schedule(() => ProcessEmailAsync(notification.Id), TimeSpan.Zero);
        }

        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new int[]{ 3 })]
        public async Task ProcessEmailAsync(string notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
                throw new InvalidOperationException($"Notification with ID {notificationId} not found.");
            // Choose the correct email service
            IEmailServiceHandler emailService = notification.Message.IsTransactional
                ? _transactionalNotificationService
                : _generalNotificationService;
            try
            {
                await emailService.SendEmailAsync(notification.Message);

                notification.Status = "Sent";
                notification.SentVia = emailService.GetType().Name;
                notification.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                notification.Status = "Failed";
                notification.SentVia = emailService.GetType().Name;
                notification.ErrorMessage = ex.Message;
            }
            finally
            {
                await _notificationRepository.UpdateAsync(notification);
            }
        }

    }
}
