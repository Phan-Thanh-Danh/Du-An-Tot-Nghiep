using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMajorSpecializationMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NganhDaoTao",
                schema: "dbo",
                columns: table => new
                {
                    ma_nganh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_code_nganh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_nganh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NganhDaoTao", x => x.ma_nganh);
                });

            migrationBuilder.CreateTable(
                name: "ChuyenNganh",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_nganh = table.Column<int>(type: "int", nullable: false),
                    ma_code_chuyen_nganh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_chuyen_nganh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenNganh", x => x.ma_chuyen_nganh);
                    table.ForeignKey(
                        name: "FK_ChuyenNganh_ma_nganh__NganhDaoTao",
                        column: x => x.ma_nganh,
                        principalSchema: "dbo",
                        principalTable: "NganhDaoTao",
                        principalColumn: "ma_nganh");
                });

            migrationBuilder.CreateTable(
                name: "ChuyenNganhTheoCoSo",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuyen_nganh_co_so = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    nam_bat_dau = table.Column<int>(type: "int", nullable: true),
                    chi_tieu_du_kien = table.Column<int>(type: "int", nullable: true),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenNganhTheoCoSo", x => x.ma_chuyen_nganh_co_so);
                    table.CheckConstraint("CK_ChuyenNganhTheoCoSo_chi_tieu_du_kien_1", "[chi_tieu_du_kien] IS NULL OR [chi_tieu_du_kien] >= 0");
                    table.CheckConstraint("CK_ChuyenNganhTheoCoSo_nam_bat_dau_1", "[nam_bat_dau] IS NULL OR [nam_bat_dau] >= 2000");
                    table.CheckConstraint("CK_ChuyenNganhTheoCoSo_trang_thai_1", "[trang_thai] IN (N'draft', N'pending_approval', N'approved', N'active', N'inactive', N'rejected')");
                    table.ForeignKey(
                        name: "FK_ChuyenNganhTheoCoSo_ma_chuyen_nganh__ChuyenNganh",
                        column: x => x.ma_chuyen_nganh,
                        principalSchema: "dbo",
                        principalTable: "ChuyenNganh",
                        principalColumn: "ma_chuyen_nganh");
                    table.ForeignKey(
                        name: "FK_ChuyenNganhTheoCoSo_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNganh_ma_nganh",
                schema: "dbo",
                table: "ChuyenNganh",
                column: "ma_nganh");

            migrationBuilder.CreateIndex(
                name: "UQ_ChuyenNganh_1",
                schema: "dbo",
                table: "ChuyenNganh",
                column: "ma_code_chuyen_nganh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNganhTheoCoSo_ma_don_vi",
                schema: "dbo",
                table: "ChuyenNganhTheoCoSo",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "UQ_ChuyenNganhTheoCoSo_1",
                schema: "dbo",
                table: "ChuyenNganhTheoCoSo",
                columns: new[] { "ma_chuyen_nganh", "ma_don_vi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_NganhDaoTao_1",
                schema: "dbo",
                table: "NganhDaoTao",
                column: "ma_code_nganh",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChuyenNganhTheoCoSo",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ChuyenNganh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "NganhDaoTao",
                schema: "dbo");
        }
    }
}
