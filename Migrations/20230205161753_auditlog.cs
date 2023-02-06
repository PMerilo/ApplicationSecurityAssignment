using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicationSecurityAssignment.Migrations
{
    /// <inheritdoc />
    public partial class auditlog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreviousPassword_AspNetUsers_ApplicationUserId",
                table: "PreviousPassword");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PreviousPassword",
                table: "PreviousPassword");

            migrationBuilder.RenameTable(
                name: "PreviousPassword",
                newName: "PreviousPasswords");

            migrationBuilder.RenameIndex(
                name: "IX_PreviousPassword_ApplicationUserId",
                table: "PreviousPasswords",
                newName: "IX_PreviousPasswords_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PreviousPasswords",
                table: "PreviousPasswords",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ApplicationUserId",
                table: "AuditLogs",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreviousPasswords_AspNetUsers_ApplicationUserId",
                table: "PreviousPasswords",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreviousPasswords_AspNetUsers_ApplicationUserId",
                table: "PreviousPasswords");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PreviousPasswords",
                table: "PreviousPasswords");

            migrationBuilder.RenameTable(
                name: "PreviousPasswords",
                newName: "PreviousPassword");

            migrationBuilder.RenameIndex(
                name: "IX_PreviousPasswords_ApplicationUserId",
                table: "PreviousPassword",
                newName: "IX_PreviousPassword_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PreviousPassword",
                table: "PreviousPassword",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PreviousPassword_AspNetUsers_ApplicationUserId",
                table: "PreviousPassword",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
