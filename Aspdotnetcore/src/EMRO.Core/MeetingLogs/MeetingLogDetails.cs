using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.MeetingLogs
{
    public class MeetingLogDetails : Entity<Guid>, IMayHaveTenant
    {
        [Column("MeetingLogId")]
        [Key]
        public override Guid Id { get; set; }
        public virtual int? TenantId { get; set; }
        public Guid UserId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid MeetingId { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
