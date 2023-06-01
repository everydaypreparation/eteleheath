using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.ConsentFormsMasters.Dto
{
   public class GetConsentFormOutput
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string SubTitle { get; set; }
        public string ShortDescription { get; set; }

        public string Disclaimer { get; set; }
        public Guid ConsentFormsId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class GetConsentFormOutputList {
        public List<Getconsentforms> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class Getconsentforms {
        public string Title { get; set; }
        public string Description { get; set; }
        public string SubTitle { get; set; }
        public string ShortDescription { get; set; }
        public Guid ConsentFormsId { get; set; }
        public string Disclaimer { get; set; }
    }

}
