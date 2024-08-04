using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken> GetByIdAsync(string token);

        Task CreateAsync(PasswordResetToken PasswordResetToken);

        Task<bool> DeleteAsync(string token);

    }
}
