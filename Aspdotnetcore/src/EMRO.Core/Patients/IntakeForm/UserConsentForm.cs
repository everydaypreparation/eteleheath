using Abp.Domain.Entities;
using EMRO.Authorization.Users;
using EMRO.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EMRO.Patients.IntakeForm
{
   public class UserConsentForm : Entity<Guid>,IMayHaveTenant
    {
        [ForeignKey("ConsentFormsMasterId")]
        public virtual ConsentFormsMaster ConsentFormsMaster { get; set; }

        [Column("UserConsentFormId")]
        [Key]
        public override Guid Id { get; set; }
        public Guid? ConsentFormsMasterId { get; set; }

       [ForeignKey("UniqueUserId")]
        public virtual User Users { get; set; }

        public string SignaturePath { get; set; }
       
        public string ConsentName { get; set; }

        public DateTime? DateConfirmation { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public Guid? UserId { get; set; }
        public int? TenantId { get; set; }

        public string VersionId { get; set; }
        public string EncryptionKeySha256 { get; set; }
        public string EncryptionScope { get; set; }
        public string BlobSequenceNumber { get; set; }
        public string CreateRequestId { get; set; }
        public bool IsBlobStorage { get; set; }
    }
}
