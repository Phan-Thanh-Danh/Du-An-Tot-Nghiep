using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSemesterNumberFromMonHocTrongChuongTrinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MonHocTrongChuongTrinh_semester_number",
                schema: "dbo",
                table: "MonHocTrongChuongTrinh");

            migrationBuilder.DropColumn(
                name: "semester_number",
                schema: "dbo",
                table: "MonHocTrongChuongTrinh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "semester_number",
                schema: "dbo",
                table: "MonHocTrongChuongTrinh",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MonHocTrongChuongTrinh_semester_number",
                schema: "dbo",
                table: "MonHocTrongChuongTrinh",
                sql: "[semester_number] > 0");
        }
    }
}
