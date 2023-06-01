using Abp.Application.Services;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EMRO.AuditReport;
using EMRO.Authorization.Users;
using EMRO.Common.AuditReport.Dtos;
using EMRO.Sessions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMRO.Common.AuditReport
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    //[AbpAuthorize]
    public class AuditReportAppService : ApplicationService, IAuditReportAppService
    {
        private readonly IRepository<User, long> _userRepository;
        private IClientInfoProvider _clientInfoProvider;
        private readonly IRepository<AuditEvent, Guid> _auditEventRepository;
        private readonly EmroAppSession _session;
        public AuditReportAppService(IRepository<User, long> userRepository
            , IClientInfoProvider clientInfoProvider
            , IRepository<AuditEvent, Guid> auditEventRepository
            , EmroAppSession session)
        {
            _userRepository = userRepository;
            _clientInfoProvider = clientInfoProvider;
            _auditEventRepository = auditEventRepository;
            _session = session;
        }

        /// <summary>
        /// Get all Audit Log in list type for specific user
        /// </summary>
        /// <param name="auditReportInput"></param>
        /// <returns>AuditReportOutputDto</returns>

        [AbpAuthorize]
        public async Task<AuditReportOutputDto> getAuditReport(AuditReportInputDto auditReportInput)
        {
            AuditReportOutputDto auditReportOutput = new AuditReportOutputDto();
            List<AuditEvent> audits = new List<AuditEvent>();

            Logger.Info("Get Audit Log for user id: " + auditReportInput.userId);
            try
            {
                if (auditReportInput.From == null)
                {
                    auditReportInput.From = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }
                if (auditReportInput.To == null)
                {
                    auditReportInput.To = DateTime.Now;
                }

                var userId = auditReportInput.userId != null ? auditReportInput.userId : AbpSession.UserId;
                var userDetails = await _userRepository.FirstOrDefaultAsync(x => x.Id == userId);
                if(userDetails.UserType == UserType.EmroAdmin.ToString())
                {
                    audits = await _auditEventRepository.GetAllListAsync(x => x.ExecutionTime <= auditReportInput.To && x.ExecutionTime >= auditReportInput.From);
                }
                else
                {
                    audits = await _auditEventRepository.GetAllListAsync(x => (x.UserId == userId) && x.ExecutionTime <= auditReportInput.To && x.ExecutionTime >= auditReportInput.From);
                }

                if (audits.Count != 0)
                {
                    var list = (from au in audits.AsEnumerable()
                                join us in _userRepository.GetAll().AsEnumerable()
                                on au.UserId equals us.Id
                                select new AuditReportDto
                                {
                                    UserId = au.UserId,
                                    ExecutionTime = au.ExecutionTime,
                                    ExecutionDuration = au.ExecutionDuration,
                                    ImpersonatorUserId = au.ImpersonatorUserId,
                                    UniqueUserId = au.UniqueUserId,
                                    Operation = au.Operation,
                                    Component = au.Component,
                                    Action = au.Action,
                                    IsImpersonating = au.IsImpersonating,
                                    UserName = us.FullName,
                                    ImpersonatorUserName = au.ImpersonatorUserId != null ? _userRepository.FirstOrDefault(x => x.Id == au.ImpersonatorUserId).FullName : null

                                }).ToList();

                    auditReportOutput.Count = list.Count;

                    auditReportOutput.Items = list.OrderByDescending(x => x.ExecutionTime).ToList();
                    
                    if(auditReportOutput.Count > 0)
                    {
                        //Apply filters on audit report records
                        if (!String.IsNullOrEmpty(auditReportInput.SearchKeyword))
                        {
                            auditReportOutput.Items = auditReportOutput.Items.Where(x => x.UserName.ToLower().StartsWith(auditReportInput.SearchKeyword.ToLower()) || x.UserName.ToLower().EndsWith(auditReportInput.SearchKeyword.ToLower()) || x.UserName.ToLower().Contains(auditReportInput.SearchKeyword.ToLower()) ||
                                                        (x.Component.ToLower().StartsWith(auditReportInput.SearchKeyword.ToLower()) || x.Component.ToLower().EndsWith(auditReportInput.SearchKeyword.ToLower()) || x.Component.ToLower().Contains(auditReportInput.SearchKeyword.ToLower())) ||
                                                        (x.Action.ToLower().StartsWith(auditReportInput.SearchKeyword.ToLower()) || x.Action.ToLower().EndsWith(auditReportInput.SearchKeyword.ToLower()) || x.Action.ToLower().Contains(auditReportInput.SearchKeyword.ToLower())) ||
                                                        ( (x.ImpersonatorUserName != null && x.ImpersonatorUserName.ToLower().StartsWith(auditReportInput.SearchKeyword.ToLower()) ) || (x.ImpersonatorUserName != null && x.ImpersonatorUserName.ToLower().EndsWith(auditReportInput.SearchKeyword.ToLower()) ) || (x.ImpersonatorUserName != null && x.ImpersonatorUserName.ToLower().Contains(auditReportInput.SearchKeyword.ToLower()))
                                                        )).ToList();

                            auditReportOutput.Count = auditReportOutput.Items.Count;
                        }
                    }
                    //Apply pagination on audit report records
                    if (Convert.ToInt32(auditReportInput.limit) > 0 && Convert.ToInt32(auditReportInput.page) > 0)
                        auditReportOutput.Items = auditReportOutput.Items.Skip((Convert.ToInt32(auditReportInput.page) - 1) * Convert.ToInt32(auditReportInput.limit)).Take(Convert.ToInt32(auditReportInput.limit)).ToList();

                    auditReportOutput.Message = "Get Audit Log successfully.";
                    auditReportOutput.StatusCode = 200;
                }
                else
                {
                    auditReportOutput.Message = "No record found.";
                    auditReportOutput.StatusCode = 204;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Get Audit Log Error " + ex.StackTrace);

                auditReportOutput.Message = "Error while fetching Audit Log";
                auditReportOutput.StatusCode = 401;
            }

            return auditReportOutput;
        }

        /// <summary>
        /// Create Audit Event for the provided input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<CreateAuditEventOutput> CreateAuditEvents(CreateEventsInputDto input)
        {
            Guid sessionUser = Guid.Empty;
            CreateAuditEventOutput output = new CreateAuditEventOutput();
            try
            {
                if (_session.UniqueUserId != null)
                {
                    sessionUser = Guid.Parse(_session.UniqueUserId);
                }
                var auditEvent = new AuditEvent
                {
                    ExecutionTime = DateTime.UtcNow,
                    BrowserInfo = _clientInfoProvider.BrowserInfo,
                    ClientName = _clientInfoProvider.ComputerName,
                    ClientIpAddress = _clientInfoProvider.ClientIpAddress,
                    ExecutionDuration = input.ExecutionDuration,
                    Parameters = input.Parameters,
                    ImpersonatorUserId = AbpSession.ImpersonatorUserId,
                    TenantId = AbpSession.TenantId,
                    UserId = AbpSession.UserId,
                    ImpersonatorTenantId = AbpSession.ImpersonatorTenantId,
                    UniqueUserId = sessionUser,
                    Operation = input.Operation,
                    Component = input.Component,
                    Action = input.Action,
                    BeforeValue = string.IsNullOrEmpty(input.BeforeValue) ? null : input.BeforeValue,
                    AfterValue = string.IsNullOrEmpty(input.AfterValue) ? null : input.AfterValue,
                    IsImpersonating = AbpSession.ImpersonatorUserId == null ? false : true
                };

                output.Id = await _auditEventRepository.InsertAndGetIdAsync(auditEvent);
                output.Message = "Event saved successfully";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Create Audit Event Error:" + ex.StackTrace);
            }
            return output;

        }

    }
}
