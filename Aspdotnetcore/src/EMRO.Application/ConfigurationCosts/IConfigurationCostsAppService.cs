using Abp.Application.Services;
using EMRO.ConfigurationCosts.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.ConfigurationCosts
{
   public interface IConfigurationCostsAppService : IApplicationService
    {
        Task<CreateCostsOutputDto> Create(CreateCostsInputDto input);
        Task<CreateCostsOutputDto> Update(UpdateCostsInputDto input);
        Task<GetCostsOutputDto> Get(Guid CostId);
        Task<GetListCostsOutputDto> GetAll();
        Task<CreateCostsOutputDto> Delete(Guid CostId);
    }
}
