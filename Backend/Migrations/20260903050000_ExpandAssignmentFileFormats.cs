using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    public partial class ExpandAssignmentFileFormats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE dbo.BaiTap
                SET dinh_dang_cho_phep = N'["zip","rar","pdf","doc","docx","xls","xlsx","ppt","pptx","txt"]'
                WHERE dinh_dang_cho_phep IN (
                    N'["zip","rar","pdf"]',
                    N'["pdf","zip","rar"]'
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE dbo.BaiTap
                SET dinh_dang_cho_phep = N'["zip","rar","pdf"]'
                WHERE dinh_dang_cho_phep = N'["zip","rar","pdf","doc","docx","xls","xlsx","ppt","pptx","txt"]';
                """);
        }
    }
}