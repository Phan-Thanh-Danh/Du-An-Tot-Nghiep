using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLoaiKyThiToKyThi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "loai_ky_thi",
                schema: "dbo",
                table: "KyThi",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "cuoi_ky");

            migrationBuilder.AddCheckConstraint(
                name: "CK_KyThi_LoaiKyThi",
                schema: "dbo",
                table: "KyThi",
                sql: "loai_ky_thi IN ('giua_ky', 'cuoi_ky')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_KyThi_LoaiKyThi",
                schema: "dbo",
                table: "KyThi");

            migrationBuilder.DropColumn(
                name: "loai_ky_thi",
                schema: "dbo",
                table: "KyThi");
        }
    }
}
