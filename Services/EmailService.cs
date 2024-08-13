using Microsoft.Extensions.Options;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using System.Net.Mail;
using System.Net.Mime;

namespace MockTestApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSetting _smtpSetting;
        private readonly string _templatePath;
        public EmailService(string templatePath, IOptions<SmtpSetting> smtpOptions)
        {
            _smtpSetting = smtpOptions.Value;
            _templatePath = templatePath;
        }

        private async Task<string> LoadTemplateAsync(string templateFileName)
        {
            var templatePath = Path.Combine(_templatePath, templateFileName);
            return await File.ReadAllTextAsync(templatePath);
        }

        public async Task SendThankYouEmailAsync(string toEmail, string name, string validUntil)
        {
            var template = await LoadTemplateAsync("template-thankyou.html");
            var emailBody = template
                .Replace("{{Name}}", name)
                .Replace("{{ValidUntil}}", validUntil);

            await SendEmailAsync(toEmail, "[ccacademy.ca] Welcome!", emailBody);
        }

        public async Task SendContactFormEmailAsync(string toEmail, string firstName, string lastName, string phone, string message, List<IFormFile>? attachments = null)
        {
            var template = await LoadTemplateAsync("template-contact.html");
            var emailBody = template
                .Replace("{{First}}", firstName)
                .Replace("{{Last}}", lastName)
                .Replace("{{Phone}}", phone)
                .Replace("{{Email}}", toEmail)
                .Replace("{{Message}}", message)
                .Replace("{{AttachmentCount}}", attachments?.Count().ToString() + " file(s)");

            await SendEmailAsync(toEmail, "[ccacademy.ca] Contact Form Submission", emailBody, attachments);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var template = await LoadTemplateAsync("template-forgot.html");
            var emailBody = template.Replace("{{reset_link}}", resetLink);

            await SendEmailAsync(toEmail, "[ccacademy.ca] Password Reset", emailBody);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string name)
        {
            var template = await LoadTemplateAsync("template-welcome.html");
            var emailBody = template.Replace("{{Name}}", name);

            await SendEmailAsync(toEmail, "[ccacademy.ca] Registration successful", emailBody);
        }
        public async Task SendEmailAsync(string to, string subject, string body, List<IFormFile>? attachments = null, string cc = null, string bcc = null)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_smtpSetting.Email),                
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            message.Sender = new MailAddress(_smtpSetting.Email, _smtpSetting.DisplayName);
            message.To.Add(to);
            if (cc != null) message.CC.Add(cc.Trim());
            if (bcc != null) message.Bcc.Add(bcc.Trim());
            if (attachments != null)
            {
                foreach (var file in attachments)
                {
                    if (file.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        var attachment = new Attachment(memoryStream, file.FileName, file.ContentType);
                        message.Attachments.Add(attachment);
                    }
                }
            }
            using var smtpClient = new SmtpClient(_smtpSetting.SmtpHost) // Replace with your SMTP server
            {
                Port = _smtpSetting.SmtpPort, 
                Credentials = new System.Net.NetworkCredential(_smtpSetting.Email, _smtpSetting.Password),
                EnableSsl = _smtpSetting.EnableSsl,
                UseDefaultCredentials = false,
            };

            await smtpClient.SendMailAsync(message);

            message.Dispose();
        }

    }
}
