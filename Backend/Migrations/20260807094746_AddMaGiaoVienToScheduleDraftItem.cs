using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMaGiaoVienToScheduleDraftItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ma_giao_vien",
                schema: "dbo",
                table: "ScheduleDraftItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "muc_do_phu_hop",
                schema: "dbo",
                table: "ScheduleDraftItem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDraftItem_ma_giao_vien",
                schema: "dbo",
                table: "ScheduleDraftItem",
                column: "ma_giao_vien");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleDraftItem_ma_giao_vien__NguoiDung",
                schema: "dbo",
                table: "ScheduleDraftItem",
                column: "ma_giao_vien",
                principalSchema: "dbo",
                principalTable: "NguoiDung",
                principalColumn: "ma_nguoi_dung");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleDraftItem_ma_giao_vien__NguoiDung",
                schema: "dbo",
                table: "ScheduleDraftItem");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleDraftItem_ma_giao_vien",
                schema: "dbo",
                table: "ScheduleDraftItem");

            migrationBuilder.DropColumn(
                name: "ma_giao_vien",
                schema: "dbo",
                table: "ScheduleDraftItem");

            migrationBuilder.DropColumn(
                name: "muc_do_phu_hop",
                schema: "dbo",
                table: "ScheduleDraftItem");
        }
    }
}
