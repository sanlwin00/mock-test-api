using MockTestApi.Models;
namespace MockTestApi.Services.Interfaces
{
    public interface IMockTestService
    {
        Task<IEnumerable<MockTest>> GetAllMockTestsAsync();
        Task<MockTest> GetMockTestByIdAsync(string id);
        Task CreateMockTestAsync(MockTest mockTest);
        Task<bool> UpdateMockTestAsync(MockTest mockTest);
        Task<bool> DeleteMockTestAsync(string id);
    }
}
