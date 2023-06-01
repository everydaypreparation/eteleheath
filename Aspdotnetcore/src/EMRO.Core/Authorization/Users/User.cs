using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Authorization.Users;
using Abp.Extensions;
using EMRO.Master;

namespace EMRO.Authorization.Users
{
    public class User : AbpUser<User>
    {
        public const string DefaultPassword = "123qwe";



        [Column(TypeName = "bytea")]
        public override string Name { get; set; }
        [Column(TypeName = "bytea")]
        public override string Surname { get; set; }
        public virtual string Timezone { get; set; }

        [Column(TypeName = "bytea")]
        public virtual string Address { get; set; }
        [Column(TypeName = "bytea")]
        public string City { get; set; }
        [Column(TypeName = "bytea")]
        public string State { get; set; }
        [Column(TypeName = "bytea")]
        public string Country { get; set; }
        [Column(TypeName = "bytea")]
        public string PostalCode { get; set; }

        [Column(TypeName = "bytea")]
        public virtual string DateOfBirth { get; set; }
        [Column(TypeName = "bytea")]
        public virtual string Gender { get; set; }

        [Column(TypeName = "bytea")]
        public override string PhoneNumber { get; set; }
        public virtual string UserType { get; set; }
        public virtual string UploadProfilePicture { get; set; }

        public virtual string Title { get; set; }
        
        //[Required]
        public Guid UniqueUserId { get; set; }

        public string VersionId { get; set; }
        public string EncryptionKeySha256 { get; set; }
        public string EncryptionScope { get; set; }
        public string BlobSequenceNumber { get; set; }
        public string CreateRequestId { get; set; }
        public bool IsBlobStorage { get; set; }

        public static string CreateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Truncate(16);
        }

        public static User CreateTenantAdminUser(int tenantId, string emailAddress)
        {
            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,
                Surname = AdminUserName,
                EmailAddress = emailAddress,
                Roles = new List<UserRole>()
            };

            user.SetNormalizedNames();

            return user;
        }
    }
}
