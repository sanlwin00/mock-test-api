using MockTestApi.Data.Interfaces;
using MockTestApi.Models;

namespace MockTestApi.Data
{
    public class QuestionSetRepository : IQuestionSetRepository
    {
        private readonly IRepository<QuestionSet> _repository;

        public QuestionSetRepository(IRepository<QuestionSet> repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<QuestionSet>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<QuestionSet> GetByIdAsync(string id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task CreateAsync(QuestionSet questionSet)
        {
            return _repository.CreateAsync(questionSet);
        }

        public Task<bool> UpdateAsync(QuestionSet questionSet)
        {
            return _repository.UpdateAsync(questionSet);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return _repository.DeleteAsync(id);
        }
    }
}
