using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.InternalMessages.Dto
{
   public class DraftMessageInput
    {
        public string Subject { get; set; }
        public string MessagesText { get; set; }
        public string SenderUserIds { get; set; }

        public string ReceiverUserIds { get; set; }
        // public string ReadBy { get; set; }
        public Guid ParentId { get; set; }
        public Guid? AppointmentId { get; set; }
    }
}
