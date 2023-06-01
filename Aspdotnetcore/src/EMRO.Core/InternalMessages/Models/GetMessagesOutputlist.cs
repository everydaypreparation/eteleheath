using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.InternalMessages.Models
{
    //public class GetMessagesOutputlist
    //{
    //    public List<GetMessageListDtoResult> Items { get; set; }
    //    public string Message { get; set; }
    //    public int StatusCode { get; set; }
    //}
    //public class AttachmentDtoOutput
    //{
    //    public long AttachmentId { get; set; }
    //    public string AttachmentName { get; set; }
    //}
    public class GetMessageListDtoResult
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid MessageId { get; set; }
        public string Subject { get; set; }
        public string MessagesText { get; set; }
        public string SenderUserIds { get; set; }
        public string ReceiverUserIds { get; set; }
        public string EmailId { get; set; }
        public string ReadBy { get; set; }
        public Guid? AppointmentId { get; set; }
        public string MailDateTime { get; set; }
    }
}
