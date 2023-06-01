using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Common.CronJob
{
    public interface ICronJobAppService: IApplicationService
    {
        Task AppointmentNotification();
        Task UserAccountNotification();
        Task UserAccountNotificationEveryYear();
    }
}
