using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Patients.Dto
{
    public class PatientIntakeOutput
    {
        public Guid UserConsetId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
