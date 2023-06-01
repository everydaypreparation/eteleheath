using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Patients.Dto
{
    public class DoctorSearchOutput
    {
        public List<DoctorOutput> Items { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class DoctorOutput
    {
        public string Title { get; set; }
        public string SpecialtyName { get; set; }
        public string ConsultantName { get; set; }
        public string HospitalName { get; set; }
        public Guid? ConsultantId { get; set; }
        public string profileUrl { get; set; }
        public TodaysAvailabilitySlotDto AvailabilitySlot { get; set; }
    }

    public class TodaysAvailabilitySlotDto
    {
        public string AvailabilityDate { get; set; }
        public List<Timeslots> timeslots { get; set; }

    }

    public class Timeslots
    {
        public string AvailabilityStartTime { get; set; }
        public string AvailabilityEndTime { get; set; }
        public string TimeZone { get; set; }

        public DateTime SlotStartTime { get; set; }
        public DateTime SlotEndTime { get; set; }
    }

}
