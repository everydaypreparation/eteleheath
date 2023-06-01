using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Consultant.Dto
{
    public class RequestForTestInputDto
    {
        public Guid PatientId { get; set; }
        public Guid? DiagnosticId { get; set; }
        public Guid ConsultantId { get; set; }
        public string ReportType { get; set; }
        public string ReportDetails { get; set; }
        public string DueDate { get; set; }
    }
}
