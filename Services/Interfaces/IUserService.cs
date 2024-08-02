using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(string id);
        Task CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(string id);
        Task<LoginResponse> AuthenticateAsync(LoginRequest loginRequest);
        Task<LoginResponse> AuthenticateWithAccessCodeAsync(string accessCode);
        Task<LoginResponse> RegisterUserAsync(RegisterRequest registerDto);

    }
}
