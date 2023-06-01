using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Appointment
{
    public interface IDoctorAppointmentRepository : IRepository<DoctorAppointment, Guid>
    {
        List<DoctorAppointment> GetAllAppointments(Guid? appointmentId);
    }
   
}
