using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDisciplineApprovalEffectFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ghi_chu_duyet",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_ap_dung",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_ap_dung",
                schema: "dbo",
                table: "HoSoKyLuat",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKyLuat_nguoi_ap_dung",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_ap_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_HoSoKyLuat_nguoi_ap_dung__NguoiDung",
                schema: "dbo",
                table: "HoSoKyLuat",
                column: "nguoi_ap_dung",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoSoKyLuat_nguoi_ap_dung__NguoiDung",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropIndex(
                name: "IX_HoSoKyLuat_nguoi_ap_dung",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ghi_chu_duyet",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "ngay_ap_dung",
                schema: "dbo",
                table: "HoSoKyLuat");

            migrationBuilder.DropColumn(
                name: "nguoi_ap_dung",
                schema: "dbo",
                table: "HoSoKyLuat");
        }
    }
}
