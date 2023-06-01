using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Patients.Dto
{
    public class GetDocumentListInput
    {
        public Guid UserId { get; set; }
        public Guid AppoinmentId { get; set; }

        public int limit { get; set; }
        public int page { get; set; }
    }
    public class GetDocumentListOutput
    {
        public List<DocumentData> Items { get; set; }
        public int Count { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }

    }
    public class DocumentData
    {
        public Guid DocumentId { get; set; }
        public string DocumentName { get; set; }
        public DateTime? DocumentDate { get; set; }
        public Guid? UploadedBy { get; set; }
        public string RoleName { get; set; }
        public string DocumentExtension { get; set; }
    }

    public class GetIntakeInput
    {
        public Guid AppoinmentId { get; set; }
    }

    public class GetIntakeConsultantReportsByPatientIdInput
    {
        public Guid UserId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
    }
}
