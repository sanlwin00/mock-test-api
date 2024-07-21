using MockTestApi.Data.Interfaces;
using MockTestApi.Models;

namespace MockTestApi.Data
{
    public class UserSessionRepository : IUserSessionRepository
    {
        private readonly IRepository<UserSession> _repository;

        public UserSessionRepository(IRepository<UserSession> repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<UserSession>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<UserSession> GetByIdAsync(string id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task CreateAsync(UserSession userSession)
        {
            return _repository.CreateAsync(userSession);
        }

        public Task<bool> UpdateAsync(UserSession userSession)
        {
            return _repository.UpdateAsync(userSession);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return _repository.DeleteAsync(id);
        }
    }
}
