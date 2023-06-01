using Abp.Domain.Entities;
using EMRO.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.UserDocuments
{
    public class UserDocument : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [Column("UserDocumentId")]
        [Key]
        public override Guid Id { get; set; }

        [ForeignKey("UniqueUserId")]
        public virtual User Users { get; set; }
        public Guid? UserId { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string Title { get; set; }
        public string Description { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string Category { get; set; }
        public string Path { get; set; }

        [Column(TypeName = "varchar(250)")]
        public string MimeType { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }

        public Guid? UserConsentsId { get; set; }

        public string VersionId { get; set; }
        public string EncryptionKeySha256 { get; set; }
        public string EncryptionScope { get; set; }
        public string BlobSequenceNumber { get; set; }
        public string CreateRequestId { get; set; }
        public bool IsBlobStorage { get; set; }


    }
}
