using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Specialty.Dto
{
   public class GetSpecialtyOutputDto
    {
        public List<GetSpecialtyDto> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
