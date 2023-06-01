using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.ConsentFormsMasters.Dto
{
   public class UpdateConsentFormInput
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string SubTitle { get; set; }
        public string ShortDescription { get; set; }
        public Guid ConsentFormsId { get; set; }

        public string Disclaimer { get; set; }
    }
}
