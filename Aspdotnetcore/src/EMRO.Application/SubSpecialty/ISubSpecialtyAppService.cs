using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EMRO.SubSpecialty.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.SubSpecialty
{
    public interface ISubSpecialtyAppService : IApplicationService
    {
        Task<SubSpecialtyOutput> Create(CreateInputDto input);
        Task<SubSpecialtyOutput> Update(UpdateInputDto input);

        Task<GetListOutput> GetSubSpecialties();
        Task<GetSubSpecialtyOutputById> GetSubSpecialtybyId(Guid Id);
        Task<SubSpecialtyOutput> Delete(Guid Id);
    }
}
