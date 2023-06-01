using Abp.Application.Services;
using EMRO.Appointment.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Appointment
{
    public interface IAppointmentAppService : IApplicationService
    {
        //Task<long> CreateAppointment(CreateAppointmentInput input);
        //Task<string> UpdateAppointment(UpdateAppointmentInput input);

        // Added on 01/01/2021 by Rishiraj start
        Task<CreateBookAppointmentOutput> Create(CreateBookAppointmentInput input);
        GetBookAppointmentOutput Get(GetBookAppointmentInput input);
        Task<BookAppointmentOutput> Reschedule(UpdateBookAppointmentInput input);
        Task<BookAppointmentOutput> Delete(DeleteBookedAppointmentInput input);
        // Added on 01/01/2021 by Rishiraj end
        Task<UpcomingBookAppointmentOutput> GetBookedAppointment(UpcomingBookAppointmentInput input);
        Task<UpcomingBookAppointmentOutput> GetMissedAppointments(UpcomingBookAppointmentInput input);
        Task<UpcomingPatientAppointmentOutput> GetUserAppointment(Guid Id);
        Task<BookAppointmentOutput> Cancel(CancelBookedAppointmentInput input);
        Task<GetAppoinmentDetailsOutput> GetAppoinmentDetailsAsync(GetBookAppointmentInput input);
        Task<GetAppoinmentDetailsOutput> GetInsuranceAppointmentDetails(GetBookAppointmentInput input);
        //Task<GetPatientAppoinmentDetailsOutput> GetPatientAppointmentDetails(GetBookAppointmentInput input);
        Task<GetPatientAppoinmentDetailsOutput> GetPatientAppointmentDetails(Guid UserId);
        Task<DashBoardOutbutDto> GetConsultantStats(Guid UserId);
        Task<LegalDashBoardOutbutDto> GetLegalStats(Guid UserId);

        Task<CreateBookAppointmentOutput> Update(UpdateBookInput input);

        Task<UpcomingPatientAppointmentOutput> GetAllAppointment(Guid Id, Guid AppointmentId);

        Task<DashBoardOutbutDto> GetFamilyDoctorStats(Guid UserId);
    }
}
