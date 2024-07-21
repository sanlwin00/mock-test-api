using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    public class MockTestHistoryService : IMockTestHistoryService
    {
        private readonly IMockTestHistoryRepository _mockTestHistoryRepository;

        public MockTestHistoryService(IMockTestHistoryRepository mockTestHistoryRepository)
        {
            _mockTestHistoryRepository = mockTestHistoryRepository;
        }

        public async Task<IEnumerable<MockTestHistory>> GetAllMockTestHistoriesAsync()
        {
            return await _mockTestHistoryRepository.GetAllAsync();
        }

        public async Task<MockTestHistory> GetMockTestHistoryByIdAsync(string id)
        {
            return await _mockTestHistoryRepository.GetByIdAsync(id);
        }

        public async Task CreateMockTestHistoryAsync(MockTestHistory mockTestHistory)
        {
            await _mockTestHistoryRepository.CreateAsync(mockTestHistory);
        }

        public async Task<bool> UpdateMockTestHistoryAsync(MockTestHistory mockTestHistory)
        {
            return await _mockTestHistoryRepository.UpdateAsync(mockTestHistory);
        }

        public async Task<bool> DeleteMockTestHistoryAsync(string id)
        {
            return await _mockTestHistoryRepository.DeleteAsync(id);
        }
    }
}
