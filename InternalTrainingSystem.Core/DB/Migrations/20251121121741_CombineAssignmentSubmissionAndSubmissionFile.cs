using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalTrainingSystem.Core.DB.Migrations
{
    /// <inheritdoc />
    public partial class CombineAssignmentSubmissionAndSubmissionFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentSubmissions_CourseEnrollments_EnrollmentId",
                table: "AssignmentSubmissions");

            migrationBuilder.DropTable(
                name: "SubmissionFiles");

            migrationBuilder.RenameColumn(
                name: "EnrollmentId",
                table: "AssignmentSubmissions",
                newName: "CourseEnrollmentEnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_AssignmentSubmissions_EnrollmentId",
                table: "AssignmentSubmissions",
                newName: "IX_AssignmentSubmissions_CourseEnrollmentEnrollmentId");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "AssignmentSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMain",
                table: "AssignmentSubmissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "AssignmentSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "AssignmentSubmissions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicUrl",
                table: "AssignmentSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SizeBytes",
                table: "AssignmentSubmissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentSubmissions_CourseEnrollments_CourseEnrollmentEnrollmentId",
                table: "AssignmentSubmissions",
                column: "CourseEnrollmentEnrollmentId",
                principalTable: "CourseEnrollments",
                principalColumn: "EnrollmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentSubmissions_CourseEnrollments_CourseEnrollmentEnrollmentId",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "IsMain",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "PublicUrl",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "SizeBytes",
                table: "AssignmentSubmissions");

            migrationBuilder.RenameColumn(
                name: "CourseEnrollmentEnrollmentId",
                table: "AssignmentSubmissions",
                newName: "EnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_AssignmentSubmissions_CourseEnrollmentEnrollmentId",
                table: "AssignmentSubmissions",
                newName: "IX_AssignmentSubmissions_EnrollmentId");

            migrationBuilder.CreateTable(
                name: "SubmissionFiles",
                columns: table => new
                {
                    FileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMain = table.Column<bool>(type: "bit", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PublicUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionFiles", x => x.FileId);
                    table.ForeignKey(
                        name: "FK_SubmissionFiles_AssignmentSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "AssignmentSubmissions",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionFiles_SubmissionId_IsMain",
                table: "SubmissionFiles",
                columns: new[] { "SubmissionId", "IsMain" });

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentSubmissions_CourseEnrollments_EnrollmentId",
                table: "AssignmentSubmissions",
                column: "EnrollmentId",
                principalTable: "CourseEnrollments",
                principalColumn: "EnrollmentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
