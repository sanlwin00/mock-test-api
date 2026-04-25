using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<Question>> GetAllAsync();
        Task<IEnumerable<Question>> GetByIdsAsync(IEnumerable<string> ids);
    }
}