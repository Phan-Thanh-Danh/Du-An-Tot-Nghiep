using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTiLeChuyenCanCauHinhDiem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "cap_nhat_luc",
                schema: "dbo",
                table: "CauHinhDiemMonHoc",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_cap_nhat",
                schema: "dbo",
                table: "CauHinhDiemMonHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ti_le_chuyen_can_toi_thieu",
                schema: "dbo",
                table: "CauHinhDiemMonHoc",
                type: "decimal(5,2)",
                nullable: false,
                defaultValueSql: "0");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhDiemMonHoc_nguoi_cap_nhat",
                schema: "dbo",
                table: "CauHinhDiemMonHoc",
                column: "nguoi_cap_nhat");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CauHinhDiemMonHoc_ti_le_chuyen_can_toi_thieu_5",
                schema: "dbo",
                table: "CauHinhDiemMonHoc",
                sql: "[ti_le_chuyen_can_toi_thieu] BETWEEN 0 AND 100");

            migrationBuilder.AddForeignKey(
                name: "FK_CauHinhDiemMonHoc_nguoi_cap_nhat__NguoiDung",
                schema: "dbo",
                table: "CauHinhDiemMonHoc",
                column: "nguoi_cap_nhat",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CauHinhDiemMonHoc_nguoi_cap_nhat__NguoiDung",
                schema: "dbo",
                table: "CauHinhDiemMonHoc");

            migrationBuilder.DropIndex(
                name: "IX_CauHinhDiemMonHoc_nguoi_cap_nhat",
                schema: "dbo",
                table: "CauHinhDiemMonHoc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CauHinhDiemMonHoc_ti_le_chuyen_can_toi_thieu_5",
                schema: "dbo",
                table: "CauHinhDiemMonHoc");

            migrationBuilder.DropColumn(
                name: "cap_nhat_luc",
                schema: "dbo",
                table: "CauHinhDiemMonHoc");

            migrationBuilder.DropColumn(
                name: "nguoi_cap_nhat",
                schema: "dbo",
                table: "CauHinhDiemMonHoc");

            migrationBuilder.DropColumn(
                name: "ti_le_chuyen_can_toi_thieu",
                schema: "dbo",
                table: "CauHinhDiemMonHoc");
        }
    }
}
