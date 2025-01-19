using Microsoft.Extensions.Options;
using MockTestApi.Models;
using SendGrid.Helpers.Mail;
using SendGrid;
using Serilog;
using System.Net.Mail;

namespace MockTestApi.Services
{
    public class SendGridEmailServiceHandler : EmailServiceHandler
    {
        private readonly SendGridApiSettings _emailApiSettings;

        public SendGridEmailServiceHandler(IOptions<SendGridApiSettings> emailApiSettings)
        {
            _emailApiSettings = emailApiSettings.Value;
        }

        public override async Task SendEmailAsync(EmailMessage emailMessage)
        {
            await SendViaSendGridAsync(emailMessage);           
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

            emailMessage.Attachments?.ForEach(file => message.AddAttachment(file.FileName, file.Base64Content));
            
            await client.SendEmailAsync(message);
        }
    }
}
