using Abp.Application.Services;
using EMRO.Documents.Dtos;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace EMRO.Documents
{
    public interface IDocumentAppService : IApplicationService
    {
        Task<DocumentOutput> Upload(CreateDocumentInput input);

        Task<DocumentOutput> Delete(Guid DocumentId);
        Task<GetDocumentsOutput> Get(Guid DocumentId);

        Task<DocumentOutput> ReUpload(CreateDocumentInput input);

        //void Update(UpdateDocumentInput input, IFormFile File);

    }
}
