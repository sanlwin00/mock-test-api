using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface ITestRepository
    {
        Task<IEnumerable<Test>> GetAllAsync();
        Task<Test> GetByIdAsync(string id);
    }
}
