using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(string id);
        Task<User> GetByAccessCodeAsync(string accessCode);
        Task<User> GetByUsernameAsync(string username);
        Task CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
    }
}
