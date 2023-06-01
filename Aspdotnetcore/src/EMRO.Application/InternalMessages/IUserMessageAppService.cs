using Abp.Application.Services;
using EMRO.InternalMessages.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.InternalMessages
{
    public interface IUserMessageAppService : IApplicationService
    {
        Task<OutputUserMessageDto> Draft(DraftMessageInput input);
        Task<OutputUserMessageDto> Send(CreateUserMessagesDto input);
        //Task<OutputUserMessageDto> Update(UpdateUserMessagesDto input);

        //Task<OutputUserMessageDto> GetById(long UserId);
        Task<OutputUserMessageDto> Delete(DeleteMessageInput input);
        Task<UploadAttachmentDto> UploadAttachment(UploadAttachmentInputDto input);
        Task<UploadAttachmentDto> DeleteAttachment(Guid AttachmentId);
        Task<OutputUserMessageDto> ReadBy(ReadByInputDto input);
        Task<OutputUserMessageDto> Restore(RestoreInputDto input);
        Task<GetMessageDto> GetById(ReadByInputDto input);
        Task<GetMessageListOutput> IndoxAsync(GetMessageInput input);
        Task<GetMessageListOutput> Sent(GetMessageInput input);
        Task<GetMessageListOutput> Thrash(GetMessageInput input);
        GetMessageListOutput Drafts(GetMessageInput input);
        Task<GetAttachments> GetDownloadAttachment(Guid AttachmentId);
        Task<GetMessageListOutput> GetUserMessages(GetUsersMessageInput input);


    }
}
