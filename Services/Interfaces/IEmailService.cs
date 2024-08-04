namespace MockTestApi.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string ToAddresses, string Subject, string Body, string CCAddresses=null, string BCCAddresses=null, object AttachmentFilePath=null);
    }
}
