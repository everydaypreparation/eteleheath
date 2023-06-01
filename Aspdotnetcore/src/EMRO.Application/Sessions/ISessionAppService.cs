using System.Threading.Tasks;
using Abp.Application.Services;
using EMRO.Sessions.Dto;

namespace EMRO.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
