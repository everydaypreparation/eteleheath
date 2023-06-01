using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.Models
{
    public class FileModel
    {
        [Required]
        public Guid Id { get; set; }
    }
}
