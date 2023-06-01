using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.ConsentFormsMasters.Dto
{
   public class CreateConsentFormOutput
    {
        public Guid ConsentFormId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
