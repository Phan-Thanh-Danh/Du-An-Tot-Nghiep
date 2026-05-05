using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "DonVi",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonVi_cap_don_vi",
                schema: "dbo",
                table: "DonVi",
                column: "cap_don_vi");

            migrationBuilder.CreateIndex(
                name: "IX_DonVi_con_hoat_dong",
                schema: "dbo",
                table: "DonVi",
                column: "con_hoat_dong");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DonVi_cap_don_vi",
                schema: "dbo",
                table: "DonVi");

            migrationBuilder.DropIndex(
                name: "IX_DonVi_con_hoat_dong",
                schema: "dbo",
                table: "DonVi");

            migrationBuilder.DropColumn(
                name: "ngay_cap_nhat",
                schema: "dbo",
                table: "DonVi");
        }
    }
}
