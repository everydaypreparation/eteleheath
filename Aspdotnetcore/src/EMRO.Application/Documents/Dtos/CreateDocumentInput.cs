using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Documents.Dtos
{
    public class CreateDocumentInput
    {
        public Guid DocumentId { get; set; }
        public Guid UserId { get; set; }
        public string Category { get; set; }
        public Guid AppointmentId { get; set; }
        public List<IFormFile> ReportDocumentPaths { get; set; }


    }

    public class DocumentOutput
    {
        public Guid DocumentId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }


    }
}
