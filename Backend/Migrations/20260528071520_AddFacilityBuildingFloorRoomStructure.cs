using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilityBuildingFloorRoomStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PhongHoc_ma_don_vi",
                schema: "dbo",
                table: "PhongHoc");

            migrationBuilder.DropIndex(
                name: "UQ_PhongHoc_1",
                schema: "dbo",
                table: "PhongHoc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PhongHoc_loai_phong_2",
                schema: "dbo",
                table: "PhongHoc");

            migrationBuilder.AddColumn<string>(
                name: "ghi_chu",
                schema: "dbo",
                table: "PhongHoc",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_tang",
                schema: "dbo",
                table: "PhongHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ma_toa_nha",
                schema: "dbo",
                table: "PhongHoc",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ToaNha",
                schema: "dbo",
                columns: table => new
                {
                    ma_toa_nha = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_don_vi = table.Column<int>(type: "int", nullable: false),
                    ma_code_toa_nha = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_toa_nha = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    dia_chi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    so_tang = table.Column<int>(type: "int", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToaNha", x => x.ma_toa_nha);
                    table.CheckConstraint("CK_ToaNha_so_tang_1", "[so_tang] IS NULL OR [so_tang] > 0");
                    table.ForeignKey(
                        name: "FK_ToaNha_ma_don_vi__DonVi",
                        column: x => x.ma_don_vi,
                        principalSchema: "dbo",
                        principalTable: "DonVi",
                        principalColumn: "ma_don_vi");
                });

            migrationBuilder.CreateTable(
                name: "Tang",
                schema: "dbo",
                columns: table => new
                {
                    ma_tang = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_toa_nha = table.Column<int>(type: "int", nullable: false),
                    ten_tang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    thu_tu_tang = table.Column<int>(type: "int", nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tang", x => x.ma_tang);
                    table.ForeignKey(
                        name: "FK_Tang_ma_toa_nha__ToaNha",
                        column: x => x.ma_toa_nha,
                        principalSchema: "dbo",
                        principalTable: "ToaNha",
                        principalColumn: "ma_toa_nha");
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [dbo].[ToaNha] ([ma_don_vi], [ma_code_toa_nha], [ten_toa_nha], [dia_chi], [so_tang], [con_hoat_dong], [ngay_tao], [ngay_cap_nhat])
                SELECT DISTINCT [ma_don_vi], N'DEFAULT', N'Tòa nhà mặc định', NULL, 1, CAST(1 AS bit), SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM [dbo].[PhongHoc]
                WHERE [ma_toa_nha] IS NULL;

                INSERT INTO [dbo].[Tang] ([ma_toa_nha], [ten_tang], [thu_tu_tang], [mo_ta], [con_hoat_dong])
                SELECT [ma_toa_nha], N'Tầng 1', 1, N'Tầng mặc định cho dữ liệu phòng học cũ', CAST(1 AS bit)
                FROM [dbo].[ToaNha]
                WHERE [ma_code_toa_nha] = N'DEFAULT'
                  AND EXISTS (
                      SELECT 1
                      FROM [dbo].[PhongHoc]
                      WHERE [PhongHoc].[ma_don_vi] = [ToaNha].[ma_don_vi]
                        AND [PhongHoc].[ma_toa_nha] IS NULL
                  );

                UPDATE p
                SET
                    [ma_toa_nha] = b.[ma_toa_nha],
                    [ma_tang] = f.[ma_tang]
                FROM [dbo].[PhongHoc] p
                INNER JOIN [dbo].[ToaNha] b
                    ON b.[ma_don_vi] = p.[ma_don_vi]
                   AND b.[ma_code_toa_nha] = N'DEFAULT'
                INNER JOIN [dbo].[Tang] f
                    ON f.[ma_toa_nha] = b.[ma_toa_nha]
                   AND f.[thu_tu_tang] = 1
                WHERE p.[ma_toa_nha] IS NULL OR p.[ma_tang] IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_PhongHoc_ma_tang",
                schema: "dbo",
                table: "PhongHoc",
                column: "ma_tang");

            migrationBuilder.CreateIndex(
                name: "IX_PhongHoc_ma_toa_nha",
                schema: "dbo",
                table: "PhongHoc",
                column: "ma_toa_nha");

            migrationBuilder.CreateIndex(
                name: "UQ_PhongHoc_DonVi_Code",
                schema: "dbo",
                table: "PhongHoc",
                columns: new[] { "ma_don_vi", "ma_code_phong" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PhongHoc_loai_phong_2",
                schema: "dbo",
                table: "PhongHoc",
                sql: "[loai_phong] IN (N'ly_thuyet', N'phong_thi_nghiem', N'thuc_hanh', N'lab', N'hoi_truong', N'truc_tuyen', N'khac')");

            migrationBuilder.CreateIndex(
                name: "IX_Tang_ma_toa_nha",
                schema: "dbo",
                table: "Tang",
                column: "ma_toa_nha");

            migrationBuilder.CreateIndex(
                name: "UQ_Tang_ToaNha_ThuTu",
                schema: "dbo",
                table: "Tang",
                columns: new[] { "ma_toa_nha", "thu_tu_tang" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToaNha_ma_don_vi",
                schema: "dbo",
                table: "ToaNha",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "UQ_ToaNha_DonVi_Code",
                schema: "dbo",
                table: "ToaNha",
                columns: new[] { "ma_don_vi", "ma_code_toa_nha" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PhongHoc_ma_tang__Tang",
                schema: "dbo",
                table: "PhongHoc",
                column: "ma_tang",
                principalSchema: "dbo",
                principalTable: "Tang",
                principalColumn: "ma_tang");

            migrationBuilder.AddForeignKey(
                name: "FK_PhongHoc_ma_toa_nha__ToaNha",
                schema: "dbo",
                table: "PhongHoc",
                column: "ma_toa_nha",
                principalSchema: "dbo",
                principalTable: "ToaNha",
                principalColumn: "ma_toa_nha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhongHoc_ma_tang__Tang",
                schema: "dbo",
                table: "PhongHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_PhongHoc_ma_toa_nha__ToaNha",
                schema: "dbo",
                table: "PhongHoc");

            migrationBuilder.DropTable(
                name: "Tang",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ToaNha",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_PhongHoc_ma_tang",
                schema: "dbo",
                table: "PhongHoc");

            migrationBuilder.DropIndex(
                name: "IX_PhongHoc_ma_toa_nha",
                schema: "dbo",
                table: "PhongHoc");

            migrationBuilder.DropIndex(
                name: "UQ_PhongHoc_DonVi_Code",
                schema: "dbo",
                table: "PhongHoc");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PhongHoc_loai_phong_2",
                schema: "dbo",
                table: "PhongHoc");

            migrationBuilder.DropColumn(
                name: "ghi_chu",
                schema: "dbo",
                table: "PhongHoc");

            migrationBuilder.DropColumn(
                name: "ma_tang",
                schema: "dbo",
                table: "PhongHoc");

            migrationBuilder.DropColumn(
                name: "ma_toa_nha",
                schema: "dbo",
                table: "PhongHoc");

            migrationBuilder.CreateIndex(
                name: "IX_PhongHoc_ma_don_vi",
                schema: "dbo",
                table: "PhongHoc",
                column: "ma_don_vi");

            migrationBuilder.CreateIndex(
                name: "UQ_PhongHoc_1",
                schema: "dbo",
                table: "PhongHoc",
                column: "ma_code_phong",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PhongHoc_loai_phong_2",
                schema: "dbo",
                table: "PhongHoc",
                sql: "[loai_phong] IN (N'ly_thuyet', N'phong_thi_nghiem', N'hoi_truong', N'truc_tuyen', N'khac')");
        }
    }
}
