using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationCenterFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "doc_luc",
                schema: "dbo",
                table: "ThongBao",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "doi_tuong_lien_ket",
                schema: "dbo",
                table: "ThongBao",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_doi_tuong_lien_ket",
                schema: "dbo",
                table: "ThongBao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ma_nhom_thong_bao",
                schema: "dbo",
                table: "ThongBao",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<string>(
                name: "muc_do",
                schema: "dbo",
                table: "ThongBao",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "info");

            migrationBuilder.AddColumn<int>(
                name: "nguoi_tao",
                schema: "dbo",
                table: "ThongBao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "noi_dung_json",
                schema: "dbo",
                table: "ThongBao",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "noi_dung_text",
                schema: "dbo",
                table: "ThongBao",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tom_tat",
                schema: "dbo",
                table: "ThongBao",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trang_thai",
                schema: "dbo",
                table: "ThongBao",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "da_gui");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_DonVi_NgayTao",
                schema: "dbo",
                table: "ThongBao",
                columns: new[] { "ma_don_vi", "ngay_tao" });

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_MaNhomThongBao",
                schema: "dbo",
                table: "ThongBao",
                column: "ma_nhom_thong_bao");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_nguoi_tao",
                schema: "dbo",
                table: "ThongBao",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_NguoiNhan_DaDoc_NgayTao",
                schema: "dbo",
                table: "ThongBao",
                columns: new[] { "ma_nguoi_nhan", "da_doc", "ngay_tao" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThongBao_muc_do",
                schema: "dbo",
                table: "ThongBao",
                sql: "[muc_do] IN (N'info', N'warning', N'important')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThongBao_noi_dung_json_ISJSON",
                schema: "dbo",
                table: "ThongBao",
                sql: "[noi_dung_json] IS NULL OR ISJSON([noi_dung_json]) = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ThongBao_trang_thai",
                schema: "dbo",
                table: "ThongBao",
                sql: "[trang_thai] IN (N'nhap', N'da_gui', N'da_huy')");

            migrationBuilder.AddForeignKey(
                name: "FK_ThongBao_nguoi_tao__NguoiDung",
                schema: "dbo",
                table: "ThongBao",
                column: "nguoi_tao",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThongBao_nguoi_tao__NguoiDung",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropIndex(
                name: "IX_ThongBao_DonVi_NgayTao",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropIndex(
                name: "IX_ThongBao_MaNhomThongBao",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropIndex(
                name: "IX_ThongBao_nguoi_tao",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropIndex(
                name: "IX_ThongBao_NguoiNhan_DaDoc_NgayTao",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThongBao_muc_do",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThongBao_noi_dung_json_ISJSON",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ThongBao_trang_thai",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "doc_luc",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "doi_tuong_lien_ket",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "ma_doi_tuong_lien_ket",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "ma_nhom_thong_bao",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "muc_do",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "nguoi_tao",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "noi_dung_json",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "noi_dung_text",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "tom_tat",
                schema: "dbo",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "trang_thai",
                schema: "dbo",
                table: "ThongBao");
        }
    }
}
