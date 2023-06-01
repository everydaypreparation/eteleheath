using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.OncologyConsultReports.Dto
{
    public class GetConsultOutput
    {
        public string Purpose { get; set; }
        public string Allergies { get; set; }
        public string Investigation { get; set; }
        public string Impression { get; set; }
        public string Plan { get; set; }
        public string FamilyHistory { get; set; }
        public string SocialHistory { get; set; }
        public string Medication { get; set; }
        public string PastMedicalHistory { get; set; }
        public string Notes { get; set; }
        public string ReviewOfHistory { get; set; }
        public Guid? UserId { get; set; }
        public Guid? AppointmentId { get; set; }
        public Guid ConsultId { get; set; }
        public string Signature { get; set; }
        public string Mimetype { get; set; }
        public string SignatureName { get; set; }

        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
