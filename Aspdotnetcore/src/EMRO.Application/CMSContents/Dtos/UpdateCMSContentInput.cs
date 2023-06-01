using Abp.Runtime.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.CMSContents.Dtos
{
    public class UpdateCMSContentInput : ICustomValidate
    {
        [Range(1, long.MaxValue)] //Data annotation attributes work as expected.
        public Guid? CMSContentId { get; set; }
        //public string PageName { get; set; }
        //public string HtmlPage { get; set; }
        public string Key { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        //public string Short_Desc { get; set; }
        //public string Description { get; set; }
        public string Keywords { get; set; }
        public string MediaImages { get; set; }
        public bool IsActive { get; set; }//New Property Added

        //Custom validation method. It's called by ABP after data annotation validations.
        public void AddValidationErrors(CustomValidationContext context)
        {
            if (CMSContentId == null)
            {
                context.Results.Add(new ValidationResult(" CMSContentId can not be null in order to update a Content!", new[] { "CMSContentId"}));
            }
        }
    }
}
