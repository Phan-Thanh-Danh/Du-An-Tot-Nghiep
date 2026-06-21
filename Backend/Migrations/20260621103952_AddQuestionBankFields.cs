using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionBankFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "con_hoat_dong",
                schema: "dbo",
                table: "CauHoi",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "giai_thich_dap_an",
                schema: "dbo",
                table: "CauHoi",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "kieu_lua_chon",
                schema: "dbo",
                table: "CauHoi",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "CauHoi",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_tao",
                schema: "dbo",
                table: "CauHoi",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CauHoi_kieu_lua_chon",
                schema: "dbo",
                table: "CauHoi",
                sql: "[kieu_lua_chon] IS NULL OR [kieu_lua_chon] IN (N'chon_mot', N'chon_nhieu')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CauHoi_kieu_lua_chon",
                schema: "dbo",
                table: "CauHoi");

            migrationBuilder.DropColumn(
                name: "con_hoat_dong",
                schema: "dbo",
                table: "CauHoi");

            migrationBuilder.DropColumn(
                name: "giai_thich_dap_an",
                schema: "dbo",
                table: "CauHoi");

            migrationBuilder.DropColumn(
                name: "kieu_lua_chon",
                schema: "dbo",
                table: "CauHoi");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "CauHoi");

            migrationBuilder.DropColumn(
                name: "ngay_tao",
                schema: "dbo",
                table: "CauHoi");
        }
    }
}
