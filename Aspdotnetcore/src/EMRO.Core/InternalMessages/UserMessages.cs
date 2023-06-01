using Abp.Domain.Entities;
using EMRO.Appointment;
using EMRO.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.InternalMessages
{
    public class UserMessages : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Column("UserMessagesId")]
        [Key]
        public override Guid Id { get; set; }

        public string Subject { get; set; }
        public string MessagesText { get; set; }
        public string SenderUserIds { get; set; }
        public string ReceiverUserIds { get; set; }
        public string ReadBy { get; set; }
        public bool IsDraft { get; set; }

        public string DeletedbyReceiver { get; set; }
        public string DeletedFromTrash { get; set; }
        public string DeletedbySender { get; set; }


        [ForeignKey("UniqueUserId")]
        public virtual User Users { get; set; }
        [ForeignKey("AppointmentId")]
        public virtual DoctorAppointment DoctorAppointments { get; set; }

        public Guid? UserId { get; set; }
        public Guid ParentId { get; set; }
        public Guid? AppointmentId { get; set; }

        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
