using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(string id);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> UpdateUserAsync(string id, UpdateUserDto updateUserDto);
        Task<LoginResponse> RegisterUserAsync(RegisterRequest registerDto);
    }
}
