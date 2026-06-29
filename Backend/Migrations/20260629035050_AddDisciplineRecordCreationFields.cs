using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDisciplineRecordCreationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "can_cu_xu_ly",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ghi_chu_noi_bo",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_huy",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_huy",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_huy",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tieu_de",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_nguoi_huy",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_huy");

            migrationBuilder.AddForeignKey(
                name: "FK_HoSoKyLuat_nguoi_huy__NguoiDung",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_huy",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoSoKyLuat_nguoi_huy__NguoiDung",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropIndex(
                name: "IX_HoSoKyLuat_nguoi_huy",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "can_cu_xu_ly",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ghi_chu_noi_bo",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ly_do_huy",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ngay_huy",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "nguoi_huy",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "tieu_de",
                schema: "dbo",
                table: "HoSoKyLuat");
        }
    }
}
