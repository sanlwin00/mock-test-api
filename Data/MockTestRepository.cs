using MockTestApi.Data.Interfaces;
using MockTestApi.Models;

namespace MockTestApi.Data
{
    public class MockTestRepository : IMockTestRepository
    {
        private readonly IRepository<MockTest> _repository;

        public MockTestRepository(IRepository<MockTest> repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<MockTest>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<MockTest> GetByIdAsync(string id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task CreateAsync(MockTest mockTest)
        {
            return _repository.CreateAsync(mockTest);
        }

        public Task<bool> UpdateAsync(MockTest mockTest)
        {
            return _repository.UpdateAsync(mockTest);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return _repository.DeleteAsync(id);
        }
    }
}
