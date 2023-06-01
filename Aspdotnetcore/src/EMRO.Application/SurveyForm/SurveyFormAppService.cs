using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EMRO.Appointment;
using EMRO.AppointmentSlot;
using EMRO.Authorization.Users;
using EMRO.Sessions;
using EMRO.SurveyForm.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace EMRO.SurveyForm
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize]
    public class SurveyFormAppService : ApplicationService, ISurveyFormAppService
    {
        private readonly IRepository<SurveyQuestionMaster, Guid> _surveyQuestionMasterRepository;
        private readonly IRepository<SurveyResponse, Guid> _surveyResponseRepository;
        private readonly EmroAppSession _session;
        private readonly IDoctorAppointmentRepository _doctorAppointmentRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<DoctorAppointmentSlot, Guid> _doctorAppointmentSlotRepository;
        public SurveyFormAppService(IRepository<SurveyQuestionMaster, Guid> surveyQuestionMasterRepository, 
            IRepository<SurveyResponse, Guid> surveyResponseRepository,
            IDoctorAppointmentRepository doctorAppointmentRepository,
            IRepository<User, long> userRepository,
             IRepository<DoctorAppointmentSlot, Guid> doctorAppointmentSlotRepository,
            EmroAppSession session)
        {
            _surveyQuestionMasterRepository = surveyQuestionMasterRepository;
            _surveyResponseRepository = surveyResponseRepository;
            _session = session;
            _doctorAppointmentRepository = doctorAppointmentRepository;
            _userRepository = userRepository;
            _doctorAppointmentSlotRepository = doctorAppointmentSlotRepository;

        }

        /// <summary>
        /// Method to create survey form questions
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        //[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<SurveyQuestionOutputDto> Create(CreateSurveyQuestionInputDto input)
        {
            SurveyQuestionOutputDto output = new SurveyQuestionOutputDto();
            try
            {
                int? TenantId = null;

                if (AbpSession.TenantId.HasValue)
                {
                    TenantId = AbpSession.TenantId.Value;
                }

                var questions = new SurveyQuestionMaster
                {
                    QuestionText = input.QuestionText,
                    QuestionType = input.QuestionType,
                    OptionSet = string.Join(",", input.OptionSet),
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    CreatedBy = Guid.Empty,
                    UpdatedBy = Guid.Empty,
                    TenantId = TenantId
                };
                output.Id = await _surveyQuestionMasterRepository.InsertAndGetIdAsync(questions);
                output.Message = "Question saved successfully";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Create Question Error:" + ex.StackTrace);
            }
            return output;
        }

        /// <summary>
        /// Method to get all the questions and its options from question master
        /// </summary>
        /// <returns></returns>
        public GetSurveyQuestionOutputDto GetAll()
        {
            GetSurveyQuestionOutputDto output = new GetSurveyQuestionOutputDto();
            try
            {
                var result = _surveyQuestionMasterRepository.GetAll();
                output.Items = result.Select(questions => new GetSurveyQuestionDto
                {
                    Id = questions.Id,
                    QuestionText = questions.QuestionText,
                    QuestionType = questions.QuestionType,
                    OptionSet = questions.OptionSet

                }).ToList();
                output.Message = "get question list successfully";
                output.StatusCode = 200;

            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Get All Survey Questions Error:" + ex.StackTrace);
            }
            return output;
        }

        /// <summary>
        /// Method to save survey response
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<SurveyResponseOutputDto> SurveyResponse(List<SurveyResponseInputDto> input)
        {          
            SurveyResponseOutputDto output = new SurveyResponseOutputDto();
            try
            {
                int? TenantId = null;
                Guid sessionUser = Guid.Empty;
                
                if (AbpSession.TenantId.HasValue)
                {
                    TenantId = AbpSession.TenantId.Value;
                }
                if (_session.UniqueUserId != null)
                {
                     sessionUser = Guid.Parse(_session.UniqueUserId);
                }

                //Decrypt Appointment Id for survey form
                var decryptedAppointmentId = Convert.FromBase64String(input[0].AppointmentId);
                var appointmentId = new Guid(decryptedAppointmentId);

                var repeatFormSubmit = _surveyResponseRepository.GetAll().Where(x => x.AppointmentId == appointmentId && x.UserId == sessionUser).ToList();
                if(repeatFormSubmit.Count > 0)
                {
                    output.Message = "Bad Rquest";
                    output.StatusCode = 401;
                    return output;
                }
                var userid = _doctorAppointmentRepository.GetAll().Where(x => x.Id == appointmentId).Select(x => x.UserId).FirstOrDefault();
                if (userid == sessionUser)
                {
                    // using method Reverse(), did this to fix order issue for Admin user
                    input.Reverse();

                    foreach (var item in input)
                    {
                        var response = new SurveyResponse
                        {
                            TenantId = TenantId,
                            QuestionId = item.QuestionId,
                            Response = item.Response,
                            ResponseTime = DateTime.UtcNow,
                            AppointmentId = appointmentId,
                            UserId = sessionUser,
                            CaseId = item.CaseId,
                            IsActive = true
                        };
                        output.Id = await _surveyResponseRepository.InsertAndGetIdAsync(response);
                    }
                    output.Message = "Response saved successfully";
                    output.StatusCode = 200;
                }
                else
                {
                    output.Message = "Bad Request";
                    output.StatusCode = 401;
                }  
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Create Survey Response Error:" + ex.StackTrace);
            }
            return output;

        }

        /// <summary>
        /// Method to get all survey responses
        /// </summary>
        /// <returns></returns>
        public async Task<GetSurveyResponseOutputDto> GetSurveyResponse()
        {
            GetSurveyResponseOutputDto output = new GetSurveyResponseOutputDto();
            List<GetSurveyResponseDto> getSurveyResponse = new List<GetSurveyResponseDto>(); 

            try
            {
                var list = await _surveyResponseRepository.GetAllListAsync(x => x.IsActive == true);
                
                var result = (from res in list.AsEnumerable()
                                  join ques in _surveyQuestionMasterRepository.GetAll()
                                  on res.QuestionId equals ques.Id
                                  join user in _userRepository.GetAll()
                                  on res.UserId equals user.UniqueUserId
                                  join appointment in _doctorAppointmentRepository.GetAll()
                                  on res.AppointmentId equals appointment.Id
                                  join slot in _doctorAppointmentSlotRepository.GetAll()
                                  on appointment.AppointmentSlotId equals slot.Id

                                  select new
                                  {
                                      res.QuestionId,
                                      ques.QuestionText,
                                      ques.QuestionType,
                                      ques.OptionSet,
                                      res.Response,
                                      res.AppointmentId,
                                      res.UserId,
                                      user.FullName,
                                      res.ResponseTime,
                                      slot.SlotZoneTime,
                                      res.Id
                                      
                                  }).OrderByDescending(x => x.ResponseTime).GroupBy(x => x.AppointmentId).ToList();
                if(result.Count > 0)
                {
                    foreach (var item in result)
                    {

                        List<Question> que = new List<Question>();

                        foreach (var fres in item)
                        {
                            que.Add(new Question
                            {
                                QuestionType = fres.QuestionType,
                                QuestionId = fres.QuestionId.ToString(),
                                QuestionText = fres.QuestionText,
                                OptionSet = fres.OptionSet,
                                Response = fres.Response,
                                ResponseId = fres.Id
                            });
                        }
                        getSurveyResponse.Add(new GetSurveyResponseDto
                        {
                            AppointmentId = item.First().AppointmentId.ToString(),
                            AppointmentDateTime = item.First().SlotZoneTime,
                            FormSubmissionDate = item.First().ResponseTime.ToString(),
                            UserName = item.First().FullName,
                            UserId = item.First().UserId,
                            questions = que

                        });
                    }

                    output.items = getSurveyResponse;
                    output.Message = "get survey response successfully";
                    output.StatusCode = 200;
                }
                else
                {
                    //output.items = getSurveyResponse;
                    output.Message = "No record found";
                    output.StatusCode = 401;
                }
                
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Get All Survey Response Error:" + ex.StackTrace);
            }
            return output;
        }

        /// <summary>
        /// Method to check user corresponding to provided appointment id
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<SurveyAuthenticateUserOutputDto> SurveyAuthenticateUser(SurveyAuthenticateUserInputDto input)
        {
            Guid sessionUser = Guid.Empty;
            SurveyAuthenticateUserOutputDto output = new SurveyAuthenticateUserOutputDto();
            try
            {
                if (_session.UniqueUserId != null)
                {
                    sessionUser = Guid.Parse(_session.UniqueUserId);
                } 

                //Decrypt Appointment Id for survey form
                var decryptedAppointmentId = Convert.FromBase64String(input.AppointmentId);
                var appointmentId = new Guid(decryptedAppointmentId);

                var repeatFormSubmit = _surveyResponseRepository.GetAll().Where(x => x.AppointmentId == appointmentId && x.UserId == sessionUser).ToList();
                if (repeatFormSubmit.Count > 0)
                {
                    output.isSurveySubmitted = true;
                }
                else
                {
                    output.isSurveySubmitted = false;
                }

                var appointmanetDetails = await _doctorAppointmentRepository.GetAllListAsync(x => x.Id == appointmentId);
                if(appointmanetDetails.Count > 0)
                {
                    output.UserId = appointmanetDetails.Select(x => x.UserId).FirstOrDefault();
                }
                else
                {
                    output.Message = "Bad Request";
                    output.StatusCode = 401;
                    return output;
                }

                output.Message = "get survey user successfully";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Create Survey Response Error:" + ex.StackTrace);
            }
            return output;
        }

        /// <summary>
        /// Method to delete survey response for the provided input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<SurveyDeleteOutputDto> Delete(SurveyDeleteInputDto input)
        {
            SurveyDeleteOutputDto output = new SurveyDeleteOutputDto();
            try
            {
                if (input.UserId != Guid.Empty && input.AppointmentId != Guid.Empty)
                {
                    var surveyResponseDetail = _surveyResponseRepository.GetAll().Where(x => x.AppointmentId == input.AppointmentId && x.UserId == input.UserId && x.IsActive == true).ToList();
                    if (surveyResponseDetail.Count > 0)
                    {
                        foreach (var item in surveyResponseDetail)
                        {
                            item.IsActive = false;
                            item.DeletedOn = DateTime.UtcNow;
                            if (_session.UniqueUserId != null)
                            {
                                item.DeletedBy = Guid.Parse(_session.UniqueUserId);
                            }
                            var result = await _surveyResponseRepository.UpdateAsync(item);
                        }

                        output.Message = "Feedback deleted successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "No record found.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {

                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("update Survey Form, stacktrace " + ex.StackTrace);
                Logger.Error("update Survey Form, message " + ex.Message);
            }

            return output;
           
        }
    }
}
