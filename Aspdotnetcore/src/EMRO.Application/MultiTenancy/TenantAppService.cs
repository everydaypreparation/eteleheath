using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using EMRO.Authorization;
using EMRO.Authorization.Roles;
using EMRO.Authorization.Users;
using EMRO.Editions;
using EMRO.Email;
using EMRO.MultiTenancy.Dto;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EMRO.MultiTenancy
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [AbpAuthorize(PermissionNames.Pages_Tenants)]
    public class TenantAppService : AsyncCrudAppService<Tenant, TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>, ITenantAppService
    {
        private readonly TenantManager _tenantManager;
        private readonly EditionManager _editionManager;
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IAbpZeroDbMigrator _abpZeroDbMigrator;
        private readonly IMailer _mailer;


        public TenantAppService(
                IRepository<Tenant, int> repository,
                TenantManager tenantManager,
                EditionManager editionManager,
                UserManager userManager,
                RoleManager roleManager,
                IAbpZeroDbMigrator abpZeroDbMigrator,
                IMailer mailer)
                : base(repository)
        {
            _tenantManager = tenantManager;
            _editionManager = editionManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _abpZeroDbMigrator = abpZeroDbMigrator;
            _mailer = mailer;
        }



        public override async Task<TenantDto> CreateAsync(CreateTenantDto input)
        {
            CheckCreatePermission();

            // Create tenant
            var tenant = ObjectMapper.Map<Tenant>(input);
            try
            {
                tenant.ConnectionString = input.ConnectionString.IsNullOrEmpty()
                    ? null
                    : SimpleStringCipher.Instance.Encrypt(input.ConnectionString);

                var defaultEdition = await _editionManager.FindByNameAsync(EditionManager.DefaultEditionName);
                if (defaultEdition != null)
                {
                    tenant.EditionId = defaultEdition.Id;
                }

                await _tenantManager.CreateAsync(tenant);
                await CurrentUnitOfWork.SaveChangesAsync(); // To get new tenant's id.

                // Create tenant database
                _abpZeroDbMigrator.CreateOrMigrateForTenant(tenant);

                // We are working entities of new tenant, so changing tenant filter
                using (CurrentUnitOfWork.SetTenantId(tenant.Id))
                {
                    string newPassword = User.CreateRandomPassword();
                    // Create static roles for new tenant
                    CheckErrors(await _roleManager.CreateStaticRoles(tenant.Id));

                    await CurrentUnitOfWork.SaveChangesAsync(); // To get static role ids

                    // Grant all permissions to admin role
                    var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
                    await _roleManager.GrantAllPermissionsAsync(adminRole);

                    // Create admin user for the tenant
                    var adminUser = User.CreateTenantAdminUser(tenant.Id, input.AdminEmailAddress);
                    await _userManager.InitializeOptionsAsync(tenant.Id);
                    CheckErrors(await _userManager.CreateAsync(adminUser, User.DefaultPassword));
                    //CheckErrors(await _userManager.CreateAsync(adminUser, newPassword));
                    await CurrentUnitOfWork.SaveChangesAsync(); // To get admin user's id

                    // Assign admin user to role!
                    CheckErrors(await _userManager.AddToRoleAsync(adminUser, adminRole.Name));
                    await CurrentUnitOfWork.SaveChangesAsync();
                    //string adminmail = _userRepository.GetAll().FirstOrDefault().EmailAddress;
                    string body = "Hello and welcome " + "<b>" + input.FirstName + " " + input.LastName + ",</b> <br /><br /> " + "Thank you for registering with ETeleHealth."
                                                         + " <br /><br /><br /> " + "DomainNme: " + input.AppDomain
                                                         + " <br /><br /><br /> " + "UserName: " + adminUser.UserName
                                                           + " <br /><br /><br /> " + "Password: " + User.DefaultPassword + " <br /><br />"
                                                         + " <br /><br /><br /> " + "Regards," + " <br />" + "ETeleHealth Team";
                    string subject = "ETeleHealth login credentials";
                    //BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.AdminEmailAddress, subject, body));
                    BackgroundJob.Enqueue<IMailer>(x => x.SendEmailAsync(input.AdminEmailAddress, subject, body, input.AdminEmailAddress));
                }
            }
            catch (Exception ex)
            {
                Logger.Info("Create Tenant Error:" + ex.StackTrace);
            }

            return MapToEntityDto(tenant);
        }


        protected override IQueryable<Tenant> CreateFilteredQuery(PagedTenantResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.TenancyName.Contains(input.Keyword) || x.Name.Contains(input.Keyword))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        }

        protected override void MapToEntity(TenantDto updateInput, Tenant entity)
        {
            // Manually mapped since TenantDto contains non-editable properties too.
            //entity.Name = updateInput.Name;
            //entity.TenancyName = updateInput.TenancyName;
            //entity.IsActive = updateInput.IsActive;

            entity.Name = updateInput.Name == null ? entity.Name : updateInput.Name;
            entity.TenancyName = updateInput.TenancyName == null ? entity.TenancyName : updateInput.TenancyName;
            entity.IsActive = updateInput.IsActive is true ? entity.IsActive : updateInput.IsActive;
            entity.FirstName = updateInput.FirstName == null ? entity.FirstName : updateInput.FirstName;
            entity.LastName = updateInput.LastName == null ? entity.LastName : updateInput.LastName;
            entity.Phone = updateInput.Phone == null ? entity.Phone : updateInput.Phone;
            entity.Address1 = updateInput.Address1 == null ? entity.Address1 : updateInput.Address1;
            entity.Address2 = updateInput.Address2 == null ? entity.Address2 : updateInput.Address2;
            entity.AppDomain = updateInput.AppDomain == null ? entity.AppDomain : updateInput.AppDomain;
            entity.Description = updateInput.Description == null ? entity.Description : updateInput.Description;
            entity.Type = updateInput.Type == null ? entity.Type : updateInput.Type;
            entity.Country = updateInput.Country== null ? entity.Country : updateInput.Country;
            entity.State = updateInput.State == null ? entity.State : updateInput.State;
            entity.City = updateInput.City == null ? entity.City : updateInput.City;
            entity.PostalCode = updateInput.PostalCode == null ? entity.PostalCode : updateInput.PostalCode;

        }

        public override async Task DeleteAsync(EntityDto<int> input)
        {
            CheckDeletePermission();

            var tenant = await _tenantManager.GetByIdAsync(input.Id);
            await _tenantManager.DeleteAsync(tenant);
        }

        private void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}

