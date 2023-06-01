using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Diagnostics.Dtos
{
    public class GetCaseListDto
    {
        public Guid CaseId { get; set; }
        public long PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime? CreatedDate { get; set; }

        public Guid PatientIdUuid { get; set; }
        public Guid AppointmentId { get; set; }
    }

    public class GetCaseListOutputDto
    {
        public List<GetCaseListDto> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public int Count { get; set; }
    }
}
