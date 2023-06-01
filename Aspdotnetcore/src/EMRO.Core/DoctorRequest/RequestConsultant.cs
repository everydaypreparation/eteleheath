using Abp.Domain.Entities;
using EMRO.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.DoctorRequest
{
    public class RequestConsultant : Entity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public int? TenantId { get; set; }
        public bool IsDeleted { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string FirstName { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string LastName { get; set; }
        //public string Specialty { get; set; }
        public string Hospital { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string PhoneNumber { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string EmailAddress { get; set; }

        [ForeignKey("UniqueUserId")]
        public virtual User Users { get; set; }
        public Guid UserId { get; set; }
        // public string AvailabilityDate { get; set; }
        //public string AvailabilityStartTime { get; set; }
        //public string AvailabilityEndTime { get; set; }
        //public string TimeZone { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        //public DateTime SlotZoneTime { get; set; }
        //public DateTime SlotZoneEndTime { get; set; }
        public bool IsCompleted { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; }

    }
}
