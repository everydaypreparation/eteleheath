using Abp.Data;
using Abp.EntityFrameworkCore;
using EMRO.Patients.IntakeForm;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.EntityFrameworkCore.Repositories
{
    public class PatientRepository : EMRORepositoryBase<UserConsentPatientsDetails, Guid>
    {
        private readonly IActiveTransactionProvider _transactionProvider;
        public PatientRepository(IDbContextProvider<EMRODbContext> dbContextProvider, IActiveTransactionProvider transactionProvider)
          : base(dbContextProvider)
        {
            _transactionProvider = transactionProvider;
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
