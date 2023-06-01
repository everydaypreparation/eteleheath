using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMRO.UserNote
{
    public interface IUserNotesRepository : IRepository<UserNotes, Guid>
    {
    }
}
