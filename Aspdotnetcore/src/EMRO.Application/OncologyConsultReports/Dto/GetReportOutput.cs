using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.OncologyConsultReports.Dto
{
    public class GetReportOutput
    {
        public string Report { get; set; }
        public string Mimetype { get; set; }
        public string ReoportName { get; set; }

        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
