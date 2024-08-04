using MockTestApi.Data.Interfaces;
using MockTestApi.Models;

namespace MockTestApi.Data
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly IRepository<PasswordResetToken> _repository;

        public PasswordResetTokenRepository(IRepository<PasswordResetToken> repository)
        {
            _repository = repository;
        }

        public Task<PasswordResetToken> GetByIdAsync(string token)
        {
            return _repository.GetByIdAsync(token);
        }

        public Task CreateAsync(PasswordResetToken PasswordResetToken)
        {
            return _repository.CreateAsync(PasswordResetToken);
        }

        public Task<bool> DeleteAsync(string token)
        {
            return _repository.DeleteAsync(token);
        }
    }
}
