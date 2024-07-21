using MockTestApi.Data.Interfaces;
using MockTestApi.Models;

namespace MockTestApi.Data
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly IRepository<Question> _repository;

        public QuestionRepository(IRepository<Question> repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Question>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<Question> GetByIdAsync(string id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task CreateAsync(Question question)
        {
            return _repository.CreateAsync(question);
        }

        public Task<bool> UpdateAsync(Question question)
        {
            return _repository.UpdateAsync(question);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return _repository.DeleteAsync(id);
        }
    }
}
