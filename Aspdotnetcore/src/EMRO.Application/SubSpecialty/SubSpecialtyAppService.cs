using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using EMRO.Sessions;
using EMRO.SubSpecialty.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.SubSpecialty
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize]
    public class SubSpecialtyAppService : ApplicationService, ISubSpecialtyAppService
    {
        private readonly IRepository<SubSpecialtyMaster, Guid> _subSpecialtyMasterRepository;
        private readonly EmroAppSession _session;
        /// <summary>
        ///In constructor, we can get needed classes/interfaces.
        ///They are sent here by dependency injection system automatically.
        /// </summary>
        public SubSpecialtyAppService(IRepository<SubSpecialtyMaster, Guid> subSpecialtyMasterRepository, EmroAppSession session)
        {
            _subSpecialtyMasterRepository = subSpecialtyMasterRepository;
            _session = session;
        }
        public async Task<SubSpecialtyOutput> Create(CreateInputDto input)
        {
            SubSpecialtyOutput output = new SubSpecialtyOutput();
            try
            {
                if (!string.IsNullOrEmpty(input.SubSpecialtyName))
                {
                    var subSpecialty = new SubSpecialtyMaster
                    {
                        SubSpecialityName = input.SubSpecialtyName.Trim(),
                        CreatedOn = DateTime.UtcNow
                    };
                    subSpecialty.TenantId = AbpSession.TenantId;
                    if (_session.UniqueUserId != null)
                    {
                        subSpecialty.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    output.Id = await _subSpecialtyMasterRepository.InsertAndGetIdAsync(subSpecialty);
                    output.Message = "SubSpecialty created successfully.";
                    output.StatusCode = 200;
                }
                else
                {
                    output.Message = "SubSpecialty is required.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Create SubSpecialty" + ex.StackTrace);
            }
            return output;
        }

        public async Task<SubSpecialtyOutput> Delete(Guid Id)
        {
            SubSpecialtyOutput output = new SubSpecialtyOutput();
            try
            {
                var user = await _subSpecialtyMasterRepository.GetAsync(Id);
                if (user != null)
                {
                    await _subSpecialtyMasterRepository.DeleteAsync(user);
                    output.Message = "SubSpecialty deleted successfully.";
                    output.StatusCode = 200;
                }
                else
                {
                    output.Message = "SubSpecialty not found.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Delete SubSpecialty" + ex.StackTrace);
            }
            return output;
        }

        public async Task<SubSpecialtyOutput> Update(UpdateInputDto input)
        {
            SubSpecialtyOutput output = new SubSpecialtyOutput();
            try
            {
                var subSpecialty = await _subSpecialtyMasterRepository.GetAsync(input.Id);
                if (subSpecialty != null)
                {
                    if (!string.IsNullOrEmpty(input.SubSpecialtyName))
                    {

                        subSpecialty.SubSpecialityName = input.SubSpecialtyName;
                        subSpecialty.UpdatedOn = DateTime.UtcNow;

                        subSpecialty.TenantId = AbpSession.TenantId;
                        if (_session.UniqueUserId != null)
                        {
                            subSpecialty.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _subSpecialtyMasterRepository.UpdateAsync(subSpecialty);
                        output.Message = "SubSpecialty updated successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "SubSpecialty is required.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "No SubSpecialty found.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Someting went wrong.";
                output.StatusCode = 500;
                Logger.Error("Update SubSpecialty" + ex.StackTrace);
            }
            return output;
        }

        public async Task<GetSubSpecialtyOutputById> GetSubSpecialtybyId(Guid Id)
        {
            GetSubSpecialtyOutputById getOutputById = new GetSubSpecialtyOutputById();
            try
            {
                var list = await _subSpecialtyMasterRepository.GetAsync(Id);
                if (list != null)
                {
                    getOutputById.SubSpecialtyName = list.SubSpecialityName;
                    getOutputById.Id = list.Id;
                    getOutputById.Message = "Get subspecialty successfully.";
                    getOutputById.StatusCode = 200;
                }
                else
                {
                    getOutputById.Message = "No record found.";
                    getOutputById.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                getOutputById.Message = ex.Message;
                getOutputById.StatusCode = 500;
                Logger.Error("Get SubSpecialtybyId" + ex.StackTrace);
            }
            return getOutputById;
        }

        public async Task<GetListOutput> GetSubSpecialties()
        {
            GetListOutput Subspecialtylist = new GetListOutput();
            try
            {
                var list = await _subSpecialtyMasterRepository.GetAllListAsync();
                if (list != null)
                {
                    Subspecialtylist.Items = (from l in list.AsEnumerable()
                                              select new UpdateInputDto
                                              {
                                                  Id = l.Id,
                                                  SubSpecialtyName = l.SubSpecialityName
                                              }).OrderBy(x => x.SubSpecialtyName).ToList();
                    Subspecialtylist.Message = "Get list successfully.";
                    Subspecialtylist.StatusCode = 200;

                }
                else
                {
                    Subspecialtylist.Message = "No record found.";
                    Subspecialtylist.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Get Sub Specialties" + ex.StackTrace);
                Subspecialtylist.Message = "Someting went wrong.";
                Subspecialtylist.StatusCode = 500;

            }
            return Subspecialtylist;
        }
    }
}
