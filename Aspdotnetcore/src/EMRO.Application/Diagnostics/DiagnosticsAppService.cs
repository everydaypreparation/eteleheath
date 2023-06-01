using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using EMRO.Appointment;
using EMRO.AppointmentSlot;
using EMRO.Authorization.Roles;
using EMRO.Authorization.Users;
using EMRO.Common;
using EMRO.Common.AuditReport;
using EMRO.Common.AuditReport.Dtos;
using EMRO.Common.Templates;
using EMRO.Diagnostics.Dtos;
using EMRO.DiagnosticsCases;
using EMRO.Email;
using EMRO.Sessions;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Diagnostics
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize]
    public class DiagnosticsAppService : ApplicationService, IDiagnosticsAppService
    {

        private readonly IDoctorAppointmentRepository _doctorAppointmentRepository;
        private readonly IRepository<DoctorAppointmentSlot, Guid> _doctorAppointmentSlotRepository;
        private readonly IRepository<DiagnosticsCase, Guid> _diagnosticsCaseRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly EmroAppSession _session;
        private readonly UserManager _userManager;
        private readonly IMailer _mailer;
        private readonly RoleManager _roleManager;
        private readonly TemplateAppService _templateAppService;
        private readonly IAuditReportAppService _auditReportAppService;

        public DiagnosticsAppService(
            IRepository<User, long> userRepository,
            EmroAppSession session,
            UserManager userManager,
             IMailer mailer,
              RoleManager roleManager,
             IRepository<DiagnosticsCase, Guid> diagnosticsCaseRepository,
             TemplateAppService templateAppService,
             IDoctorAppointmentRepository doctorAppointmentRepository,
             IRepository<DoctorAppointmentSlot, Guid> doctorAppointmentSlotRepository,
             IAuditReportAppService auditReportAppService)
        {
            _userRepository = userRepository;
            _session = session;
            _userManager = userManager;
            _mailer = mailer;
            _diagnosticsCaseRepository = diagnosticsCaseRepository;
            _roleManager = roleManager;
            _templateAppService = templateAppService;
            _doctorAppointmentRepository = doctorAppointmentRepository;
            _doctorAppointmentSlotRepository = doctorAppointmentSlotRepository;
            _auditReportAppService = auditReportAppService;
        }

        public async Task<GetCaseListOutputDto> ActivePatient(CaseInputDto input)
        {
            GetCaseListOutputDto output = new GetCaseListOutputDto();
            try
            {
                if (input.UserId != Guid.Empty)
                {
                    var caselist = await _diagnosticsCaseRepository.GetAllListAsync(x => x.IsCompleted == false && x.UserId == input.UserId);
                    if (caselist.Count > 0)
                    {
                        output.Items = (from cl in caselist.AsEnumerable()
                                        join u in _userRepository.GetAll().AsEnumerable()
                                        on cl.PatientId equals u.UniqueUserId
                                        join a in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                        on cl.PatientId equals a.UserId
                                        where a.IsBooked == 1
                                        join sl in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                        on a.AppointmentSlotId equals sl.Id
                                        orderby sl.SlotZoneEndTime descending
                                        select new GetCaseListDto
                                        {
                                            CaseId = cl.Id,
                                            PatientId = u.Id,
                                            PatientName = u.FullName,
                                            CreatedDate = cl.CreatedOn,
                                            PatientIdUuid = u.UniqueUserId,
                                            AppointmentId = a.Id
                                        }).GroupBy(x => x.PatientIdUuid).Select(r => r.First()).OrderBy(k => k.CreatedDate).ToList();

                        output.Count = output.Items.Count;
                        if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && output.Items.Count > 0)
                        {
                            output.Items = output.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                        }

                        output.Message = "Get Active patient list successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 200;
                    }
                }
                else
                {
                    output.Message = "Bad Request.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Archive Patient by diagnostic Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<GetCaseListOutputDto> ArchivePatient(CaseInputDto input)
        {
            GetCaseListOutputDto output = new GetCaseListOutputDto();
            try
            {
                if (input.UserId != Guid.Empty)
                {
                    var caselist = await _diagnosticsCaseRepository.GetAllListAsync(x => x.IsCompleted == true && x.UserId == input.UserId);
                    if (caselist.Count > 0)
                    {
                        output.Items = (from cl in caselist.AsEnumerable()
                                        join u in _userRepository.GetAll().AsEnumerable()
                                        on cl.PatientId equals u.UniqueUserId
                                        join a in _doctorAppointmentRepository.GetAll().AsEnumerable()
                                        on cl.PatientId equals a.UserId
                                        where a.IsBooked == 1
                                        join sl in _doctorAppointmentSlotRepository.GetAll().AsEnumerable()
                                        on a.AppointmentSlotId equals sl.Id
                                        orderby sl.SlotZoneEndTime descending
                                        select new GetCaseListDto
                                        {
                                            CaseId = cl.Id,
                                            PatientId = u.Id,
                                            PatientName = u.FullName,
                                            CreatedDate = cl.CreatedOn,
                                            PatientIdUuid = u.UniqueUserId,
                                            AppointmentId = a.Id
                                        }).GroupBy(x => x.PatientIdUuid).Select(r => r.First()).OrderBy(k => k.CreatedDate).ToList();

                        output.Count = output.Items.Count;
                        if (Convert.ToInt32(input.limit) > 0 && Convert.ToInt32(input.page) > 0 && output.Items.Count > 0)
                        {
                            output.Items = output.Items.Skip((Convert.ToInt32(input.page) - 1) * Convert.ToInt32(input.limit)).Take(Convert.ToInt32(input.limit)).ToList();
                        }

                        output.Message = "Get Archive patient list successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 200;
                    }
                }
                else
                {
                    output.Message = "Bad Request.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Archive Patient by diagnostic Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<DiagnosticDashboardOutputDto> Dashboard(CaseInputDto input)
        {
            DiagnosticDashboardOutputDto output = new DiagnosticDashboardOutputDto();
            try
            {
                //var caselist = await _diagnosticsCaseRepository.GetAllListAsync(x => x.IsCompleted == false && x.UserId == input.UserId);
                //var archivedlist = await _diagnosticsCaseRepository.GetAllListAsync(x => x.IsCompleted == true && x.UserId == input.UserId);
                var caselist = await ActivePatient(input);
                var archivedlist = await ArchivePatient(input);

                output.TotalCase = caselist.Items == null ? 0 : caselist.Items.Count;
                output.TotalPatient = (caselist.Items == null && archivedlist.Items == null) ? 0
                    : (caselist.Items != null && archivedlist.Items == null) ? caselist.Items.Count
                    : (caselist.Items == null && archivedlist.Items != null) ? archivedlist.Items.Count
                    : caselist.Items.Count + archivedlist.Items.Count;
                output.ReportDue = caselist.Items == null ? 0 : caselist.Items.Count;
                output.Message = "Get dashboard count successfully.";
                output.StatusCode = 200;

            }
            catch (Exception ex)
            {

                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Dashboard diagnostic Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<PatientSerchOutputDto> Delete(UpdateCaseInputDto input)
        {
            PatientSerchOutputDto output = new PatientSerchOutputDto();
            try
            {
                var stopwatch = Stopwatch.StartNew();
                if (input.CaseId != Guid.Empty)
                {
                    var record = await _diagnosticsCaseRepository.GetAsync(input.CaseId);
                    if (record != null)
                    {
                        record.IsDeleted = true;
                        record.Status = "Completed";
                        record.DeletedOn = DateTime.UtcNow;
                        if (_session.UniqueUserId != null)
                        {
                            record.DeletedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        output.Message = "Patient report deleted successfully.";
                        output.StatusCode = 200;

                        stopwatch.Stop();

                        var PatientId = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == record.PatientId);
                        //Log Audit Events 
                        CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
                        createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                        createEventsInput.Parameters = Utility.Serialize(record);
                        createEventsInput.Operation = "Case Id(" + PatientId.Id + ") case has been deleted successfully.";
                        createEventsInput.Component = "Diagnostics";
                        createEventsInput.Action = "Delete";

                        await _auditReportAppService.CreateAuditEvents(createEventsInput);
                    }
                    else
                    {
                        output.Message = "No reocrd found.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad Request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Update by diagnostic Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<PatientSerchOutputDto> Search(PatientSerchInputDto input)
        {
            PatientSerchOutputDto output = new PatientSerchOutputDto();
            try
            {
                if (input.PatientId > 0 && input.UserId != Guid.Empty)
                {
                    var user = _userRepository.GetAllIncluding(x => x.Roles).Where(x => x.Id == input.PatientId).FirstOrDefault();
                    if (user != null)
                    {
                        var roleIds = user.Roles.Select(x => x.RoleId).ToArray();

                        var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);
                        if (roles.Contains("PATIENT"))
                        {
                            var record = await _diagnosticsCaseRepository.FirstOrDefaultAsync(x => x.PatientId == user.UniqueUserId && x.UserId == input.UserId && x.IsCompleted == false && x.IsDeleted == false);
                            if (record != null)
                            {
                                output.Message = "This patient already exists in your active patient list. ";
                                output.StatusCode = 200;
                            }
                            else
                            {

                                var request = new DiagnosticsCase
                                {
                                    CreatedOn = DateTime.UtcNow,
                                    PatientId = user.UniqueUserId,
                                    UserId = input.UserId,
                                    IsCompleted = false,
                                    Status = "Pending"
                                };

                                if (_session.UniqueUserId != null)
                                {
                                    request.CreatedBy = Guid.Parse(_session.UniqueUserId);
                                }
                                request.TenantId = AbpSession.TenantId;
                                Guid result = await _diagnosticsCaseRepository.InsertAndGetIdAsync(request);
                                output.CaseId = result;
                                output.Message = "Patient details get successfully.please upload his report from active patient list.";
                                output.StatusCode = 200;
                            }
                        }
                        else
                        {
                            output.Message = "No user exists with provided Id.";
                            output.StatusCode = 401;
                        }
                    }
                    else
                    {
                        output.Message = "No user exists with provided Id.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad Request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Search patient by diagnostic Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<PatientSerchOutputDto> Update(UpdateCaseInputDto input)
        {
            PatientSerchOutputDto output = new PatientSerchOutputDto();
            CreateEventsInputDto createEventsInput = new CreateEventsInputDto();
            try
            {
                var stopwatch = Stopwatch.StartNew();
                if (input.CaseId != Guid.Empty)
                {
                    var record = await _diagnosticsCaseRepository.GetAsync(input.CaseId);
                    if (record != null)
                    {
                        //Create Audit log for before update
                        createEventsInput.BeforeValue = Utility.Serialize(record);

                        record.IsCompleted = true;
                        record.Status = "Completed";
                        record.UpdatedOn = DateTime.UtcNow;
                        if (_session.UniqueUserId != null)
                        {
                            record.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        output.Message = "Patient report archived successfully.";
                        output.StatusCode = 200;

                        stopwatch.Stop();                

                        var patient = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == record.PatientId);
                        var consultant = await _userRepository.FirstOrDefaultAsync(x => x.UniqueUserId == record.UserId);
                        string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                        if (patient != null && consultant != null)
                        {
                            //Log Audit Events
                            createEventsInput.ExecutionDuration = stopwatch.ElapsedMilliseconds;
                            createEventsInput.Parameters = Utility.Serialize(record); ;
                            createEventsInput.Operation = patient.Name + " " + patient.Surname + " report has been completed and upload it on ETeleHealth cloud by " + consultant.Name + " " + consultant.Surname;
                            createEventsInput.Component = "Diagnostics";
                            createEventsInput.Action = "Update";  
                            createEventsInput.AfterValue = Utility.Serialize(record); ;
                            await _auditReportAppService.CreateAuditEvents(createEventsInput);

                            string Name = patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase();
                            string message = consultant.Name.ToPascalCase() + " " + consultant.Surname.ToPascalCase() + "  has completed your report and upload it on ETeleHealth cloud.Please login on ETeleHealth cloud and check it in document section.";
                            string doctorbody = _templateAppService.GetTemplates(Name, message,"");
                            //string doctorbody = "Hello " + "<b>" + patient.Name.ToPascalCase() + " " + patient.Surname.ToPascalCase() + ",</b> <br /><br /> "
                            //           + consultant.Name.ToPascalCase() + " " + consultant.Surname.ToPascalCase() + "  has completed your report and upload it on emro cloud.Please login on emro cloud and check it in document section." + " <br /> " +
                            //           "Regards," + " <br />" + "EMRO Team";

                            BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(patient.EmailAddress, "Request for Test", doctorbody, adminmail));
                        }
                    }
                    else
                    {
                        output.Message = "No reocrd found.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad Request.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Update by diagnostic Error:" + ex.StackTrace);
            }
            return output;
        }
    }
}
