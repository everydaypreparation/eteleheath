using Abp.Application.Services;
using EMRO.OncologyConsultReports.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.OncologyConsultReports
{
    public interface IConsultReportAppService : IApplicationService
    {
        Task<ConsultReportOutput> Create(ConsultReportInput input);
        Task<ConsultReportOutput> Completed(CompelteConsultReport input);
        Task<GetConsultOutput> Get(string ConsultId);
        Task<CasesOutput> ActiveCase(ActiveCaseInput input);
        Task<CasesOutput> ArchiveCase(ActiveCaseInput input);
        Task<ConsultReportOutput> Delete(string ConsultId);

        UserCasesOutput UserConsultantReport(UserActiveCaseInput input);

        Task<GetReportOutput> GetReport(string ConsultId);
    }
}
