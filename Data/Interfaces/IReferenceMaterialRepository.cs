using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface IReferenceMaterialRepository
    {
        Task<IEnumerable<ReferenceMaterial>> GetAllAsync();
        Task<ReferenceMaterial> GetByIdAsync(string id);
    }
}
