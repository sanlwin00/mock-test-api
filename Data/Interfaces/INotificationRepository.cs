using MockTestApi.Models;

namespace MockTestApi.Data.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> GetByIdAsync(string id);
        Task CreateAsync(Notification notification);
        Task<bool> UpdateAsync(Notification notification);
    }
}
