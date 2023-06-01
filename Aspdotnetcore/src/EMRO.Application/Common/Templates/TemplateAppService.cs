using Abp.Application.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EMRO.Common.Templates
{
    public class TemplateAppService : ApplicationService, ITemplateAppService
    {
        private readonly IWebHostEnvironment _env;
        private IConfiguration _configuration;
        private string _apiEndpoint;
        public TemplateAppService(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
            _apiEndpoint = _configuration["App:ServerRootAddress"] + "api/V1/File/DownloadImage";
        }
        /// <summary>
        /// Get email template for email
        /// </summary>
        /// <param name="Name">user name</param>
        /// <param name="message">message body</param>
        /// <param name="signature">signature for email</param>
        /// <returns></returns>
        public string GetTemplates(string Name, string message, string signature)
        {
            string body = string.Empty;
            try
            {
                string imageUrl = _apiEndpoint + "?fileName=logo-emro.png";
                string facebookUrl = _apiEndpoint + "?fileName=023-facebook.png";
                string twitterUrl = _apiEndpoint + "?fileName=007-twitter-1.png";
                
                

                var pathToFile = _env.WebRootPath
                                            + Path.DirectorySeparatorChar.ToString()
                                            + "Templates"
                                            + Path.DirectorySeparatorChar.ToString()
                                            + "EmailTemplate"
                                            + Path.DirectorySeparatorChar.ToString()
                                            + "index.html";

                var builder = new BodyBuilder();
                using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
                {
                    builder.HtmlBody = SourceReader.ReadToEnd();
                }

                body = string.Format(builder.HtmlBody,
                         Name,
                         message,
                         DateTime.Now.Year,
                         imageUrl,
                         twitterUrl,
                         facebookUrl,
                         string.IsNullOrEmpty(signature) ? "Regards <br />ETeleHealth Team": signature
                   );

            }
            catch (Exception ex)
            {
                Logger.Error("Get Templates" + ex.StackTrace);
            }
            return body;
        }
    }
}
