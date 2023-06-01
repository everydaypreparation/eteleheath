using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Azure.Storage.Blobs.Models;
using EMRO.Common.AzureBlobStorage.Dto;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace EMRO.Common.AzureBlobStorage
{
    public interface IBlobContainer : IApplicationService
    {
        List<string> ListFiles();
        Task<Azure.Response<BlobContentInfo>> Upload(UploadDocumentInput uploadDocumentInput);
        string Download(string azureDirectoryPath);
        bool Delete(string azureDirectoryPath);

        Task<Azure.Response<BlobContentInfo>> UploadPdf(string blobpath,string filepath);

        Task<string> DownloadSignature(string azureDirectoryPath);

    }
}
