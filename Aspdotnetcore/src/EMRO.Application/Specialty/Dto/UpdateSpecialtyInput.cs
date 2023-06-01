using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Specialty.Dto
{
   public class UpdateSpecialtyInput
    {
        public Guid Id { get; set; }
        public string SpecialtyName { get; set; }
    }
}
