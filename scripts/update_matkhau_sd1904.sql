USE LMS;
GO

-- Cập nhật mật khẩu cho sinh viên sd1904-student01..39 (ma_nguoi_dung 32..70)
-- Mật khẩu: 123456
-- Hash: PBKDF2 (16-byte salt, 100000 iterations, SHA256) - sinh bằng đúng thuật toán Backend/Helpers/PasswordHelper.cs
-- Verify: (Rfc2898DeriveBytes.Pbkdf2("123456", salt, 100000, SHA256, 32)) -> True

DECLARE @MatKhauHash NVARCHAR(200) = N'PBKDF2.100000.aeGeNzDWuN/RwgIdVghWsw==.6LjDCvGYI2YWini67iCy+SGTQQVtaWMUeJ1e1zYhWFo=';

UPDATE NguoiDung
SET mat_khau_hash = @MatKhauHash,
    so_lan_sai_mat_khau = 0,
    dang_nhap_lan_dau = 0
WHERE ma_nguoi_dung BETWEEN 32 AND 70
  AND vai_tro_chinh = 'hoc_sinh'
  AND (mat_khau_hash IS NULL OR mat_khau_hash = '');

-- Kiểm tra kết quả
SELECT ma_nguoi_dung, email, mat_khau_hash
FROM NguoiDung
WHERE ma_nguoi_dung BETWEEN 32 AND 70
ORDER BY ma_nguoi_dung;
GO
