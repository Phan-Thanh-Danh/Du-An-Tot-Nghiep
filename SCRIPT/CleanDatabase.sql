USE LMS;
GO

PRINT N'======================================================';
PRINT N' BẮT ĐẦU XÓA TOÀN BỘ DỮ LIỆU (RESET DATABASE) ';
PRINT N'======================================================';

-- 1. Tắt toàn bộ Foreign Key Constraints (Để không bị lỗi ràng buộc khóa ngoại khi xóa)
PRINT N'>> Đang vô hiệu hóa các ràng buộc Khóa ngoại...';
EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- 2. Dùng con trỏ (Cursor) duyệt qua tất cả các bảng và DELETE dữ liệu
-- LƯU Ý: Phải loại trừ bảng __EFMigrationsHistory để EF Core không bị hỏng lịch sử Migration
PRINT N'>> Đang tiến hành dọn dẹp dữ liệu và Reset Identity...';
DECLARE @tableName NVARCHAR(255);
DECLARE @sql NVARCHAR(MAX);

DECLARE tableCursor CURSOR FOR 
SELECT QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME <> '__EFMigrationsHistory';

OPEN tableCursor;
FETCH NEXT FROM tableCursor INTO @tableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Câu lệnh DELETE dữ liệu
    SET @sql = 'DELETE FROM ' + @tableName + ';';
    EXEC(@sql);

    -- Câu lệnh Reset cột tự tăng (IDENTITY) về 0
    -- Đưa vào BEGIN TRY CATCH vì sẽ có những bảng không có cột Identity, gọi lệnh sẽ bị văng lỗi nhẹ
    BEGIN TRY
        SET @sql = 'DBCC CHECKIDENT (''' + @tableName + ''', RESEED, 0) WITH NO_INFOMSGS;';
        EXEC(@sql);
    END TRY
    BEGIN CATCH
        -- Bỏ qua thầm lặng nếu bảng không có cột Identity
    END CATCH

    FETCH NEXT FROM tableCursor INTO @tableName;
END

CLOSE tableCursor;
DEALLOCATE tableCursor;

-- 3. Bật lại toàn bộ Foreign Key Constraints sau khi xóa xong
PRINT N'>> Đang kích hoạt lại các ràng buộc Khóa ngoại...';
EXEC sp_MSForEachTable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';

PRINT N'======================================================';
PRINT N' HOÀN TẤT RESET DỮ LIỆU! DATABASE ĐÃ TRẮNG TINH ';
PRINT N'======================================================';
GO

