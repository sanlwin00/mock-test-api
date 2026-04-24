using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IMockTestRepository
    {
        Task<IEnumerable<MockTest>> GetAllAsync();
        Task<MockTest> GetByIdAsync(string id);
        Task<IEnumerable<MockTest>> GetByUserIdAsync(string userId);
        Task CreateAsync(MockTest mockTest);
        Task<bool> UpdateAsync(MockTest mockTest);
        Task<bool> DeleteAsync(string id);
        Task<bool> UpdateProgressAsync(string id, string questionId, string answer, int? selectedOption, bool reviewLater);
        Task<bool> CompleteTestAsync(string id, MockTestResults mockTestResults);
    }
}
