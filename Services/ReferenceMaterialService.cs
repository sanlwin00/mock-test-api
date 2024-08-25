
using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    public class ReferenceMaterialService : IReferenceMaterialService
    {
        private readonly IReferenceMaterialRepository _ReferenceMaterialRepository;

        public ReferenceMaterialService(IReferenceMaterialRepository ReferenceMaterialRepository)
        {
            _ReferenceMaterialRepository = ReferenceMaterialRepository;
        }

        public async Task<IEnumerable<ReferenceMaterial>> GetAllReferenceMaterialsAsync()
        {
            return await _ReferenceMaterialRepository.GetAllAsync();
        }

        public async Task<ReferenceMaterial> GetReferenceMaterialByIdAsync(string id)
        {
            return await _ReferenceMaterialRepository.GetByIdAsync(id);
        }

        public async Task CreateReferenceMaterialAsync(ReferenceMaterial ReferenceMaterial)
        {
            await _ReferenceMaterialRepository.CreateAsync(ReferenceMaterial);
        }

        public async Task<bool> UpdateReferenceMaterialAsync(ReferenceMaterial ReferenceMaterial)
        {
            return await _ReferenceMaterialRepository.UpdateAsync(ReferenceMaterial);
        }

        public async Task<bool> DeleteReferenceMaterialAsync(string id)
        {
            return await _ReferenceMaterialRepository.DeleteAsync(id);
        }
    }
}
