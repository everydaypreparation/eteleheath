using Abp.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.CustomNotification.Dtos
{
    public class GetCustomNotificationOutputList
    {
        public List<UserNotification> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
