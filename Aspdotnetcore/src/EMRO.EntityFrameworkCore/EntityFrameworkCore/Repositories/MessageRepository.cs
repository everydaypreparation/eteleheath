using Abp.Data;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore;
using EMRO.CommonSetting;
using EMRO.InternalMessages;
using EMRO.InternalMessages.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.EntityFrameworkCore.Repositories
{
    public class MessageRepository : EMRORepositoryBase<UserMessages, Guid>, IMessageRepository
    {
        private readonly IActiveTransactionProvider _transactionProvider;
        private readonly IRepository<UserMessagesAttachment, Guid> _userMessagesAttachmentRepository;
        private readonly DataEncryptionSetting _dataEncryptionSetting;
        public MessageRepository(IDbContextProvider<EMRODbContext> dbContextProvider, IActiveTransactionProvider transactionProvider,
            IRepository<UserMessagesAttachment, Guid> userMessagesAttachmentRepository
            , IOptions<DataEncryptionSetting> dataEncryptionSetting)
           : base(dbContextProvider)
        {
            _transactionProvider = transactionProvider;
            _userMessagesAttachmentRepository = userMessagesAttachmentRepository;
            _dataEncryptionSetting = dataEncryptionSetting.Value;
        }

        public async Task<List<GetMessageListDtoResult>> GetInboxAsync(string UserId)
        {
            var result = new List<GetMessageListDtoResult>();
            await EnsureConnectionOpenAsync();
            using (var command = CreateCommand("SELECT * FROM  public." + '"' + "usp_GetInbox" + '"' + "(" + "'" + UserId + "'" + "," + "'" + _dataEncryptionSetting.SecretKey + "'" + "," + "'" + _dataEncryptionSetting.Algotype + "'" + ")", CommandType.Text))
            {
                using (var dataReader = await command.ExecuteReaderAsync())
                {

                    while (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            result.Add(
                                new GetMessageListDtoResult
                                {
                                    FirstName = dataReader["firstname"].ToString(),
                                    LastName = dataReader["lastname"].ToString(),
                                    MessageId = (Guid)dataReader["messageid"],
                                    Subject = dataReader["subject"].ToString(),
                                    MessagesText = dataReader["messagestext"].ToString(),
                                    SenderUserIds = dataReader["senderuserids"].ToString(),
                                    ReceiverUserIds = dataReader["receiveruserids"].ToString(),
                                    MailDateTime = Convert.ToDateTime(dataReader["createdon"]).ToString(),
                                    EmailId = dataReader["emailaddress"].ToString(),
                                    ReadBy = dataReader["readby"].ToString()
                                });
                        }
                        dataReader.NextResult();
                    }

                }
            }

            return result.ToList();
        }

        public async Task<List<GetMessageListDtoResult>> GetMessagesAsync(string FromUserId, string ToUserId)
        {
            var result = new List<GetMessageListDtoResult>();
            await EnsureConnectionOpenAsync();
            using (var command = CreateCommand("SELECT * FROM  public." + '"' + "usp_GetMessages" + '"' + "(" + "'" + FromUserId + "'" + "," + "'" + ToUserId + "'" + "," + "'" + _dataEncryptionSetting.SecretKey + "'" + "," + "'" + _dataEncryptionSetting.Algotype + "'" + ")", CommandType.Text))
            {
                using (var dataReader = await command.ExecuteReaderAsync())
                {

                    while (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            result.Add(
                                new GetMessageListDtoResult
                                {
                                    FirstName = dataReader["firstname"].ToString(),
                                    LastName = dataReader["lastname"].ToString(),
                                    MessageId = (Guid)dataReader["messageid"],
                                    Subject = dataReader["subject"].ToString(),
                                    MessagesText = dataReader["messagestext"].ToString(),
                                    SenderUserIds = dataReader["senderuserids"].ToString(),
                                    ReceiverUserIds = dataReader["receiveruserids"].ToString(),
                                    MailDateTime = Convert.ToDateTime(dataReader["createdon"]).ToString(),
                                    EmailId = dataReader["emailaddress"].ToString(),
                                    ReadBy = dataReader["readby"].ToString(),
                                    AppointmentId =  Convert.IsDBNull(dataReader["appointmentid"]) ? null : (Guid?)dataReader["appointmentid"]
                                });
                        }
                        dataReader.NextResult();
                    }

                }
            }

            return result.ToList();
        }

        public async Task<List<GetMessageListDtoResult>> GetSentAsync(string UserId)
        {
            var result = new List<GetMessageListDtoResult>();
            await EnsureConnectionOpenAsync();
            using (var command = CreateCommand("SELECT * FROM  public." + '"' + "usp_GetSent" + '"' + "(" + "'" + UserId + "'" + "," + "'" + _dataEncryptionSetting.SecretKey + "'" + "," + "'" + _dataEncryptionSetting.Algotype + "'" + ")", CommandType.Text))
            {
                using (var dataReader = await command.ExecuteReaderAsync())
                {

                    while (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            result.Add(
                                new GetMessageListDtoResult
                                {
                                    FirstName = dataReader["firstname"].ToString(),
                                    LastName = dataReader["lastname"].ToString(),
                                    MessageId = (Guid)dataReader["messageid"],
                                    Subject = dataReader["subject"].ToString(),
                                    MessagesText = dataReader["messagestext"].ToString(),
                                    SenderUserIds = dataReader["senderuserids"].ToString(),
                                    ReceiverUserIds = dataReader["receiveruserids"].ToString(),
                                    MailDateTime = Convert.ToDateTime(dataReader["createdon"]).ToString(),
                                    EmailId = dataReader["emailaddress"].ToString(),
                                });
                        }
                        dataReader.NextResult();
                    }

                }
            }

            return result.ToList();
        }

        public async Task<List<GetMessageListDtoResult>> GetTrashAsync(string UserId)
        {
            var result = new List<GetMessageListDtoResult>();
            await EnsureConnectionOpenAsync();
            using (var command = CreateCommand("SELECT * FROM  public." + '"' + "usp_GetTrash" + '"' + "(" + "'" + UserId + "'" + "," + "'" + _dataEncryptionSetting.SecretKey + "'" + "," + "'" + _dataEncryptionSetting.Algotype + "'" + ")", CommandType.Text))
            {
                using (var dataReader = await command.ExecuteReaderAsync())
                {

                    while (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            result.Add(
                                new GetMessageListDtoResult
                                {
                                    FirstName = dataReader["firstname"].ToString(),
                                    LastName = dataReader["lastname"].ToString(),
                                    MessageId = (Guid)dataReader["messageid"],
                                    Subject = dataReader["subject"].ToString(),
                                    MessagesText = dataReader["messagestext"].ToString(),
                                    SenderUserIds = dataReader["senderuserids"].ToString(),
                                    ReceiverUserIds = dataReader["receiveruserids"].ToString(),
                                    MailDateTime = Convert.ToDateTime(dataReader["createdon"]).ToString(),
                                    EmailId = dataReader["emailaddress"].ToString(),
                                });
                        }
                        dataReader.NextResult();
                    }

                }
            }

            return result.ToList();
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
