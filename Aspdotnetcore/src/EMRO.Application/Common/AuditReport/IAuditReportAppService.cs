using Abp.Application.Services;
using EMRO.Common.AuditReport.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Common.AuditReport
{
    public interface IAuditReportAppService : IApplicationService
    {
        Task<AuditReportOutputDto> getAuditReport(AuditReportInputDto auditReportInputDto);
        Task<CreateAuditEventOutput> CreateAuditEvents(CreateEventsInputDto input);
    }
}
