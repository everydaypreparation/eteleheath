using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.InternalMessages.Dto
{
    public class GetMessageDto
    {
        public Guid MessageId { get; set; }
        public string Subject { get; set; }
        public string MessagesText { get; set; }
        public string SenderUserIds { get; set; }
        public string ReceiverUserIds { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string MailDateTime { get; set; }
        public Guid? AppointmentId { get; set; }
    }

    public class AttachmentDto
    {
        public Guid AttachmentId { get; set; }
        public string AttachmentName { get; set; }
    }

    public class GetMessageListDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid MessageId { get; set; }
        public string Subject { get; set; }
        public string MessagesText { get; set; }
        public string SenderUserIds { get; set; }
        public string ReceiverUserIds { get; set; }
        public string MailDateTime { get; set; }
        public Guid? AppointmentId { get; set; }
        public string EmailId { get; set; }
        public string ReadBy { get; set; }
        public List<AttachmentDto> Attachments { get; set; }
    }
    public class GetMessageListOutput
    {
        public List<GetMessageListDto> Items { get; set; }
        public int Count { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class GetMessageInput 
    {
        public Guid UserId { get; set; }
        public int limit { get; set; }
        public int page { get; set; }

    }
}
