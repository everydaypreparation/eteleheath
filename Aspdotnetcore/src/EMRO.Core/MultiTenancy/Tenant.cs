using Abp.MultiTenancy;
using EMRO.Authorization.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMRO.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {

        [Column(TypeName = "varchar(250)")]
        public virtual string FirstName { get; set; }

        [Column(TypeName = "varchar(250)")]
        public virtual string LastName { get; set; }

        //[Column(TypeName = "varchar(250)")]
        //public string EmailId { get; set; }

        [Column(TypeName = "varchar(50)")]
        public virtual string Phone { get; set; }

        [Column(TypeName = "varchar(512)")]
        public string Address1 { get; set; }

        [Column(TypeName = "varchar(512)")]
        public virtual string Address2 { get; set; }

        [Column(TypeName = "varchar(250)")]
        public virtual string AppDomain { get; set; }

        [Column(TypeName = "varchar(512)")]
        public string Description { get; set; }

        [Column(TypeName = "varchar(50)")]
        public virtual string Type { get; set; }
        [Column(TypeName = "varchar(50)")]
        public virtual string Country { get; set; }
        [Column(TypeName = "varchar(50)")]
        public virtual string State { get; set; }
        [Column(TypeName = "varchar(50)")]
        public virtual string City { get; set; }
        [Column(TypeName = "varchar(50)")]
        public virtual string PostalCode { get; set; }

        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
