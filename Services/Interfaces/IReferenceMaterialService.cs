using MockTestApi.Models;

namespace MockTestApi.Services.Interfaces
{
    public interface IReferenceMaterialService
    {
        Task<IEnumerable<ReferenceMaterial>> GetAllReferenceMaterialsAsync();
        Task<ReferenceMaterial> GetReferenceMaterialByIdAsync(string id);       
    }
}