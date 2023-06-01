using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using EMRO.Authorization.Users;
using EMRO.Roles.Dto;
using EMRO.Users.Dto;

namespace EMRO.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task<ListResultDto<RoleDto>> GetRoles();

       
        Task ChangeLanguage(ChangeUserLanguageDto input);

        Task<ChangePasswordOutputDto> ChangePassword(ChangePasswordDto input);

        Task<SignUpOutput> SignUp(SignUpInput input);

        Task<GetUserDtoOutput> GetUserDetailsId(GetUserDto input);

        Task<UserDetailsListOutput> GetUserByRoles(GetUsetByRoleInput input);
      
        Task<UserDto> DeleteUser(GetUserDto input);
        Task<UserDto> IsUserStatus(GetUserDto input);

        Task<GetUserlistoutput> GetUserEmails(Guid? Id);

        Task<UserDto> CreateConsultant(CreateUserDto input);
        Task<UserDto> UpdateConsultant(UpdateUserDtoOutput input);

        Task<UserDto> CreatePatient(PatientCreateUserDto input);
        Task<UserDto> UpdatePatient(PatientUpdateUserDtoOutput input);

        Task<UserDto> CreateFamilyDoctor(FamilyCreateUserDto input);
        Task<UserDto> UpdateFamilyDoctor(FamilyUpdateUserDtoOutput input);

        Task<UserDto> CreateDiagnostic(DiagnosticCreateUserDto input);
        Task<UserDto> UpdateDiagnostic(DiagnosticUpdateUserDtoOutput input);

        Task<UserDto> CreateInsurance(InsuranceCreateUserDto input);
        Task<UserDto> UpdateInsurance(InsuranceUpdateUserDtoOutput input);

        Task<UserDto> CreateMedicalLegal(InsuranceCreateUserDto input);
        Task<UserDto> UpdateMedicalLegal(InsuranceUpdateUserDtoOutput input);

        Task<UserDto> GetById(Guid Id);

        Task<UserDashBoardOutputDto> DashBoard();
    }
}
