using MockTestApi.Models;
namespace MockTestApi.Services.Interfaces
{
    public interface IMockTestService
    {
        Task<IEnumerable<MockTest>> GetAllMockTestsAsync();
        Task<MockTest> GetMockTestByIdAsync(string id);
        Task<IEnumerable<MockTest>> GetMockTestsByUserIdAsync(string userId);
        Task<MockTest> CreateAsync(MockTest mockTestDto);
        Task<bool> UpdateProgressAsync(string id, UpdateMockTestDto updateMockTestDto);
        Task<bool> CompleteTestAsync(string id, CompleteMockTestDto completeMockTestDto);
    }
}
