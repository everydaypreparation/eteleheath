using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.AzureBlobStorage.Dto
{
    /// <summary>
    /// In input it will allow to upload file and 
    /// give azure directory path where you want to upload the file
    /// e.g. Documents/Patient/{date}/PatientId/Radiation/{fileName}
    /// </summary>
    public class UploadDocumentInput
    {
        public IFormFile Document { get; set; }
        public string azureDirectoryPath { get; set; }
    }
}
