using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalTrainingSystem.Core.DB.Migrations
{
    /// <inheritdoc />
    public partial class updateAssignmentSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssignmentSubmissions_AssignmentId_UserId_AttemptNumber",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "AttemptNumber",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "IsMain",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "AssignmentSubmissions");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "AssignmentSubmissions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_AssignmentId_UserId",
                table: "AssignmentSubmissions",
                columns: new[] { "AssignmentId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssignmentSubmissions_AssignmentId_UserId",
                table: "AssignmentSubmissions");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "AssignmentSubmissions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttemptNumber",
                table: "AssignmentSubmissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsMain",
                table: "AssignmentSubmissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "AssignmentSubmissions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "AssignmentSubmissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_AssignmentId_UserId_AttemptNumber",
                table: "AssignmentSubmissions",
                columns: new[] { "AssignmentId", "UserId", "AttemptNumber" },
                unique: true);
        }
    }
}
