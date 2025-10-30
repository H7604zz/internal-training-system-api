using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalTrainingSystem.Core.DB.Migrations
{
    /// <inheritdoc />
    public partial class RecreateClassEmployeesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Xóa nếu đã tồn tại (tránh lỗi khi chạy update)
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.ClassEmployees', 'U') IS NOT NULL
                DROP TABLE dbo.ClassEmployees;
            ");

            migrationBuilder.CreateTable(
                name: "ClassEmployees",
                columns: table => new
                {
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassEmployees", x => new { x.ClassId, x.EmployeeId });

                    table.ForeignKey(
                        name: "FK_ClassEmployees_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "ClassId",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_ClassEmployees_AspNetUsers_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassEmployees_EmployeeId",
                table: "ClassEmployees",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassEmployees");
        }
    }
}
