using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalTrainingSystem.Core.DB.Migrations
{
    /// <inheritdoc />
    public partial class removeGradeInAssignmentSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "AssignmentSubmissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "AssignmentSubmissions",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
