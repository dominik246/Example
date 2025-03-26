using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Example.AuthApi.Database.Migrations.AuthOutboxDb
{
    /// <inheritdoc />
    public partial class StateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordRecoveryStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    EmailTemplate = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EmailSubject = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordRecoveryStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegisterNewUserModelStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentState = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    EmailConfirmHash = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EmailTemplate = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EmailSubject = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisterNewUserModelStates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordRecoveryStates");

            migrationBuilder.DropTable(
                name: "RegisterNewUserModelStates");
        }
    }
}
