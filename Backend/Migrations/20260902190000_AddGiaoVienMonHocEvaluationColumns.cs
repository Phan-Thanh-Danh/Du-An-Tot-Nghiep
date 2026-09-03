using Backend.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260902190000_AddGiaoVienMonHocEvaluationColumns")]
public partial class AddGiaoVienMonHocEvaluationColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The column patch was deployed directly to some existing LargeDemo
        // databases before this migration was introduced.  Guard each DDL
        // operation so those databases can record this migration normally.
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'[GiaoVienMonHoc]', N'phu_hop_chuyen_mon') IS NULL
                ALTER TABLE [GiaoVienMonHoc] ADD [phu_hop_chuyen_mon] bit NULL;
            """);
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'[GiaoVienMonHoc]', N'diem_danh_gia') IS NULL
                ALTER TABLE [GiaoVienMonHoc] ADD [diem_danh_gia] decimal(5,2) NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'[GiaoVienMonHoc]', N'phu_hop_chuyen_mon') IS NOT NULL
                ALTER TABLE [GiaoVienMonHoc] DROP COLUMN [phu_hop_chuyen_mon];
            """);
        migrationBuilder.Sql("""
            IF COL_LENGTH(N'[GiaoVienMonHoc]', N'diem_danh_gia') IS NOT NULL
                ALTER TABLE [GiaoVienMonHoc] DROP COLUMN [diem_danh_gia];
            """);
    }
}
