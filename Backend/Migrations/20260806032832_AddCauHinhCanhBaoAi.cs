using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCauHinhCanhBaoAi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CauHinhCanhBaoAi",
                schema: "dbo",
                columns: table => new
                {
                    MaCauHinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenQuyTac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DieuKienKichHoat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguongTriSo = table.Column<int>(type: "int", nullable: false),
                    KenhNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhCanhBaoAi", x => x.MaCauHinh);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauHinhCanhBaoAi",
                schema: "dbo");
        }
    }
}
