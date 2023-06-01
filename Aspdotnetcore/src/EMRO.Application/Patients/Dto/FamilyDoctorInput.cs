using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Patients.Dto
{
    public class FamilyDoctorInput
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid UserId { get; set; }
        public bool IsPatient { get; set; }
    }

    public class FamilyDoctorOutput
    {

        public List<FamliyDoctorlist> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class FamliyDoctorlist
    {
        public string Title { get; set; }
        public string DoctorFirstName { get; set; }
        public string DoctorLastName { get; set; }
        public string DoctorAddress1 { get; set; }
        public string DoctorAddress2 { get; set; }
        public string DoctorCity { get; set; }
        public string DoctorState { get; set; }
        public string DoctorCountry { get; set; }
        public string DoctorPostalCodes { get; set; }
        public string DoctorTelePhone { get; set; }
        public string DoctorEmailID { get; set; }
        public string DoctorDOB { get; set; }
        public string DoctorGender { get; set; }

        public Guid? FamilyDoctorId { get; set; }
    }
}
