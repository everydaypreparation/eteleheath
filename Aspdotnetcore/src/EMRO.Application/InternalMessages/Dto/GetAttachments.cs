using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.InternalMessages.Dto
{
    public class GetAttachments
    {
        public string Filedate { get; set; }
        public string MimeType { get; set; }
        public Guid AttachmentId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
