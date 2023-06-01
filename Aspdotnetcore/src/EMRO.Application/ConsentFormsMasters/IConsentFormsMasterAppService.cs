using Abp.Application.Services;
using EMRO.ConsentFormsMasters.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.ConsentFormsMasters
{
    public interface IConsentFormsMasterAppService : IApplicationService
    {
        Task<CreateConsentFormOutput> Create(CreateConsentFormInput input);
        GetConsentFormOutput GetById(Guid Id);

        GetConsentFormOutputList GetAll();
        Task<CreateConsentFormOutput> Update(UpdateConsentFormInput input);
        Task<CreateConsentFormOutput> Delete(Guid Id);
    }
}
