using Abp.Application.Services;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Common.Paubox
{
    public interface IPaubox : IApplicationService
    {
        IRestResponse PauboxSendEmailAsync(string email, string subject, string body);
    }
}
