using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Notifications;
using Abp.UI;
using EMRO.Authorization.Users;
using EMRO.Common.CustomNotification.Dtos;
using Microsoft.AspNetCore.Mvc;


namespace EMRO.Common.CustomNotification
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class CustomNotificationAppService : ApplicationService, ICustomNotificationAppService
    {
        private readonly IUserNotificationManager _userNotificationManager;
        private readonly UserManager _userManager;
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IRepository<User, long> _userRepository;

        public CustomNotificationAppService(IUserNotificationManager userNotificationManager,
            UserManager userManager,
            INotificationPublisher notificationPublisher,
            IRepository<User, long> userRepository)
        {
            _userNotificationManager = userNotificationManager;
            _userManager = userManager;
            _notificationPublisher = notificationPublisher;
            _userRepository = userRepository;
        }

        [HttpGet]
        [AbpAuthorize]
        public GetCustomNotificationOutputList GetNotifications(int limit, int page)
        {
            GetCustomNotificationOutputList output = new GetCustomNotificationOutputList();
            try
            {
                Logger.Info("Calling GetNotifications");
                //var userId = _userRepository.FirstOrDefault(x => x.UniqueUserId == Guid.Parse(_session.UniqueUserId)).Id;
                //UserIdentifier user = _userManager.GetUserById(Convert.ToInt64(userId)).ToUserIdentifier();
                UserIdentifier user = _userManager.GetUserById(Convert.ToInt64(AbpSession.UserId)).ToUserIdentifier();
                var notifications = _userNotificationManager.GetUserNotifications(user, UserNotificationState.Read);
                notifications.AddRange(_userNotificationManager.GetUserNotifications(user, UserNotificationState.Unread));
                notifications = notifications.Where(x => x.Notification.CreationTime > DateTime.UtcNow.AddDays(-7)).ToList();
                notifications = notifications.OrderByDescending(x => x.Notification.CreationTime).ToList();
                if (Convert.ToInt32(limit) > 0 && Convert.ToInt32(page) > 0)
                    notifications = notifications.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(limit)).Take(Convert.ToInt32(limit)).ToList();
                //return notifications;
                output.Items = notifications;
                output.Message = "Get notification list";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                Logger.Error("GetNotifications Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace);
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;
        }

        [AbpAuthorize]
        public async Task<SetCustomNotificationOutputDto> SetNotifications(Guid[] notficationId)
        {
            SetCustomNotificationOutputDto output = new SetCustomNotificationOutputDto();
            try
            {
                Logger.Info("Calling SetNotifications");
                foreach (var item in notficationId)
                {
                    await _userNotificationManager.UpdateUserNotificationStateAsync(AbpSession.TenantId, item, UserNotificationState.Read);
                }
                output.Message = "Notification read status set succesfully.";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                Logger.Error("SetNotifications Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace);
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;
        }

        [AbpAuthorize]
        public async Task<CustomNotificationOutputDto> DeleteNotifications(Guid notficationId)
        {
            CustomNotificationOutputDto output = new CustomNotificationOutputDto();
            try
            {
                Logger.Info("Calling DeleteNotifications");

                //await _userNotificationManager.UpdateUserNotificationStateAsync(AbpSession.TenantId, notficationId, UserNotificationState.Read);

                //Set notification to Delete State
                await _userNotificationManager.DeleteUserNotificationAsync(AbpSession.TenantId, notficationId);
                output.Id = notficationId;
                output.Message = "Notification deleted succesfully.";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                Logger.Error("DeleteNotifications Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace);
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;
        }

        [AbpAuthorize]
        public async Task SendNotifications(NotificationInputDto input)
        {
            try
            {
                Logger.Info("Calling SendNotifications");
                if (input.RoleName.Length > 0)
                {
                    foreach (var roles in input.RoleName)
                    {
                        var users = await _userManager.GetUsersInRoleAsync(roles.ToUpper());
                        foreach (var item in users)
                        {
                            var hostAdmin = new UserIdentifier(AbpSession.TenantId, item.Id);

                            _notificationPublisher.Publish(
                             input.Title,
                             new MessageNotificationData(input.Content),
                             severity: NotificationSeverity.Info,
                             userIds: new[] { hostAdmin }
                             );
                        }
                    }
                }
                else
                {
                    var list = await _userRepository.GetAllListAsync();
                    foreach (var item in list)
                    {
                        var hostAdmin = new UserIdentifier(AbpSession.TenantId, item.Id);

                        _notificationPublisher.Publish(
                         input.Title,
                         new MessageNotificationData(input.Content),
                         severity: NotificationSeverity.Info,
                         userIds: new[] { hostAdmin }
                         );
                    }


                }

            }
            catch (Exception ex)
            {
                Logger.Error("SendNotifications Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace);
                throw new UserFriendlyException("SendNotifications has encountered an error.");
            }
        }

        /// <summary>
        /// GetNotificationsCount
        /// </summary>
        /// <returns></returns>
        [AbpAuthorize]
        public async Task<CustomNotificationCoutntOutputDto> GetNotificationsCount()
        {
            CustomNotificationCoutntOutputDto output = new CustomNotificationCoutntOutputDto();
            try
            {
                //Logging
                Logger.Info("Calling GetNotificationsCount");

                //Fetch notification count for current user
                UserIdentifier user = _userManager.GetUserById(Convert.ToInt64(AbpSession.UserId)).ToUserIdentifier();
                var notifications = _userNotificationManager.GetUserNotifications(user, UserNotificationState.Unread);
                notifications = notifications.Where(x => x.Notification.CreationTime > DateTime.UtcNow.AddDays(-7)).ToList();
                output.Count = notifications.Count;
                //output.Count = await _userNotificationManager.GetUserNotificationCountAsync(user, UserNotificationState.Unread);
                output.Message = "Get Notification count succesfully.";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                Logger.Error("GetNotificationsCount Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace);
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;
        }

        /// <summary>
        /// Method to publish Notifications
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Title"></param>
        /// <param name="Message"></param>
        [ApiExplorerSettings(IgnoreApi = true)]
        public NotificationPublishOutputDto NotificationPublish(long Id, string Title,string Message)
        {
            NotificationPublishOutputDto output = new NotificationPublishOutputDto();
            try
            {
                var hostAdmin = new UserIdentifier(AbpSession.TenantId, Id);
                _notificationPublisher.Publish(
                    Title,
                    new MessageNotificationData(Message),
                    severity: NotificationSeverity.Info,
                    userIds: new[] { hostAdmin }
                );
                output.Id = Id;
                output.Message = "Notification inserted successfully";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                Logger.Error("Notification Publish Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace);
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;


        }

    }

}
