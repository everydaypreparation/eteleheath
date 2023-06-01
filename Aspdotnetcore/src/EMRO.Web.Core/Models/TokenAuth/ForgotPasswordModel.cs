using Abp.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.Models.TokenAuth
{
   public class ForgotPasswordModel
    {
        [Required]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        public string BaseUrl { get; set; }

    }

    public class ForgotPasswordModelOutput
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }

    }
}
