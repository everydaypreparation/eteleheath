using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Patients.Dto
{
   public class DoctorSearchInput
    {
        public string SpecialtyName { get; set; }
        public string ConsultantName { get; set; }
        public string HospitalName { get; set; }
        public string NextAvailability { get; set; }
        public string TimeZone { get; set; }
    }
}
