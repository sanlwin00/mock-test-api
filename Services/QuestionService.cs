using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionService(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public async Task<IEnumerable<Question>> GetAllQuestionsAsync()
        {
            return await _questionRepository.GetAllAsync();
        }

        public async Task<Question> GetQuestionByIdAsync(string id)
        {
            return await _questionRepository.GetByIdAsync(id);
        }

        public async Task CreateQuestionAsync(Question question)
        {
            await _questionRepository.CreateAsync(question);
        }

        public async Task<bool> UpdateQuestionAsync(Question question)
        {
            return await _questionRepository.UpdateAsync(question);
        }

        public async Task<bool> DeleteQuestionAsync(string id)
        {
            return await _questionRepository.DeleteAsync(id);
        }
    }
}
