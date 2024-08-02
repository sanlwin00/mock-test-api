using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IUserStore
    {
        Task<User> GetByAccessCodeAsync(string accessCode);
        Task<User> GetByUsernameAsync(string username);
    }
}
