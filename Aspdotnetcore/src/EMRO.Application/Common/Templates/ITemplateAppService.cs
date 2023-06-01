using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.Templates
{
    public interface ITemplateAppService : IApplicationService
    {
        string GetTemplates(string Name, string message, string signature);
    }
}
