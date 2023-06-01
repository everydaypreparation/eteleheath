using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Consultant.Dto
{
    public class GetRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string DoctorFirstName { get; set; }
        public string DoctorLastName { get; set; }
        public string Hospital { get; set; }
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string DoctorEmailAddress { get; set; }
        public Guid Id { get; set; }
        public string Status { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOnboarded { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class GetRequestOutputDto
    {
        public List<GetRequestDto> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public int Count { get; set; }
    }

    public class GetRequestByIdDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string DoctorFirstName { get; set; }
        public string DoctorLastName { get; set; }
        public string Hospital { get; set; }
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string DoctorEmailAddress { get; set; }
        public Guid Id { get; set; }
        public string Status { get; set; }
        public bool IsCompleted { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class GetAllRequestInputDto
    {
        public int limit { get; set; }
        public int page { get; set; }
    }
}
