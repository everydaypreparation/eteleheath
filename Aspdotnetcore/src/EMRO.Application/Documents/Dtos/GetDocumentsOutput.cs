using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Documents.Dtos
{
    public class GetDocumentsOutput
    {
        public string Filedata { get; set; }
        public string MimeType { get; set; }
        public Guid DocumentId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string Category { get; set; }
        public string DocumentName { get; set; }
    }
}
