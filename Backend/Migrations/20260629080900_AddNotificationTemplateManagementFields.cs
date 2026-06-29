using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTemplateManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_MauThongBao_kenh_gui_1",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.AddColumn<string>(
                name: "bien_cho_phep_json",
                schema: "dbo",
                table: "MauThongBao",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "dang_hoat_dong",
                schema: "dbo",
                table: "MauThongBao",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "doi_tuong_mac_dinh",
                schema: "dbo",
                table: "MauThongBao",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "la_he_thong",
                schema: "dbo",
                table: "MauThongBao",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "loai_thong_bao",
                schema: "dbo",
                table: "MauThongBao",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_don_vi",
                schema: "dbo",
                table: "MauThongBao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ma_mau",
                schema: "dbo",
                table: "MauThongBao",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "muc_do_uu_tien",
                schema: "dbo",
                table: "MauThongBao",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "MauThongBao",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_tao",
                schema: "dbo",
                table: "MauThongBao",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "nguoi_cap_nhat",
                schema: "dbo",
                table: "MauThongBao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_tao",
                schema: "dbo",
                table: "MauThongBao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ten_mau",
                schema: "dbo",
                table: "MauThongBao",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MauThongBao_DangHoatDong",
                schema: "dbo",
                table: "MauThongBao",
                column: "dang_hoat_dong");

            migrationBuilder.CreateIndex(
                name: "IX_MauThongBao_LoaiThongBao",
                schema: "dbo",
                table: "MauThongBao",
                column: "loai_thong_bao");

            migrationBuilder.CreateIndex(
                name: "IX_MauThongBao_MaDonVi",
                schema: "dbo",
                table: "MauThongBao",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_MauThongBao_MaMau",
                schema: "dbo",
                table: "MauThongBao",
                column: "ma_mau");

            migrationBuilder.CreateIndex(
                name: "IX_MauThongBao_nguoi_cap_nhat",
                schema: "dbo",
                table: "MauThongBao",
                column: "nguoi_cap_nhat");

            migrationBuilder.CreateIndex(
                name: "IX_MauThongBao_nguoi_tao",
                schema: "dbo",
                table: "MauThongBao",
                column: "nguoi_tao");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MauThongBao_kenh_gui_1",
                schema: "dbo",
                table: "MauThongBao",
                sql: "[kenh_gui] IN (N'email', N'thong_bao_day', N'sms', N'in_app')");

            migrationBuilder.AddForeignKey(
                name: "FK_MauThongBao_DonVi",
                schema: "dbo",
                table: "MauThongBao",
                column: "ma_don_vi",
                principalSchema: "dbo",
                principalTable: "DonVi",
                principalColumn: "ma_don_vi");

            migrationBuilder.AddForeignKey(
                name: "FK_MauThongBao_NguoiCapNhat",
                schema: "dbo",
                table: "MauThongBao",
                column: "nguoi_cap_nhat",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_MauThongBao_NguoiTao",
                schema: "dbo",
                table: "MauThongBao",
                column: "nguoi_tao",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MauThongBao_DonVi",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropForeignKey(
                name: "FK_MauThongBao_NguoiCapNhat",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropForeignKey(
                name: "FK_MauThongBao_NguoiTao",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropIndex(
                name: "IX_MauThongBao_DangHoatDong",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropIndex(
                name: "IX_MauThongBao_LoaiThongBao",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropIndex(
                name: "IX_MauThongBao_MaDonVi",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropIndex(
                name: "IX_MauThongBao_MaMau",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropIndex(
                name: "IX_MauThongBao_nguoi_cap_nhat",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropIndex(
                name: "IX_MauThongBao_nguoi_tao",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MauThongBao_kenh_gui_1",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "bien_cho_phep_json",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "dang_hoat_dong",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "doi_tuong_mac_dinh",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "la_he_thong",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "loai_thong_bao",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "ma_don_vi",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "ma_mau",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "muc_do_uu_tien",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "ngay_tao",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "nguoi_cap_nhat",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "nguoi_tao",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.DropColumn(
                name: "ten_mau",
                schema: "dbo",
                table: "MauThongBao");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MauThongBao_kenh_gui_1",
                schema: "dbo",
                table: "MauThongBao",
                sql: "[kenh_gui] IN (N'email', N'thong_bao_day', N'sms')");
        }
    }
}
