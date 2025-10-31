using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalTrainingSystem.Core.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleNameToNotificationRecipient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationRecipient_Notifications_NotificationId",
                table: "NotificationRecipient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationRecipient",
                table: "NotificationRecipient");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Notifications");

            migrationBuilder.RenameTable(
                name: "NotificationRecipient",
                newName: "NotificationRecipients");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationRecipient_NotificationId",
                table: "NotificationRecipients",
                newName: "IX_NotificationRecipients_NotificationId");

            migrationBuilder.AddColumn<string>(
                name: "RoleName",
                table: "NotificationRecipients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationRecipients",
                table: "NotificationRecipients",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationRecipients_Notifications_NotificationId",
                table: "NotificationRecipients",
                column: "NotificationId",
                principalTable: "Notifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationRecipients_Notifications_NotificationId",
                table: "NotificationRecipients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationRecipients",
                table: "NotificationRecipients");

            migrationBuilder.DropColumn(
                name: "RoleName",
                table: "NotificationRecipients");

            migrationBuilder.RenameTable(
                name: "NotificationRecipients",
                newName: "NotificationRecipient");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationRecipients_NotificationId",
                table: "NotificationRecipient",
                newName: "IX_NotificationRecipient_NotificationId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationRecipient",
                table: "NotificationRecipient",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationRecipient_Notifications_NotificationId",
                table: "NotificationRecipient",
                column: "NotificationId",
                principalTable: "Notifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
