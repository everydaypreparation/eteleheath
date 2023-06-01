using Abp.Domain.Entities;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.Appointment
{
    public class DoctorAppointment : Entity<Guid>, IMayHaveTenant
    {
        [Column("AppointmentId")]
        [Key]
        public override Guid Id { get; set; }
        public virtual int? TenantId { get; set; }
        public string Title { get; set; }
        public string Agenda { get; set; }
        public Guid DoctorId { get; set; }
        public Guid? AppointmentSlotId { get; set; }
        public Guid UserId { get; set; }

        //public long AppointmentFrom { get; set; }
        //public long AppointmentWith { get; set; }
        //public DateTime StartTime { get; set; }
        //public DateTime EndTime { get; set; }
        //public DateTime AppointmentDate { get; set; }
        //public string AdditionalAttendee { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public string Type { get; set; }
        public string Referral { get; set; }
        public int Flag { get; set; }
        public string Status { get; set; }
        public int IsBooked { get; set; }
        public Guid Appointment { get; set; }
        public string meetingId { get; set; }
        public string Reason { get; set; }
        public int NoOfPages { get; set; }
        [DefaultValue(true)]
        public bool MissedAppointment { get; set; }
    }
}
