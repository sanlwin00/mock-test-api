using Microsoft.Extensions.Options;
using MockTestApi.Models;
using Newtonsoft.Json;
using Serilog;
using System.Text;

namespace MockTestApi.Services
{
    public class BrevoEmailServiceHandler : EmailServiceHandler
    {
        private readonly HttpClient _httpClient;
        private readonly BrevoApiSettings _emailApiSettings;

        public BrevoEmailServiceHandler(IOptions<BrevoApiSettings> emailApiSettings, HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _emailApiSettings = emailApiSettings.Value ?? throw new ArgumentNullException( nameof(emailApiSettings));
        }

        public override async Task<bool> SendEmailAsync(EmailMessage emailMessage)
        {
            try
            {
                await SendViaBrevoAsync(emailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send email via Brevo");
                return false;
            }
        }

        private async Task SendViaBrevoAsync(EmailMessage emailMessage)
        {
            var payload = new
            {
                sender = new
                {
                    name = _emailApiSettings.DisplayName, 
                    email = _emailApiSettings.Email     
                },
                to = new[]
                {
                    new
                    {
                        email = emailMessage.To,
                        name = emailMessage.ToName
                    }
                },
                cc = string.IsNullOrWhiteSpace(emailMessage.Cc)
                    ? null
                    : BuildRecipientArray(emailMessage.Cc),
                bcc = string.IsNullOrWhiteSpace(emailMessage.Bcc)
                    ? null
                    : BuildRecipientArray(emailMessage.Bcc),
                subject = emailMessage.Subject,
                htmlContent = emailMessage.Body,
                textContent = emailMessage.BodyPlainText 
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);

            var response = await _httpClient.PostAsync("smtp/email", new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to send email via Brevo. StatusCode: {response.StatusCode}, Response: {responseContent}");
            }
        }

        /// <summary>
        /// Builds a recipient array for CC and BCC fields.
        /// </summary>
        /// <param name="recipientsString">A semi-colon separated string of email addresses.</param>
        /// <returns>An array of email/name objects for Brevo API.</returns>
        private object[] BuildRecipientArray(string recipientsString)
        {
            return recipientsString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(email => new
                {
                    email = email.Trim(),
                    name = email.Trim(),
                })
                .ToArray();
        }
    }
}