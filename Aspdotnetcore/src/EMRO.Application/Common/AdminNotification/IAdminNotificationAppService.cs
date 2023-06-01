using Abp.Application.Services;
using EMRO.Common.AdminNotification.Dtos;
using EMRO.Master;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Common.AdminNotification
{
    public interface IAdminNotificationAppService : IApplicationService
    {
        Task<GetNotificationOutputList> GetNotifications(int limit, int page);

        Task<NotificationOutputDto> UpdateNotifications(UpdateAdminNotificationInputDto input);

        Task<NotificationOutputDto> DeleteNotifications(Guid notificationId);
        Task<GetNotificationById> GetNotificationsById(Guid notificationId);
        Task<NotificationOutputDto> SendNotifications(AdminNotificationInputDto input);

        Task<GetNotificationOutputList> GetRolesNotifications(string RoleName,int limit, int page);
    }
}
