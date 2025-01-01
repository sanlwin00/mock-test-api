namespace MockTestApi.Services.Interfaces
{
    public interface IPasswordResetService
    {
        Task RequestPasswordResetAsync(string email, string passwordResetUrl);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}