using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramTuitionConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CauHinhHocPhiChuongTrinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hinh_hoc_phi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_chuong_trinh_dao_tao = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    nam_hoc_trong_chuong_trinh = table.Column<int>(type: "int", nullable: false),
                    hoc_ky_trong_nam = table.Column<int>(type: "int", nullable: false),
                    so_thu_tu_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    loai_cach_tinh_hoc_phi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "co_dinh_theo_hoc_ky"),
                    so_tien_hoc_phi = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    tien_hoc_lieu = table.Column<decimal>(type: "decimal(15,2)", nullable: false, defaultValue: 0m),
                    tong_tien_du_kien = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhHocPhiChuongTrinh", x => x.ma_cau_hinh_hoc_phi);
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_hoc_ky_trong_nam", "[hoc_ky_trong_nam] IN (1, 2, 3)");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_loai_cach_tinh", "[loai_cach_tinh_hoc_phi] IN (N'co_dinh_theo_hoc_ky', N'theo_tin_chi', N'theo_mon_hoc')");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_nam_hoc", "[nam_hoc_trong_chuong_trinh] >= 1");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_so_thu_tu", "[so_thu_tu_hoc_ky] >= 1");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_so_tien_hoc_phi", "[so_tien_hoc_phi] >= 0");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_tien_hoc_lieu", "[tien_hoc_lieu] >= 0");
                    table.CheckConstraint("CK_CauHinhHocPhiChuongTrinh_tong_tien", "[tong_tien_du_kien] = [so_tien_hoc_phi] + [tien_hoc_lieu]");
                    table.ForeignKey(
                        name: "FK_CauHinhHocPhiChuongTrinh_ma_chuong_trinh__ChuongTrinhDaoTao",
                        column: x => x.ma_chuong_trinh_dao_tao,
                        principalSchema: "dbo",
                        principalTable: "ChuongTrinhDaoTao",
                        principalColumn: "ma_chuong_trinh");
                    table.ForeignKey(
                        name: "FK_CauHinhHocPhiChuongTrinh_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_CauHinhHocPhiChuongTrinh_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhHocPhiChuongTrinh_ma_chuong_trinh_dao_tao",
                schema: "dbo",
                table: "CauHinhHocPhiChuongTrinh",
                column: "ma_chuong_trinh_dao_tao");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhHocPhiChuongTrinh_ma_hoc_ky",
                schema: "dbo",
                table: "CauHinhHocPhiChuongTrinh",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "UQ_CauHinhHocPhiChuongTrinh_active_scope",
                schema: "dbo",
                table: "CauHinhHocPhiChuongTrinh",
                columns: new[] { "ma_don_vi", "ma_chuong_trinh_dao_tao", "ma_hoc_ky" },
                unique: true,
                filter: "[con_hoat_dong] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauHinhHocPhiChuongTrinh",
                schema: "dbo");
        }
    }
}
