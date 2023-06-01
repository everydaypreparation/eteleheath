using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using EMRO.Configuration.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EMRO.Configuration
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize]
    public class ConfigurationAppService : EMROAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
