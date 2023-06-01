using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EMRO.Common.CronJob.Dto
{
    public class IsProfileCompleteDto
    {
        public long Id { get; set; }
        public string EmailAddress { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserType { get; set; }

        [DisplayName("Date Of Birth")]
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        [DisplayName("Telephone")]
        public string PhoneNumber { get; set; }
        public string Credentials { get; set; }

        [DisplayName("Current Affiliation")]
        public string CurrentAffiliation { get; set; }

        [DisplayName("Short Professional Bio")]
        public string ProfessionalBio { get; set; }
        public string Residency1 { get; set; }
        public string Residency2 { get; set; }
        [DisplayName("Certificates")]
        public string Certificate { get; set; }

        [DisplayName("Experience / Training")]
        public string ExperienceOrTraining { get; set; }
        public string Fellowship { get; set; }

        [DisplayName("Undergraduate Medical Training")]
        public string UndergraduateMedicalTraining { get; set; }

        [DisplayName("Date Confirmed")]
        public DateTime? DateConfirmed { get; set; }

        [DisplayName("Licensing Number")]
        public string LicensingNumber { get; set; }

        [DisplayName("Medical Association Membership")]
        public string MedicalAssociationMembership { get; set; }
       
    }

    public class LegalProfileCompleteDto
    {
        public long Id { get; set; }
        public string EmailAddress { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserType { get; set; }
        [DisplayName("Date Of Birth")]
        public virtual string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        [DisplayName("Telephone")]
        public string PhoneNumber { get; set; }
        public string Company { get; set; }
        [DisplayName("Requested Oncology Subspecialty")]
        public string RequestedOncologySubspecialty { get; set; }
    }
}
