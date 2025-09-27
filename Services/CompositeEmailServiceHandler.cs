using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    /// <summary>
    /// A composite email service handler that allows different providers for transactional and general emails
    /// </summary>
    public class CompositeEmailServiceHandler : IEmailServiceHandler
    {
        private readonly IEmailServiceHandler _transactionalServiceHandler;
        private readonly IEmailServiceHandler _generalServiceHandler;

        public CompositeEmailServiceHandler(
            IEmailServiceHandler transactionalServiceHandler,
            IEmailServiceHandler generalServiceHandler)
        {
            _transactionalServiceHandler = transactionalServiceHandler;
            _generalServiceHandler = generalServiceHandler;
        }

        public async Task SendEmailAsync(EmailMessage emailMessage)
      {
            // Choose the appropriate service based on the email type
            var serviceHandler = emailMessage.IsTransactional ? _transactionalServiceHandler  : _generalServiceHandler;

            await serviceHandler.SendEmailAsync(emailMessage);
        }

        public IEmailServiceHandler SetNext(IEmailServiceHandler handler)
        {
            throw new NotImplementedException();
        }
    }
}