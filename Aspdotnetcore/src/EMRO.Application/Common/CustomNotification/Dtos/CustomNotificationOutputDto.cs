using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.CustomNotification.Dtos
{
    public class CustomNotificationOutputDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class SetCustomNotificationOutputDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class CustomNotificationCoutntOutputDto
    {
        public int Count { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
    public class NotificationPublishOutputDto
    {
        public long Id { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
