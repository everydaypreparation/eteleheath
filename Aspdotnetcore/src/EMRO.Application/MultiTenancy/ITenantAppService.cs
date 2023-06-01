using Abp.Application.Services;
using EMRO.MultiTenancy.Dto;

namespace EMRO.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

