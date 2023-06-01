using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class DashBoardOutbutDto
    {
        public long NewPatient { get; set; }
        public long ReportDue { get; set; }
        public long TotalPatient { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class LegalDashBoardOutbutDto
    {
        public long NewCases { get; set; }
        public long ReportDue { get; set; }
        public long TotalCases { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
