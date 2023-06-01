using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.SurveyForm.Dto
{
    public class SurveyDeleteInputDto
    {
        public Guid AppointmentId { get; set; }
        public Guid UserId { get; set; }
    }
    public class SurveyDeleteOutputDto
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
