using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalTrainingSystem.Core.DB.Migrations
{
    /// <inheritdoc />
    public partial class DeleteRoleHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRoleHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRoleHistories",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RoleId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleHistories", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_UserRoleHistories_AspNetUsers_ActionBy",
                        column: x => x.ActionBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserRoleHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleHistories_Action",
                table: "UserRoleHistories",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleHistories_ActionBy",
                table: "UserRoleHistories",
                column: "ActionBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleHistories_ActionDate",
                table: "UserRoleHistories",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleHistories_RoleId",
                table: "UserRoleHistories",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleHistories_UserId_ActionDate",
                table: "UserRoleHistories",
                columns: new[] { "UserId", "ActionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleHistories_UserId_RoleId_ActionDate",
                table: "UserRoleHistories",
                columns: new[] { "UserId", "RoleId", "ActionDate" },
                unique: true);
        }
    }
}
