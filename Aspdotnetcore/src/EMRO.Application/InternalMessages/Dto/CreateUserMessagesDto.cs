using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.InternalMessages.Dto
{
    public class CreateUserMessagesDto
    {
        //[Required(ErrorMessage ="Please enter subject.")]
        public string Subject { get; set; }
        //[Required(ErrorMessage = "Please enter message.")]
        public string MessagesText { get; set; }
        [Required(ErrorMessage = "Please enter sender email.")]
        public string SenderUserIds { get; set; }

        [Required(ErrorMessage = "Please enter recevier email.")]
        public string ReceiverUserIds { get; set; }
       // public string ReadBy { get; set; }
        public Guid ParentId { get; set; }
        public Guid MessageId { get; set; }
        public Guid? AppointmentId { get; set; }
        //public string AttachmentName { get; set; }
        //public List<IFormFile> AttachmentFilePath { get; set; }
    }

    public class SamvaadMeetingUserDetails
    {
        public string emailId { get; set; }
        public string fullName { get; set; }
    }
}
