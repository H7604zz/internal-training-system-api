using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalTrainingSystem.Core.DB.Migrations
{
    /// <inheritdoc />
    public partial class deleteIsLateOfAssignmentSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLate",
                table: "AssignmentSubmissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLate",
                table: "AssignmentSubmissions",
                type: "bit",
                maxLength: 1000,
                nullable: false,
                defaultValue: false);
        }
    }
}
