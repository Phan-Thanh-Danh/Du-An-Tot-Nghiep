using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseSyllabusMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseSyllabus",
                schema: "dbo",
                columns: table => new
                {
                    ma_syllabus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: true),
                    ten_syllabus = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    hoc_ky_du_kien = table.Column<int>(type: "int", nullable: true),
                    bat_buoc = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSyllabus", x => x.ma_syllabus);
                    table.CheckConstraint("CK_CourseSyllabus_hoc_ky_du_kien_1", "[hoc_ky_du_kien] IS NULL OR [hoc_ky_du_kien] > 0");
                    table.CheckConstraint("CK_CourseSyllabus_trang_thai_1", "[trang_thai] IN (N'draft', N'pending_approval', N'approved', N'active', N'inactive', N'archived')");
                    table.ForeignKey(
                        name: "FK_CourseSyllabus_ma_chuyen_nganh__ChuyenNganh",
                        column: x => x.ma_chuyen_nganh,
                        principalSchema: "dbo",
                        principalTable: "ChuyenNganh",
                        principalColumn: "ma_chuyen_nganh");
                    table.ForeignKey(
                        name: "FK_CourseSyllabus_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_CourseSyllabus_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseSyllabus_ma_chuyen_nganh",
                schema: "dbo",
                table: "CourseSyllabus",
                column: "ma_chuyen_nganh");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSyllabus_ma_don_vi",
                schema: "dbo",
                table: "CourseSyllabus",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "UQ_CourseSyllabus_1",
                schema: "dbo",
                table: "CourseSyllabus",
                columns: new[] { "ma_mon_hoc", "ma_chuyen_nganh", "ma_don_vi", "version" },
                unique: true,
                filter: "[ma_don_vi] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseSyllabus",
                schema: "dbo");
        }
    }
}
