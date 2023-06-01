using Abp.Runtime.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.Appointment.Dto
{
    public class UpdateBookAppointmentInput : ICustomValidate
    {
        //public long? Id { get; set; }
        //public string Title { get; set; }
        //public string Agenda { get; set; }
        ////public long AppointmentFrom { get; set; }
        //public long AppointmentWith { get; set; }
        //public DateTime StartTime { get; set; }
        //public DateTime EndTime { get; set; }
        //public DateTime AppointmentDate { get; set; }
        //public string[] AdditionalAttendee { get; set; }
        //public string Type { get; set; }
        //public string Referral { get; set; }
        //public int Flag { get; set; }
        //public string Status { get; set; }

        public Guid? Id { get; set; }
        public string Title { get; set; }
        public string Agenda { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid SlotId { get; set; }
        public string Type { get; set; }
        public string Referral { get; set; }
        //public int Flag { get; set; }
        public string Status { get; set; }
        public Guid UserId { get; set; }
        public string meetingId { get; set; }

        public void AddValidationErrors(CustomValidationContext context)
        {
            if (Id == null)
            {
                context.Results.Add(new ValidationResult(" Appointment Id can not be null in order to update a booked Appointment!", new[] { "Id" }));
            }
        }
    }
}
