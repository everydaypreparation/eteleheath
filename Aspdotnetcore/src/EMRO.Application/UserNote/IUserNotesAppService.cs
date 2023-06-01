using Abp.Application.Services;
using EMRO.UserNote.Dto;
using System;
using System.Threading.Tasks;

namespace EMRO.UserNote
{

    public interface IUserNotesAppService : IApplicationService
    {
        Task<UserNotesOutput> Create(UserNotesInput input);
        Task<UserNotesOutput> Update(UserNotesInput input);
        Task<UserNotesOutput> Get(string NotesId);
        Task<GetNotesList> GetAll(GetNoteListInput getNoteListInput);
        Task<UserNotesOutput> GetNotesForSamvaad(Guid AppointmentId, Guid UserId);
        Task<UserNotesOutput> Delete(string NotesId);
        Task<UserNotesOutput> CreateUpdateNotesForSamvaad(UserNotesInput input);
    }
}
