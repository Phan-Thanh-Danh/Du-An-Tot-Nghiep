using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class Task1FinanceFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM [dbo].[TaiKhoanNhanTien]
                    WHERE [nha_cung_cap_thanh_toan] NOT IN (N'payos', N'vietqr')
                )
                BEGIN
                    THROW 51001, N'TaiKhoanNhanTien co nha_cung_cap_thanh_toan nam ngoai MVP payos/vietqr. Hay doi soat du lieu truoc khi ap dung migration Task1FinanceFoundation.', 1;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM [dbo].[GiaoDich]
                    WHERE [nha_cung_cap_thanh_toan] IS NOT NULL
                        AND [nha_cung_cap_thanh_toan] NOT IN (N'payos', N'vietqr')
                )
                BEGIN
                    THROW 51002, N'GiaoDich co nha_cung_cap_thanh_toan nam ngoai MVP payos/vietqr. Hay doi soat du lieu truoc khi ap dung migration Task1FinanceFoundation.', 1;
                END;
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_TaiKhoanNhanTien_provider",
                schema: "dbo",
                table: "TaiKhoanNhanTien");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GiaoDich_provider",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.AddColumn<string>(
                name: "ghi_chu",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_tu_choi",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_yeu_cau",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_tao",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<int>(
                name: "nguoi_cap_nhat",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_tao",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cau_hinh_provider_json",
                schema: "dbo",
                table: "TaiKhoanNhanTien",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_huy",
                schema: "dbo",
                table: "HoaDon",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_huy",
                schema: "dbo",
                table: "HoaDon",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nguoi_huy",
                schema: "dbo",
                table: "HoaDon",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoanPhi_nguoi_cap_nhat",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                column: "nguoi_cap_nhat");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauHoanPhi_nguoi_tao",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                column: "nguoi_tao");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TaiKhoanNhanTien_cau_hinh_provider_json",
                schema: "dbo",
                table: "TaiKhoanNhanTien",
                sql: "[cau_hinh_provider_json] IS NULL OR ISJSON([cau_hinh_provider_json]) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TaiKhoanNhanTien_provider",
                schema: "dbo",
                table: "TaiKhoanNhanTien",
                sql: "[nha_cung_cap_thanh_toan] IN (N'payos', N'vietqr')");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_nguoi_huy",
                schema: "dbo",
                table: "HoaDon",
                column: "nguoi_huy");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GiaoDich_provider",
                schema: "dbo",
                table: "GiaoDich",
                sql: "[nha_cung_cap_thanh_toan] IS NULL OR [nha_cung_cap_thanh_toan] IN (N'payos', N'vietqr')");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_nguoi_huy__NguoiDung",
                schema: "dbo",
                table: "HoaDon",
                column: "nguoi_huy",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauHoanPhi_nguoi_cap_nhat__NguoiDung",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                column: "nguoi_cap_nhat",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauHoanPhi_nguoi_tao__NguoiDung",
                schema: "dbo",
                table: "YeuCauHoanPhi",
                column: "nguoi_tao",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_nguoi_huy__NguoiDung",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauHoanPhi_nguoi_cap_nhat__NguoiDung",
                schema: "dbo",
                table: "YeuCauHoanPhi");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauHoanPhi_nguoi_tao__NguoiDung",
                schema: "dbo",
                table: "YeuCauHoanPhi");

            migrationBuilder.DropIndex(
                name: "IX_YeuCauHoanPhi_nguoi_cap_nhat",
                schema: "dbo",
                table: "YeuCauHoanPhi");

            migrationBuilder.DropIndex(
                name: "IX_YeuCauHoanPhi_nguoi_tao",
                schema: "dbo",
                table: "YeuCauHoanPhi");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TaiKhoanNhanTien_cau_hinh_provider_json",
                schema: "dbo",
                table: "TaiKhoanNhanTien");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TaiKhoanNhanTien_provider",
                schema: "dbo",
                table: "TaiKhoanNhanTien");

            migrationBuilder.DropIndex(
                name: "IX_HoaDon_nguoi_huy",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropCheckConstraint(
                name: "CK_GiaoDich_provider",
                schema: "dbo",
                table: "GiaoDich");

            migrationBuilder.DropColumn(
                name: "ghi_chu",
                schema: "dbo",
                table: "YeuCauHoanPhi");

            migrationBuilder.DropColumn(
                name: "ly_do_tu_choi",
                schema: "dbo",
                table: "YeuCauHoanPhi");

            migrationBuilder.DropColumn(
                name: "ly_do_yeu_cau",
                schema: "dbo",
                table: "YeuCauHoanPhi");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "YeuCauHoanPhi");

            migrationBuilder.DropColumn(
                name: "ngay_tao",
                schema: "dbo",
                table: "YeuCauHoanPhi");

            migrationBuilder.DropColumn(
                name: "nguoi_cap_nhat",
                schema: "dbo",
                table: "YeuCauHoanPhi");

            migrationBuilder.DropColumn(
                name: "nguoi_tao",
                schema: "dbo",
                table: "YeuCauHoanPhi");

            migrationBuilder.DropColumn(
                name: "cau_hinh_provider_json",
                schema: "dbo",
                table: "TaiKhoanNhanTien");

            migrationBuilder.DropColumn(
                name: "ly_do_huy",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "ngay_huy",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "nguoi_huy",
                schema: "dbo",
                table: "HoaDon");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TaiKhoanNhanTien_provider",
                schema: "dbo",
                table: "TaiKhoanNhanTien",
                sql: "[nha_cung_cap_thanh_toan] IN (N'payos', N'vietqr', N'casso', N'sepay', N'mb_bank')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_GiaoDich_provider",
                schema: "dbo",
                table: "GiaoDich",
                sql: "[nha_cung_cap_thanh_toan] IS NULL OR [nha_cung_cap_thanh_toan] IN (N'payos', N'vietqr', N'casso', N'sepay', N'mb_bank')");
        }
    }
}
