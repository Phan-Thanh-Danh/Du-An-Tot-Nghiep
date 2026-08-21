using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddKyThiMaNganh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ma_nganh",
                schema: "dbo",
                table: "KyThi",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KyThi_ma_nganh",
                schema: "dbo",
                table: "KyThi",
                column: "ma_nganh");

            migrationBuilder.AddForeignKey(
                name: "FK_KyThi_ma_nganh__NganhDaoTao",
                schema: "dbo",
                table: "KyThi",
                column: "ma_nganh",
                principalSchema: "dbo",
                principalTable: "NganhDaoTao",
                principalColumn: "ma_nganh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KyThi_ma_nganh__NganhDaoTao",
                schema: "dbo",
                table: "KyThi");

            migrationBuilder.DropIndex(
                name: "IX_KyThi_ma_nganh",
                schema: "dbo",
                table: "KyThi");

            migrationBuilder.DropColumn(
                name: "ma_nganh",
                schema: "dbo",
                table: "KyThi");
        }
    }
}
