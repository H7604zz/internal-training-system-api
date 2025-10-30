using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalTrainingSystem.Core.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContentHtml",
                table: "Lessons",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentFilePath",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentMimeType",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AttachmentSizeBytes",
                table: "Lessons",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentFilePath",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "AttachmentMimeType",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "AttachmentSizeBytes",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "Lessons");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Lessons",
                newName: "ContentHtml");
        }
    }
}
