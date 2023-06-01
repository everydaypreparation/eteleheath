using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.OncologyConsultReports.Dto
{
    public class ConsultReportInput
    {
        public string ConsultId { get; set; }
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
        public Guid UserId { get; set; }
        public Guid? AppointmentId { get; set; }
        public IFormFile SignaturePath { get; set; }
        //public DateTime? CompletedDate { get; set; }
        //public IFormFile SignaturePath { get; set; }
        //public bool IsCompleted { get; set; }

    }

    public class CompelteConsultReport
    {
        public string ConsultId { get; set; }
        [Required(ErrorMessage = "Please enter purpose")]
        public string Purpose { get; set; }
        [Required(ErrorMessage = "Please enter allergies")]
        public string Allergies { get; set; }
        [Required(ErrorMessage = "Please enter investigation")]
        public string Investigation { get; set; }
        [Required(ErrorMessage = "Please enter impression")]
        public string Impression { get; set; }
        [Required(ErrorMessage = "Please enter plan")]
        public string Plan { get; set; }
        [Required(ErrorMessage = "Please enter family history")]
        public string FamilyHistory { get; set; }
        [Required(ErrorMessage = "Please enter social history")]
        public string SocialHistory { get; set; }
        [Required(ErrorMessage = "Please enter medication")]
        public string Medication { get; set; }
        [Required(ErrorMessage = "Please enter past medical history")]
        public string PastMedicalHistory { get; set; }
        [Required(ErrorMessage = "Please enter notes")]
        public string Notes { get; set; }
        [Required(ErrorMessage = "Please enter review of history")]
        public string ReviewOfHistory { get; set; }
        [Required(ErrorMessage = "Please enter userId")]
        public Guid? UserId { get; set; }
        [Required(ErrorMessage = "Please enter appointmentId")]
        public Guid? AppointmentId { get; set; }

        public IFormFile SignaturePath { get; set; }
    }

    public class Pdfoutput
    {
        public string PdfPath { get; set; }
        public bool IsPdfGenerated { get; set; }

        public string CreateRequestId { get; set; }
    }
}
