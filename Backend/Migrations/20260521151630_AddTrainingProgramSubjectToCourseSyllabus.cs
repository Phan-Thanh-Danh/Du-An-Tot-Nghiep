using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingProgramSubjectToCourseSyllabus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ma_chuong_trinh_mon_hoc",
                schema: "dbo",
                table: "CourseSyllabus",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseSyllabus_ma_chuong_trinh_mon_hoc",
                schema: "dbo",
                table: "CourseSyllabus",
                column: "ma_chuong_trinh_mon_hoc");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSyllabus_ma_chuong_trinh_mon_hoc__MonHocTrongChuongTrinh",
                schema: "dbo",
                table: "CourseSyllabus",
                column: "ma_chuong_trinh_mon_hoc",
                principalSchema: "dbo",
                principalTable: "MonHocTrongChuongTrinh",
                principalColumn: "ma_chuong_trinh_mon_hoc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseSyllabus_ma_chuong_trinh_mon_hoc__MonHocTrongChuongTrinh",
                schema: "dbo",
                table: "CourseSyllabus");

            migrationBuilder.DropIndex(
                name: "IX_CourseSyllabus_ma_chuong_trinh_mon_hoc",
                schema: "dbo",
                table: "CourseSyllabus");

            migrationBuilder.DropColumn(
                name: "ma_chuong_trinh_mon_hoc",
                schema: "dbo",
                table: "CourseSyllabus");
        }
    }
}
