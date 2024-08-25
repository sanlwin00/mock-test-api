using MockTestApi.Data.Interfaces;
using MockTestApi.Models;

namespace MockTestApi.Data
{
    public class ReferenceMaterialRepository: IReferenceMaterialRepository
    {
        private readonly IRepository<ReferenceMaterial> _repository;

        public ReferenceMaterialRepository(IRepository<ReferenceMaterial> repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<ReferenceMaterial>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<ReferenceMaterial> GetByIdAsync(string id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task CreateAsync(ReferenceMaterial ReferenceMaterial)
        {
            return _repository.CreateAsync(ReferenceMaterial);
        }

        public Task<bool> UpdateAsync(ReferenceMaterial ReferenceMaterial)
        {
            return _repository.UpdateAsync(ReferenceMaterial);
        }

        public Task<bool> DeleteAsync(string id)
        {
            return _repository.DeleteAsync(id);
        }
    }
}
