using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using EMRO.Appointment;
using EMRO.AppointmentSlot;
using EMRO.Common.MeetingStatistics.Dto;
using EMRO.MeetingLogs;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Common.MeetingStatistics
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize]
    public class MeetingLog : ApplicationService, IMeetingLog
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<MeetingLogDetails, Guid> _meetingLogDetails;
        private readonly IDoctorAppointmentRepository _doctorAppointmentRepository;
        private readonly IRepository<DoctorAppointmentSlot, Guid> _doctorAppointmentSlotRepository;
        public MeetingLog(IUnitOfWorkManager unitOfWorkManager,
            IRepository<MeetingLogDetails, Guid> meetingLogDetails,
            IRepository<DoctorAppointmentSlot, Guid> doctorAppointmentSlotRepository,
            IDoctorAppointmentRepository doctorAppointmentRepository)
        {
            _doctorAppointmentSlotRepository = doctorAppointmentSlotRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _meetingLogDetails = meetingLogDetails;
            _doctorAppointmentRepository = doctorAppointmentRepository;
        }
        public async Task<MeetingLogOutput> Create(MeetingLogInput input)
        {
            MeetingLogOutput meetingLogOutput = new MeetingLogOutput();
            try
            {
                Logger.Info("Creating Meeting Log for input: " + input);


                using (var unitOfWork = _unitOfWorkManager.Begin())
                {

                    int? TenantId = null;

                    if (AbpSession.TenantId.HasValue)
                    {
                        TenantId = AbpSession.TenantId.Value;
                    }

                    int count = _meetingLogDetails.GetAllList(x => x.MeetingId == input.MeetingId && x.UserId == input.UserId && x.AppointmentId == input.AppointmentId).Count;

                    if (count == 0)
                    {
                        var meetingLogDetail = new MeetingLogDetails
                        {
                            TenantId = TenantId,
                            UserId = input.UserId,
                            AppointmentId = input.AppointmentId,
                            MeetingId = input.MeetingId,
                            CreatedOn = DateTime.UtcNow
                        };
                        meetingLogOutput.MeetingLogId = await _meetingLogDetails.InsertAndGetIdAsync(meetingLogDetail);

                        /**
                         * Get meeting log basis of current meeting id, if it is more than 2 then
                         * appointment table --> field MissedAppointment, need to set to false
                         */
                        count = _meetingLogDetails.GetAllList(x => x.MeetingId == input.MeetingId).Count + 1;
                        if (count >= 2)
                        {
                            meetingLogOutput.Count = count;

                            var appointment = await _doctorAppointmentRepository.FirstOrDefaultAsync(x => x.meetingId == Convert.ToString(input.MeetingId));
                            var slot = await _doctorAppointmentSlotRepository.FirstOrDefaultAsync(x => x.Id == appointment.AppointmentSlotId);

                            BackgroundJob.Schedule<IMeetingLog>(x => x.updateAppointment(appointment.meetingId), slot.SlotZoneEndTime.TimeOfDay);
                        }
                        else
                        {
                            meetingLogOutput.Count = count;
                        }

                        meetingLogOutput.Message = "Meeting Log created successfully";
                        meetingLogOutput.StatusCode = 200;
                    }
                    else
                    {
                        meetingLogOutput.Message = "Meeting Log already exist";
                        meetingLogOutput.StatusCode = 409;
                    }

                    unitOfWork.Complete();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Create Meeting Log Error" + ex.StackTrace);

                meetingLogOutput.Message = "Error while creating Meeting Log.";
                meetingLogOutput.StatusCode = 401;
            }
            return meetingLogOutput;
        }

        public async Task updateAppointment(string meetingId)
        {
            try
            {
                var appointment = await _doctorAppointmentRepository.FirstOrDefaultAsync(x => x.meetingId == Convert.ToString(meetingId));
                var slot = await _doctorAppointmentSlotRepository.FirstOrDefaultAsync(x => x.Id == appointment.AppointmentSlotId);

                appointment.MissedAppointment = false;

                await _doctorAppointmentRepository.UpdateAsync(appointment);
            }
            catch (Exception ex)
            {
                Logger.Error("Update Appointment for missing meeting Error stack trace,  " + ex.StackTrace);
                Logger.Error("Update Appointment for missing meeting Error message,  " + ex.Message);
            }
        }

        /**
         * Not using any more
         */
        //private MeetingLogCountOutput Get(Guid MeetingId)
        //{
        //    MeetingLogCountOutput meetingLogCountOutput = new MeetingLogCountOutput();
        //    try
        //    {
        //        if (MeetingId != Guid.Empty)
        //        {

        //            int count = _meetingLogDetails.GetAllList(x => x.MeetingId == MeetingId).Count;
        //            if (count != 0)
        //            {
        //                meetingLogCountOutput.Count = count;
        //                meetingLogCountOutput.MeetingId = MeetingId;
        //                meetingLogCountOutput.Message = " Get Meeting Log details successfully.";
        //                meetingLogCountOutput.StatusCode = 200;
        //            }
        //            else
        //            {
        //                meetingLogCountOutput.Message = "No record found.";
        //                meetingLogCountOutput.StatusCode = 401;
        //            }
        //        }
        //        else
        //        {
        //            meetingLogCountOutput.Message = "Bad request.";
        //            meetingLogCountOutput.StatusCode = 401;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        meetingLogCountOutput.Message = "Someting went wrong.";
        //        meetingLogCountOutput.StatusCode = 500;
        //        Logger.Error("Get Meeting Logs" + ex.StackTrace);
        //    }
        //    return meetingLogCountOutput;
        //}
    }
}
