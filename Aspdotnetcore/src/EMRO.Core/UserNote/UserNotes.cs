using Abp.Domain.Entities;
using EMRO.Appointment;
using EMRO.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.UserNote
{
    public class UserNotes : Entity<Guid>, IMayHaveTenant
    {
        [Column("UserNotesId")]
        [Key]
        public override Guid Id { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public int? TenantId { get; set; }

        [ForeignKey("UniqueUserId")]
        public virtual User Users { get; set; }
        public Guid? UserId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual DoctorAppointment DoctorAppointment { get; set; }
        public Guid? AppointmentId { get; set; }

        public bool IsSamvaad { get; set; }

    }
}
