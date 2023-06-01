using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.AdminNotification.Dtos
{
    public class GetNotificationList
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public string RoleName { get; set; }
        public DateTime? CreationTime { get; set; }
    }
    public class GetNotificationOutputList
    {
        public List<GetNotificationList> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class GetNotificationById
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public string[] RoleName { get; set; }
        public DateTime? CreationTime { get; set; }

        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
