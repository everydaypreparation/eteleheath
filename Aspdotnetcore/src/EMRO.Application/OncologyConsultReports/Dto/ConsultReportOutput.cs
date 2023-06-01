using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.OncologyConsultReports.Dto
{
    public class ConsultReportOutput
    {
        public Guid ConsultId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
