using MockTestApi.Models;

namespace MockTestApi.Services
{
    public interface IEmailServiceHandler
    {
        IEmailServiceHandler SetNext(IEmailServiceHandler handler);
        Task<bool> SendEmailAsync(EmailMessage emailMessage);
    }

    public abstract class EmailServiceHandler : IEmailServiceHandler
    {
        private IEmailServiceHandler _nextHandler;

        public IEmailServiceHandler SetNext(IEmailServiceHandler handler)
        {
            _nextHandler = handler;
            return handler;
        }

        public virtual async Task<bool> SendEmailAsync(EmailMessage emailMessage)
        {
            if (_nextHandler != null)
            {
                return await _nextHandler.SendEmailAsync(emailMessage);
            }

            return false;
        }
    }

}
