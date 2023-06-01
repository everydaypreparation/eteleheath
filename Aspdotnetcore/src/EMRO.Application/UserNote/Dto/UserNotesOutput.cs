using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.UserNote.Dto
{
    public class UserNotesOutput
    {
        public Guid NotesId { get; set; }
        public string Notes { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }
}
