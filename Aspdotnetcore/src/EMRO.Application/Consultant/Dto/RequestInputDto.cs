using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.Consultant.Dto
{
    public class RequestInputDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
       
        public string Hospital { get; set; }
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string RoleName { get; set; }
       
    }

    public class RequestOutputDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class RequestUpdateInputDto
    {
        public Guid Id { get; set; }

        public bool IsCompleted { get; set; }
    }
}
