using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalTrainingSystem.Core.DB.Migrations
{
    /// <inheritdoc />
    public partial class deleteStatusInAssignmentSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "AssignmentSubmissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AssignmentSubmissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
