using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EMRO.ConfigurationCosts.Dtos
{
    public class CreateCostsInputDto
    {
        [Required]
        public string KeyName { get; set; }
        [Required]
        public string Value { get; set; }
    }

    public class UpdateCostsInputDto
    {
        public Guid CostId { get; set; }
        [Required]
        public string KeyName { get; set; }
        [Required]
        public string Value { get; set; }
    }


}
