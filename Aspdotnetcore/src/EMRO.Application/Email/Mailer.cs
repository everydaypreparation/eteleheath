
using Abp.Application.Services;
using Abp.Domain.Repositories;
using EMRO.Authorization.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using EMRO.Common.Paubox;
using Microsoft.Extensions.Configuration;

namespace EMRO.Email
{
    public class Mailer : ApplicationService, IMailer
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly Paubox _paubox;
        private readonly IWebHostEnvironment _env;
        private IConfiguration _configuration;
        //IRepository<User, long> _repository;
        public Mailer(IOptions<SmtpSettings> smtpSettings, 
        IWebHostEnvironment env, 
        IConfiguration configuration, 
        Paubox paubox)
        {
            _smtpSettings = smtpSettings.Value;
            _env = env;
            //_repository = repository;
            _configuration = configuration;
            _paubox = paubox;
        }

        public Task SendEmailAsync(string email, string subject, string body, string adminmail)
        {
            try
            {
                if (_configuration["App:ServerRootAddress"] == "https://api.emro.cloud/" || _configuration["App:ServerRootAddress"] == "https://devapi.emro.cloud/")
                {
                    _paubox.PauboxSendEmailAsync(email, subject, body);
                }
                else
                {
                    // string adminmail = _repository.GetAll().FirstOrDefault().EmailAddress;
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient(_smtpSettings.Server);
                    mail.From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName);
                    mail.To.Add(email);
                    //mail.CC.Add(adminmail); //No CC required for now.
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    SmtpServer.Port = _smtpSettings.Port;
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
                    SmtpServer.EnableSsl = true;
                    SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    SmtpServer.Send(mail);

                    //var message = new MimeMessage();
                    //message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                    //message.To.Add(new MailboxAddress(email.Trim()));
                    //message.Subject = subject;
                    //message.Body = new TextPart("html")
                    //{
                    //    Text = body
                    //};

                    //using (var client = new SmtpClient())
                    //{
                    //    //client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    //    //client.CheckCertificateRevocation = false;
                    //     if (_env.IsDevelopment())
                    //     {
                    //         await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.Auto);
                    //     }
                    //     else
                    //     {
                    //         await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.Auto);
                    //     }

                    //     await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                    //     await client.SendAsync(message);
                    //     await client.DisconnectAsync(true);
                    // }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Email Error " + ex.StackTrace);
                Logger.Error("Email Error " + ex.Message);
            }

            return Task.CompletedTask;
        }


    }
}
