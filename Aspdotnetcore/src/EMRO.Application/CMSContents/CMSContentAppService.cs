using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EMRO.CMSContents.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMRO.CMSContents
{

    [AbpAuthorize]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class CMSContentAppService : ApplicationService, ICMSContentAppService
    {
        private readonly IRepository<CMSContent, Guid> _cmsContentRepository;

        /// <summary>
        ///In constructor, we can get needed classes/interfaces.
        ///They are sent here by dependency injection system automatically.
        /// </summary>
        public CMSContentAppService(IRepository<CMSContent, Guid> cmsContenttRepository)
        {
            _cmsContentRepository = cmsContenttRepository;
        }

        //Create CMS Content
        public void CreateCMSContent(CreateCMSContentInput input)
        {
            //throw new NotImplementedException();
            //We can use Logger, it's defined in ApplicationService class.
            Logger.Info("Creating a CMS Content for input: " + input);

            //Creating a new Task entity with given input's properties

            //var content = ObjectMapper.Map<CMSContent>(input);

            var content = new CMSContent
            {
                //PageName = input.PageName,
                Key = input.Key,
                //HtmlPage = input.HtmlPage,
                Content = input.Content,
                Title = input.Title,
                SubTitle = input.SubTitle,
                //Short_Desc = input.Short_Desc,
                //Description = input.Description,
                Keywords = input.Keywords,
                MediaImages = input.MediaImages,
                TenantId = AbpSession.TenantId,
                IsActive = input.IsActive
            };

            //Saving entity with standard Insert method of repositories.
            _cmsContentRepository.Insert(content);
        }

        //Update CMS Content
        public void UpdateCMSContent(UpdateCMSContentInput input)
        {
            //We can use Logger, it's defined in ApplicationService base class.
            Logger.Info("Updating a CMS Content for input: " + input);

            //Retrieving a CMSContent entity with given id using standard Get method of repositories.
            var content = _cmsContentRepository.Get((Guid)input.CMSContentId);

            //content.PageName = input.PageName;
            //content.HtmlPage = input.HtmlPage;
            content.Key = input.Key;
            content.Content = input.Content;
            content.Title = input.Title;
            content.SubTitle = input.SubTitle;
            //content.Short_Desc = input.Short_Desc;
            //content.Description = input.Description;
            content.Keywords = input.Keywords;
            content.MediaImages = input.MediaImages;
            content.TenantId = AbpSession.TenantId;
            content.IsActive = input.IsActive;

        }

        public void DeleteCMSContent(DeleteCMSContentInput input)
        {
            //We can use Logger, it's defined in ApplicationService class.
            Logger.Info("Delete CMS Content for input: " + input);

            //Retrieving a task entity with given id using standard Get method of repositories.
            var cmsContent = _cmsContentRepository.Get(input.CMSContentId);

            //Deleteing a task entity sing standard Get method of repositories.
            _cmsContentRepository.Delete(cmsContent);

        }

        public GetCMSContentOutput GetCMSContents()
        {

            var content = _cmsContentRepository.GetAll();

            var contentDtoList = content
            .Select(cms => new CMSContentDto
            {
                Key = cms.Key,
                Content = cms.Content,
                Title = cms.Title,
                SubTitle = cms.SubTitle,
                Keywords = cms.Keywords,
                IsActive = cms.IsActive,
                MediaImages = cms.MediaImages
                //PageName = cms.PageName,
                //HtmlPage = cms.HtmlPage,
                //Description = cms.Description,

            }).ToList();
            return new GetCMSContentOutput { CMSContents = contentDtoList };

        }

        public GetCMSContentOutput GetCMSContentsByKey(KeyInput input)
        {
            var contentByKey = _cmsContentRepository.GetAllList(key => key.Key == input.Key);

            var contentByKeyDtoList = contentByKey
            .Select(cms => new CMSContentDto
            {
                Key = cms.Key,
               //Content = cms.Content,
               Title = cms.Title,
                SubTitle = cms.SubTitle,
                Keywords = cms.Keywords,
               //IsActive = cms.IsActive,
               //MediaImages = cms.MediaImages

            }).ToList();
            return new GetCMSContentOutput { CMSContents = contentByKeyDtoList };
        }
    }
}
