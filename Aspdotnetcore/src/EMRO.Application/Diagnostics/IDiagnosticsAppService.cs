using Abp.Application.Services;
using EMRO.Diagnostics.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Diagnostics
{
    public interface IDiagnosticsAppService : IApplicationService
    {
        Task<PatientSerchOutputDto> Search(PatientSerchInputDto input);
        Task<GetCaseListOutputDto> ActivePatient(CaseInputDto input);
        Task<GetCaseListOutputDto> ArchivePatient(CaseInputDto input);
        Task<PatientSerchOutputDto> Update(UpdateCaseInputDto input);
        Task<DiagnosticDashboardOutputDto> Dashboard(CaseInputDto input);
        Task<PatientSerchOutputDto> Delete(UpdateCaseInputDto input);
    }
}
