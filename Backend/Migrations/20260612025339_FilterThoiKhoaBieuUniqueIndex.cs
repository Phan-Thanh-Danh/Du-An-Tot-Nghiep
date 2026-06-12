using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class FilterThoiKhoaBieuUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_ThoiKhoaBieu_KhoaHoc_Thu_Ca",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.CreateIndex(
                name: "UQ_ThoiKhoaBieu_KhoaHoc_Thu_Ca",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                columns: new[] { "ma_khoa_hoc", "thu_trong_tuan", "ma_ca_hoc" },
                unique: true,
                filter: "[trang_thai] <> N'da_huy'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_ThoiKhoaBieu_KhoaHoc_Thu_Ca",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.CreateIndex(
                name: "UQ_ThoiKhoaBieu_KhoaHoc_Thu_Ca",
                schema: "dbo",
                table: "ThoiKhoaBieu",
                columns: new[] { "ma_khoa_hoc", "thu_trong_tuan", "ma_ca_hoc" },
                unique: true);
        }
    }
}
