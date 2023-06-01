using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.SubSpecialty.Dto
{
    public class GetListOutput
    {
        public List<UpdateInputDto> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
