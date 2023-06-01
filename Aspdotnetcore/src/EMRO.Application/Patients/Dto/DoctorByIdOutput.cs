using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Patients.Dto
{
    public class DoctorByIdOutput
    {
        public string Title { get; set; }

        public string SpecialtyName { get; set; }
        public string ConsultantName { get; set; }
        public string HospitalName { get; set; }
        public Guid? ConsultantId { get; set; }
        public string profileUrl { get; set; }
        public string ProfessionalBio { get; set; }
        public string AffiliationSecondary { get; set; }
        public string UndergraduateMedicalTraining { get; set; }
        public string PostGraduateAndResidencyTraining { get; set; }
        public string OncologySpecialty { get; set; }
        public string OncologySubSpecialty { get; set; }
        public string MedicalAssociationMembership { get; set; }
        public string LicensingNumber { get; set; }
        public string ParticipationInInsurancePlans { get; set; }
        public string Choice1 { get; set; }
        public string Choice2 { get; set; }
        public virtual string Gender { get; set; }

        public List<DoctorAppointmentlist> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
    public class DoctorAppointmentlist
    {

        public string Slots { get; set; }
    }
}
