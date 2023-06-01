using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.AuditReport.Dtos
{
    public class CreateEventsInputDto
    {
        public long ExecutionDuration { get; set; }
        public string Parameters { get; set; }
        public string Operation { get; set; }
        public string Component { get; set; }
        public string Action { get; set; }
        public string BeforeValue { get; set; }
        public string AfterValue { get; set; }
    }
}
