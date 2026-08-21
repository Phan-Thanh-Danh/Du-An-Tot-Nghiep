using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMauDanhGia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MauDanhGia",
                schema: "dbo",
                columns: table => new
                {
                    ma_mau_danh_gia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ten_mau = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    cau_hinh_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dang_hoat_dong = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauDanhGia", x => x.ma_mau_danh_gia);
                    table.CheckConstraint("CK_MauDanhGia_cau_hinh_json_ISJSON", "ISJSON([cau_hinh_json]) = 1");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MauDanhGia",
                schema: "dbo");
        }
    }
}
