using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceUnlockRequestProcessingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ghi_chu",
                schema: "dbo",
                table: "YeuCauMoKhoaDiemDanh",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ly_do_tu_choi",
                schema: "dbo",
                table: "YeuCauMoKhoaDiemDanh",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "thoi_gian_xu_ly",
                schema: "dbo",
                table: "YeuCauMoKhoaDiemDanh",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ghi_chu",
                schema: "dbo",
                table: "YeuCauMoKhoaDiemDanh");

            migrationBuilder.DropColumn(
                name: "ly_do_tu_choi",
                schema: "dbo",
                table: "YeuCauMoKhoaDiemDanh");

            migrationBuilder.DropColumn(
                name: "thoi_gian_xu_ly",
                schema: "dbo",
                table: "YeuCauMoKhoaDiemDanh");
        }
    }
}
