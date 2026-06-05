using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class Task1_LessonContentManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BaiHoc_loai_bai_hoc_1",
                schema: "dbo",
                table: "BaiHoc");

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "Chuong",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_tao",
                schema: "dbo",
                table: "Chuong",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "BaiHoc",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_tao",
                schema: "dbo",
                table: "BaiHoc",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "trang_thai",
                schema: "dbo",
                table: "BaiHoc",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "nhap");

            migrationBuilder.CreateTable(
                name: "BaiHocNoiDung",
                schema: "dbo",
                columns: table => new
                {
                    ma_noi_dung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_bai_hoc = table.Column<int>(type: "int", nullable: false),
                    loai_noi_dung = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    noi_dung_html = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    noi_dung_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    url_tap_tin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    storage_key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    kich_thuoc_byte = table.Column<long>(type: "bigint", nullable: true),
                    thoi_luong_giay = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "nhap"),
                    thu_tu = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiHocNoiDung", x => x.ma_noi_dung);
                    table.CheckConstraint("CK_BaiHocNoiDung_loai_noi_dung", "[loai_noi_dung] IN (N'video', N'slide_html', N'tai_lieu', N'quiz', N'van_ban')");
                    table.CheckConstraint("CK_BaiHocNoiDung_noi_dung_json_ISJSON", "[noi_dung_json] IS NULL OR ISJSON([noi_dung_json]) = 1");
                    table.CheckConstraint("CK_BaiHocNoiDung_thoi_luong_giay", "[thoi_luong_giay] IS NULL OR [thoi_luong_giay] >= 0");
                    table.CheckConstraint("CK_BaiHocNoiDung_trang_thai", "[trang_thai] IS NULL OR [trang_thai] IN (N'nhap', N'da_xuat_ban')");
                    table.ForeignKey(
                        name: "FK_BaiHocNoiDung_ma_bai_hoc__BaiHoc",
                        column: x => x.ma_bai_hoc,
                        principalSchema: "dbo",
                        principalTable: "BaiHoc",
                        principalColumn: "ma_bai_hoc",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_BaiHoc_loai_bai_hoc_1",
                schema: "dbo",
                table: "BaiHoc",
                sql: "[loai_bai_hoc] IN (N'video', N'pdf', N'van_ban', N'trac_nghiem', N'slide_html')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BaiHoc_trang_thai",
                schema: "dbo",
                table: "BaiHoc",
                sql: "[trang_thai] IS NULL OR [trang_thai] IN (N'nhap', N'da_xuat_ban')");

            migrationBuilder.CreateIndex(
                name: "IX_BaiHocNoiDung_ma_bai_hoc",
                schema: "dbo",
                table: "BaiHocNoiDung",
                column: "ma_bai_hoc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaiHocNoiDung",
                schema: "dbo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BaiHoc_loai_bai_hoc_1",
                schema: "dbo",
                table: "BaiHoc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BaiHoc_trang_thai",
                schema: "dbo",
                table: "BaiHoc");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "Chuong");

            migrationBuilder.DropColumn(
                name: "ngay_tao",
                schema: "dbo",
                table: "Chuong");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "BaiHoc");

            migrationBuilder.DropColumn(
                name: "ngay_tao",
                schema: "dbo",
                table: "BaiHoc");

            migrationBuilder.DropColumn(
                name: "trang_thai",
                schema: "dbo",
                table: "BaiHoc");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BaiHoc_loai_bai_hoc_1",
                schema: "dbo",
                table: "BaiHoc",
                sql: "[loai_bai_hoc] IN (N'video', N'pdf', N'van_ban', N'trac_nghiem')");
        }
    }
}
