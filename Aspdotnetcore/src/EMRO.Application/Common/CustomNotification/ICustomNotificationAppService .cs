using Abp.Notifications;
using EMRO.Common.CustomNotification.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Common.CustomNotification
{
    public interface ICustomNotificationAppService
    {
        //List<UserNotification> GetNotifications(int limit, int page);
        GetCustomNotificationOutputList GetNotifications(int limit, int page);

        //Task SetNotifications(Guid[] notficationId);
        Task<SetCustomNotificationOutputDto> SetNotifications(Guid[] notficationId);

        //Task DeleteNotifications(Guid notficationId);
        Task<CustomNotificationOutputDto> DeleteNotifications(Guid notficationId);
        Task SendNotifications(NotificationInputDto input);
        Task<CustomNotificationCoutntOutputDto> GetNotificationsCount();
        //void NotificationPublish(long Id, string Title, string Message);
        NotificationPublishOutputDto NotificationPublish(long Id, string Title, string Message);

    }
}
