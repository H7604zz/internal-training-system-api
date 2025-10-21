using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalTrainingSystem.Core.DB.Migrations
{
    /// <inheritdoc />
    public partial class updateCourseModuleLessonaddLessonProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "IsPreview",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "EstimatedMinutes",
                table: "CourseModules");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CourseModules");

            migrationBuilder.RenameColumn(
                name: "VideoUrl",
                table: "Lessons",
                newName: "MimeType");

            migrationBuilder.RenameColumn(
                name: "FileUrl",
                table: "Lessons",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "ExternalUrl",
                table: "Lessons",
                newName: "ContentUrl");

            migrationBuilder.AddColumn<long>(
                name: "SizeBytes",
                table: "Lessons",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LessonProgresses",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    IsDone = table.Column<bool>(type: "bit", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonProgresses", x => new { x.UserId, x.LessonId });
                    table.ForeignKey(
                        name: "FK_LessonProgresses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonProgresses_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonProgresses_IsDone",
                table: "LessonProgresses",
                column: "IsDone");

            migrationBuilder.CreateIndex(
                name: "IX_LessonProgresses_LessonId",
                table: "LessonProgresses",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonProgresses");

            migrationBuilder.DropColumn(
                name: "SizeBytes",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "MimeType",
                table: "Lessons",
                newName: "VideoUrl");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Lessons",
                newName: "FileUrl");

            migrationBuilder.RenameColumn(
                name: "ContentUrl",
                table: "Lessons",
                newName: "ExternalUrl");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Lessons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPreview",
                table: "Lessons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "Lessons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedMinutes",
                table: "CourseModules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CourseModules",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
