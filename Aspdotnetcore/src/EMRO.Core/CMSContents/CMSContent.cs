using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.CMSContents
{
    public class CMSContent : Entity<Guid>,IMayHaveTenant
    {
		public virtual int? TenantId { get; set; }
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
		//Add new property
		public bool IsActive { get; set; }

		public DateTime? CreatedOn { get; set; }
		public Guid? CreatedBy { get; set; }
		public DateTime? UpdatedOn { get; set; }
		public Guid? UpdatedBy { get; set; }
		public DateTime? DeletedOn { get; set; }
		public Guid? DeletedBy { get; set; }

	}
}
