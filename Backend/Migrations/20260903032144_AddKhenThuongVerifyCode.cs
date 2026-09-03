using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddKhenThuongVerifyCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing LargeDemo copies may have received this schema before
            // the migration history row was introduced. Preserve that data.
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[KhenThuong]', N'ma_code_xac_thuc') IS NULL
                    ALTER TABLE [dbo].[KhenThuong] ADD [ma_code_xac_thuc] nvarchar(20) NULL;
                ELSE IF EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    JOIN sys.types t ON t.user_type_id = c.user_type_id
                    WHERE c.object_id = OBJECT_ID(N'[dbo].[KhenThuong]')
                      AND c.name = N'ma_code_xac_thuc'
                      AND (t.name <> N'nvarchar' OR c.max_length <> 40 OR c.is_nullable <> 1))
                    THROW 51000, 'KhenThuong.ma_code_xac_thuc schema mismatch.', 1;

                """);
            // Keep the direct column reference in a separate command. SQL
            // Server compiles a whole batch before an ALTER TABLE takes effect.
            migrationBuilder.Sql("""
                EXEC sp_executesql N'
                    IF EXISTS (
                        SELECT 1 FROM dbo.KhenThuong
                        WHERE ma_code_xac_thuc IS NOT NULL
                        GROUP BY ma_code_xac_thuc HAVING COUNT(*) > 1)
                        THROW 51001, ''KhenThuong.ma_code_xac_thuc contains duplicate non-null values.'', 1;';
                """);
            migrationBuilder.Sql("""
                DECLARE @indexId int = INDEXPROPERTY(OBJECT_ID(N'[dbo].[KhenThuong]'), N'UQ_KhenThuong_MaCodeXacThuc', 'IndexID');
                IF @indexId IS NOT NULL
                BEGIN
                    DECLARE @isLegacy bit = 0;
                    DECLARE @filter nvarchar(max) = (SELECT filter_definition FROM sys.indexes WHERE object_id=OBJECT_ID(N'[dbo].[KhenThuong]') AND index_id=@indexId);
                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'[dbo].[KhenThuong]') AND index_id=@indexId AND is_unique=1)
                       AND (SELECT COUNT(*) FROM sys.index_columns WHERE object_id=OBJECT_ID(N'[dbo].[KhenThuong]') AND index_id=@indexId AND key_ordinal>0) = 1
                       AND NOT EXISTS (SELECT 1 FROM sys.index_columns WHERE object_id=OBJECT_ID(N'[dbo].[KhenThuong]') AND index_id=@indexId AND is_included_column=1)
                       AND EXISTS (SELECT 1 FROM sys.index_columns WHERE object_id=OBJECT_ID(N'[dbo].[KhenThuong]') AND index_id=@indexId AND key_ordinal=1 AND COL_NAME(object_id,column_id)=N'ma_code_xac_thuc')
                    BEGIN
                        IF @filter IS NULL SET @isLegacy = 1;
                        ELSE IF LOWER(REPLACE(REPLACE(REPLACE(REPLACE(@filter,N'[',N''),N']',N''),N' ',N''),N'(',N'')) NOT LIKE N'%ma_code_xac_thucisnotnull%'
                            THROW 51002, 'UQ_KhenThuong_MaCodeXacThuc filter mismatch.', 1;
                    END
                    ELSE THROW 51002, 'UQ_KhenThuong_MaCodeXacThuc schema mismatch.', 1;

                    IF @isLegacy = 1
                    BEGIN
                        -- A legacy unfiltered uniqueness rule can be either a
                        -- standalone index or a UNIQUE constraint.  SQL Server
                        -- refuses DROP INDEX for the latter, so remove the
                        -- owning constraint when present before creating the
                        -- canonical filtered unique index below.
                        DECLARE @constraintName sysname = (
                            SELECT kc.name
                            FROM sys.key_constraints kc
                            WHERE kc.parent_object_id = OBJECT_ID(N'[dbo].[KhenThuong]')
                              AND kc.unique_index_id = @indexId);
                        IF @constraintName IS NOT NULL
                        BEGIN
                            DECLARE @dropConstraintSql nvarchar(max) =
                                N'ALTER TABLE [dbo].[KhenThuong] DROP CONSTRAINT ' + QUOTENAME(@constraintName);
                            EXEC(@dropConstraintSql);
                        END
                        ELSE
                            DROP INDEX [UQ_KhenThuong_MaCodeXacThuc] ON [dbo].[KhenThuong];
                    END
                END
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[KhenThuong]') AND name = N'UQ_KhenThuong_MaCodeXacThuc')
                    CREATE UNIQUE INDEX [UQ_KhenThuong_MaCodeXacThuc]
                    ON [dbo].[KhenThuong]([ma_code_xac_thuc])
                    WHERE [ma_code_xac_thuc] IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_KhenThuong_MaCodeXacThuc",
                schema: "dbo",
                table: "KhenThuong");

            migrationBuilder.DropColumn(
                name: "ma_code_xac_thuc",
                schema: "dbo",
                table: "KhenThuong");

        }
    }
}
