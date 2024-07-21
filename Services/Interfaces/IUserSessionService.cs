using MockTestApi.Models;
namespace MockTestApi.Services.Interfaces
{
    public interface IUserSessionService
    {
        Task<IEnumerable<UserSession>> GetAllUserSessionsAsync();
        Task<UserSession> GetUserSessionByIdAsync(string id);
        Task CreateUserSessionAsync(UserSession userSession);
        Task<bool> UpdateUserSessionAsync(UserSession userSession);
        Task<bool> DeleteUserSessionAsync(string id);
    }
}
