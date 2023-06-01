using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.AuditReport.Dtos
{
    public class CreateAuditEventOutput
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
