using Abp.Application.Services;
using EMRO.SurveyForm.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.SurveyForm
{
    public interface ISurveyFormAppService : IApplicationService
    {
        Task<SurveyQuestionOutputDto> Create(CreateSurveyQuestionInputDto input);
        GetSurveyQuestionOutputDto GetAll();
        Task<SurveyResponseOutputDto> SurveyResponse(List<SurveyResponseInputDto> input);
        Task<GetSurveyResponseOutputDto> GetSurveyResponse();
        Task<SurveyAuthenticateUserOutputDto> SurveyAuthenticateUser(SurveyAuthenticateUserInputDto input);
        Task<SurveyDeleteOutputDto> Delete(SurveyDeleteInputDto input);
    }
}
