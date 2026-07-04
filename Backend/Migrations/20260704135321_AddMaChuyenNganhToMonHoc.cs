using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMaChuyenNganhToMonHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ma_chuyen_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucMonHoc_ma_chuyen_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc",
                column: "ma_chuyen_nganh");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhMucMonHoc_ma_chuyen_nganh__ChuyenNganh",
                schema: "dbo",
                table: "DanhMucMonHoc",
                column: "ma_chuyen_nganh",
                principalSchema: "dbo",
                principalTable: "ChuyenNganh",
                principalColumn: "ma_chuyen_nganh",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhMucMonHoc_ma_chuyen_nganh__ChuyenNganh",
                schema: "dbo",
                table: "DanhMucMonHoc");

            migrationBuilder.DropIndex(
                name: "IX_DanhMucMonHoc_ma_chuyen_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc");

            migrationBuilder.DropColumn(
                name: "ma_chuyen_nganh",
                schema: "dbo",
                table: "DanhMucMonHoc");
        }
    }
}
