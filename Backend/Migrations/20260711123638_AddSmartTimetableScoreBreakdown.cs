using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSmartTimetableScoreBreakdown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LyDoGoiYJson",
                schema: "dbo",
                table: "ScheduleDraftItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScoreBreakdownJson",
                schema: "dbo",
                table: "ScheduleDraftItem",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LyDoGoiYJson",
                schema: "dbo",
                table: "ScheduleDraftItem");

            migrationBuilder.DropColumn(
                name: "ScoreBreakdownJson",
                schema: "dbo",
                table: "ScheduleDraftItem");
        }
    }
}
