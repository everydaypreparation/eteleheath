using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.MultiTenancy;

namespace EMRO.MultiTenancy.Dto
{
    [AutoMapTo(typeof(Tenant))]
    public class CreateTenantDto
    {
        [Required]
        [StringLength(AbpTenantBase.MaxTenancyNameLength)]
        [RegularExpression(AbpTenantBase.TenancyNameRegex)]
        public string TenancyName { get; set; }

        [Required]
        [StringLength(AbpTenantBase.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string AdminEmailAddress { get; set; }

        [StringLength(AbpTenantBase.MaxConnectionStringLength)]
        public string ConnectionString { get; set; }

        public bool IsActive {get; set;}

        public virtual string FirstName { get; set; }

        public virtual string LastName { get; set; }

        //[Column(TypeName = "varchar(250)")]
        //public string EmailId { get; set; }

        public virtual string Phone { get; set; }

        public string Address1 { get; set; }

        public virtual string Address2 { get; set; }

        public virtual string AppDomain { get; set; }

        public string Description { get; set; }

        public virtual string Type { get; set; }
        public virtual string Country { get; set; }
        public virtual string State { get; set; }
        public virtual string City { get; set; }
        public virtual string PostalCode { get; set; }
    }
}
