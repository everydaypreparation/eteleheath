using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.DiagnosticsRequestTest
{
    public class RequestTest : Entity<Guid>, IMayHaveTenant
    {
        [Column("RequestTestId")]
        [Key]
        public override Guid Id { get; set; }
        public int? TenantId { get; set; }
        public Guid PatientId { get; set; }
        public Guid? DiagnosticId { get; set; }
        public Guid ConsultantId { get; set; }
        public string ReportType { get; set; }
        public string ReportDetails { get; set; }
        public string DueDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
