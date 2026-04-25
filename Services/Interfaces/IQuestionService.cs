using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync(string? locale = null);
        Task<IEnumerable<QuestionDto>> GetQuestionsByTestIdAsync(string testId, string? locale = null);
    }
}
