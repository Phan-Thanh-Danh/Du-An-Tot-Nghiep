using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMaDonViToMauBangKhen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ghi_chu",
                schema: "dbo",
                table: "TienDoBaiHoc",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_job_nguon",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "so_xung_dot_cung",
                schema: "dbo",
                table: "ScheduleGenerationJob",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_don_vi",
                schema: "dbo",
                table: "MauBangKhen",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE [dbo].[MauBangKhen] SET [ma_don_vi] = 1 WHERE [ma_don_vi] IS NULL;");

            migrationBuilder.CreateTable(
                name: "MonHocChuyenNganh",
                schema: "dbo",
                columns: table => new
                {
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_chuyen_nganh = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonHocChuyenNganh", x => new { x.ma_mon_hoc, x.ma_chuyen_nganh });
                    table.ForeignKey(
                        name: "FK_MonHocChuyenNganh_ma_chuyen_nganh__ChuyenNganh",
                        column: x => x.ma_chuyen_nganh,
                        principalSchema: "dbo",
                        principalTable: "ChuyenNganh",
                        principalColumn: "ma_chuyen_nganh",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MonHocChuyenNganh_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThoiKhoaBieu_ma_job_nguon",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_job_nguon");

            migrationBuilder.CreateIndex(
                name: "IX_MauBangKhen_ma_don_vi",
                schema: "dbo",
                table: "MauBangKhen",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocChuyenNganh_MaChuyenNganh",
                schema: "dbo",
                table: "MonHocChuyenNganh",
                column: "ma_chuyen_nganh");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocChuyenNganh_MaMonHoc",
                schema: "dbo",
                table: "MonHocChuyenNganh",
                column: "ma_mon_hoc");

            migrationBuilder.AddForeignKey(
                name: "FK_MauBangKhen_ma_don_vi__DonVi",
                schema: "dbo",
                table: "MauBangKhen",
                column: "ma_don_vi",
                principalSchema: "dbo",
                principalTable: "DonVi",
                principalColumn: "ma_don_vi");

            migrationBuilder.AddForeignKey(
                name: "FK_ThoiKhoaBieu_ma_job_nguon__ScheduleGenerationJob",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                column: "ma_job_nguon",
                principalSchema: "dbo",
                principalTable: "ScheduleGenerationJob",
                principalColumn: "ma_job",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MauBangKhen_ma_don_vi__DonVi",
                schema: "dbo",
                table: "MauBangKhen");

            migrationBuilder.DropForeignKey(
                name: "FK_ThoiKhoaBieu_ma_job_nguon__ScheduleGenerationJob",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropTable(
                name: "MonHocChuyenNganh",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_ThoiKhoaBieu_ma_job_nguon",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "IX_MauBangKhen_ma_don_vi",
                schema: "dbo",
                table: "MauBangKhen");

            migrationBuilder.DropColumn(
                name: "ghi_chu",
                schema: "dbo",
                table: "TienDoBaiHoc");

            migrationBuilder.DropColumn(
                name: "ma_job_nguon",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "so_xung_dot_cung",
                schema: "dbo",
                table: "ScheduleGenerationJob");

            migrationBuilder.DropColumn(
                name: "ma_don_vi",
                schema: "dbo",
                table: "MauBangKhen");
        }
    }
}
