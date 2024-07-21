using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    public class QuestionSetService : IQuestionSetService
    {
        private readonly IQuestionSetRepository _questionSetRepository;

        public QuestionSetService(IQuestionSetRepository questionSetRepository)
        {
            _questionSetRepository = questionSetRepository;
        }

        public async Task<IEnumerable<QuestionSet>> GetAllQuestionSetsAsync()
        {
            return await _questionSetRepository.GetAllAsync();
        }

        public async Task<QuestionSet> GetQuestionSetByIdAsync(string id)
        {
            return await _questionSetRepository.GetByIdAsync(id);
        }

        public async Task CreateQuestionSetAsync(QuestionSet questionSet)
        {
            await _questionSetRepository.CreateAsync(questionSet);
        }

        public async Task<bool> UpdateQuestionSetAsync(QuestionSet questionSet)
        {
            return await _questionSetRepository.UpdateAsync(questionSet);
        }

        public async Task<bool> DeleteQuestionSetAsync(string id)
        {
            return await _questionSetRepository.DeleteAsync(id);
        }
    }
}
