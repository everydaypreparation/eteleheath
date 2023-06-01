using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Authorization.Users
{
    public interface IUserRepository : IRepository<User, long>
    {
        Task<bool> CheckDuplicatePhone(string PhoneNumber, long UserId);
    }
}
