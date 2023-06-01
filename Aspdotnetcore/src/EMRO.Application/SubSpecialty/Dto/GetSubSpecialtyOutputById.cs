using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.SubSpecialty.Dto
{
    public class GetSubSpecialtyOutputById
    {
        public Guid Id { get; set; }
        public string SubSpecialtyName { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
