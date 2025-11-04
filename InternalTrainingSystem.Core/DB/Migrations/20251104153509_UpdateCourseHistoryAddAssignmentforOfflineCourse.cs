using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalTrainingSystem.Core.DB.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCourseHistoryAddAssignmentforOfflineCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_AspNetUsers_UserId",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_CourseEnrollments_EnrollmentId",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_Courses_CourseId",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_QuizAttempts_QuizAttemptId",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_Schedules_ScheduleId",
                table: "CourseHistories");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_UserId_QuizId_AttemptNumber",
                table: "QuizAttempts");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "CourseHistories",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "CourseHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LessonId",
                table: "CourseHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                table: "CourseHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuizId",
                table: "CourseHistories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    ScheduleId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CloseAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AllowLateSubmit = table.Column<bool>(type: "bit", nullable: false),
                    MaxSubmissions = table.Column<int>(type: "int", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentMimeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentSizeBytes = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_Assignments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assignments_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "ScheduleId");
                });

            migrationBuilder.CreateTable(
                name: "AssignmentSubmissions",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: true),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    Grade = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsLate = table.Column<bool>(type: "bit", nullable: false),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EnrollmentId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentSubmissions", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmissions_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmissions_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "AssignmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmissions_CourseEnrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "CourseEnrollments",
                        principalColumn: "EnrollmentId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmissions_CourseEnrollments_EnrollmentId1",
                        column: x => x.EnrollmentId1,
                        principalTable: "CourseEnrollments",
                        principalColumn: "EnrollmentId");
                    table.ForeignKey(
                        name: "FK_AssignmentSubmissions_Lessons_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionFiles",
                columns: table => new
                {
                    FileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    PublicUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsMain = table.Column<bool>(type: "bit", nullable: false)
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
                name: "IX_QuizAttempts_UserId_QuizId_AttemptNumber",
                table: "QuizAttempts",
                columns: new[] { "UserId", "QuizId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseHistories_ApplicationUserId",
                table: "CourseHistories",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseHistories_CourseId1",
                table: "CourseHistories",
                column: "CourseId1");

            migrationBuilder.CreateIndex(
                name: "IX_CourseHistories_LessonId",
                table: "CourseHistories",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseHistories_ModuleId",
                table: "CourseHistories",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseHistories_QuizId",
                table: "CourseHistories",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_CourseId",
                table: "Assignments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ScheduleId",
                table: "Assignments",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_AssignmentId_UserId_AttemptNumber",
                table: "AssignmentSubmissions",
                columns: new[] { "AssignmentId", "UserId", "AttemptNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_EnrollmentId",
                table: "AssignmentSubmissions",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_EnrollmentId1",
                table: "AssignmentSubmissions",
                column: "EnrollmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_UserId",
                table: "AssignmentSubmissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmissions_UserId1",
                table: "AssignmentSubmissions",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionFiles_SubmissionId",
                table: "SubmissionFiles",
                column: "SubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_AspNetUsers_ApplicationUserId",
                table: "CourseHistories",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_AspNetUsers_UserId",
                table: "CourseHistories",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_CourseEnrollments_EnrollmentId",
                table: "CourseHistories",
                column: "EnrollmentId",
                principalTable: "CourseEnrollments",
                principalColumn: "EnrollmentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_CourseModules_ModuleId",
                table: "CourseHistories",
                column: "ModuleId",
                principalTable: "CourseModules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_Courses_CourseId",
                table: "CourseHistories",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_Courses_CourseId1",
                table: "CourseHistories",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_Lessons_LessonId",
                table: "CourseHistories",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_QuizAttempts_QuizAttemptId",
                table: "CourseHistories",
                column: "QuizAttemptId",
                principalTable: "QuizAttempts",
                principalColumn: "AttemptId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_Quizzes_QuizId",
                table: "CourseHistories",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "QuizId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_Schedules_ScheduleId",
                table: "CourseHistories",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "ScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_AspNetUsers_ApplicationUserId",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_AspNetUsers_UserId",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_CourseEnrollments_EnrollmentId",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_CourseModules_ModuleId",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_Courses_CourseId",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_Courses_CourseId1",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_Lessons_LessonId",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_QuizAttempts_QuizAttemptId",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_Quizzes_QuizId",
                table: "CourseHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseHistories_Schedules_ScheduleId",
                table: "CourseHistories");

            migrationBuilder.DropTable(
                name: "SubmissionFiles");

            migrationBuilder.DropTable(
                name: "AssignmentSubmissions");

            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_UserId_QuizId_AttemptNumber",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_CourseHistories_ApplicationUserId",
                table: "CourseHistories");

            migrationBuilder.DropIndex(
                name: "IX_CourseHistories_CourseId1",
                table: "CourseHistories");

            migrationBuilder.DropIndex(
                name: "IX_CourseHistories_LessonId",
                table: "CourseHistories");

            migrationBuilder.DropIndex(
                name: "IX_CourseHistories_ModuleId",
                table: "CourseHistories");

            migrationBuilder.DropIndex(
                name: "IX_CourseHistories_QuizId",
                table: "CourseHistories");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "CourseHistories");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "CourseHistories");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "CourseHistories");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "CourseHistories");

            migrationBuilder.DropColumn(
                name: "QuizId",
                table: "CourseHistories");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_UserId_QuizId_AttemptNumber",
                table: "QuizAttempts",
                columns: new[] { "UserId", "QuizId", "AttemptNumber" });

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_AspNetUsers_UserId",
                table: "CourseHistories",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_CourseEnrollments_EnrollmentId",
                table: "CourseHistories",
                column: "EnrollmentId",
                principalTable: "CourseEnrollments",
                principalColumn: "EnrollmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_Courses_CourseId",
                table: "CourseHistories",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_QuizAttempts_QuizAttemptId",
                table: "CourseHistories",
                column: "QuizAttemptId",
                principalTable: "QuizAttempts",
                principalColumn: "AttemptId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseHistories_Schedules_ScheduleId",
                table: "CourseHistories",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "ScheduleId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
