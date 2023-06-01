using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Users.Dto
{
    public class UserDashBoardOutputDto
    {
        public int Patient { get; set; }
        public int Consultant { get; set; }
        public int FamilyDoctor { get; set; }
        public int Diagnostic { get; set; }
        public int Insurance { get; set; }
        public int MedicalLegal { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
