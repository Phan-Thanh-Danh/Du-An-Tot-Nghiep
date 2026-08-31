using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMaDonViToMauBangKhen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.MauBangKhen') AND name = 'ma_don_vi')
BEGIN
    ALTER TABLE [dbo].[MauBangKhen] ADD [ma_don_vi] INT NULL;
    EXEC('UPDATE [dbo].[MauBangKhen] SET [ma_don_vi] = 1 WHERE [ma_don_vi] IS NULL;');
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MauBangKhen_ma_don_vi' AND object_id = OBJECT_ID('dbo.MauBangKhen'))
BEGIN
    CREATE INDEX [IX_MauBangKhen_ma_don_vi] ON [dbo].[MauBangKhen]([ma_don_vi]);
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MauBangKhen_ma_don_vi__DonVi')
BEGIN
    ALTER TABLE [dbo].[MauBangKhen] ADD CONSTRAINT [FK_MauBangKhen_ma_don_vi__DonVi] 
    FOREIGN KEY ([ma_don_vi]) REFERENCES [dbo].[DonVi]([ma_don_vi]);
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TienDoBaiHoc') AND name = 'ghi_chu')
BEGIN
    ALTER TABLE [dbo].[TienDoBaiHoc] ADD [ghi_chu] NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ThoiKhoaBieu') AND name = 'ma_job_nguon')
BEGIN
    ALTER TABLE [dbo].[ThoiKhoaBieu] ADD [ma_job_nguon] INT NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ScheduleGenerationJob') AND name = 'so_xung_dot_cung')
BEGIN
    ALTER TABLE [dbo].[ScheduleGenerationJob] ADD [so_xung_dot_cung] INT NULL;
END

IF OBJECT_ID('dbo.MonHocChuyenNganh', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MonHocChuyenNganh] (
        [ma_mon_hoc] INT NOT NULL,
        [ma_chuyen_nganh] INT NOT NULL,
        CONSTRAINT [PK_MonHocChuyenNganh] PRIMARY KEY ([ma_mon_hoc], [ma_chuyen_nganh]),
        CONSTRAINT [FK_MonHocChuyenNganh_ma_chuyen_nganh__ChuyenNganh] FOREIGN KEY ([ma_chuyen_nganh]) REFERENCES [dbo].[ChuyenNganh]([ma_chuyen_nganh]) ON DELETE CASCADE,
        CONSTRAINT [FK_MonHocChuyenNganh_ma_mon_hoc__DanhMucMonHoc] FOREIGN KEY ([ma_mon_hoc]) REFERENCES [dbo].[DanhMucMonHoc]([ma_mon_hoc]) ON DELETE CASCADE
    );
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MauBangKhen_ma_don_vi__DonVi",
                schema: "dbo",
                table: "MauBangKhen");

            migrationBuilder.DropForeignKey(
                name: "FK_ThoiKhoaBieu_ma_job_nguon__ScheduleGenerationJob",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropTable(
                name: "MonHocChuyenNganh",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_ThoiKhoaBieu_ma_job_nguon",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropIndex(
                name: "IX_MauBangKhen_ma_don_vi",
                schema: "dbo",
                table: "MauBangKhen");

            migrationBuilder.DropColumn(
                name: "ghi_chu",
                schema: "dbo",
                table: "TienDoBaiHoc");

            migrationBuilder.DropColumn(
                name: "ma_job_nguon",
                schema: "dbo",
                table: "ThoiKhoaBieu");

            migrationBuilder.DropColumn(
                name: "so_xung_dot_cung",
                schema: "dbo",
                table: "ScheduleGenerationJob");

            migrationBuilder.DropColumn(
                name: "ma_don_vi",
                schema: "dbo",
                table: "MauBangKhen");
        }
    }
}
