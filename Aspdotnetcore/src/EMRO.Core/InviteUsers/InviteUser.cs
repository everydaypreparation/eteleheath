using Abp.Domain.Entities;
using EMRO.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.InviteUsers
{
    public class InviteUser : Entity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public int? TenantId { get; set; }
        public bool IsDeleted { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string FirstName { get; set; }
        [Column(TypeName = "varchar(250)")]
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string PostalCodes { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string TelePhone { get; set; }
        [Column(TypeName = "varchar(250)")]
        public string EmailAddress { get; set; }
        //public string ConsentMedicalInformationWithCancerCareProvider { get; set; }
        public string Hospital { get; set; }
        public string UserType { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }

        [ForeignKey("UniqueUserId")]
        public virtual User Users { get; set; }
        public Guid ReferedBy { get; set; }

        public DateTime? CreatedOn { get; set; }
        public DateTime? OnboardedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOnboarded { get; set; }
        public bool IsActive { get; set; }
       
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; }
    }
}
