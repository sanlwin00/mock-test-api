using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface ITestService
    {
        Task<IEnumerable<Test>> GetAllTestsAsync();
        Task<Test> GetTestByIdAsync(string id);
        Task CreateTestAsync(Test test);
        Task<bool> UpdateTestAsync(Test test);
        Task<bool> DeleteTestAsync(string id);
    }
}
