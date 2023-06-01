using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.OncologyConsultReports.Dto
{
    public class GetCasesOutput
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public Guid ConsultId { get; set; }
        public string AppointmentDate { get; set; }
        public Guid AppointmentId { get; set; }
        public string BookedBy { get; set; }

        public bool IsBookLater { get; set; }

        public Guid DoctorId { get; set; }
        public Guid? UserId { get; set; }

        public bool IsPayment { get; set; }

        public string PatientId { get; set; }
    }

    public class CasesOutput
    {
        public List<GetCasesOutput> Items { get; set; }
        public int Count { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class GetUserCasesOutput
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public Guid ConsultId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string BookedBy { get; set; }
    }

    public class UserCasesOutput
    {
        public List<GetUserCasesOutput> Items { get; set; }
        public int Count { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
