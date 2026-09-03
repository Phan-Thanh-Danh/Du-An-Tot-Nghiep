using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    public partial class NormalizeAssignmentFileFormats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE b
                SET dinh_dang_cho_phep = N'["zip","rar","pdf","doc","docx","xls","xlsx","ppt","pptx","txt"]'
                FROM dbo.BaiTap AS b
                WHERE ISJSON(b.dinh_dang_cho_phep) = 1
                  AND EXISTS (
                      SELECT 1
                      FROM OPENJSON(b.dinh_dang_cho_phep)
                      WHERE LOWER(value) = N'pdf'
                  )
                  AND NOT EXISTS (
                      SELECT 1
                      FROM OPENJSON(b.dinh_dang_cho_phep)
                      WHERE LOWER(value) NOT IN (N'zip', N'rar', N'pdf')
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