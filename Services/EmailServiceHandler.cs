using MockTestApi.Models;

namespace MockTestApi.Services
{
    public interface IEmailServiceHandler
    {
        IEmailServiceHandler SetNext(IEmailServiceHandler handler);
        Task SendEmailAsync(EmailMessage emailMessage);
    }

    public abstract class EmailServiceHandler : IEmailServiceHandler
    {
        private IEmailServiceHandler _nextHandler;

        public IEmailServiceHandler SetNext(IEmailServiceHandler handler)
        {
            _nextHandler = handler;
            return handler;
        }

        public virtual async Task SendEmailAsync(EmailMessage emailMessage)
        {
            if (_nextHandler != null)
            {
                await _nextHandler.SendEmailAsync(emailMessage);
            }
        }
    }

}
