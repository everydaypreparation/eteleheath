using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.InternalMessages.Dto
{
   public class OutputUserMessageDto
    {
        public Guid MessageId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
