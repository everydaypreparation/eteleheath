using Abp.Data;
using Abp.EntityFrameworkCore;
using EMRO.Authorization.Users;
using EMRO.CommonSetting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.EntityFrameworkCore.Repositories
{
    public class UserRepository : EMRORepositoryBase<User, long>, IUserRepository
    {
        private readonly IActiveTransactionProvider _transactionProvider;
        private readonly DataEncryptionSetting _dataEncryptionSetting;
        public UserRepository(IDbContextProvider<EMRODbContext> dbContextProvider, IActiveTransactionProvider transactionProvider, IOptions<DataEncryptionSetting> dataEncryptionSetting)
           : base(dbContextProvider)
        {
            _transactionProvider = transactionProvider;
            _dataEncryptionSetting = dataEncryptionSetting.Value;
        }

        public async Task<bool> CheckDuplicatePhone(string PhoneNumber, long UserId)
        {
            bool IsDuplicate = false;
            await EnsureConnectionOpenAsync();
            using (var command = CreateCommand("SELECT" + '"' + "PhoneNumber" + '"' + "FROM  public." + '"' + "Users" + '"' + " Where public." + '"' + "emro_sym_decrypt" + '"' + "(" + '"' + "PhoneNumber" + '"' + "," + "'" + _dataEncryptionSetting.SecretKey + "'" + "," + "'" + _dataEncryptionSetting.Algotype + "'" + ") =" + "'" + PhoneNumber + "'" + " AND " + '"' + "Id" + '"' + "!=" + UserId, CommandType.Text))
            {
                using (var dataReader = await command.ExecuteReaderAsync())
                {
                    var result = new List<string>();
                    while (dataReader.HasRows)
                    {
                        IsDuplicate = true;
                        return IsDuplicate;
                    }
                }
            }
            return IsDuplicate;
        }

        private DbCommand CreateCommand(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            var command = Context.Database.GetDbConnection().CreateCommand();

            command.CommandText = commandText;
            command.CommandType = commandType;
            command.Transaction = GetActiveTransaction();

            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            return command;
        }

        private async Task EnsureConnectionOpenAsync()
        {
            var connection = Context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
        }

        private DbTransaction GetActiveTransaction()
        {
            return (DbTransaction)_transactionProvider.GetActiveTransaction(new ActiveTransactionProviderArgs
                    {
                        {"ContextType", typeof(EMRODbContext) },
                        {"MultiTenancySide", MultiTenancySide }
                    });
        }
    }
}
