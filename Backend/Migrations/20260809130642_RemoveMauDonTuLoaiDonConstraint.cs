using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMauDonTuLoaiDonConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_MauDonTu_loai_don')
BEGIN
    DECLARE @tableName NVARCHAR(256);
    SELECT @tableName = OBJECT_NAME(parent_object_id) FROM sys.check_constraints WHERE name = 'CK_MauDonTu_loai_don';
    EXEC('ALTER TABLE [dbo].[' + @tableName + '] DROP CONSTRAINT [CK_MauDonTu_loai_don]');
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_MauDonTu_loai_don",
                schema: "dbo",
                table: "MauDonTu",
                sql: "[loai_don] IN (N'nghi_phep', N'thi_lai', N'chuyen_truong', N'cap_chung_chi', N'khac', N'phuc_tra_diem', N'bao_luu', N'chuyen_nganh', N'chuyen_co_so', N'xac_nhan', N'rut_hoc')");
        }
    }
}
