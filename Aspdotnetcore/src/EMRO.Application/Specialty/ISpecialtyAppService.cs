using Abp.Application.Services;
using EMRO.Specialty.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Specialty
{
    public interface ISpecialtyAppService : IApplicationService
    {
        Task<CreateSpecialtyOutput> Create(SpecialtyInputDto input);
        Task<CreateSpecialtyOutput> Update(UpdateSpecialtyInput input);
        Task<GetSpecialtyOutputDto> GetSpecialties();
        Task<GetOutputById> GetSpecialtybyId(Guid Id);
        Task<CreateSpecialtyOutput> Delete(Guid Id);
    }
}
