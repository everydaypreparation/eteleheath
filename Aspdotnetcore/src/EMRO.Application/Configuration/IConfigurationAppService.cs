using System.Threading.Tasks;
using EMRO.Configuration.Dto;

namespace EMRO.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
