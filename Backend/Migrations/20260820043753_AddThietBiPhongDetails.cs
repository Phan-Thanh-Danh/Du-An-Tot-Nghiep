using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddThietBiPhongDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "chung_loai",
                schema: "dbo",
                table: "ThietBiPhong",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ghi_chu",
                schema: "dbo",
                table: "ThietBiPhong",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ma_code_thiet_bi",
                schema: "dbo",
                table: "ThietBiPhong",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_kiem_dinh",
                schema: "dbo",
                table: "ThietBiPhong",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tinh_trang",
                schema: "dbo",
                table: "ThietBiPhong",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "chung_loai",
                schema: "dbo",
                table: "ThietBiPhong");

            migrationBuilder.DropColumn(
                name: "ghi_chu",
                schema: "dbo",
                table: "ThietBiPhong");

            migrationBuilder.DropColumn(
                name: "ma_code_thiet_bi",
                schema: "dbo",
                table: "ThietBiPhong");

            migrationBuilder.DropColumn(
                name: "ngay_kiem_dinh",
                schema: "dbo",
                table: "ThietBiPhong");

            migrationBuilder.DropColumn(
                name: "tinh_trang",
                schema: "dbo",
                table: "ThietBiPhong");
        }
    }
}
