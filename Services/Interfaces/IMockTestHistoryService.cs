using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IMockTestHistoryService
    {
        Task<IEnumerable<MockTestHistory>> GetAllMockTestHistoriesAsync();
        Task<MockTestHistory> GetMockTestHistoryByIdAsync(string id);
        Task CreateMockTestHistoryAsync(MockTestHistory mockTestHistory);
        Task<bool> UpdateMockTestHistoryAsync(MockTestHistory mockTestHistory);
        Task<bool> DeleteMockTestHistoryAsync(string id);
    }
}
