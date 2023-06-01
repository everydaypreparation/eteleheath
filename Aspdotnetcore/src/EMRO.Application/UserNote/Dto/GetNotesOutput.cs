using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.UserNote.Dto
{
    public class GetNotesOutput
    {
        public string Notes { get; set; }

        public string NoteDate { get; set; }
        public Guid NoteId { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? AppointmentId { get; set; }
    }

    public class GetNotesList
    {
        public List<GetNotesOutput> Items { get; set; }
        public int Count { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class GetNoteListInput
    {
        public Guid UserId { get; set; }
        public Guid AppointmentId { get; set; }
        public int limit { get; set; }
        public int page { get; set; }
    }

}
