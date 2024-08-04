using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using SharpCompress.Common;

namespace MockTestApi.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly IRepository<User> _repository;

        public UserRepository(IRepository<User> repository) 
        {
            _repository = repository;
        }

        public Task<IEnumerable<User>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<User> GetByIdAsync(string id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task CreateAsync(User user)
        {
            return _repository.CreateAsync(user);
        }

        public Task<bool> UpdateAsync(User user)
        {
            return _repository.UpdateAsync(user);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return _repository.DeleteAsync(id);
        }

    }
}
