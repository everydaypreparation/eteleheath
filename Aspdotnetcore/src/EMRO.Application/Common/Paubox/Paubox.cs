using Abp.Application.Services;
using EMRO.Common.Paubox.Dto;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;

namespace EMRO.Common.Paubox
{
    public class Paubox : ApplicationService, IPaubox
    {
        private readonly PauboxParams _pauboxParams;
       
        public Paubox(IOptions<PauboxParams> pauboxParams)
        {
            _pauboxParams = pauboxParams.Value;
        }
        public IRestResponse PauboxSendEmailAsync(string email, string subject, string body)
        {
            string[] multipleEmails = email.Split(',');

            //Create Rest Client based on base URL
            var client = new RestClient(_pauboxParams.PAUBOX_API_URL);
            client.Timeout = -1;

            Headers headers = new Headers();
            headers.subject = subject;
            headers.from = _pauboxParams.FROM;

            Content content = new Content();
            content.textHtml = body;

            Message message = new Message();
            message.content = content;
            message.headers = headers;
            message.recipients = multipleEmails;

            Data data = new Data();
            data.message = message;

            PauboxMain main = new PauboxMain();
            main.data = data;

            var jsonString = JsonConvert.SerializeObject(main);

            //Create Request Object
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Token " + _pauboxParams.PAUBOX_TOKEN );
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", jsonString, ParameterType.RequestBody);

            //Paubox API Call
            IRestResponse response = client.Execute(request);
            return response;
        }
    }
}
