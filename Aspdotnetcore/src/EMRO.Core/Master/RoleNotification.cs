using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Master
{
    public class RoleNotification : Entity<Guid>, IMayHaveTenant,ISoftDelete
    {
        public int? TenantId { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public string RoleName { get; set; }
        public DateTime? CreationTime { get; set; }
        public bool IsDeleted { get ; set; }

        public DateTime? DeletionTime { get; set; }
    }
}
