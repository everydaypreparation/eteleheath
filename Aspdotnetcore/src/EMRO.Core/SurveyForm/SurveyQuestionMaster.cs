using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.SurveyForm
{
    public class SurveyQuestionMaster : Entity<Guid>, IMayHaveTenant
    {
        [Column("QuestionId")]
        [Key]
        public override Guid Id { get; set; }
        public virtual int? TenantId { get; set; }
		public string QuestionText { get; set; }

		[Column(TypeName = "varchar(512)")]
		public string QuestionType { get; set; }
		public string OptionSet { get; set; }
		public DateTime? CreatedOn { get; set; }

		[DefaultValue("00000000-0000-0000-0000-000000000000")]
		public Guid? CreatedBy { get; set; }
		public DateTime? UpdatedOn { get; set; }

		[DefaultValue("00000000-0000-0000-0000-000000000000")]
		public Guid? UpdatedBy { get; set; }
		public DateTime? DeletedOn { get; set; }
		public Guid? DeletedBy { get; set; }
	}
}
