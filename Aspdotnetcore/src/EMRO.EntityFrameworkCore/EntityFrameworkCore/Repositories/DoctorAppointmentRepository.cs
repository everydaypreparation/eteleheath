using Abp.EntityFrameworkCore;
using EMRO.Appointment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMRO.EntityFrameworkCore.Repositories
{
    public class DoctorAppointmentRepository: EMRORepositoryBase<DoctorAppointment, Guid>, IDoctorAppointmentRepository
    {
        public DoctorAppointmentRepository(IDbContextProvider<EMRODbContext> dbContextProvider)
           : base(dbContextProvider)
        {
        }

        public List<DoctorAppointment> GetAllAppointments(Guid? appointmentId)
        {
            var query = GetAll();

            if (appointmentId.HasValue)
            {
                query = query.Where(appointment => appointment.Id == appointmentId.Value);
            }

            return query
                .OrderByDescending(appointment => appointment.CreatedOn).ToList();
        }
    }
}
