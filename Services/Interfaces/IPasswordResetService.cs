namespace MockTestApi.Services.Interfaces
{
    public interface IPasswordResetService
    {
        Task<bool> RequestPasswordResetAsync(string email, string passwordResetUrl);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}