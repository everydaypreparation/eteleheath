using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Diagnostics.Dtos
{
    public class DiagnosticDashboardOutputDto
    {
        public int TotalCase { get; set; }
        public int ReportDue { get; set; }
        public int TotalPatient { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
