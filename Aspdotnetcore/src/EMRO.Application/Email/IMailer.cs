using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMRO.Email
{
    public interface IMailer : IApplicationService
    {
        Task SendEmailAsync(string email, string subject, string body, string adminmail);
    }
}
