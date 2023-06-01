using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using EMRO.ConsentFormsMasters.Dto;
using EMRO.Master;
using EMRO.Sessions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.ConsentFormsMasters
{
    [AbpAuthorize]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class ConsentFormsMasterAppService : ApplicationService, IConsentFormsMasterAppService
    {
        private readonly IRepository<ConsentFormsMaster, Guid> _consentFormsMasterRepository;
        private readonly EmroAppSession _session;
        /// <summary>
        ///In constructor, we can get needed classes/interfaces.
        ///They are sent here by dependency injection system automatically.
        /// </summary>
        public ConsentFormsMasterAppService(IRepository<ConsentFormsMaster, Guid> consentFormsMasterRepository, EmroAppSession session
            )
        {
            _consentFormsMasterRepository = consentFormsMasterRepository;
            _session = session;
        }

        /// <summary>
        /// Create conset form 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<CreateConsentFormOutput> Create(CreateConsentFormInput input)
        {
            CreateConsentFormOutput createConsentFormOutput = new CreateConsentFormOutput();
            try
            {
                if (!string.IsNullOrEmpty(input.Description))
                {
                    var consentFormsMaster = new ConsentFormsMaster
                    {
                        Title = input.Title,
                        Description = input.Description,
                        ShortDescription = input.ShortDescription,
                        SubTitle = input.SubTitle,
                        CreatedOn = DateTime.UtcNow,
                        IsActive = true,
                        Disclaimer = input.Disclaimer
                    };

                    if (_session.UniqueUserId != null)
                    {
                        consentFormsMaster.CreatedBy = Guid.Parse(_session.UniqueUserId);
                    }
                    consentFormsMaster.TenantId = AbpSession.TenantId;
                    Guid newId = await _consentFormsMasterRepository.InsertAndGetIdAsync(consentFormsMaster);
                    createConsentFormOutput.ConsentFormId = newId;
                    createConsentFormOutput.Message = "Conset Form data inserted successfully.";
                    createConsentFormOutput.StatusCode = 200;
                }
                else
                {
                    createConsentFormOutput.Message = "Description is required.";
                    createConsentFormOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {

                createConsentFormOutput.Message = ex.Message;
                createConsentFormOutput.StatusCode = 500;
                Logger.Error("Create Consent Form" + ex.StackTrace);
            }

            return createConsentFormOutput;
        }

        /// <summary>
        /// Delete consent form
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<CreateConsentFormOutput> Delete(Guid Id)
        {
            CreateConsentFormOutput createConsentFormOutput = new CreateConsentFormOutput();
            try
            {
                if (Id != null && Id != Guid.Empty)
                {
                    var data = await _consentFormsMasterRepository.GetAsync(Id);
                    if (data != null)
                    {
                        data.IsActive = false;
                        data.DeletedOn = DateTime.UtcNow;
                        if (_session.UniqueUserId != null)
                        {
                            data.DeletedBy = Guid.Parse(_session.UniqueUserId);
                        }
                        var result = await _consentFormsMasterRepository.UpdateAsync(data);
                        createConsentFormOutput.Message = "Conset form data deleted successfully.";
                        createConsentFormOutput.StatusCode = 200;
                    }
                    else
                    {
                        createConsentFormOutput.Message = "No record found.";
                        createConsentFormOutput.StatusCode = 401;
                    }
                }
                else
                {
                    createConsentFormOutput.Message = "Consent form id cannot be 0.";
                    createConsentFormOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {

                createConsentFormOutput.Message = ex.Message;
                createConsentFormOutput.StatusCode = 500;
                Logger.Error("update Consent Form" + ex.StackTrace);
            }

            return createConsentFormOutput;
        }

        /// <summary>
        /// Get all consent forms
        /// </summary>
        /// <returns></returns>
        public GetConsentFormOutputList GetAll()
        {
            GetConsentFormOutputList getConsentFormOutput = new GetConsentFormOutputList();
            try
            {
                var list = _consentFormsMasterRepository.GetAll().Where(x => x.IsActive == true);
                getConsentFormOutput.Items = list.Select(list =>
                    new Getconsentforms
                    {
                        Title = list.Title,
                        Description = list.Description,
                        SubTitle = list.SubTitle,
                        ShortDescription = list.ShortDescription,
                        ConsentFormsId = list.Id,
                        Disclaimer = list.Disclaimer

                    }).ToList();
                getConsentFormOutput.Message = "get List successfully.";
                getConsentFormOutput.StatusCode = 200;
            }
            catch (Exception ex)
            {
                getConsentFormOutput.Message = ex.Message;
                getConsentFormOutput.StatusCode = 500;
                Logger.Error("GetAll Consent Form" + ex.StackTrace);
            }

            return getConsentFormOutput;
        }

        /// <summary>
        /// Get consent form by id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public GetConsentFormOutput GetById(Guid Id)
        {
            GetConsentFormOutput getConsentFormOutput = new GetConsentFormOutput();
            try
            {
                if (Id != null && Id != Guid.Empty)
                {
                    var data = _consentFormsMasterRepository.GetAll().Where(x => x.IsActive == true && x.Id == Id).FirstOrDefault();
                    if (data != null)
                    {
                        getConsentFormOutput.Title = data.Title;
                        getConsentFormOutput.Description = data.Description;
                        getConsentFormOutput.ShortDescription = data.ShortDescription;
                        getConsentFormOutput.SubTitle = data.SubTitle;
                        getConsentFormOutput.ConsentFormsId = data.Id;
                        getConsentFormOutput.Disclaimer = data.Disclaimer;

                        getConsentFormOutput.Message = "Get dtailed successfully.";
                        getConsentFormOutput.StatusCode = 200;
                    }
                }
                else
                {
                    getConsentFormOutput.Message = "No record found.";
                    getConsentFormOutput.StatusCode = 500;
                }
            }
            catch (Exception ex)
            {
                getConsentFormOutput.Message = ex.Message;
                getConsentFormOutput.StatusCode = 500;
                Logger.Error("GetById Form" + ex.StackTrace);
            }
            return getConsentFormOutput;
        }

        /// <summary>
        /// Update consent form details
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<CreateConsentFormOutput> Update(UpdateConsentFormInput input)
        {
            CreateConsentFormOutput createConsentFormOutput = new CreateConsentFormOutput();
            try
            {
                if (input.ConsentFormsId != null)
                {

                    if (!string.IsNullOrEmpty(input.Description))
                    {

                        var data = await _consentFormsMasterRepository.GetAsync(input.ConsentFormsId);
                        if (data != null)
                        {
                            data.Title = !string.IsNullOrEmpty(input.Title) ? input.Title : data.Title;
                            data.Description = input.Description;
                            data.ShortDescription = !string.IsNullOrEmpty(input.ShortDescription) ? input.ShortDescription : data.ShortDescription;
                            data.SubTitle = !string.IsNullOrEmpty(input.SubTitle) ? input.SubTitle : data.SubTitle;
                            data.Id = data.Id;
                            data.UpdatedOn = DateTime.UtcNow;
                            data.Disclaimer = input.Disclaimer;
                            if (_session.UniqueUserId != null)
                            {
                                data.CreatedBy = Guid.Parse(_session.UniqueUserId);
                            }
                            var result = await _consentFormsMasterRepository.UpdateAsync(data);
                            createConsentFormOutput.Message = "Conset form data updated successfully.";
                            createConsentFormOutput.StatusCode = 200;
                        }
                        else
                        {
                            createConsentFormOutput.Message = "No record found.";
                            createConsentFormOutput.StatusCode = 401;
                        }

                    }
                    else
                    {
                        createConsentFormOutput.Message = "Description is required.";
                        createConsentFormOutput.StatusCode = 401;
                    }
                }
                else
                {
                    createConsentFormOutput.Message = "Consent form id cannot be 0.";
                    createConsentFormOutput.StatusCode = 401;
                }
            }
            catch (Exception ex)
            {

                createConsentFormOutput.Message = ex.Message;
                createConsentFormOutput.StatusCode = 500;
                Logger.Error("update Consent Form" + ex.StackTrace);
            }

            return createConsentFormOutput;
        }
    }
}
