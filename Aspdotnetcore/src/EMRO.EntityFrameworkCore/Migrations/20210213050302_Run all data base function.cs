using EMRO.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EMRO.Migrations
{
    public partial class Runalldatabasefunction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            #region Custom migrate sqls
            migrationBuilder.CreatePgCryptoFunctions_20200108053432();
           // migrationBuilder.InboxFunctions_20210119053432();
            //migrationBuilder.SentFunctions_20210119053432();
            //migrationBuilder.TrashFunctions_20210119053432();
           // migrationBuilder.UpdateMessageApiFunctionFunctions_20200108053432();
           // migrationBuilder.IssueMessageApiFunctionFunctions_20200108053432();
            //migrationBuilder.AddIssueMessageApiFunctionFunctions_20200108053432();
            migrationBuilder.InsertTimeZoneApiFunctionFunctions_20200108053432();
            //migrationBuilder.GetUserMessagesFunctionFunctions_20200108053432();
            #endregion
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
