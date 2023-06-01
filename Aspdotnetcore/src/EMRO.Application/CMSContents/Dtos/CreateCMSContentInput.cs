using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.CMSContents.Dtos
{
	[AutoMapTo(typeof(CMSContent))]
	public class CreateCMSContentInput
    {
		//public string PageName { get; set; }
		public string Key { get; set; }
		//public string HtmlPage { get; set; }
		public string Content { get; set; }
		public string Title { get; set; }
		public string SubTitle { get; set; }
		//public string Short_Desc { get; set; }
		//public string Description { get; set; }
		public string Keywords { get; set; }
		public string MediaImages { get; set; }
		public bool IsActive { get; set; }
	}
}
