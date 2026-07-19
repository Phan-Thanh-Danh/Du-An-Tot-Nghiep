using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeConfigAndProgressTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DeKiemTra_loai_de_thi",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.AddColumn<int>(
                name: "MaCauHinhDauDiem",
                schema: "dbo",
                table: "BaiTap",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LoaiDauDiemQuaTrinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_loai_dau_diem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_loai = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    thu_tu_hien_thi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiDauDiemQuaTrinh", x => x.ma_loai_dau_diem);
                });

            migrationBuilder.CreateTable(
                name: "CauHinhDauDiemQuaTrinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_cau_hinh_dau_diem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_mon_hoc = table.Column<int>(type: "int", nullable: false),
                    ma_hoc_ky = table.Column<int>(type: "int", nullable: false),
                    ma_loai_dau_diem = table.Column<int>(type: "int", nullable: false),
                    so_luong_cot = table.Column<int>(type: "int", nullable: false),
                    trong_so_noi_bo = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhDauDiemQuaTrinh", x => x.ma_cau_hinh_dau_diem);
                    table.CheckConstraint("CK_CauHinhDauDiemQuaTrinh_so_luong_cot", "[so_luong_cot] > 0");
                    table.CheckConstraint("CK_CauHinhDauDiemQuaTrinh_trong_so_noi_bo", "[trong_so_noi_bo] BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_CauHinhDauDiemQuaTrinh_ma_hoc_ky__HocKy",
                        column: x => x.ma_hoc_ky,
                        principalSchema: "dbo",
                        principalTable: "HocKy",
                        principalColumn: "ma_hoc_ky");
                    table.ForeignKey(
                        name: "FK_CauHinhDauDiemQuaTrinh_ma_loai_dau_diem__LoaiDauDiemQuaTrinh",
                        column: x => x.ma_loai_dau_diem,
                        principalSchema: "dbo",
                        principalTable: "LoaiDauDiemQuaTrinh",
                        principalColumn: "ma_loai_dau_diem");
                    table.ForeignKey(
                        name: "FK_CauHinhDauDiemQuaTrinh_ma_mon_hoc__DanhMucMonHoc",
                        column: x => x.ma_mon_hoc,
                        principalSchema: "dbo",
                        principalTable: "DanhMucMonHoc",
                        principalColumn: "ma_mon_hoc");
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "LoaiDauDiemQuaTrinh",
                columns: new[] { "ma_loai_dau_diem", "ma_code", "ten_loai", "thu_tu_hien_thi" },
                values: new object[,]
                {
                    { 1, "chuyen_can", "Chuyên cần", 1 },
                    { 2, "quiz", "Quiz", 2 },
                    { 3, "lab", "Lab", 3 },
                    { 4, "progress_test", "Progress Test", 4 },
                    { 5, "assignment", "Assignment", 5 }
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeKiemTra_loai_de_thi",
                schema: "dbo",
                table: "DeKiemTra",
                sql: "[loai_de_thi] IS NULL OR [loai_de_thi] IN (N'trac_nghiem', N'tu_luan', N'ket_hop', N'quiz_bai_hoc', N'progress_test')");

            migrationBuilder.CreateIndex(
                name: "IX_BaiTap_MaCauHinhDauDiem",
                schema: "dbo",
                table: "BaiTap",
                column: "MaCauHinhDauDiem");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhDauDiemQuaTrinh_ma_hoc_ky",
                schema: "dbo",
                table: "CauHinhDauDiemQuaTrinh",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhDauDiemQuaTrinh_ma_loai_dau_diem",
                schema: "dbo",
                table: "CauHinhDauDiemQuaTrinh",
                column: "ma_loai_dau_diem");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhDauDiemQuaTrinh_ma_mon_hoc",
                schema: "dbo",
                table: "CauHinhDauDiemQuaTrinh",
                column: "ma_mon_hoc");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiTap_ma_cau_hinh_dau_diem__CauHinhDauDiemQuaTrinh",
                schema: "dbo",
                table: "BaiTap",
                column: "MaCauHinhDauDiem",
                principalSchema: "dbo",
                principalTable: "CauHinhDauDiemQuaTrinh",
                principalColumn: "ma_cau_hinh_dau_diem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiTap_ma_cau_hinh_dau_diem__CauHinhDauDiemQuaTrinh",
                schema: "dbo",
                table: "BaiTap");

            migrationBuilder.DropTable(
                name: "CauHinhDauDiemQuaTrinh",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LoaiDauDiemQuaTrinh",
                schema: "dbo");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DeKiemTra_loai_de_thi",
                schema: "dbo",
                table: "DeKiemTra");

            migrationBuilder.DropIndex(
                name: "IX_BaiTap_MaCauHinhDauDiem",
                schema: "dbo",
                table: "BaiTap");

            migrationBuilder.DropColumn(
                name: "MaCauHinhDauDiem",
                schema: "dbo",
                table: "BaiTap");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DeKiemTra_loai_de_thi",
                schema: "dbo",
                table: "DeKiemTra",
                sql: "[loai_de_thi] IS NULL OR [loai_de_thi] IN (N'trac_nghiem', N'tu_luan', N'ket_hop', N'quiz_bai_hoc')");
        }
    }
}
