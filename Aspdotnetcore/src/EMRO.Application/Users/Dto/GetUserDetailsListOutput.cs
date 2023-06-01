using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Users.Dto
{
    public class GetUserDetailsListOutput
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Surname { get; set; }
        public Guid UserId { get; set; }
        public bool IsActive { get; set; }
        public virtual string Locale { get; set; }
        public virtual string Timezone { get; set; }
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }
        public string CityId { get; set; }
        public string StateId { get; set; }
        public string CountryId { get; set; }
        public string PostalCodeId { get; set; }
        public virtual string DateOfBirth { get; set; }
        public virtual string Gender { get; set; }
        public virtual string UserType { get; set; }
        public virtual string Title { get; set; }
        public string OncologySpecialty { get; set; }
        public string OncologySubSpecialty { get; set; }
        public string LicensingNumber { get; set; }
        public string Choice1 { get; set; }
        public string Choice2 { get; set; }
        public string Company { get; set; }
        public string RequestedOncologySubspecialty { get; set; }

    }

    public class UserDetailsListOutput
    {
        public List<GetUserDetailsListOutput> getUserDetailsListOutputs { get; set; }

        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
