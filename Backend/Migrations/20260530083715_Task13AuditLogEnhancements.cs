using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class Task13AuditLogEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ma_don_vi",
                schema: "dbo",
                table: "NhatKyKiemToan",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ma_doi_tuong",
                schema: "dbo",
                table: "NhatKyKiemToan",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "dia_chi_ip",
                schema: "dbo",
                table: "NhatKyKiemToan",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mo_ta",
                schema: "dbo",
                table: "NhatKyKiemToan",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trace_id",
                schema: "dbo",
                table: "NhatKyKiemToan",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_agent",
                schema: "dbo",
                table: "NhatKyKiemToan",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dia_chi_ip",
                schema: "dbo",
                table: "NhatKyKiemToan");

            migrationBuilder.DropColumn(
                name: "mo_ta",
                schema: "dbo",
                table: "NhatKyKiemToan");

            migrationBuilder.DropColumn(
                name: "trace_id",
                schema: "dbo",
                table: "NhatKyKiemToan");

            migrationBuilder.DropColumn(
                name: "user_agent",
                schema: "dbo",
                table: "NhatKyKiemToan");

            migrationBuilder.AlterColumn<int>(
                name: "ma_don_vi",
                schema: "dbo",
                table: "NhatKyKiemToan",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ma_doi_tuong",
                schema: "dbo",
                table: "NhatKyKiemToan",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
