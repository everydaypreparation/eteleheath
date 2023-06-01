using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using EMRO.Authorization.Users;
using EMRO.Common.AdminNotification.Dtos;
using EMRO.Master;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Common.AdminNotification
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class AdminNotificationAppService : ApplicationService, IAdminNotificationAppService
    {
        private readonly UserManager _userManager;
        private readonly IRepository<RoleNotification, Guid> _notificationPublisher;
        private readonly IRepository<User, long> _userRepository;

        public AdminNotificationAppService(
            UserManager userManager,
            IRepository<RoleNotification, Guid> notificationPublisher,
            IRepository<User, long> userRepository)
        {
            _userManager = userManager;
            _notificationPublisher = notificationPublisher;
            _userRepository = userRepository;
        }

        [AbpAuthorize]
        public async Task<NotificationOutputDto> DeleteNotifications(Guid notificationId)
        {
            NotificationOutputDto output = new NotificationOutputDto();
            try
            {
                Logger.Info("Calling DeleteNotifications");

                await _notificationPublisher.DeleteAsync(notificationId);
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

        [HttpGet]
        [AbpAuthorize]
        public async Task<GetNotificationOutputList> GetNotifications(int limit, int page)
        {
            GetNotificationOutputList output = new GetNotificationOutputList();
            try
            {
                Logger.Info("Calling GetNotifications");
                var user = _userManager.GetUserById(Convert.ToInt64(AbpSession.UserId)).ToUserIdentifier();
                var notifications = await _notificationPublisher.GetAllListAsync();
                output.Items = (from noti in notifications.AsEnumerable()
                                select new GetNotificationList
                                {
                                    Title = noti.Title,
                                    Content = noti.Content,
                                    RoleName = noti.RoleName,
                                    CreationTime = noti.CreationTime,
                                    Id = noti.Id
                                }).OrderByDescending(x => x.CreationTime).ToList();
                if (Convert.ToInt32(limit) > 0 && Convert.ToInt32(page) > 0)
                    output.Items = output.Items.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(limit)).Take(Convert.ToInt32(limit)).ToList();

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

        public async Task<GetNotificationById> GetNotificationsById(Guid notificationId)
        {
            GetNotificationById output = new GetNotificationById();
            try
            {
                Logger.Info("Calling DeleteNotifications");
                if (notificationId !=Guid.Empty)
                {
                    var result = await _notificationPublisher.GetAsync(notificationId);
                    if (result != null)
                    {
                        output.Title = result.Title;
                        output.Content = result.Content;
                        output.Id = result.Id;
                        output.RoleName = result.RoleName.Split(",");
                        output.CreationTime = result.CreationTime;
                        output.Message = "Get Notification details succesfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 401;
                    }
                    
                }
                else
                {
                    output.Message = "Bad Request.";
                    output.StatusCode = 401;
                }
                
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
        public async Task<GetNotificationOutputList> GetRolesNotifications(string RoleName, int limit, int page)
        {
            GetNotificationOutputList output = new GetNotificationOutputList();
            try
            {
                Logger.Info("Calling GetNotifications");
                var notifications = await _notificationPublisher.GetAllListAsync(x => x.RoleName.ToUpper().Contains(RoleName.ToUpper()));
                output.Items = (from noti in notifications.AsEnumerable()
                                select new GetNotificationList
                                {
                                    Title = noti.Title,
                                    Content = noti.Content,
                                    RoleName = noti.RoleName,
                                    CreationTime = noti.CreationTime,
                                    Id = noti.Id
                                }).OrderByDescending(x => x.CreationTime).ToList();
                if (Convert.ToInt32(limit) > 0 && Convert.ToInt32(page) > 0)
                    output.Items = output.Items.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(limit)).Take(Convert.ToInt32(limit)).ToList();

                output.Message = "Get notification list";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                Logger.Error("GetRolesNotifications Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace);
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;
        }

        [AbpAuthorize]
        public async Task<NotificationOutputDto> SendNotifications(AdminNotificationInputDto input)
        {
            NotificationOutputDto output = new NotificationOutputDto();
            try
            {
                Logger.Info("Calling SendNotifications");
                if (input.RoleName.Length > 0)
                {
                    string roles = String.Join(",", input.RoleName);
                    var request = new RoleNotification
                    {
                        Title = input.Title,
                        Content = input.Content,
                        CreationTime = DateTime.UtcNow,
                        RoleName = roles
                    };

                    var newNotificationId = await _notificationPublisher.InsertAndGetIdAsync(request);
                    output.Id = newNotificationId;
                    output.Message = "Notification sent succesfully.";
                    output.StatusCode = 200;
                }

            }
            catch (Exception ex)
            {
                Logger.Error("SendNotifications Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace);
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;
        }

        [AbpAuthorize]
        public async Task<NotificationOutputDto> UpdateNotifications(UpdateAdminNotificationInputDto input)
        {
            NotificationOutputDto output = new NotificationOutputDto();
            try
            {
                Logger.Info("Calling SendNotifications");
                var notification = await _notificationPublisher.GetAsync(input.Id);
                if (notification != null)
                {
                    if (input.RoleName.Length > 0)
                    {
                        string roles = String.Join(",", input.RoleName);

                        notification.Title = input.Title;
                        notification.Content = input.Content;
                        notification.CreationTime = DateTime.UtcNow;
                        notification.RoleName = roles;
                        await _notificationPublisher.UpdateAsync(notification);
                        output.Message = "Notification updated succesfully.";
                        output.StatusCode = 200;
                    }

                }
                else
                {
                    output.Message = "No record found.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                Logger.Error("UpdateNotifications Error. Message:" + ex.Message + " StackTrace: " + ex.StackTrace);
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
            }
            return output;
        }
    }
}
