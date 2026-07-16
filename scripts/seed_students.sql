DECLARE @i INT = 1;
DECLARE @lopId INT = 4;
DECLARE @maDonVi INT = 1;

WHILE @i <= 39
BEGIN
    INSERT INTO NguoiDung (
        email, ho_ten, mat_khau_hash, 
        vai_tro_chinh, trang_thai, ma_don_vi, ma_lop, 
        ngay_tao
    )
    VALUES (
        'sv' + CAST(@i AS VARCHAR(10)) + 'sd1904@edulms.local', 
        N'Sinh viên SD1904 - ' + CAST(@i AS VARCHAR(10)), 
        '$2a$11$N5c7V9a.tVf/V6iF0IqKk.o2/l0d2F9jM5q9M5q9M5q9M5q9M5q9', 
        'hoc_sinh', 'hoat_dong', @maDonVi, @lopId, 
        GETDATE()
    );
    SET @i = @i + 1;
END
