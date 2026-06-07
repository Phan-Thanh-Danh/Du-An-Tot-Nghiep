using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceSessionFieldsToBuoiHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "diem_danh_bat_dau_luc",
                schema: "dbo",
                table: "BuoiHoc",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "diem_danh_da_gui_luc",
                schema: "dbo",
                table: "BuoiHoc",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "diem_danh_han_chinh_sua_luc",
                schema: "dbo",
                table: "BuoiHoc",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "diem_danh_han_gui_luc",
                schema: "dbo",
                table: "BuoiHoc",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "diem_danh_khoa_luc",
                schema: "dbo",
                table: "BuoiHoc",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trang_thai_diem_danh",
                schema: "dbo",
                table: "BuoiHoc",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "chua_mo");

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_DiemDanh_HanChinhSua",
                schema: "dbo",
                table: "BuoiHoc",
                columns: new[] { "trang_thai_diem_danh", "diem_danh_han_chinh_sua_luc" });

            migrationBuilder.CreateIndex(
                name: "IX_BuoiHoc_DiemDanh_HanGui",
                schema: "dbo",
                table: "BuoiHoc",
                columns: new[] { "trang_thai_diem_danh", "diem_danh_han_gui_luc" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_BuoiHoc_trang_thai_diem_danh",
                schema: "dbo",
                table: "BuoiHoc",
                sql: "[trang_thai_diem_danh] IN (N'chua_mo', N'dang_diem_danh', N'da_gui', N'da_khoa')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BuoiHoc_DiemDanh_HanChinhSua",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropIndex(
                name: "IX_BuoiHoc_DiemDanh_HanGui",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BuoiHoc_trang_thai_diem_danh",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "diem_danh_bat_dau_luc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "diem_danh_da_gui_luc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "diem_danh_han_chinh_sua_luc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "diem_danh_han_gui_luc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "diem_danh_khoa_luc",
                schema: "dbo",
                table: "BuoiHoc");

            migrationBuilder.DropColumn(
                name: "trang_thai_diem_danh",
                schema: "dbo",
                table: "BuoiHoc");
        }
    }
}
