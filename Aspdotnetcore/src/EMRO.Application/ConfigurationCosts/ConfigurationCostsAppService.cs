using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EMRO.ConfigurationCosts.Dtos;
using EMRO.Master;
using EMRO.Sessions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.ConfigurationCosts
{
    [AbpAuthorize]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class ConfigurationCostsAppService : ApplicationService, IConfigurationCostsAppService
    {
        private readonly IRepository<CostConfiguration, Guid> _costConfigurationRepository;
        private readonly EmroAppSession _session;
        /// <summary>
        ///In constructor, we can get needed classes/interfaces.
        ///They are sent here by dependency injection system automatically.
        /// </summary>
        public ConfigurationCostsAppService(IRepository<CostConfiguration, Guid> costConfigurationRepository, EmroAppSession session
            )
        {
            _costConfigurationRepository = costConfigurationRepository;
            _session = session;
        }
        public async Task<CreateCostsOutputDto> Create(CreateCostsInputDto input)
        {
            CreateCostsOutputDto output = new CreateCostsOutputDto();
            try
            {

                var data = await _costConfigurationRepository.FirstOrDefaultAsync(x => x.KeyName.ToLower() == input.KeyName.ToString());

                if (data == null)
                {

                    var request = new CostConfiguration
                    {
                        KeyName = input.KeyName,
                        Value = input.Value,
                    };
                    if (_session.UniqueUserId != null)
                    {
                        request.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    request.TenantId = AbpSession.TenantId;
                    Guid newId = await _costConfigurationRepository.InsertAndGetIdAsync(request);
                    output.CostId = newId;
                    output.Message = "Data inserted successfully.";
                    output.StatusCode = 200;
                }
                else
                {
                    output.Message = "keyName can not be duplicate.";
                    output.StatusCode = 401;
                }

            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Create Cost Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<CreateCostsOutputDto> Delete(Guid CostId)
        {
            CreateCostsOutputDto output = new CreateCostsOutputDto();
            try
            {
                if (CostId != null && CostId != Guid.Empty)
                {
                    var data = await _costConfigurationRepository.GetAsync(CostId);
                    if (data != null)
                    {
                        data.IsDeleted = true;
                        data.DeletedOn = DateTime.UtcNow;
                        if (_session.UniqueUserId != null)
                        {
                            data.DeletedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        var result = await _costConfigurationRepository.UpdateAsync(data);
                        output.Message = "Data deleted successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad Request.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Delete Cost Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<GetCostsOutputDto> Get(Guid CostId)
        {
            GetCostsOutputDto output = new GetCostsOutputDto();
            try
            {
                if (CostId != null && CostId != Guid.Empty)
                {
                    var data = await _costConfigurationRepository.GetAsync(CostId);
                    if (data != null)
                    {
                        output.CostId = data.Id;
                        output.KeyName = data.KeyName;
                        output.Value = data.Value;
                        output.Message = "Get Data successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "No record found.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "Bad Request.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Get Cost Error:" + ex.StackTrace);
            }
            return output;
        }

        public async Task<GetListCostsOutputDto> GetAll()
        {
            GetListCostsOutputDto output = new GetListCostsOutputDto();
            try
            {
                var list = await _costConfigurationRepository.GetAllListAsync(x => x.IsDeleted == false);
                output.Items = list.Select(list =>
                    new UpdateCostsInputDto
                    {
                        Value = list.Value,
                        CostId = list.Id,
                        KeyName = list.KeyName
                    }).ToList();
                output.Message = "get List successfully.";
                output.StatusCode = 200;
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("GetAll Cost Error:" + ex.StackTrace);
            }
            return output;
        }

        //public async Task<GetCostsOutputDto> GetByRoleName(string RoleName)
        //{
        //    GetCostsOutputDto output = new GetCostsOutputDto();
        //    try
        //    {
        //        var data = await _costConfigurationRepository.FirstOrDefaultAsync(x => x.RoleName.ToLower() == RoleName.ToLower());
        //        if (data != null)
        //        {
        //            output.CostId = data.Id;
        //            output.ConsultationFee = data.ConsultationFee;
        //            output.BaseRate = data.BaseRate;
        //            output.PerPageRate = data.PerPageRate;
        //            output.RoleName = data.RoleName;
        //            output.UptoPages = data.UptoPages;
        //            output.Message = "Get Data successfully.";
        //            output.StatusCode = 200;
        //        }
        //        else
        //        {
        //            output.Message = "No record found.";
        //            output.StatusCode = 401;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        output.Message = "Something went wrong, please try again.";
        //        output.StatusCode = 500;
        //        Logger.Error("Get By Role Name Cost Error:" + ex.StackTrace);
        //    }
        //    return output;
        //}

        public async Task<CreateCostsOutputDto> Update(UpdateCostsInputDto input)
        {
            CreateCostsOutputDto output = new CreateCostsOutputDto();
            try
            {

                var data = await _costConfigurationRepository.GetAsync(input.CostId);
                if (data != null)
                {

                    var dataKeyname = await _costConfigurationRepository.FirstOrDefaultAsync(x => x.KeyName.ToLower() == input.KeyName.ToString() && x.Id != input.CostId);
                    if (dataKeyname == null)
                    {
                        data.Id = input.CostId;
                        data.KeyName = input.KeyName;
                        data.Value = input.Value;
                        output.Message = "Data Updated successfully.";
                        output.StatusCode = 200;
                    }
                    else
                    {
                        output.Message = "keyName can not be duplicate.";
                        output.StatusCode = 401;
                    }
                }
                else
                {
                    output.Message = "No record found.";
                    output.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {
                output.Message = "Something went wrong, please try again.";
                output.StatusCode = 500;
                Logger.Error("Update Cost Error:" + ex.StackTrace);
            }
            return output;
        }
    }
}
