using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.AuditReport.Dtos
{
    public class AuditReportInputDto
    {
        public long? userId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int limit { get; set; }
        public int page { get; set; }
        public string SearchKeyword { get; set; }
    }
}
