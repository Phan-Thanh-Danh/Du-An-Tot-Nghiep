using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLopHanhChinhToKhoaHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KhoaHoc_ma_don_vi",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.AddColumn<int>(
                name: "ma_lop",
                schema: "dbo",
                table: "KhoaHoc",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE kh
                SET ma_lop = lop.ma_lop
                FROM dbo.KhoaHoc AS kh
                CROSS APPLY (
                    SELECT TOP (1) l.ma_lop
                    FROM dbo.LopHanhChinh AS l
                    WHERE l.ma_don_vi = kh.ma_don_vi
                    ORDER BY l.ma_lop
                ) AS lop
                WHERE kh.ma_lop IS NULL;
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM dbo.KhoaHoc WHERE ma_lop IS NULL)
                BEGIN
                    THROW 50001, N'Không thể gán ma_lop cho KhoaHoc cũ. Hãy reset database dev hoặc tạo LopHanhChinh cùng cơ sở trước khi chạy migration AddLopHanhChinhToKhoaHoc.', 1;
                END
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ma_lop",
                schema: "dbo",
                table: "KhoaHoc",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_ma_lop",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_lop");

            migrationBuilder.CreateIndex(
                name: "UQ_KhoaHoc_DonVi_MonHoc_HocKy_Lop",
                schema: "dbo",
                table: "KhoaHoc",
                columns: new[] { "ma_don_vi", "ma_mon_hoc", "ma_hoc_ky", "ma_lop" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_KhoaHoc_ma_lop__LopHanhChinh",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_lop",
                principalSchema: "dbo",
                principalTable: "LopHanhChinh",
                principalColumn: "ma_lop");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KhoaHoc_ma_lop__LopHanhChinh",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.DropIndex(
                name: "IX_KhoaHoc_ma_lop",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.DropIndex(
                name: "UQ_KhoaHoc_DonVi_MonHoc_HocKy_Lop",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.DropColumn(
                name: "ma_lop",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_ma_don_vi",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_don_vi");
        }
    }
}
