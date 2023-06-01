using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.OncologyConsultReports.Dto
{
    public class ActiveCaseInput
    {
        public Guid UserId { get; set; }
        public string RoleName { get; set; }
        public int limit { get; set; }
        public int page { get; set; }
    }

    public class UserActiveCaseInput
    {
        public Guid AppointmentId { get; set; }
        public string RoleName { get; set; }
        public int limit { get; set; }
        public int page { get; set; }
    }
    public class PatientActiveCaseInput
    {
        public Guid UserId { get; set; }
        public Guid DoctorId { get; set; }
        public string RoleName { get; set; }
    }

    public class GetConsultantReportInput
    {
        public Guid UserId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public string RoleName { get; set; }
        public int limit { get; set; }
        public int page { get; set; }
    }
}
