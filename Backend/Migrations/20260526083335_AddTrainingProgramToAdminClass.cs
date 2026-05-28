using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingProgramToAdminClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ma_chuong_trinh",
                schema: "dbo",
                table: "LopHanhChinh",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LopHanhChinh_ma_chuong_trinh",
                schema: "dbo",
                table: "LopHanhChinh",
                column: "ma_chuong_trinh");

            migrationBuilder.AddForeignKey(
                name: "FK_LopHanhChinh_ma_chuong_trinh__ChuongTrinhDaoTao",
                schema: "dbo",
                table: "LopHanhChinh",
                column: "ma_chuong_trinh",
                principalSchema: "dbo",
                principalTable: "ChuongTrinhDaoTao",
                principalColumn: "ma_chuong_trinh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopHanhChinh_ma_chuong_trinh__ChuongTrinhDaoTao",
                schema: "dbo",
                table: "LopHanhChinh");

            migrationBuilder.DropIndex(
                name: "IX_LopHanhChinh_ma_chuong_trinh",
                schema: "dbo",
                table: "LopHanhChinh");

            migrationBuilder.DropColumn(
                name: "ma_chuong_trinh",
                schema: "dbo",
                table: "LopHanhChinh");
        }
    }
}
