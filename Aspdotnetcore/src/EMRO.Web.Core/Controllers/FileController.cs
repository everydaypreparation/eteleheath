using Abp.Domain.Repositories;
using Abp.Runtime.Security;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EMRO.Common;
using EMRO.Documents;
using EMRO.InternalMessages;
using EMRO.Models;
using EMRO.UserDocuments;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Controllers
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiversion}/[controller]/[action]")]
    public class FileController : EMROControllerBase
    {
        // private readonly IDocumentRepository _documentRepository;
        //IDocumentRepository documentRepository
        private readonly IRepository<UserDocument, Guid> _userDocumentRepositor;
        private IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IRepository<UserMessagesAttachment, Guid> _userMessagesAttachmentRepository;
        public FileController(IRepository<UserDocument, Guid> userDocumentRepositor, IConfiguration configuration, IWebHostEnvironment env, IRepository<UserMessagesAttachment, Guid> userMessagesAttachmentRepository)
        {
            _userDocumentRepositor = userDocumentRepositor;
            _configuration = configuration;
            _env = env;
            _userMessagesAttachmentRepository = userMessagesAttachmentRepository;
            //_documentRepository = documentRepository;
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> DownloadBlob(string Uri)
        {
            try
            {
                // string url = System.Web.HttpUtility.UrlDecode(Uri);
                BlobClient blobClient = new BlobClient(new Uri(Uri));
                BlobProperties properties = await blobClient.GetPropertiesAsync();
                MemoryStream fs = new MemoryStream();
                var download = await blobClient.DownloadToAsync(fs);
                if (properties.ContentType.Contains("image"))
                {
                    return File(fs.ToArray(), properties.ContentType);
                }
                else
                {
                    return File(fs.ToArray(), properties.ContentType, blobClient.Name);
                }


            }
            catch (Exception ex)
            {
                Logger.Info("Document Download Error:" + ex);

            }
            return null;

        }
        [HttpGet]
        public IActionResult DownloadImage(string fileName)
        {

            try
            {
                string imageUrl = _env.WebRootPath + "/Templates/EmailTemplate/images/"+ fileName;
                byte[] binaryImage = System.IO.File.ReadAllBytes(imageUrl);
                return File(binaryImage, "image/png");
            }
            catch (Exception ex)
            {
                Logger.Info("Download ImageError:" + ex);

            }
            return null;

        }



    }
}
