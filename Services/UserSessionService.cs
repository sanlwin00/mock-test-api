using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    public class UserSessionService : IUserSessionService
    {
        private readonly IUserSessionRepository _userSessionRepository;

        public UserSessionService(IUserSessionRepository userSessionRepository)
        {
            _userSessionRepository = userSessionRepository;
        }

        public async Task<IEnumerable<UserSession>> GetAllUserSessionsAsync()
        {
            return await _userSessionRepository.GetAllAsync();
        }

        public async Task<UserSession> GetUserSessionByIdAsync(string id)
        {
            return await _userSessionRepository.GetByIdAsync(id);
        }

        public async Task CreateUserSessionAsync(UserSession userSession)
        {
            await _userSessionRepository.CreateAsync(userSession);
        }

        public async Task<bool> UpdateUserSessionAsync(UserSession userSession)
        {
            return await _userSessionRepository.UpdateAsync(userSession);
        }

        public async Task<bool> DeleteUserSessionAsync(string id)
        {
            return await _userSessionRepository.DeleteAsync(id);
        }
    }
}
