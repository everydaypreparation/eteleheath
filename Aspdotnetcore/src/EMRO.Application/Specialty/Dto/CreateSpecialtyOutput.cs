using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Specialty.Dto
{
   public class CreateSpecialtyOutput
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
