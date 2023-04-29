using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lowque.DataAccess.Migrations
{
    public partial class DataModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationDefinitions",
                columns: table => new
                {
                    ApplicationDefinitionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Template = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsTemplate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAccessModuleDefinition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentityModuleDefinition = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationDefinitions", x => x.ApplicationDefinitionId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "FlowDefinitions",
                columns: table => new
                {
                    FlowDefinitionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UseResourceIdentifier = table.Column<bool>(type: "bit", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationDefinitionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowDefinitions", x => x.FlowDefinitionId);
                    table.ForeignKey(
                        name: "FK_FlowDefinitions_ApplicationDefinitions_ApplicationDefinitionId",
                        column: x => x.ApplicationDefinitionId,
                        principalTable: "ApplicationDefinitions",
                        principalColumn: "ApplicationDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleUser",
                columns: table => new
                {
                    RolesRoleId = table.Column<int>(type: "int", nullable: false),
                    UsersUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.RolesRoleId, x.UsersUserId });
                    table.ForeignKey(
                        name: "FK_RoleUser_Roles_RolesRoleId",
                        column: x => x.RolesRoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_Users_UsersUserId",
                        column: x => x.UsersUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ApplicationDefinitions",
                columns: new[] { "ApplicationDefinitionId", "CreatedAt", "DataAccessModuleDefinition", "IdentityModuleDefinition", "IsTemplate", "Name", "Template" },
                values: new object[] { 1, new DateTime(2021, 10, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), "[{\"Name\":\"Process\",\"Properties\":[{\"Name\":\"ProcessId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"ProcessStatus\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"CreationDate\",\"Type\":\"DateAndTime\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"LastModificationDate\",\"Type\":\"DateAndTime\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"CandidateFirstName\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"CandidateLastName\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"CandidateFullName\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":101,\"List\":false},{\"Name\":\"CandidateEmail\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":100,\"List\":false},{\"Name\":\"CandidatePhoneNumber\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":20,\"List\":false},{\"Name\":\"CandidateAge\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"UserId\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"PreferredEmploymentDimension\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"PreferredTypeOfContract\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"PreferredSalary\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"PreferredStartDate\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":100,\"List\":false},{\"Name\":\"OfferedEmploymentDimension\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"OfferedTypeOfContract\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"OfferedSalary\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"OfferedStartDate\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":100,\"List\":false},{\"Name\":\"User\",\"Type\":\"User\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":false},{\"Name\":\"Documents\",\"Type\":\"Document\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true},{\"Name\":\"Meetings\",\"Type\":\"Meeting\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true},{\"Name\":\"Comments\",\"Type\":\"Comment\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true}]},{\"Name\":\"Meeting\",\"Properties\":[{\"Name\":\"MeetingId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"MeetingTime\",\"Type\":\"DateAndTime\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"ProcessId\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"IsOnline\",\"Type\":\"Binary\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"OfficeId\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Link\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":500,\"List\":false},{\"Name\":\"DidTakePlace\",\"Type\":\"Binary\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Process\",\"Type\":\"Process\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":false},{\"Name\":\"Office\",\"Type\":\"Office\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":false}]},{\"Name\":\"Document\",\"Properties\":[{\"Name\":\"DocumentId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Name\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"Type\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":1000,\"List\":false},{\"Name\":\"ProcessId\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Process\",\"Type\":\"Process\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":false}]},{\"Name\":\"Office\",\"Properties\":[{\"Name\":\"OfficeId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Name\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"Meetings\",\"Type\":\"Meeting\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true}]},{\"Name\":\"Comment\",\"Properties\":[{\"Name\":\"CommentId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Content\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"ProcessId\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"UserId\",\"Type\":\"IntegralNumber\",\"Key\":false,\"Required\":false,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"CreationTime\",\"Type\":\"DateAndTime\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Process\",\"Type\":\"Process\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":false},{\"Name\":\"User\",\"Type\":\"User\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":false}]},{\"Name\":\"User\",\"Properties\":[{\"Name\":\"UserId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Email\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":100,\"List\":false},{\"Name\":\"EmailConfirmed\",\"Type\":\"Binary\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"PasswordHash\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":100,\"List\":false},{\"Name\":\"FirstName\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"LastName\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"FullName\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":101,\"List\":false},{\"Name\":\"Roles\",\"Type\":\"Role\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true},{\"Name\":\"Comments\",\"Type\":\"Comment\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true},{\"Name\":\"Processes\",\"Type\":\"Process\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true}]},{\"Name\":\"Role\",\"Properties\":[{\"Name\":\"RoleId\",\"Type\":\"IntegralNumber\",\"Key\":true,\"Required\":true,\"Navigation\":false,\"MaxLength\":null,\"List\":false},{\"Name\":\"Name\",\"Type\":\"TextPhrase\",\"Key\":false,\"Required\":true,\"Navigation\":false,\"MaxLength\":50,\"List\":false},{\"Name\":\"Users\",\"Type\":\"User\",\"Key\":false,\"Required\":false,\"Navigation\":true,\"MaxLength\":null,\"List\":true}]}]", "{}", true, "HrApp_Template", null });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[] { 1, "Admin" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "EmailConfirmed", "PasswordHash" },
                values: new object[] { 1, "admin@lowque.pl", true, "AQAAAAEAACcQAAAAEHODCl8npca0JLvVFumIsB+TyOqbvoexBZ9YsEqtOyA7Dpy3mOa9OmPf0odb1NClyQ==" });

            migrationBuilder.CreateIndex(
                name: "IX_FlowDefinitions_ApplicationDefinitionId",
                table: "FlowDefinitions",
                column: "ApplicationDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UsersUserId",
                table: "RoleUser",
                column: "UsersUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlowDefinitions");

            migrationBuilder.DropTable(
                name: "RoleUser");

            migrationBuilder.DropTable(
                name: "ApplicationDefinitions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
