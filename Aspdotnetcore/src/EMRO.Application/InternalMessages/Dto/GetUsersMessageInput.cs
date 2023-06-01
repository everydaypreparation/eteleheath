using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.InternalMessages.Dto
{
    public class GetUsersMessageInput
    {
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public Guid? AppointmentId { get; set; }
        public int limit { get; set; }
        public int page { get; set; }

    }
}
