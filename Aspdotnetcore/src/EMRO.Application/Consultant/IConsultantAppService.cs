using Abp.Application.Services;
using EMRO.Consultant.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Consultant
{
    public interface IConsultantAppService : IApplicationService
    {
        Task<RequestOutputDto> Create(RequestInputDto input);
        Task<RequestOutputDto> Delete(Guid RequestId);
        Task<GetRequestOutputDto> GetAll(GetAllRequestInputDto getAllRequestInputDto);
        Task<GetRequestByIdDto> Get(Guid Id);

        Task<RequestOutputDto> Update(RequestUpdateInputDto input);
        Task<RequestOutputDto> RequestForTest(RequestForTestInputDto input);

    }
}
