using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Master
{
    public class CronHistory : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public Guid AppointmentId { get; set; }
        public int ScheduleTime { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
