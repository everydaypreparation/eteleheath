using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.AuditReport
{
    public class AuditEvent : Entity<Guid>, IMayHaveTenant
    {
        [Column("AuditEventId")]
        [Key]
        public override Guid Id { get; set; }
        public  DateTime ExecutionTime { get; set; }
        public  long? ImpersonatorUserId { get; set; }
        public  string BrowserInfo { get; set; }
        public  string ClientName { get; set; }
        public  string ClientIpAddress { get; set; }
        public  long ExecutionDuration { get; set; }
        public  int? TenantId { get; set; }
        public  long? UserId { get; set; }
        public  int? ImpersonatorTenantId { get; set; }
        public  string Parameters { get; set; }
        public Guid UniqueUserId { get; set; }
        public string Operation { get; set; }
        public string Component { get; set; }
        public string Action { get; set; }
        public string BeforeValue { get; set; }
        public string AfterValue { get; set; }
        public bool IsImpersonating { get; set; }
    }
}
