using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using EMRO.Authorization.Users;

namespace EMRO.Users.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UserDto : EntityDto<long>
    {
        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        public bool IsActive { get; set; }

        public string FullName { get; set; }

        public DateTime? LastLoginTime { get; set; }

        public DateTime CreationTime { get; set; }

        public string[] RoleNames { get; set; }

        //## Pooja: 14/12/2020 - Start: Add new properties (Locale,TimeZone) in existing user table
        public virtual string Timezone { get; set; }
        //## Pooja: 14/12/2020 - End: Add new properties (Locale,TimeZone) in existing user table
        public string password { get; set; }
        public Guid UniqueUserId { get; set; }
        public string ProfileUrl { get; set; }
        public Guid AppoinmentId { get; set; }
        public Guid DoctorId { get; set; }
        public bool IsPayment { get; set; }
        public bool IsIntake { get; set; }
        public bool IsAppointment { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public double DepositAmount { get; set; }

        public bool IsAllowtoNewBooking { get; set; }
        public bool IsMissedAppointment { get; set; }

        public int NumberofPages { get; set; }

        public bool IsBookLater { get; set; }
        public bool IsCase { get; set; }
    }
}
