using System.ComponentModel.DataAnnotations;

namespace EMRO.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}