using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.Samvaad
{
    public interface IJoinMeeting : IApplicationService
    {
        string JoinSamvaadMeeting(string meetingID, string fullName);
    }
}
