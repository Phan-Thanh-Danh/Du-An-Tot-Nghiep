using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTrainingProgramToCentralizedCurriculum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChuongTrinhDaoTao_ma_chuyen_nganh_co_so__ChuyenNganhTheoCoSo",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropIndex(
                name: "UQ_ChuongTrinhDaoTao_chuyen_nganh_co_so_khoa_version",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.AddColumn<int>(
                name: "ma_chuyen_nganh",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE ctdt
                SET ma_chuyen_nganh = cncs.ma_chuyen_nganh
                FROM dbo.ChuongTrinhDaoTao AS ctdt
                INNER JOIN dbo.ChuyenNganhTheoCoSo AS cncs
                    ON ctdt.ma_chuyen_nganh_co_so = cncs.ma_chuyen_nganh_co_so
                WHERE ctdt.ma_chuyen_nganh IS NULL;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.ChuongTrinhDaoTao
                    WHERE ma_chuyen_nganh IS NULL
                )
                BEGIN
                    THROW 51000, N'Khong the migrate ChuongTrinhDaoTao: ma_chuyen_nganh_co_so khong map duoc sang ChuyenNganh.', 1;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.ChuongTrinhDaoTao
                    GROUP BY ma_chuyen_nganh, ma_khoa_tuyen_sinh, [version]
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 51001, N'Khong the migrate ChuongTrinhDaoTao: ton tai chuong trinh trung ChuyenNganh + KhoaTuyenSinh + Version sau khi tap trung hoa.', 1;
                END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ma_chuyen_nganh",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "ma_chuyen_nganh_co_so",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.CreateIndex(
                name: "UQ_ChuongTrinhDaoTao_chuyen_nganh_khoa_version",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                columns: new[] { "ma_chuyen_nganh", "ma_khoa_tuyen_sinh", "version" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChuongTrinhDaoTao_ma_chuyen_nganh__ChuyenNganh",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "ma_chuyen_nganh",
                principalSchema: "dbo",
                principalTable: "ChuyenNganh",
                principalColumn: "ma_chuyen_nganh",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChuongTrinhDaoTao_ma_chuyen_nganh__ChuyenNganh",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.DropIndex(
                name: "UQ_ChuongTrinhDaoTao_chuyen_nganh_khoa_version",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.AddColumn<int>(
                name: "ma_chuyen_nganh_co_so",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE ctdt
                SET ma_chuyen_nganh_co_so = cncs.ma_chuyen_nganh_co_so
                FROM dbo.ChuongTrinhDaoTao AS ctdt
                OUTER APPLY (
                    SELECT TOP (1) ma_chuyen_nganh_co_so
                    FROM dbo.ChuyenNganhTheoCoSo AS candidate
                    WHERE candidate.ma_chuyen_nganh = ctdt.ma_chuyen_nganh
                    ORDER BY
                        CASE
                            WHEN candidate.con_hoat_dong = 1 AND candidate.trang_thai IN (N'approved', N'active') THEN 0
                            WHEN candidate.con_hoat_dong = 1 THEN 1
                            ELSE 2
                        END,
                        candidate.ma_chuyen_nganh_co_so
                ) AS cncs
                WHERE ctdt.ma_chuyen_nganh_co_so IS NULL;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.ChuongTrinhDaoTao
                    WHERE ma_chuyen_nganh_co_so IS NULL
                )
                BEGIN
                    THROW 51002, N'Khong the rollback ChuongTrinhDaoTao: khong tim thay ChuyenNganhTheoCoSo cho ChuyenNganh.', 1;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.ChuongTrinhDaoTao
                    GROUP BY ma_chuyen_nganh_co_so, ma_khoa_tuyen_sinh, [version]
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 51003, N'Khong the rollback ChuongTrinhDaoTao: ton tai chuong trinh trung ChuyenNganhTheoCoSo + KhoaTuyenSinh + Version.', 1;
                END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ma_chuyen_nganh_co_so",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "ma_chuyen_nganh",
                schema: "dbo",
                table: "ChuongTrinhDaoTao");

            migrationBuilder.CreateIndex(
                name: "UQ_ChuongTrinhDaoTao_chuyen_nganh_co_so_khoa_version",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                columns: new[] { "ma_chuyen_nganh_co_so", "ma_khoa_tuyen_sinh", "version" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChuongTrinhDaoTao_ma_chuyen_nganh_co_so__ChuyenNganhTheoCoSo",
                schema: "dbo",
                table: "ChuongTrinhDaoTao",
                column: "ma_chuyen_nganh_co_so",
                principalSchema: "dbo",
                principalTable: "ChuyenNganhTheoCoSo",
                principalColumn: "ma_chuyen_nganh_co_so",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
