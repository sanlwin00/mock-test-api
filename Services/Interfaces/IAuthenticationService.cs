using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> AuthenticateAsync(LoginRequest loginRequest);
        Task<LoginResponse> AuthenticateWithAccessCodeAsync(string accessCode);

    }
}