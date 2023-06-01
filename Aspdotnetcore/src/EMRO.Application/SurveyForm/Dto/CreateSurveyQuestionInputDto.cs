using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.SurveyForm.Dto
{
    public class CreateSurveyQuestionInputDto
    {
		public string QuestionText { get; set; }
		public string QuestionType { get; set; }
		public string[] OptionSet { get; set; }
	}

	public class SurveyQuestionOutputDto
	{
		public Guid Id { get; set; }
		public string Message { get; set; }
		public int StatusCode { get; set; }
	}
}
