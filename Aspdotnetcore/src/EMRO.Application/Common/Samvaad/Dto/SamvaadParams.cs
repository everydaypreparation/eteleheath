using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.Common.Samvaad.Dto
{
    public class SamvaadParams
    {
        public string API_URL { get; set; }
        public string API_SECRET { get; set; }
        public string MODERATOR_PASS { get; set; }
        public string ATTENDEE_PASS { get; set; }
        public SamvaadMeetingParams samvaadMeetingParams { get; set; }
    }
}
