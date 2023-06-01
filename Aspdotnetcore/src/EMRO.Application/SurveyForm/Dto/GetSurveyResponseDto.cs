using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.SurveyForm.Dto
{
    public class GetSurveyResponseOutputDto
    {
        //public List<GetSurveyResponseDto> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public List<GetSurveyResponseDto> items { get; set; }
    }

    public class Question
    {
        public string QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public string OptionSet { get; set; }
        public string Response { get; set; }
        public Guid ResponseId { get; set; }
    }

    public class GetSurveyResponseDto
    {
        public string AppointmentId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string FormSubmissionDate { get; set; }
        public string UserName { get; set; }
        public List<Question> questions { get; set; }
        public Guid UserId { get; set; }
    }

}
