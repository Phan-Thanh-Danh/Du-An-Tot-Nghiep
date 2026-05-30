/*
WARNING:
- Do not run this script on production.
- Use only for local dev/demo databases when you intentionally want to reset demo data
  or repair identity counters after manual cleanup.
- This script never drops the database or drops tables.
- __EFMigrationsHistory is preserved so EF Core migration state is not broken.

Usage:
1. Set @Mode = N'RESEED_ONLY' to reseed every identity table to MAX(identity_column).
   This does not change IDs that already exist, for example existing 1002/1003 rows
   will remain 1002/1003.
2. Set @Mode = N'DELETE_AND_RESEED' to delete all data except __EFMigrationsHistory,
   then reseed identity tables to 0. Use this when you want seed data to start again
   from small IDs such as 1, 2, 3, 4.
3. @DisableIdentityCache = 1 turns off SQL Server identity cache for this database
   in local dev to reduce 1000-step jumps after SQL Server service restarts.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Mode nvarchar(30) = N'RESEED_ONLY';
DECLARE @DisableIdentityCache bit = 1;

IF @Mode NOT IN (N'RESEED_ONLY', N'DELETE_AND_RESEED')
BEGIN
    THROW 50000, N'Invalid @Mode. Use RESEED_ONLY or DELETE_AND_RESEED.', 1;
END;

IF @DisableIdentityCache = 1
BEGIN
    EXEC(N'ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = OFF;');
END;

BEGIN TRY
    BEGIN TRANSACTION;

    IF @Mode = N'DELETE_AND_RESEED'
    BEGIN
        DECLARE @disableConstraintsSql nvarchar(max) = N'';
        DECLARE @deleteSql nvarchar(max) = N'';
        DECLARE @enableConstraintsSql nvarchar(max) = N'';

        SELECT @disableConstraintsSql =
            STRING_AGG(
                CONVERT(nvarchar(max), N'ALTER TABLE '
                    + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
                    + N' NOCHECK CONSTRAINT ALL;'),
                CHAR(13) + CHAR(10))
        FROM sys.tables t
        WHERE t.is_ms_shipped = 0
          AND t.name <> N'__EFMigrationsHistory';

        SELECT @deleteSql =
            STRING_AGG(
                CONVERT(nvarchar(max), N'DELETE FROM '
                    + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
                    + N';'),
                CHAR(13) + CHAR(10))
        FROM sys.tables t
        WHERE t.is_ms_shipped = 0
          AND t.name <> N'__EFMigrationsHistory';

        SELECT @enableConstraintsSql =
            STRING_AGG(
                CONVERT(nvarchar(max), N'ALTER TABLE '
                    + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
                    + N' WITH CHECK CHECK CONSTRAINT ALL;'),
                CHAR(13) + CHAR(10))
        FROM sys.tables t
        WHERE t.is_ms_shipped = 0
          AND t.name <> N'__EFMigrationsHistory';

        IF NULLIF(@disableConstraintsSql, N'') IS NOT NULL
        BEGIN
            EXEC sp_executesql @disableConstraintsSql;
        END;

        IF NULLIF(@deleteSql, N'') IS NOT NULL
        BEGIN
            EXEC sp_executesql @deleteSql;
        END;

        IF NULLIF(@enableConstraintsSql, N'') IS NOT NULL
        BEGIN
            EXEC sp_executesql @enableConstraintsSql;
        END;
    END;

    DECLARE @schemaName sysname;
    DECLARE @tableName sysname;
    DECLARE @identityColumn sysname;
    DECLARE @qualifiedTable nvarchar(517);
    DECLARE @maxIdentityValue decimal(38, 0);
    DECLARE @reseedValue decimal(38, 0);
    DECLARE @sql nvarchar(max);

    DECLARE identity_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT
            SCHEMA_NAME(t.schema_id) AS schema_name,
            t.name AS table_name,
            c.name AS identity_column
        FROM sys.tables t
        INNER JOIN sys.identity_columns c
            ON c.object_id = t.object_id
        WHERE t.is_ms_shipped = 0
          AND t.name <> N'__EFMigrationsHistory'
        ORDER BY SCHEMA_NAME(t.schema_id), t.name;

    OPEN identity_cursor;
    FETCH NEXT FROM identity_cursor INTO @schemaName, @tableName, @identityColumn;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @qualifiedTable = QUOTENAME(@schemaName) + N'.' + QUOTENAME(@tableName);

        IF @Mode = N'DELETE_AND_RESEED'
        BEGIN
            SET @reseedValue = 0;
        END
        ELSE
        BEGIN
            SET @sql = N'SELECT @maxOut = COALESCE(CONVERT(decimal(38, 0), MAX('
                + QUOTENAME(@identityColumn) + N')), 0) FROM ' + @qualifiedTable + N';';

            EXEC sp_executesql
                @sql,
                N'@maxOut decimal(38, 0) OUTPUT',
                @maxOut = @maxIdentityValue OUTPUT;

            SET @reseedValue = COALESCE(@maxIdentityValue, 0);
        END;

        SET @sql = N'DBCC CHECKIDENT (N'''
            + REPLACE(@qualifiedTable, N'''', N'''''')
            + N''', RESEED, '
            + CONVERT(nvarchar(50), @reseedValue)
            + N') WITH NO_INFOMSGS;';

        EXEC(@sql);

        FETCH NEXT FROM identity_cursor INTO @schemaName, @tableName, @identityColumn;
    END;

    CLOSE identity_cursor;
    DEALLOCATE identity_cursor;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF CURSOR_STATUS('local', 'identity_cursor') >= 0
    BEGIN
        CLOSE identity_cursor;
    END;

    IF CURSOR_STATUS('local', 'identity_cursor') > -3
    BEGIN
        DEALLOCATE identity_cursor;
    END;

    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
