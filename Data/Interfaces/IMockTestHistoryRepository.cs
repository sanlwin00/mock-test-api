using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IMockTestHistoryRepository
    {
        Task<IEnumerable<MockTestHistory>> GetAllAsync();
        Task<MockTestHistory> GetByIdAsync(string id);
        Task CreateAsync(MockTestHistory mockTestHistory);
        Task<bool> UpdateAsync(MockTestHistory mockTestHistory);
        Task<bool> DeleteAsync(string id);
    }
}
