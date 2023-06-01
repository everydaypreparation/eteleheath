using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.InternalMessages.Dto
{
    public class UploadAttachmentDto
    {
        public Guid AttachmentId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class UploadAttachmentInputDto
    {
        //public string AttachmentName { get; set; }
        public string FileSize { get; set; }
        public IFormFile AttachmentFile { get; set; }
        public Guid MessageId { get; set; }
    }

}
