using System.ComponentModel.DataAnnotations;

namespace EMRO.Users.Dto
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }

    public class ChangePasswordOutputDto
    {
        public bool Ischanged { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
