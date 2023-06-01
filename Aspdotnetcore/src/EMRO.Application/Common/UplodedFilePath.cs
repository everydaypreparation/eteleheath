using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common
{
    public class UplodedFilePath
    {
        public string DocumentPath { get; set; }
        public string ProfilePicturePath { get; set; }
        public string Signatures { get; set; }
        public string MailAttachment { get; set; }
        public string MyDocument { get; set; }
        public string ConsultSignature { get; set; }
        public string ConsultReport { get; set; }
        public string Slash { get; set; }
        public bool IsBlob { get; set; }

        public string BlobDocumentPath { get; set; }
        public string BlobProfilePicturePath { get; set; }
        public string BlobSignatures { get; set; }
        public string BlobMailAttachment { get; set; }
        public string BlobConsultSignature { get; set; }
        public string BlobConsultReport { get; set; }
    }
}
