using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IQuestionSetService
    {
        Task<IEnumerable<QuestionSet>> GetAllQuestionSetsAsync();
        Task<QuestionSet> GetQuestionSetByIdAsync(string id);
        Task CreateQuestionSetAsync(QuestionSet questionSet);
        Task<bool> UpdateQuestionSetAsync(QuestionSet questionSet);
        Task<bool> DeleteQuestionSetAsync(string id);
    }
}
