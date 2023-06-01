using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.UserNote.Dto
{
    public class UserNotesInput
    {
        public string NotesId { get; set; }

        public string Notes { get; set; }
        public Guid UserId { get; set; }
        public Guid? AppointmentId { get; set; }
    }

    //public long Id { get; set; }
    //public string Message { get; set; }
    //public int StatusCode { get; set; }
}
