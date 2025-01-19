using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<Question>> GetAllAsync();
        Task<Question> GetByIdAsync(string id);
        Task CreateAsync(Question question);
        Task<bool> UpdateAsync(Question question);
        Task<bool> DeleteAsync(string id);
    }
}