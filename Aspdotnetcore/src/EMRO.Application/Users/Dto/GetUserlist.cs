using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Users.Dto
{
    public class GetUserlist
    {
        public Guid? Id { get; set; }
        public string EmailId { get; set; }
        public string FullName { get; set; }
    }
    public class GetUserlistoutput {
        public List<GetUserlist> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
