using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Patients.Dto
{
   public class ConsultationReportDto
    {
        public long PatientId { get; set; }
        public long? UserId { get; set; }
        public long ReportTypeId { get; set; }
        public string ReportPath { get; set; }
        public DateTime CreationTime { get; set; }
        public int CreatedBy { get; set; }
        public DateTime ModificationTime { get; set; }
        public int ModifiedBy { get; set; }
    }
}
