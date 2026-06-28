using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardCampaignCandidates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UngVienKhenThuong",
                schema: "dbo",
                columns: table => new
                {
                    ma_ung_vien_khen_thuong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_dot_khen_thuong = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_sinh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: true),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    xep_hang = table.Column<int>(type: "int", nullable: true),
                    diem_xet = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    gpa_hoc_ky = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    tong_tin_chi = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ly_do_loai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ly_do_loai_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tieu_chi_snapshot_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ho_ten_snapshot = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    mssv_snapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ten_hoc_ky_snapshot = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    nguoi_tao = table.Column<int>(type: "int", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UngVienKhenThuong", x => x.ma_ung_vien_khen_thuong);
                    table.CheckConstraint("CK_UngVienKhenThuong_diem_xet", "[diem_xet] >= 0");
                    table.CheckConstraint("CK_UngVienKhenThuong_ly_do_loai_json_ISJSON", "[ly_do_loai_json] IS NULL OR ISJSON([ly_do_loai_json]) = 1");
                    table.CheckConstraint("CK_UngVienKhenThuong_tieu_chi_snapshot_json_ISJSON", "[tieu_chi_snapshot_json] IS NULL OR ISJSON([tieu_chi_snapshot_json]) = 1");
                    table.CheckConstraint("CK_UngVienKhenThuong_xep_hang", "[xep_hang] IS NULL OR [xep_hang] > 0");
                    table.ForeignKey(
                        name: "FK_UngVienKhenThuong_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_UngVienKhenThuong_ma_dot_khen_thuong__DotKhenThuong",
                        column: x => x.ma_dot_khen_thuong,
                        principalSchema: "dbo",
                        principalTable: "DotKhenThuong",
                        principalColumn: "ma_dot_khen_thuong",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UngVienKhenThuong_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_UngVienKhenThuong_ma_hoc_sinh__NguoiDung",
                        column: x => x.ma_hoc_sinh,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                    table.ForeignKey(
                        name: "FK_UngVienKhenThuong_nguoi_tao__NguoiDung",
                        column: x => x.nguoi_tao,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UngVienKhenThuong_CampaignRank",
                schema: "dbo",
                table: "UngVienKhenThuong",
                columns: new[] { "ma_dot_khen_thuong", "trang_thai", "xep_hang" });

            migrationBuilder.CreateIndex(
                name: "IX_UngVienKhenThuong_ma_hoc_ky",
                schema: "dbo",
                table: "UngVienKhenThuong",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_UngVienKhenThuong_nguoi_tao",
                schema: "dbo",
                table: "UngVienKhenThuong",
                column: "nguoi_tao");

            migrationBuilder.CreateIndex(
                name: "IX_UngVienKhenThuong_OrgStatus",
                schema: "dbo",
                table: "UngVienKhenThuong",
                columns: new[] { "ma_don_vi", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_UngVienKhenThuong_StudentTerm",
                schema: "dbo",
                table: "UngVienKhenThuong",
                columns: new[] { "ma_hoc_sinh", "ma_hoc_ky" });

            migrationBuilder.CreateIndex(
                name: "UQ_UngVienKhenThuong_CampaignStudent",
                schema: "dbo",
                table: "UngVienKhenThuong",
                columns: new[] { "ma_dot_khen_thuong", "ma_hoc_sinh" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UngVienKhenThuong",
                schema: "dbo");
        }
    }
}
