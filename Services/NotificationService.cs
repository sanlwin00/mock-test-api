using MockTestApi.Data.Interfaces;
using MockTestApi.Models;
using MockTestApi.Services.Interfaces;

namespace MockTestApi.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<IEnumerable<Notification>> GetAllNotificationsAsync()
        {
            return await _notificationRepository.GetAllAsync();
        }

        public async Task<Notification> GetNotificationByIdAsync(string id)
        {
            return await _notificationRepository.GetByIdAsync(id);
        }

        public async Task CreateNotificationAsync(Notification notification)
        {
            await _notificationRepository.CreateAsync(notification);
        }

        public async Task<bool> UpdateNotificationAsync(Notification notification)
        {
            return await _notificationRepository.UpdateAsync(notification);
        }

        public async Task<bool> DeleteNotificationAsync(string id)
        {
            return await _notificationRepository.DeleteAsync(id);
        }
    }
}
