using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IReferenceMaterialService
    {
        Task<IEnumerable<ReferenceMaterial>> GetAllReferenceMaterialsAsync();
        Task<ReferenceMaterial> GetReferenceMaterialByIdAsync(string id);
        Task CreateReferenceMaterialAsync(ReferenceMaterial ReferenceMaterial);
        Task<bool> UpdateReferenceMaterialAsync(ReferenceMaterial ReferenceMaterial);
        Task<bool> DeleteReferenceMaterialAsync(string id);
    }
}