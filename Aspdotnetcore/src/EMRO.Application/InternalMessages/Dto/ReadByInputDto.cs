using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.InternalMessages.Dto
{
    public class ReadByInputDto
    {
        public Guid UserId { get; set; }
        public Guid MessageId { get; set; }
    }

    public class RestoreInputDto
    {
        public Guid UserId { get; set; }
        public string MessageId { get; set; }
    }
}
