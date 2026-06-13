using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSpecializationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChuyenNganh_ma_nganh",
                schema: "dbo",
                table: "ChuyenNganh");

            migrationBuilder.DropIndex(
                name: "UQ_ChuyenNganh_1",
                schema: "dbo",
                table: "ChuyenNganh");

            migrationBuilder.DropColumn(
                name: "ma_code_chuyen_nganh",
                schema: "dbo",
                table: "ChuyenNganh");

            migrationBuilder.CreateIndex(
                name: "UQ_ChuyenNganh_nganh_ten",
                schema: "dbo",
                table: "ChuyenNganh",
                columns: new[] { "ma_nganh", "ten_chuyen_nganh" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_ChuyenNganh_nganh_ten",
                schema: "dbo",
                table: "ChuyenNganh");

            migrationBuilder.AddColumn<string>(
                name: "ma_code_chuyen_nganh",
                schema: "dbo",
                table: "ChuyenNganh",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE [dbo].[ChuyenNganh] SET [ma_code_chuyen_nganh] = N'CN' + CONVERT(nvarchar(20), [ma_chuyen_nganh]);");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNganh_ma_nganh",
                schema: "dbo",
                table: "ChuyenNganh",
                column: "ma_nganh");

            migrationBuilder.CreateIndex(
                name: "UQ_ChuyenNganh_1",
                schema: "dbo",
                table: "ChuyenNganh",
                column: "ma_code_chuyen_nganh",
                unique: true);
        }
    }
}
