using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Patients.Dto
{
   public class UserConsentDto
    {
        public long PatientId { get; set; }
        public string ConsentMedicalInformationWithCancerCareProvider { get; set; }
        public long? UserId { get; set; }

        public DateTime CreationTime { get; set; }
        public int CreatedBy { get; set; }
        public DateTime ModificationTime { get; set; }
        public int ModifiedBy { get; set; }
    }
}
