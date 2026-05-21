using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingProgramSubjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonHocTrongChuongTrinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_chuong_trinh_mon_hoc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_chuong_trinh = table.Column<int>(type: "int", nullable: false),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    hoc_ky_du_kien = table.Column<int>(type: "int", nullable: false),
                    so_tin_chi = table.Column<int>(type: "int", nullable: false),
                    loai_mon_hoc = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    bat_buoc = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    thu_tu = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ghi_chu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHocTrongChuongTrinh", x => x.ma_chuong_trinh_mon_hoc);
                    table.CheckConstraint("CK_MonHocTrongChuongTrinh_hoc_ky_du_kien", "[hoc_ky_du_kien] > 0");
                    table.CheckConstraint("CK_MonHocTrongChuongTrinh_loai_mon_hoc", "[loai_mon_hoc] IN (N'bat_buoc', N'tu_chon', N'thay_the')");
                    table.CheckConstraint("CK_MonHocTrongChuongTrinh_so_tin_chi", "[so_tin_chi] > 0");
                    table.ForeignKey(
                        name: "FK_MonHocTrongChuongTrinh_ma_chuong_trinh__ChuongTrinhDaoTao",
                        column: x => x.ma_chuong_trinh,
                        principalSchema: "dbo",
                        principalTable: "ChuongTrinhDaoTao",
                        principalColumn: "ma_chuong_trinh",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MonHocTrongChuongTrinh_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonHocTrongChuongTrinh_chuong_trinh_hoc_ky",
                schema: "dbo",
                table: "MonHocTrongChuongTrinh",
                columns: new[] { "ma_chuong_trinh", "hoc_ky_du_kien" });

            migrationBuilder.CreateIndex(
                name: "IX_MonHocTrongChuongTrinh_ma_mon_hoc",
                schema: "dbo",
                table: "MonHocTrongChuongTrinh",
                column: "ma_mon_hoc");

            migrationBuilder.CreateIndex(
                name: "UQ_MonHocTrongChuongTrinh_chuong_trinh_mon_hoc",
                schema: "dbo",
                table: "MonHocTrongChuongTrinh",
                columns: new[] { "ma_chuong_trinh", "ma_mon_hoc" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonHocTrongChuongTrinh",
                schema: "dbo");
        }
    }
}
