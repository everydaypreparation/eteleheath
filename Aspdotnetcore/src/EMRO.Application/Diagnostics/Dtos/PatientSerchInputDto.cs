using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Diagnostics.Dtos
{
    public class PatientSerchInputDto
    {
        public long PatientId { get; set; }
        public Guid UserId { get; set; }
    }

    public class CaseInputDto
    {
        public Guid UserId { get; set; }
        public int limit { get; set; }
        public int page { get; set; }
    }

    public class UpdateCaseInputDto
    {
        public Guid CaseId { get; set; }
    }
}
