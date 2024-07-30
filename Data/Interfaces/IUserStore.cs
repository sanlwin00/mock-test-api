using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IUserStore
    {
        Task<User> GetByUsernameAsync(string username);
    }
}
