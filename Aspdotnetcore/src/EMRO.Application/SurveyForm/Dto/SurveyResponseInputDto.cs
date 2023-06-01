using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.SurveyForm.Dto
{
    public class SurveyResponseInputDto
    {
        public Guid? QuestionId { get; set; }
        public string Response { get; set; }        
        public string AppointmentId { get; set; }
        public string CaseId { get; set; }
    }

    public class SurveyResponseOutputDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
   
}
