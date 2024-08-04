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

        public EmailService(IOptions<SmtpSetting> smtpOptions)
        {
            _smtpSetting = smtpOptions.Value;
        }
        public async Task SendEmailAsync(string ToAddresses, string Subject, string Body, string CCAddresses = null, string BCCAddresses = null, object AttachmentFilePath = null)
        {
            System.Net.Mail.MailMessage mEmail = new System.Net.Mail.MailMessage(_smtpSetting.Email, ToAddresses);
            mEmail.IsBodyHtml = true;
            mEmail.Subject = Subject;
            mEmail.Body = Body;

            if (AttachmentFilePath != null)
                mEmail.Attachments.Add(new Attachment(AttachmentFilePath.ToString()));
            if (CCAddresses != null)
                mEmail.CC.Add(CCAddresses);
            if (BCCAddresses != null)
                mEmail.Bcc.Add(BCCAddresses.Trim());

            mEmail.Sender = new MailAddress(_smtpSetting.Email, _smtpSetting.DisplayName);

            SmtpClient smtpClient = new SmtpClient(_smtpSetting.SmtpHost);
            smtpClient.Port = Convert.ToInt32(_smtpSetting.SmtpPort);
            smtpClient.EnableSsl = Convert.ToBoolean(_smtpSetting.EnableSsl);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new System.Net.NetworkCredential(_smtpSetting.Email, _smtpSetting.Password);

            smtpClient.Send(mEmail);

            mEmail.Dispose();
        }
    }
}
