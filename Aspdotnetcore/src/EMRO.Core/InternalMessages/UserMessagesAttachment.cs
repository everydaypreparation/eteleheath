using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.InternalMessages
{
   public class UserMessagesAttachment : Entity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Column("UserMessagesAttachmentId")]
        [Key]
        public override Guid Id { get; set; }

        public string AttachmentName { get; set; }
        public string FileSize { get; set; }
        public string FilePath { get; set; }

        public string UserEmailId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }

        [ForeignKey("UserMessagesId")]
        public virtual UserMessages Users { get; set; }
        public Guid? UserMessagesId { get; set; }
        [Column(TypeName = "varchar(250)")]
        public string Mimetype { get; set; }

        public string VersionId { get; set; }
        public string EncryptionKeySha256 { get; set; }
        public string EncryptionScope { get; set; }
        public string BlobSequenceNumber { get; set; }
        public string CreateRequestId { get; set; }
        public bool IsBlobStorage { get; set; }
    }
}
