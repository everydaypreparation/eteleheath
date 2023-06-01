using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common
{
    public class UploadProfileOutput
    {
        public string VersionId { get; set; }
        public string EncryptionKeySha256 { get; set; }
        public string EncryptionScope { get; set; }
        public string BlobSequenceNumber { get; set; }
        public string CreateRequestId { get; set; }
        public bool IsBlobStorage { get; set; }
        public string ProfilePath { get; set; }
    }
}
