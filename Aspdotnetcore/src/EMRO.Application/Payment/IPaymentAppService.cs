using Abp.Application.Services;
using EMRO.Payment.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Payment
{
    public interface IPaymentAppService : IApplicationService
    {
        Task<PaymentResultOutput> AppointmentPayment(StripeCardInput input);
        Task<PaymentDetailOutput> PaymentDetails(PaymentDetails input);
        Task<PaymentResultOutput> PaymentByPaypal(PayPalInput input);
        Task<PayPalDetailOutput> PayPalDetail(PaymentDetails input);

        Task<PaymentResultOutput> PayByDepositeAmount(PayByDepositeInput input);
    }
}
