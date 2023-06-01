using Abp.Application.Services;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EMRO.Common.AzureBlobStorage.Dto;
using System.Threading.Tasks;
using System.Security.Policy;

namespace EMRO.Common.AzureBlobStorage
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class BlobContainer : ApplicationService, IBlobContainer
    {
        private IConfiguration _configuration;
        private Uri _blobContainerUri;
        private string _apiEndpoint;
        private BlobContainerClient _container;

        public BlobContainer(IConfiguration configuration)
        {
            _configuration = configuration;
            _blobContainerUri = new Uri(_configuration["BlobContainer"]);
            _container = new BlobContainerClient(_blobContainerUri);
            _apiEndpoint = _configuration["App:ServerRootAddress"] + "api/V1/File/DownloadBlob";
        }

        /// <summary>
        /// delete blob from container
        /// </summary>
        /// <param name="azureDirectoryPath">Documents/Patient/{date}/PatientId/Radiation/{fileName}</param>
        /// <returns>boolean value</returns>
        public bool Delete(string azureDirectoryPath)
        {
            try
            {
                
                _container.DeleteBlobIfExists(azureDirectoryPath);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Delete File Error" + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// download file from blob storage
        /// </summary>
        /// <param name="azureDirectoryPath">Documents/Patient/{date}/PatientId/Radiation/{fileName}</param>
        /// <returns>Blob Uri</returns>
        [HttpGet]
        public string Download(string azureDirectoryPath)
        {
            try
            {
                Logger.Info("URL" + _blobContainerUri.AbsoluteUri);
                BlobClient blob = _container.GetBlobClient(azureDirectoryPath);
                Uri blobBaseClientUri = blob.Uri;
                string apiendpoint = _apiEndpoint + "?Uri=" + System.Web.HttpUtility.UrlEncode(blobBaseClientUri.ToString());

                return apiendpoint;
            }
            catch (Exception ex)
            {
                Logger.Error("Download File Error" + ex.StackTrace);
                return null;
            }
        }

        public async Task<string> DownloadSignature(string azureDirectoryPath)
        {
            try
            {
                Logger.Info("URL = " + _blobContainerUri.AbsoluteUri);
                BlobClient blob = _container.GetBlobClient(azureDirectoryPath);
                //Uri blobBaseClientUri = blob.Uri;
                MemoryStream fs = new MemoryStream();
                var download = await blob.DownloadToAsync(fs);
                string result = Convert.ToBase64String(fs.ToArray());
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error("Download File Error" + ex.StackTrace);
                return null;
            }
        }
        /// <summary>
        /// list file from container
        /// </summary>
        /// <returns>return list of file name</returns>
        [HttpGet]
        public List<string> ListFiles()
        {
            List<string> blobs = new List<string>();
            try
            {
                Logger.Info("URL = " + _blobContainerUri.AbsoluteUri);
                // Print out all the blob names
                foreach (BlobItem blob in _container.GetBlobs())
                {
                    Logger.Info(blob.Name);
                    blobs.Add(blob.Name);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get ListFiles Error" + ex.StackTrace);
            }

            return blobs;
        }

        /// <summary>
        /// upload file to blob storage
        /// </summary>
        /// <param name="uploadDocumentInput"></param>
        /// <returns></returns>
        public async Task<Azure.Response<BlobContentInfo>> Upload([FromForm] UploadDocumentInput uploadDocumentInput)
        {
            try
            {
                Logger.Info("URL = " + _blobContainerUri.AbsoluteUri);
                var data = uploadDocumentInput.Document.OpenReadStream();
                Azure.Response<BlobContentInfo> response = await _container.UploadBlobAsync(uploadDocumentInput.azureDirectoryPath, data);
                BlobClient blob = _container.GetBlobClient(uploadDocumentInput.azureDirectoryPath);
                BlobProperties properties = await blob.GetPropertiesAsync();

                BlobHttpHeaders headers = new BlobHttpHeaders
                {
                    ContentType = uploadDocumentInput.Document.ContentType,
                    ContentLanguage = "en-us",

                    CacheControl = properties.CacheControl,
                    ContentDisposition = properties.ContentDisposition,
                    ContentEncoding = properties.ContentEncoding,
                    ContentHash = properties.ContentHash
                };

                // Set the blob's properties.
                await blob.SetHttpHeadersAsync(headers);
                return response;
            }
            catch (Exception ex)
            {
                Logger.Error("Upload File Error" + ex.Message + " Exception" + ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// upload pdf file to blob storage this function is for consultant report
        /// </summary>
        /// <param name="uploadDocumentInput"></param>
        /// <returns></returns>
        public async Task<Azure.Response<BlobContentInfo>> UploadPdf(string blobpath, string filepath)
        {
            try
            {
                var data = File.OpenRead(filepath);
                Azure.Response<BlobContentInfo> response = await _container.UploadBlobAsync(blobpath, data);
                BlobClient blob = _container.GetBlobClient(blobpath);
                BlobProperties properties = await blob.GetPropertiesAsync();

                BlobHttpHeaders headers = new BlobHttpHeaders
                {
                    ContentType = "application/pdf",
                    ContentLanguage = "en-us",

                    CacheControl = properties.CacheControl,
                    ContentDisposition = properties.ContentDisposition,
                    ContentEncoding = properties.ContentEncoding,
                    ContentHash = properties.ContentHash
                };

                // Set the blob's properties.
                await blob.SetHttpHeadersAsync(headers);
                return response;
            }
            catch (Exception ex)
            {
                Logger.Error("Upload File Error" + ex.StackTrace);
                return null;
            }
        }
        /* Belom mention method and variables is used for testing purpose.
         * In near future if any changes need to made or any bug occur we can use this 
         * to fix issues or make changes in this service.
         */

        /// <summary>
        /// Lorem Ipsum sample file content
        /// </summary>
        protected const string SampleFileContent = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras dolor purus, 
interdum in turpis ut, ultrices ornare augue. Donec mollis varius sem, et mattis ex gravida eget. 
Duis nibh magna, ultrices a nisi quis, pretium tristique ligula. Class aptent taciti 
sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Vestibulum in dui arcu. 
Nunc at orci volutpat, elementum magna eget, pellentesque sem. Etiam id placerat nibh. 
Vestibulum varius at elit ut mattis.  Suspendisse ipsum sem, placerat id blandit ac, cursus eget purus. 
Vestibulum pretium ante eu augue aliquam, ultrices fermentum nibh condimentum. Pellentesque pulvinar feugiat augue vel accumsan. 
Nulla imperdiet viverra nibh quis rhoncus. Nunc tincidunt sollicitudin urna, eu efficitur elit gravida ut. Quisque eget urna convallis, 
commodo diam eu, pretium erat. Nullam quis magna a dolor ullamcorper malesuada. Donec bibendum sem lectus, sit amet faucibus nisi sodales eget. 
Integer lobortis lacus et volutpat dignissim. Suspendisse cras amet.";

        /// <summary>
        /// Create a temporary path for creating files.
        /// </summary>
        /// <param name="extension">An optional file extension.</param>
        /// <returns>A temporary path for creating files.</returns>
        private string CreateTempPath(string extension = ".txt") =>
            Path.ChangeExtension("App_Data/newFile.txt", extension);

        /// <summary>
        /// Create a temporary file on disk.
        /// </summary>
        /// <param name="content">Optional content for the file.</param>
        /// <returns>Path to the temporary file.</returns>
        private string CreateTempFile(string content = SampleFileContent)
        {
            string path = CreateTempPath();
            File.WriteAllText(path, content);
            return path;
        }

        /// <summary>
        /// Get a random name so we won't have any conflicts when creating
        /// resources.
        /// </summary>
        /// <param name="prefix">Optional prefix for the random name.</param>
        /// <returns>A random name.</returns>
        private string Randomize(string prefix = "sample") =>
            $"{prefix}-{Guid.NewGuid()}";


    }
}
