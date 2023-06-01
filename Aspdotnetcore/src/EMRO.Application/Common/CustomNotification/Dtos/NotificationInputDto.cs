using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.Common.CustomNotification.Dtos
{
    public class NotificationInputDto
    {
        public string[] RoleName { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string Title { get; set; }
    }
}
