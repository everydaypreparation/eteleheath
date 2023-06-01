using Abp.Application.Services;
using EMRO.CMSContents.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.CMSContents
{
    public interface ICMSContentAppService: IApplicationService
    {
        void CreateCMSContent(CreateCMSContentInput input);

        void UpdateCMSContent(UpdateCMSContentInput input);

        void DeleteCMSContent(DeleteCMSContentInput input);
        GetCMSContentOutput GetCMSContents();
        GetCMSContentOutput GetCMSContentsByKey(KeyInput input);

    }
}
