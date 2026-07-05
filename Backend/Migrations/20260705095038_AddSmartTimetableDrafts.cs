using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSmartTimetableDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleGenerationJob",
                schema: "dbo",
                columns: table => new
                {
                    ma_job = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    draft_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    nguoi_yeu_cau = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "draft"),
                    tong_course = table.Column<int>(type: "int", nullable: true),
                    so_xep_duoc = table.Column<int>(type: "int", nullable: true),
                    so_khong_xep_duoc = table.Column<int>(type: "int", nullable: true),
                    score = table.Column<double>(type: "float", nullable: true),
                    tom_tat_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_xuat_ban = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleGenerationJob", x => x.ma_job);
                    table.CheckConstraint("CK_ScheduleGenerationJob_trang_thai", "[trang_thai] IN (N'draft', N'da_xuat_ban')");
                    table.ForeignKey(
                        name: "FK_ScheduleGenerationJob_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                    table.ForeignKey(
                        name: "FK_ScheduleGenerationJob_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_ScheduleGenerationJob_nguoi_yeu_cau__NguoiDung",
                        column: x => x.nguoi_yeu_cau,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung");
                });

            migrationBuilder.CreateTable(
                name: "ScheduleDraftItem",
                schema: "dbo",
                columns: table => new
                {
                    ma_draft_item = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_job = table.Column<int>(type: "int", nullable: false),
                    ma_khoa_hoc = table.Column<int>(type: "int", nullable: false),
                    thu_trong_tuan = table.Column<int>(type: "int", nullable: true),
                    ma_ca_hoc = table.Column<int>(type: "int", nullable: true),
                    ma_phong = table.Column<int>(type: "int", nullable: true),
                    trang_thai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "pending"),
                    score = table.Column<double>(type: "float", nullable: true),
                    canh_bao_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loi_json = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleDraftItem", x => x.ma_draft_item);
                    table.CheckConstraint("CK_ScheduleDraftItem_thu_trong_tuan", "[thu_trong_tuan] IS NULL OR [thu_trong_tuan] BETWEEN 1 AND 7");
                    table.CheckConstraint("CK_ScheduleDraftItem_trang_thai", "[trang_thai] IN (N'pending', N'xep_duoc', N'khong_xep_duoc')");
                    table.ForeignKey(
                        name: "FK_ScheduleDraftItem_ma_ca_hoc__CaHoc",
                        column: x => x.ma_ca_hoc,
                        principalSchema: "dbo",
                        principalTable: "CaHoc",
                        principalColumn: "ma_ca_hoc");
                    table.ForeignKey(
                        name: "FK_ScheduleDraftItem_ma_job__ScheduleGenerationJob",
                        column: x => x.ma_job,
                        principalSchema: "dbo",
                        principalTable: "ScheduleGenerationJob",
                        principalColumn: "ma_job",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleDraftItem_ma_khoa_hoc__KhoaHoc",
                        column: x => x.ma_khoa_hoc,
                        principalSchema: "dbo",
                        principalTable: "KhoaHoc",
                        principalColumn: "ma_khoa_hoc");
                    table.ForeignKey(
                        name: "FK_ScheduleDraftItem_ma_phong__PhongHoc",
                        column: x => x.ma_phong,
                        principalSchema: "dbo",
                        principalTable: "PhongHoc",
                        principalColumn: "ma_phong");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDraftItem_Job_KhoaHoc",
                schema: "dbo",
                table: "ScheduleDraftItem",
                columns: new[] { "ma_job", "ma_khoa_hoc" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDraftItem_ma_ca_hoc",
                schema: "dbo",
                table: "ScheduleDraftItem",
                column: "ma_ca_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDraftItem_ma_khoa_hoc",
                schema: "dbo",
                table: "ScheduleDraftItem",
                column: "ma_khoa_hoc");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDraftItem_ma_phong",
                schema: "dbo",
                table: "ScheduleDraftItem",
                column: "ma_phong");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleGenerationJob_DonVi_HocKy",
                schema: "dbo",
                table: "ScheduleGenerationJob",
                columns: new[] { "ma_don_vi", "ma_hoc_ky" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleGenerationJob_ma_hoc_ky",
                schema: "dbo",
                table: "ScheduleGenerationJob",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleGenerationJob_nguoi_yeu_cau",
                schema: "dbo",
                table: "ScheduleGenerationJob",
                column: "nguoi_yeu_cau");

            migrationBuilder.CreateIndex(
                name: "UQ_ScheduleGenerationJob_DraftId",
                schema: "dbo",
                table: "ScheduleGenerationJob",
                column: "draft_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleDraftItem",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ScheduleGenerationJob",
                schema: "dbo");
        }
    }
}
