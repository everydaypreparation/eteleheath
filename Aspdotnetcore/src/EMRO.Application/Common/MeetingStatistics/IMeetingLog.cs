using Abp.Application.Services;
using EMRO.Common.MeetingStatistics.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Common.MeetingStatistics
{
    public interface IMeetingLog : IApplicationService
    {
        Task<MeetingLogOutput> Create(MeetingLogInput input);
        Task updateAppointment(string meetingId);
    }
}
