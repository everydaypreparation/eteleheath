using Abp;
using Abp.Extensions;
using Abp.Notifications;
using Abp.Timing;
using EMRO.Controllers;
using EMRO.Email;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EMRO.Web.Host.Controllers
{
    public class HomeController : EMROControllerBase
    {
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IMailer _mailer;
        private readonly INotificationStore _notificationStore;

        public HomeController(INotificationPublisher notificationPublisher, IMailer mailer, INotificationStore notificationStore)
        {
            _notificationPublisher = notificationPublisher;
            _mailer = mailer;
            _notificationStore = notificationStore;
        }

        public IActionResult Index()
        {
            return Redirect("/swagger");
        }

        /// <summary>
        /// This is a demo code to demonstrate sending notification to default tenant admin and host admin uers.
        /// Don't use this code in production !!!
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<ActionResult> TestNotification(string message = "")
        {
            //await _mailer.SendEmailAsync("test@gmail.com", "Weather Report", "Detailed Weather Report");

            //_notificationStore.InsertSubscription(new NotificationSubscriptionInfo(new System.Guid(), 1, 1, "Test Notification", null));

            if (message.IsNullOrEmpty())
            {
                message = "This is a test notification, created at " + Clock.Now;
            }

            var defaultTenantAdmin = new UserIdentifier(1, 2);
            var hostAdmin = new UserIdentifier(null, 1);

            await _notificationPublisher.PublishAsync(
                "App.SimpleMessage",
                new MessageNotificationData(message),
                severity: NotificationSeverity.Info,
                userIds: new[] { defaultTenantAdmin, hostAdmin }
            );

            return Content("Sent notification: " + message);
        }
    }
}
