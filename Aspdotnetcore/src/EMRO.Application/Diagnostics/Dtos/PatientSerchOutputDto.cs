using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Diagnostics.Dtos
{
    public class PatientSerchOutputDto
    {
        public Guid CaseId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }

    }
}
