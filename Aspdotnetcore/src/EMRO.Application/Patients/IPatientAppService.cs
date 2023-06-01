using Abp.Application.Services;
using EMRO.Patients.Dto;
using System;
using System.Threading.Tasks;

namespace EMRO.Patients
{
    public interface IPatientAppService : IApplicationService
    {
        Task<PatientIntakeOutput> Create (CreatePatientInput input);
        Task<DoctorSearchOutput> Search(DoctorSearchInput input);
        Task<DoctorByIdOutput> GetById(Guid Id);
        Task<FamilyDoctorOutput> GetByName(FamilyDoctorInput input);
        GetDocumentListOutput GetDocumentList(GetDocumentListInput input);
        Task<GetIntakeDetailsOutputDto> GetDetails(GetIntakeInput input);

        Task<PatientIntakeOutput> Update(CreatePatientInput input);
    }
}
