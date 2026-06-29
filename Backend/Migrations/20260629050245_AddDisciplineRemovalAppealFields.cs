using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDisciplineRemovalAppealFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KhieuNaiKyLuat",
                schema: "dbo",
                columns: table => new
                {
                    ma_khieu_nai_ky_luat = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_ho_so_ky_luat = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: true),
                    ly_do_khieu_nai = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    chung_tu_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ly_do_xu_ly = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ghi_chu_xu_ly = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    nguoi_xu_ly = table.Column<int>(type: "int", nullable: true),
                    ngay_xu_ly = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhieuNaiKyLuat", x => x.ma_khieu_nai_ky_luat);
                    table.CheckConstraint("CK_KhieuNaiKyLuat_chung_tu_json_ISJSON", "[chung_tu_json] IS NULL OR ISJSON([chung_tu_json]) = 1");
                    table.ForeignKey(
                        name: "FK_KhieuNaiKyLuat_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_KhieuNaiKyLuat_ma_ho_so_ky_luat__HoSoKyLuat",
                        column: x => x.ma_ho_so_ky_luat,
                        principalSchema: "dbo",
                        principalTable: "HoSoKyLuat",
                        principalColumn: "ma_ky_luat",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KhieuNaiKyLuat_ma_hoc_sinh__NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_KhieuNaiKyLuat_nguoi_xu_ly__NguoiDung",
                        column: x => x.nguoi_xu_ly,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNaiKyLuat_MaDonVi",
                schema: "dbo",
                table: "KhieuNaiKyLuat",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNaiKyLuat_MaHocSinh",
                schema: "dbo",
                table: "KhieuNaiKyLuat",
                column: "ma_hoc_sinh");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNaiKyLuat_MaHoSoKyLuat",
                schema: "dbo",
                table: "KhieuNaiKyLuat",
                column: "ma_ho_so_ky_luat");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNaiKyLuat_nguoi_xu_ly",
                schema: "dbo",
                table: "KhieuNaiKyLuat",
                column: "nguoi_xu_ly");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNaiKyLuat_TrangThai",
                schema: "dbo",
                table: "KhieuNaiKyLuat",
                column: "trang_thai");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KhieuNaiKyLuat",
                schema: "dbo");
        }
    }
}
