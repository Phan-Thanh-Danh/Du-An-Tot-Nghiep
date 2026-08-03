using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFileConstraintsToBaiTap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DungLuongToiDaMB",
                schema: "dbo",
                table: "BaiTap",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DungLuongToiThieuKB",
                schema: "dbo",
                table: "BaiTap",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DungLuongToiDaMB",
                schema: "dbo",
                table: "BaiTap");

            migrationBuilder.DropColumn(
                name: "DungLuongToiThieuKB",
                schema: "dbo",
                table: "BaiTap");
        }
    }
}
