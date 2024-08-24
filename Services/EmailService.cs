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
        private readonly MailTemplateSettings _templateSettings;
        public EmailService(IOptions<SmtpSetting> smtpSettings, IOptions<MailTemplateSettings> templateSettings)
        {
            _smtpSetting = smtpSettings.Value;
            _templateSettings = templateSettings.Value;
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

        public async Task SendThankYouEmailAsync(string toEmail, string name, string validUntil)
        {
            var template = await LoadTemplateAsync(_templateSettings.ThankYouTemplate);
            var emailBody = template
                .Replace("{{Name}}", name)
                .Replace("{{ValidUntil}}", validUntil);

            await SendEmailAsync(toEmail, $"[{_templateSettings.AppName}] Welcome!", emailBody);
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

            await SendEmailAsync(toEmail, $"[{_templateSettings.AppName}] Message received", emailBody, attachments, _templateSettings.CcEmail);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var template = await LoadTemplateAsync(_templateSettings.PasswordResetTemplate);
            var emailBody = template.Replace("{{reset_link}}", resetLink);

            await SendEmailAsync(toEmail, $"[{_templateSettings.AppName}] Password Reset", emailBody);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string name)
        {
            var template = await LoadTemplateAsync(_templateSettings.SignUpTemplate);
            var emailBody = template.Replace("{{Name}}", name);

            await SendEmailAsync(toEmail, $"[{_templateSettings.AppName}] Sign up successful", emailBody);
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
                        var memoryStream = new MemoryStream();
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

            try
            {
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw ex;
            }
            finally
            {
                // Dispose of message and any attachments
                foreach (var attachment in message.Attachments)
                {
                    attachment.Dispose();
                }
                message.Dispose();
            }
        }

    }
}
