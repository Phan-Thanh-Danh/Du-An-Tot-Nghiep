using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionCatalogAndMatrix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuyenHan",
                schema: "dbo",
                columns: table => new
                {
                    ma_quyen_han = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ten_quyen_han = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    mo_ta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyenHan", x => x.ma_quyen_han);
                });

            migrationBuilder.CreateTable(
                name: "VaiTroQuyenHan",
                schema: "dbo",
                columns: table => new
                {
                    ma_vai_tro = table.Column<int>(type: "int", nullable: false),
                    ma_quyen_han = table.Column<int>(type: "int", nullable: false),
                    ngay_cap = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    nguoi_cap = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTroQuyenHan", x => new { x.ma_vai_tro, x.ma_quyen_han });
                    table.ForeignKey(
                        name: "FK_VaiTroQuyenHan_NguoiCap",
                        column: x => x.nguoi_cap,
                        principalSchema: "dbo",
                        principalTable: "NguoiDung",
                        principalColumn: "ma_nguoi_dung",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VaiTroQuyenHan_QuyenHan",
                        column: x => x.ma_quyen_han,
                        principalSchema: "dbo",
                        principalTable: "QuyenHan",
                        principalColumn: "ma_quyen_han",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VaiTroQuyenHan_VaiTro",
                        column: x => x.ma_vai_tro,
                        principalSchema: "dbo",
                        principalTable: "VaiTro",
                        principalColumn: "ma_vai_tro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuyenHan_MaCode",
                schema: "dbo",
                table: "QuyenHan",
                column: "ma_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VaiTroQuyenHan_ma_quyen_han",
                schema: "dbo",
                table: "VaiTroQuyenHan",
                column: "ma_quyen_han");

            migrationBuilder.CreateIndex(
                name: "IX_VaiTroQuyenHan_nguoi_cap",
                schema: "dbo",
                table: "VaiTroQuyenHan",
                column: "nguoi_cap");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VaiTroQuyenHan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "QuyenHan",
                schema: "dbo");
        }
    }
}
