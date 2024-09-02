using Microsoft.Extensions.Options;
using MockTestApi.Models;
using SendGrid.Helpers.Mail;
using SendGrid;
using Serilog;

namespace MockTestApi.Services
{
    public class SendGridEmailServiceHandler : EmailServiceHandler
    {
        private readonly EmailApiSettings _emailApiSettings;

        public SendGridEmailServiceHandler(IOptions<EmailApiSettings> emailApiSettings)
        {
            _emailApiSettings = emailApiSettings.Value;
        }

        public override async Task<bool> SendEmailAsync(EmailMessage emailMessage)
        {
            try
            {
                // Try to send the email using SendGrid
                await SendViaSendGridAsync(emailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to send email via SendGrid", ex);
                return false;
            }
        }

        private async Task SendViaSendGridAsync(EmailMessage emailMessage)
        {
            var apiKey =_emailApiSettings.ApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_emailApiSettings.Email, _emailApiSettings.DisplayName);
            var subject = emailMessage.Subject;
            var to = new EmailAddress(emailMessage.To);

            var plainTextContent = emailMessage.BodyPlainText;
            var htmlContent = emailMessage.Body;
            var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            if (!string.IsNullOrWhiteSpace(emailMessage.Cc))
                message.AddCc(emailMessage.Cc.Trim());

            if (!string.IsNullOrWhiteSpace(emailMessage.Bcc))
                message.AddBcc(emailMessage.Bcc.Trim());

            await AddAttachmentsAsync(message, emailMessage.Attachments);

            await client.SendEmailAsync(message);
        }

        private async Task AddAttachmentsAsync(SendGridMessage message, List<IFormFile>? attachments)
        {
            if (attachments == null) return;

            foreach (var file in attachments)
            {
                if (file.Length > 0)
                {
                    var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    await message.AddAttachmentAsync(file.Name, memoryStream, file.ContentType);
                }
            }
        }
    }

}
