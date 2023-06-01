using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.Common.AdminNotification.Dtos
{
    public class AdminNotificationInputDto
    {
        public string[] RoleName { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string Title { get; set; }
    }

    public class UpdateAdminNotificationInputDto
    {
        public Guid Id { get; set; }
        public string[] RoleName { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string Title { get; set; }
    }
}
