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
    }
}
