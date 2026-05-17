using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCohortMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KhoaTuyenSinh",
                schema: "dbo",
                columns: table => new
                {
                    ma_khoa_tuyen_sinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_code_khoa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ten_khoa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    nam_bat_dau = table.Column<int>(type: "int", nullable: false),
                    nam_ket_thuc_du_kien = table.Column<int>(type: "int", nullable: true),
                    mo_ta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    con_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaTuyenSinh", x => x.ma_khoa_tuyen_sinh);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_KhoaTuyenSinh_1",
                schema: "dbo",
                table: "KhoaTuyenSinh",
                column: "ma_code_khoa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KhoaTuyenSinh",
                schema: "dbo");
        }
    }
}
