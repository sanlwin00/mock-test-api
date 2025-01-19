using AutoMapper;
using Microsoft.Extensions.Options;
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;
using MockTestApi.Utils;
using Serilog;

namespace MockTestApi.Services
{
    public class PasswordResetService: IPasswordResetService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetRepository;
        private readonly INotificationService _emailService;
        public PasswordResetService(IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            INotificationService emailService,
            IUserService userService
            )
        {
            _userRepository = userRepository;
            _passwordResetRepository = passwordResetTokenRepository;
            _emailService = emailService;
        }

        public async Task RequestPasswordResetAsync(string email, string passwordResetUrl)
        {
            var user = await _userRepository.GetByUsernameAsync(email);
            if (user == null)
                throw new ArgumentException("User not found!");

            var resetToken = await GeneratePasswordResetTokenAsync(user.Id);
            var resetLink = $"{passwordResetUrl}?token={resetToken.Id}";

            try
            {
                await _emailService.SendPasswordResetEmailAsync(email, user.DisplayName, resetLink);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send email to {email}: {ex}", email, ex);
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = await _passwordResetRepository.GetByIdAsync(token);
            if (resetToken == null || resetToken.ExpiryDate < DateTime.UtcNow)
                return false;

            var user = await _userRepository.GetByIdAsync(resetToken.UserId);
            if (user == null)
                return false;

            SetUserPassword(user, newPassword);
            await _userRepository.UpdateAsync(user);
            await _passwordResetRepository.DeleteAsync(token);

            return true;
        }

        private void SetUserPassword(User user, string newPassword)
        {
            var salt = MyUtility.GetSalt();
            user.PasswordSalt = salt;
            user.PasswordHash = MyUtility.GetHash(newPassword + salt);
        }

        private async Task<PasswordResetToken> GeneratePasswordResetTokenAsync(string userId)
        {
            var token = new PasswordResetToken
            {
                Id = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.UtcNow.AddHours(24),
                UserId = userId
            };
            await _passwordResetRepository.CreateAsync(token);
            return token;
        }

    }
}
