using Microsoft.Extensions.Options;
using MockTestApi.Models;
using Serilog;
using System.Net.Mail;

namespace MockTestApi.Services
{
    public class SmtpEmailServiceHandler : EmailServiceHandler
    {
        private readonly SmtpSettings _smtpSetting;

        public SmtpEmailServiceHandler(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSetting = smtpSettings.Value;
        }
        public override async Task SendEmailAsync(EmailMessage emailMessage)
        {
            try
            {
                // Try to send the email using SMTP
                await SendViaSmtpAsync(emailMessage);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to send email to { emailMessage.To } via SMTP ({ _smtpSetting.SmtpHost }) - { ex.Message }");
                // If SMTP fails, pass the request to the next handler
                await base.SendEmailAsync(emailMessage);
            }
        }

        private async Task SendViaSmtpAsync(EmailMessage emailMessage)
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_smtpSetting.Email),
                Subject = emailMessage.Subject,
                Body = emailMessage.Body,
                IsBodyHtml = true,
                Sender = new MailAddress(_smtpSetting.Email, _smtpSetting.DisplayName)
            };

            message.To.Add(emailMessage.To);

            if (!string.IsNullOrWhiteSpace(emailMessage.Cc))
                message.CC.Add(emailMessage.Cc.Trim());

            if (!string.IsNullOrWhiteSpace(emailMessage.Bcc))
                message.Bcc.Add(emailMessage.Bcc.Trim());

            AddAttachments(message, emailMessage.Attachments);

            using var smtpClient = new SmtpClient(_smtpSetting.SmtpHost)
            {
                Port = _smtpSetting.SmtpPort,
                Credentials = new System.Net.NetworkCredential(_smtpSetting.Username, _smtpSetting.Password),
                EnableSsl = _smtpSetting.EnableSsl,
                UseDefaultCredentials = false,
            };
            
            await smtpClient.SendMailAsync(message);
            
        }
        private void AddAttachments(System.Net.Mail.MailMessage message, List<FileAttachment>? attachments)
        {
            if (attachments == null) return;

            foreach (var file in attachments)
            {
                if (!string.IsNullOrEmpty(file.Base64Content))
                {
                    byte[] fileBytes = Convert.FromBase64String(file.Base64Content);

                    var memoryStream = new MemoryStream(fileBytes);

                    var attachment = new Attachment(memoryStream, file.FileName, file.ContentType);

                    message.Attachments.Add(attachment);
                }
            }
        }

    }

}
