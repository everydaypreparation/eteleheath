using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EMRO.Sessions;
using EMRO.Specialty.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Specialty
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize]
    public class SpecialtyAppService: ApplicationService, ISpecialtyAppService
    {
        private readonly IRepository<SpecialtyMaster, Guid> _specialtyMasterRepository;
        private readonly EmroAppSession _session;
        /// <summary>
        ///In constructor, we can get needed classes/interfaces.
        ///They are sent here by dependency injection system automatically.
        /// </summary>
        public SpecialtyAppService(IRepository<SpecialtyMaster, Guid> specialtyMasterRepository, EmroAppSession session)
        {
            _specialtyMasterRepository = specialtyMasterRepository;
            _session = session;
        }
        public async Task<CreateSpecialtyOutput> Create(SpecialtyInputDto input)
        {
            CreateSpecialtyOutput createSpecialtyOutput = new CreateSpecialtyOutput();
            try
            {
                if (!string.IsNullOrEmpty(input.SpecialtyName))
                {
                    var specialty = new SpecialtyMaster
                    {
                        SpecialtyName = input.SpecialtyName,
                        CreatedOn = DateTime.UtcNow
                    };
                    specialty.TenantId = AbpSession.TenantId;
                    if (_session.UniqueUserId != null)
                    {
                        specialty.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    createSpecialtyOutput.Id = await _specialtyMasterRepository.InsertAndGetIdAsync(specialty);
                    createSpecialtyOutput.Message = "Specialty created successfully.";
                    createSpecialtyOutput.StatusCode = 200;
                }
                else
                {
                    createSpecialtyOutput.Message = "Specialty is required.";
                    createSpecialtyOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                createSpecialtyOutput.Message = "Someting went wrong.";
                createSpecialtyOutput.StatusCode = 500;
                Logger.Error("Create Specialty" + ex.StackTrace);
            }
            return createSpecialtyOutput;
        }

        public async Task<GetSpecialtyOutputDto> GetSpecialties()
        {
            GetSpecialtyOutputDto getSpecialtyOutputDto = new GetSpecialtyOutputDto();
            try
            {
                var list = await _specialtyMasterRepository.GetAllListAsync();
                if (list != null)
                {
                    getSpecialtyOutputDto.Items = (from l in list.AsEnumerable()
                                                    select new GetSpecialtyDto
                                                    {
                                                        Id = l.Id,
                                                        SpecialtyName = l.SpecialtyName
                                                    }).OrderBy(x=>x.SpecialtyName).ToList();
                    getSpecialtyOutputDto.Message = "Get list successfully.";
                    getSpecialtyOutputDto.StatusCode = 200;
                }
                else
                {
                    getSpecialtyOutputDto.Message = "No record found.";
                    getSpecialtyOutputDto.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {

                getSpecialtyOutputDto.Message = "Someting went wrong.";
                getSpecialtyOutputDto.StatusCode = 500;
                Logger.Error("Get Specialties" + ex.StackTrace);
            }
            return getSpecialtyOutputDto;
        }

        public async Task<GetOutputById> GetSpecialtybyId(Guid Id)
        {
            GetOutputById getOutputById = new GetOutputById();
            try
            {
                var list = await _specialtyMasterRepository.GetAsync(Id);
                if (list != null)
                {
                    getOutputById.SpecialityName = list.SpecialtyName;
                    getOutputById.Id = list.Id;
                    getOutputById.Message = "Get specialty Successfully.";
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
                Logger.Error("Get SpecialtybyId" + ex.StackTrace);
            }
            return getOutputById;
        }

        public async Task<CreateSpecialtyOutput> Update(UpdateSpecialtyInput input)
        {
            CreateSpecialtyOutput createSpecialtyOutput = new CreateSpecialtyOutput();
            try
            {
                var specialty = await _specialtyMasterRepository.GetAsync(input.Id);
                if (specialty != null)
                {
                    if (!string.IsNullOrEmpty(input.SpecialtyName))
                    {
                        specialty.SpecialtyName = input.SpecialtyName;
                        specialty.UpdatedOn = DateTime.UtcNow;

                        specialty.TenantId = AbpSession.TenantId;
                        if (_session.UniqueUserId != null)
                        {
                            specialty.UpdatedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        await _specialtyMasterRepository.UpdateAsync(specialty);
                        createSpecialtyOutput.Message = "Specialty updated successfully.";
                        createSpecialtyOutput.StatusCode = 200;
                    }
                    else
                    {
                        createSpecialtyOutput.Message = "No Specialty found.";
                        createSpecialtyOutput.StatusCode = 401;
                    }
                   
                }
                else
                {
                    createSpecialtyOutput.Message = "Specialty is required.";
                    createSpecialtyOutput.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                createSpecialtyOutput.Message = "Someting went wrong.";
                createSpecialtyOutput.StatusCode = 500;
                Logger.Error("Update Specialty" + ex.StackTrace);
            }
            return createSpecialtyOutput;
        }

        public async Task<CreateSpecialtyOutput> Delete(Guid Id)
        {
            CreateSpecialtyOutput createSpecilityOutput = new CreateSpecialtyOutput();
            try
            {
                var user = await _specialtyMasterRepository.GetAsync(Id);
                if (user != null)
                {
                    await _specialtyMasterRepository.DeleteAsync(user);
                    createSpecilityOutput.Message = "Specialty deleted successfully.";
                    createSpecilityOutput.StatusCode = 200;
                }
                else
                {
                    createSpecilityOutput.Message = "Specialty not found.";
                    createSpecilityOutput.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                createSpecilityOutput.Message = "Someting went wrong.";
                createSpecilityOutput.StatusCode = 500;
                Logger.Error("Delete Specialty" + ex.StackTrace);
            }
            return createSpecilityOutput;


        }
    }
}
