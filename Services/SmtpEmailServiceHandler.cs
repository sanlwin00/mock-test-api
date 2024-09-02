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
        public override async Task<bool> SendEmailAsync(EmailMessage emailMessage)
        {
            try
            {
                // Try to send the email using SMTP
                await SendViaSmtpAsync(emailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to send email via SMTP: {ex}", ex);
                // If SMTP fails, pass the request to the next handler (e.g: SendGrid)
                return await base.SendEmailAsync(emailMessage);
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

            await AddAttachmentsAsync(message, emailMessage.Attachments);

            using var smtpClient = new SmtpClient(_smtpSetting.SmtpHost)
            {
                Port = _smtpSetting.SmtpPort,
                Credentials = new System.Net.NetworkCredential(_smtpSetting.Username, _smtpSetting.Password),
                EnableSsl = _smtpSetting.EnableSsl,
                UseDefaultCredentials = false,
            };
            
            await smtpClient.SendMailAsync(message);
            
        }


        private async Task AddAttachmentsAsync(MailMessage message, List<IFormFile>? attachments)
        {
            if (attachments == null) return;

            foreach (var file in attachments)
            {
                if (file.Length > 0)
                {
                    var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    var attachment = new Attachment(memoryStream, file.FileName, file.ContentType);
                    message.Attachments.Add(attachment);
                }
            }
        }

    }

}
