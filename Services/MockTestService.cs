using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    public class MockTestService : IMockTestService
    {
        private readonly IMockTestRepository _mockTestRepository;

        public MockTestService(IMockTestRepository mockTestRepository)
        {
            _mockTestRepository = mockTestRepository;
        }

        public async Task<IEnumerable<MockTest>> GetAllMockTestsAsync()
        {
            return await _mockTestRepository.GetAllAsync();
        }

        public async Task<MockTest> GetMockTestByIdAsync(string id)
        {
            return await _mockTestRepository.GetByIdAsync(id);
        }

        public async Task CreateMockTestAsync(MockTest mockTest)
        {
            await _mockTestRepository.CreateAsync(mockTest);
        }

        public async Task<bool> UpdateMockTestAsync(MockTest mockTest)
        {
            return await _mockTestRepository.UpdateAsync(mockTest);
        }

        public async Task<bool> DeleteMockTestAsync(string id)
        {
            return await _mockTestRepository.DeleteAsync(id);
        }
    }
}
