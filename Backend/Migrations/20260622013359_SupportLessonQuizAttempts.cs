using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class SupportLessonQuizAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_PhienThiHocSinh_1",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeKiemTra_loai_de_thi",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.AddColumn<string>(
                name: "de_thi_snapshot_json",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "han_nop_luc",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ket_qua_dat",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "lan_thu",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "so_cau_dung",
                schema: "dbo",
                table: "PhienThiHocSinh",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_PhienThiHocSinh_De_HocSinh_LanThu",
                schema: "dbo",
                table: "PhienThiHocSinh",
                columns: new[] { "ma_de_kiem_tra", "ma_hoc_sinh", "lan_thu" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PhienThiHocSinh_de_thi_snapshot_json_ISJSON",
                schema: "dbo",
                table: "PhienThiHocSinh",
                sql: "[de_thi_snapshot_json] IS NULL OR ISJSON([de_thi_snapshot_json]) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PhienThiHocSinh_lan_thu",
                schema: "dbo",
                table: "PhienThiHocSinh",
                sql: "[lan_thu] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PhienThiHocSinh_so_cau_dung",
                schema: "dbo",
                table: "PhienThiHocSinh",
                sql: "[so_cau_dung] IS NULL OR [so_cau_dung] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeKiemTra_loai_de_thi",
                schema: "dbo",
                table: "DeKiemTra",
                sql: "[loai_de_thi] IS NULL OR [loai_de_thi] IN (N'trac_nghiem', N'tu_luan', N'ket_hop', N'quiz_bai_hoc')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_PhienThiHocSinh_De_HocSinh_LanThu",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PhienThiHocSinh_de_thi_snapshot_json_ISJSON",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PhienThiHocSinh_lan_thu",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PhienThiHocSinh_so_cau_dung",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeKiemTra_loai_de_thi",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropColumn(
                name: "de_thi_snapshot_json",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropColumn(
                name: "han_nop_luc",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropColumn(
                name: "ket_qua_dat",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropColumn(
                name: "lan_thu",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.DropColumn(
                name: "so_cau_dung",
                schema: "dbo",
                table: "PhienThiHocSinh");

            migrationBuilder.CreateIndex(
                name: "UQ_PhienThiHocSinh_1",
                schema: "dbo",
                table: "PhienThiHocSinh",
                columns: new[] { "ma_de_kiem_tra", "ma_hoc_sinh" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeKiemTra_loai_de_thi",
                schema: "dbo",
                table: "DeKiemTra",
                sql: "[loai_de_thi] IS NULL OR [loai_de_thi] IN (N'trac_nghiem', N'tu_luan', N'ket_hop')");
        }
    }
}
