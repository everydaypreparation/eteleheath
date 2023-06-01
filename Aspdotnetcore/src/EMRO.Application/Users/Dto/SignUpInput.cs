using Abp.Authorization.Users;
using Abp.Runtime.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.Users.Dto
{
    public class SignUpInput : IShouldNormalize
    {

        //[Required(ErrorMessage = "Please enter user name")]
        //[StringLength(AbpUserBase.MaxUserNameLength)]
       // public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter first name")]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter last name")]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Please enter Email Id")]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        public string[] RoleNames { get; set; }
        public void Normalize()
        {
            if (RoleNames == null)
            {
                RoleNames = new string[0];
            }
        }
    }
}
