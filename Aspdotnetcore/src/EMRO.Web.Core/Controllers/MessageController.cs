using System;
using System.Collections.Generic;
using System.Text;
using Abp.Authorization;
using Microsoft.AspNetCore.Mvc;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace EMRO.Controllers
{
    [AbpAuthorize]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiversion}/[controller]/[action]")]
    public class MessageController : EMROControllerBase
    {
        private readonly ITwilioRestClient _client;

        public MessageController(ITwilioRestClient client)
        {
            _client = client;
        }

        [HttpPost]
        public IActionResult SendSms(MessageModel model)
        {
            var message = MessageResource.Create(
                to: new PhoneNumber(model.To),
                from: new PhoneNumber(model.From),
                body: model.Message,
                client: _client);

            return Ok(message.Sid);
        }

        public class MessageModel
        {
            public string To { get; set; }
            public string From { get; set; }
            public string Message { get; set; }
        }
    }
}
