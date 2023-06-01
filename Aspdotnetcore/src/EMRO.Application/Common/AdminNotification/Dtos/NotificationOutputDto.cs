using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.AdminNotification.Dtos
{
    public class NotificationOutputDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
