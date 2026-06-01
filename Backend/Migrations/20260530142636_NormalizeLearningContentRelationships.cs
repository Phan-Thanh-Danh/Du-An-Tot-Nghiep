using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeLearningContentRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiTap_ma_khoa_hoc__KhoaHoc",
                schema: "dbo",
                table: "BaiTap");

            migrationBuilder.DropForeignKey(
                name: "FK_Chuong_ma_khoa_hoc__KhoaHoc",
                schema: "dbo",
                table: "Chuong");

            migrationBuilder.RenameColumn(
                name: "ma_khoa_hoc",
                schema: "dbo",
                table: "Chuong",
                newName: "ma_mon_hoc");

            migrationBuilder.RenameIndex(
                name: "IX_Chuong_ma_khoa_hoc",
                schema: "dbo",
                table: "Chuong",
                newName: "IX_Chuong_ma_mon_hoc");

            migrationBuilder.RenameColumn(
                name: "ma_khoa_hoc",
                schema: "dbo",
                table: "BaiTap",
                newName: "ma_mon_hoc");

            migrationBuilder.RenameIndex(
                name: "IX_BaiTap_ma_khoa_hoc",
                schema: "dbo",
                table: "BaiTap",
                newName: "IX_BaiTap_ma_mon_hoc");

            migrationBuilder.AddColumn<int>(
                name: "ma_hoc_ky",
                schema: "dbo",
                table: "KhoaHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_lop_hoc_phan",
                schema: "dbo",
                table: "KhoaHoc",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE c
                SET ma_mon_hoc = kh.ma_mon_hoc
                FROM dbo.Chuong AS c
                INNER JOIN dbo.KhoaHoc AS kh ON kh.ma_khoa_hoc = c.ma_mon_hoc;
                """);

            migrationBuilder.Sql(
                """
                UPDATE bt
                SET ma_mon_hoc = kh.ma_mon_hoc
                FROM dbo.BaiTap AS bt
                INNER JOIN dbo.KhoaHoc AS kh ON kh.ma_khoa_hoc = bt.ma_mon_hoc;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_ma_hoc_ky",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_hoc_ky");

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_ma_lop_hoc_phan",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_lop_hoc_phan");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiTap_ma_mon_hoc__DanhMucMonHoc",
                schema: "dbo",
                table: "BaiTap",
                column: "ma_mon_hoc",
                principalSchema: "dbo",
                principalTable: "DanhMucMonHoc",
                principalColumn: "ma_mon_hoc");

            migrationBuilder.AddForeignKey(
                name: "FK_Chuong_ma_mon_hoc__DanhMucMonHoc",
                schema: "dbo",
                table: "Chuong",
                column: "ma_mon_hoc",
                principalSchema: "dbo",
                principalTable: "DanhMucMonHoc",
                principalColumn: "ma_mon_hoc");

            migrationBuilder.AddForeignKey(
                name: "FK_KhoaHoc_ma_hoc_ky__HocKy",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_hoc_ky",
                principalSchema: "dbo",
                principalTable: "HocKy",
                principalColumn: "ma_hoc_ky");

            migrationBuilder.AddForeignKey(
                name: "FK_KhoaHoc_ma_lop_hoc_phan__LopHocPhan",
                schema: "dbo",
                table: "KhoaHoc",
                column: "ma_lop_hoc_phan",
                principalSchema: "dbo",
                principalTable: "LopHocPhan",
                principalColumn: "ma_lop_hoc_phan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiTap_ma_mon_hoc__DanhMucMonHoc",
                schema: "dbo",
                table: "BaiTap");

            migrationBuilder.DropForeignKey(
                name: "FK_Chuong_ma_mon_hoc__DanhMucMonHoc",
                schema: "dbo",
                table: "Chuong");

            migrationBuilder.DropForeignKey(
                name: "FK_KhoaHoc_ma_hoc_ky__HocKy",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_KhoaHoc_ma_lop_hoc_phan__LopHocPhan",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.DropIndex(
                name: "IX_KhoaHoc_ma_hoc_ky",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.DropIndex(
                name: "IX_KhoaHoc_ma_lop_hoc_phan",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.DropColumn(
                name: "ma_hoc_ky",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.DropColumn(
                name: "ma_lop_hoc_phan",
                schema: "dbo",
                table: "KhoaHoc");

            migrationBuilder.RenameColumn(
                name: "ma_mon_hoc",
                schema: "dbo",
                table: "Chuong",
                newName: "ma_khoa_hoc");

            migrationBuilder.RenameIndex(
                name: "IX_Chuong_ma_mon_hoc",
                schema: "dbo",
                table: "Chuong",
                newName: "IX_Chuong_ma_khoa_hoc");

            migrationBuilder.RenameColumn(
                name: "ma_mon_hoc",
                schema: "dbo",
                table: "BaiTap",
                newName: "ma_khoa_hoc");

            migrationBuilder.RenameIndex(
                name: "IX_BaiTap_ma_mon_hoc",
                schema: "dbo",
                table: "BaiTap",
                newName: "IX_BaiTap_ma_khoa_hoc");

            migrationBuilder.Sql(
                """
                UPDATE c
                SET ma_khoa_hoc = kh.ma_khoa_hoc
                FROM dbo.Chuong AS c
                CROSS APPLY (
                    SELECT TOP (1) k.ma_khoa_hoc
                    FROM dbo.KhoaHoc AS k
                    WHERE k.ma_mon_hoc = c.ma_khoa_hoc
                    ORDER BY k.ma_khoa_hoc
                ) AS kh;
                """);

            migrationBuilder.Sql(
                """
                UPDATE bt
                SET ma_khoa_hoc = kh.ma_khoa_hoc
                FROM dbo.BaiTap AS bt
                CROSS APPLY (
                    SELECT TOP (1) k.ma_khoa_hoc
                    FROM dbo.KhoaHoc AS k
                    WHERE k.ma_mon_hoc = bt.ma_khoa_hoc
                    ORDER BY k.ma_khoa_hoc
                ) AS kh;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_BaiTap_ma_khoa_hoc__KhoaHoc",
                schema: "dbo",
                table: "BaiTap",
                column: "ma_khoa_hoc",
                principalSchema: "dbo",
                principalTable: "KhoaHoc",
                principalColumn: "ma_khoa_hoc");

            migrationBuilder.AddForeignKey(
                name: "FK_Chuong_ma_khoa_hoc__KhoaHoc",
                schema: "dbo",
                table: "Chuong",
                column: "ma_khoa_hoc",
                principalSchema: "dbo",
                principalTable: "KhoaHoc",
                principalColumn: "ma_khoa_hoc");
        }
    }
}
