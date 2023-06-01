using Abp.Domain.Entities;
using EMRO.Appointment;
using EMRO.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.SurveyForm
{
    public class SurveyResponse : Entity<Guid>, IMayHaveTenant
    {
        [Column("ResponseId")]
        [Key]
        public override Guid Id { get; set; }
        public int? TenantId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual SurveyQuestionMaster SurveyQuestionMaster { get; set; }
        public Guid? QuestionId { get; set; }

        public string Response { get; set; }
        public DateTime? ResponseTime { get; set; }
        public Guid? AppointmentId { get; set; }
        public Guid UserId { get; set; }
        public string CaseId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
