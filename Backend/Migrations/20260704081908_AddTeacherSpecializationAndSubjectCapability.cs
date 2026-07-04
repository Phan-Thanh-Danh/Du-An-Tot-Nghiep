using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherSpecializationAndSubjectCapability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NangLucGiangVien");

            migrationBuilder.CreateTable(
                name: "GiaoVienChuyenNganh",
                columns: table => new
                {
                    ma_giao_vien = table.Column<int>(type: "int", nullable: false),
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false),
                    la_chuyen_mon_chinh = table.Column<bool>(type: "bit", nullable: false),
                    muc_do_phu_hop = table.Column<int>(type: "int", nullable: false),
                    so_nam_kinh_nghiem = table.Column<int>(type: "int", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoVienChuyenNganh", x => new { x.ma_giao_vien, x.ma_chuyen_nganh });
                    table.CheckConstraint("CK_GiaoVienChuyenNganh_muc_do_phu_hop", "[muc_do_phu_hop] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_GiaoVienChuyenNganh_so_nam_kinh_nghiem", "[so_nam_kinh_nghiem] IS NULL OR [so_nam_kinh_nghiem] >= 0");
                    table.ForeignKey(
                        name: "FK_GiaoVienChuyenNganh_ma_chuyen_nganh__ChuyenNganh",
                        column: x => x.ma_chuyen_nganh,
                        principalSchema: "dbo",
                        principalTable: "ChuyenNganh",
                        principalColumn: "ma_chuyen_nganh");
                    table.ForeignKey(
                        name: "FK_GiaoVienChuyenNganh_ma_giao_vien__NguoiDung",
                        column: x => x.ma_giao_vien,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "GiaoVienMonHoc",
                columns: table => new
                {
                    ma_giao_vien = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    muc_do_phu_hop = table.Column<int>(type: "int", nullable: false),
                    so_lan_da_day = table.Column<int>(type: "int", nullable: false),
                    so_nam_kinh_nghiem = table.Column<int>(type: "int", nullable: true),
                    la_mon_chinh = table.Column<bool>(type: "bit", nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoVienMonHoc", x => new { x.ma_giao_vien, x.ma_mon_hoc });
                    table.CheckConstraint("CK_GiaoVienMonHoc_muc_do_phu_hop", "[muc_do_phu_hop] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_GiaoVienMonHoc_so_lan_da_day", "[so_lan_da_day] >= 0");
                    table.CheckConstraint("CK_GiaoVienMonHoc_so_nam_kinh_nghiem", "[so_nam_kinh_nghiem] IS NULL OR [so_nam_kinh_nghiem] >= 0");
                    table.ForeignKey(
                        name: "FK_GiaoVienMonHoc_ma_giao_vien__NguoiDung",
                        column: x => x.ma_giao_vien,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_GiaoVienMonHoc_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienChuyenNganh_MaChuyenNganh",
                table: "GiaoVienChuyenNganh",
                column: "ma_chuyen_nganh");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienChuyenNganh_MaGiaoVien",
                table: "GiaoVienChuyenNganh",
                column: "ma_giao_vien");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienMonHoc_MaGiaoVien",
                table: "GiaoVienMonHoc",
                column: "ma_giao_vien");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVienMonHoc_MaMonHoc",
                table: "GiaoVienMonHoc",
                column: "ma_mon_hoc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiaoVienChuyenNganh");

            migrationBuilder.DropTable(
                name: "GiaoVienMonHoc");

            migrationBuilder.CreateTable(
                name: "NangLucGiangVien",
                columns: table => new
                {
                    MaNangLuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaGiaoVien = table.Column<int>(type: "int", nullable: false),
                    MaMonHoc = table.Column<int>(type: "int", nullable: false),
                    MucDoPhuHop = table.Column<int>(type: "int", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLanDaDay = table.Column<int>(type: "int", nullable: false),
                    UuTien = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NangLucGiangVien", x => x.MaNangLuc);
                    table.ForeignKey(
                        name: "FK_NangLucGiangVien_DanhMucMonHoc_MaMonHoc",
                        column: x => x.MaMonHoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NangLucGiangVien_NguoiDung_MaGiaoVien",
                        column: x => x.MaGiaoVien,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NangLucGiangVien_MaGiaoVien",
                table: "NangLucGiangVien",
                column: "MaGiaoVien");

            migrationBuilder.CreateIndex(
                name: "IX_NangLucGiangVien_MaMonHoc",
                table: "NangLucGiangVien",
                column: "MaMonHoc");
        }
    }
}
