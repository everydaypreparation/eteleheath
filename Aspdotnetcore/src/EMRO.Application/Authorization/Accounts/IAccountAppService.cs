using System.Threading.Tasks;
using Abp.Application.Services;
using EMRO.Authorization.Accounts.Dto;

namespace EMRO.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
