using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Patients.Dto
{
   public class PatientDto
    {
        public long? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }

        public long CityId { get; set; }
        public long StateId { get; set; }
        public long CountryId { get; set; }
        public long PostalCodeId { get; set; }
        public string TelePhone { get; set; }
        public string EmailID { get; set; }
        public string DateOfBirth { get; set; }
        public int Gender { get; set; }

        public DateTime CreationTime { get; set; }
        public int CreatedBy { get; set; }
        public DateTime ModificationTime { get; set; }
        public int ModifiedBy { get; set; }
    }
}
