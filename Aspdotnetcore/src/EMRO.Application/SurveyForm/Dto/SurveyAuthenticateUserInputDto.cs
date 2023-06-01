using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.SurveyForm.Dto
{
    public class SurveyAuthenticateUserInputDto
    {
        public string AppointmentId { get; set; }
    }

    public class SurveyAuthenticateUserOutputDto
    {
        public Guid? UserId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public bool isSurveySubmitted { get; set; }
    }
}
