using MockTestApi.Data.Interfaces;
using MockTestApi.Models;

namespace MockTestApi.Data
{
    public class MockTestHistoryRepository : IMockTestHistoryRepository
    {
        private readonly IRepository<MockTestHistory> _repository;

        public MockTestHistoryRepository(IRepository<MockTestHistory> repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<MockTestHistory>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<MockTestHistory> GetByIdAsync(string id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task CreateAsync(MockTestHistory mockTestHistory)
        {
            return _repository.CreateAsync(mockTestHistory);
        }

        public Task<bool> UpdateAsync(MockTestHistory mockTestHistory)
        {
            return _repository.UpdateAsync(mockTestHistory);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return _repository.DeleteAsync(id);
        }
    }
}
