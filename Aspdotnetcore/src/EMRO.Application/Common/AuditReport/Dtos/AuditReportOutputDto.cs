using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.AuditReport.Dtos
{
    public class AuditReportOutputDto
    {
        public List<AuditReportDto> Items { get; set; }
        public int Count { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
    public class AuditReportDto
    {
        public DateTime ExecutionTime { get; set; }
        public long? ImpersonatorUserId { get; set; }
        public long ExecutionDuration { get; set; }
        public long? UserId { get; set; }
        public Guid UniqueUserId { get; set; }
        public string Operation { get; set; }
        public string Component { get; set; }
        public string Action { get; set; }
        public bool IsImpersonating { get; set; }
        public string UserName { get; set; }
        public string ImpersonatorUserName { get; set; }
    }
}