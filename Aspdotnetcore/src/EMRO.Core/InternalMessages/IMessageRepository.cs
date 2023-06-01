using Abp.Domain.Repositories;
using EMRO.InternalMessages.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.InternalMessages
{
    public interface IMessageRepository : IRepository<UserMessages, Guid>
    {
        Task<List<GetMessageListDtoResult>> GetInboxAsync(string UserId);
        Task<List<GetMessageListDtoResult>> GetSentAsync(string UserId);
        Task<List<GetMessageListDtoResult>> GetTrashAsync(string UserId);
        Task<List<GetMessageListDtoResult>> GetMessagesAsync(string FromUserId, string ToUserId);
    }
}
