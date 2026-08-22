using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDataExport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "yeu_cau_xuat_du_lieu",
                columns: table => new
                {
                    ma_yeu_cau = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    loai_bao_cao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ten_bao_cao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    hoc_ky = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cap_don_vi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dinh_dang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    trang_thai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    duong_dan_file = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    nguoi_yeu_cau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    thoi_gian_yeu_cau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    thoi_gian_hoan_thanh = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_yeu_cau_xuat_du_lieu", x => x.ma_yeu_cau);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "yeu_cau_xuat_du_lieu");
        }
    }
}
