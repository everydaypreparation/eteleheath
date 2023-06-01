using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.ConfigurationCosts.Dtos
{
   public class CreateCostsOutputDto
    {
        public Guid CostId { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class  GetListCostsOutputDto
    {
        public List<UpdateCostsInputDto> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class GetCostsOutputDto
    {
        public Guid CostId { get; set; }
        public string KeyName { get; set; }
        public string Value { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
