using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddKhenThuongVerifyCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ma_code_xac_thuc",
                schema: "dbo",
                table: "KhenThuong",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "diem_danh_gia",
                table: "GiaoVienMonHoc",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "phu_hop_chuyen_mon",
                table: "GiaoVienMonHoc",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_KhenThuong_MaCodeXacThuc",
                schema: "dbo",
                table: "KhenThuong",
                column: "ma_code_xac_thuc",
                unique: true,
                filter: "[ma_code_xac_thuc] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_KhenThuong_MaCodeXacThuc",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "ma_code_xac_thuc",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "diem_danh_gia",
                table: "GiaoVienMonHoc");

            migrationBuilder.DropColumn(
                name: "phu_hop_chuyen_mon",
                table: "GiaoVienMonHoc");
        }
    }
}
