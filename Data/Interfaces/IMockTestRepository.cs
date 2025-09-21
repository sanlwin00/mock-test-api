using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IMockTestRepository
    {
        Task<IEnumerable<MockTest>> GetAllAsync();
        Task<MockTest> GetByIdAsync(string id);
        Task CreateAsync(MockTest mockTest);
        Task<bool> UpdateAsync(MockTest mockTest);
        Task<bool> DeleteAsync(string id);
        Task<bool> UpdateProgressAsync(string id, string questionId, string answer, int? selectedOption);
        Task<bool> CompleteTestAsync(string id, MockTestResults completeMockTestDto);
    }
}
