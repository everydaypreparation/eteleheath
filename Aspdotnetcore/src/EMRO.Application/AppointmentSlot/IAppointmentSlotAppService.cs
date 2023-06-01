using Abp.Application.Services;
using EMRO.AppointmentSlot.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.AppointmentSlot
{
    public interface IAppointmentSlotAppService : IApplicationService
    {
        
        Task<AppointmentSlotOutput> Create(List<CreateAppointmentSlotInput> input);
        Task<AppointmentSlotOutput> Update(UpdateAppointmentSlotInput input);
        Task<AppointmentSlotOutput> Delete(DeleteAppointmentSlotInput input);
        Task<GetDoctorAppointmentSlot> GetAppointmentSlotbyId(Guid Id);
        Task<DoctorAppointmentSlotOutput> GetAllAppointmentSlotbyDoctorId(DoctorAppointmentSlotInputDto doctorAppointmentSlotInputDto);
        Task<DoctorAppointmentSlotOutput> GetAllUnbookedAppointmentSlotbyDoctorId(DoctorAppointmentSlotInputDto doctorAppointmentSlotInputDto);
        Task<AppointmentSlotAvailableOutput> IsSlotAvailable(AvailableAppointmentSlotInput input);

    }
}
