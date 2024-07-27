using MockTestApi.Data.Interfaces;
using MockTestApi.Models;

namespace MockTestApi.Data
{
    public class TestRepository : ITestRepository
    {
        private readonly IRepository<Test> _repository;

        public TestRepository(IRepository<Test> repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Test>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<Test> GetByIdAsync(string id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task CreateAsync(Test test)
        {
            return _repository.CreateAsync(test);
        }

        public Task<bool> UpdateAsync(Test test)
        {
            return _repository.UpdateAsync(test);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return _repository.DeleteAsync(id);
        }
    }
}
