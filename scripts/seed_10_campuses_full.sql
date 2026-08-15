-- ============================================================
-- Seed 10 FPT Polytechnic Campuses — Roles, Teachers, Students,
-- Admin Classes, Teacher-Specialty, Teacher-Subject, Courses
-- Database: LMS  |  Password cho tất cả tài khoản mới: Test@123
--
-- Số lượng mỗi cơ sở (ma_don_vi 2..11):
--   + 20 Giảng viên  (chia đều 9 chuyên ngành: 3,3,2,2,2,2,2,2,2)
--   + 100 Sinh viên  (CNTT 34, TKDH 34, MKT 32)
--   + 50 Lớp hành chính (CNTT 17, TKDH 17, MKT 16 — mỗi lớp 2 SV)
--   + 9 tài khoản vai trò khác (Giáo vụ, BGH, Phụ huynh, Quản trị CS,
--     Quản trị CS con, Admin tài chính, Kế toán CS, Kế toán trưởng CS,
--     Hội đồng nội dung) + 1 Chủ tịch toàn hệ thống
--   + 50 Khóa học (1 khóa/lớp, GV cùng cơ sở & chuyên ngành)
-- Tổng: 200 GV, 1000 SV, 500 lớp, 500 khóa học, 90+ tài khoản vai trò
-- Idempotent: không tạo trùng email / ma_code_lop / (chuyen_nganh, don_vi)
-- Lưu ý: @pwd được khai báo lại trong mỗi batch (biến mất sau mỗi GO)
-- ============================================================

-- ============================================================
-- 1. ChuyenNganhTheoCoSo — mở 9 chuyên ngành cho 10 cơ sở
-- ============================================================
INSERT INTO dbo.ChuyenNganhTheoCoSo (ma_chuyen_nganh, ma_don_vi, trang_thai, chi_tieu_du_kien, con_hoat_dong, ngay_tao)
SELECT cn.ma_chuyen_nganh, dv.ma_don_vi, N'active', 40, 1, SYSUTCDATETIME()
FROM dbo.ChuyenNganh cn
CROSS JOIN dbo.DonVi dv
WHERE dv.cap_don_vi = N'co_so'
  AND NOT EXISTS (
    SELECT 1 FROM dbo.ChuyenNganhTheoCoSo x
    WHERE x.ma_chuyen_nganh = cn.ma_chuyen_nganh AND x.ma_don_vi = dv.ma_don_vi
  );
GO

-- ============================================================
-- 2. Lớp hành chính: 50 lớp/cơ sở (CNTT 17, TKDH 17, MKT 16)
--    Prefix: HN_/HCM_/DN_/CT_/TN_/HP_/DN_/BD_/QN_/HUE_
-- ============================================================
DECLARE @cs INT = 2;
WHILE @cs <= 11
BEGIN
    DECLARE @px NVARCHAR(10) = CASE @cs
        WHEN 2 THEN 'HN' WHEN 3 THEN 'HCM' WHEN 4 THEN 'DN'
        WHEN 5 THEN 'CT' WHEN 6 THEN 'TN' WHEN 7 THEN 'HP'
        WHEN 8 THEN 'DNA' WHEN 9 THEN 'BD' WHEN 10 THEN 'QN'
        ELSE 'HUE' END;

    -- CNTT: 17 lớp (ma_chuong_trinh 1)
    DECLARE @i INT = 1;
    WHILE @i <= 17
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.LopHanhChinh WHERE ma_code_lop = @px + N'_CNTT' + RIGHT('00' + CAST(@i AS NVARCHAR), 2))
        BEGIN
            INSERT INTO dbo.LopHanhChinh (ma_don_vi, ma_code_lop, ten_lop, ma_chuong_trinh, nam_nhap_hoc, si_so_du_kien, con_hoat_dong)
            VALUES (@cs, @px + N'_CNTT' + RIGHT('00' + CAST(@i AS NVARCHAR), 2),
                    @px + N' - CNTT ' + RIGHT('00' + CAST(@i AS NVARCHAR), 2) + N' (K2026)', 1, 2026, 2, 1);
        END
        SET @i = @i + 1;
    END;

    -- TKDH: 17 lớp (ma_chuong_trinh 2)
    SET @i = 1;
    WHILE @i <= 17
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.LopHanhChinh WHERE ma_code_lop = @px + N'_TKDH' + RIGHT('00' + CAST(@i AS NVARCHAR), 2))
        BEGIN
            INSERT INTO dbo.LopHanhChinh (ma_don_vi, ma_code_lop, ten_lop, ma_chuong_trinh, nam_nhap_hoc, si_so_du_kien, con_hoat_dong)
            VALUES (@cs, @px + N'_TKDH' + RIGHT('00' + CAST(@i AS NVARCHAR), 2),
                    @px + N' - TKDH ' + RIGHT('00' + CAST(@i AS NVARCHAR), 2) + N' (K2026)', 2, 2026, 2, 1);
        END
        SET @i = @i + 1;
    END;

    -- MKT: 16 lớp (ma_chuong_trinh 3)
    SET @i = 1;
    WHILE @i <= 16
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.LopHanhChinh WHERE ma_code_lop = @px + N'_MKT' + RIGHT('00' + CAST(@i AS NVARCHAR), 2))
        BEGIN
            INSERT INTO dbo.LopHanhChinh (ma_don_vi, ma_code_lop, ten_lop, ma_chuong_trinh, nam_nhap_hoc, si_so_du_kien, con_hoat_dong)
            VALUES (@cs, @px + N'_MKT' + RIGHT('00' + CAST(@i AS NVARCHAR), 2),
                    @px + N' - MKT ' + RIGHT('00' + CAST(@i AS NVARCHAR), 2) + N' (K2026)', 3, 2026, 2, 1);
        END
        SET @i = @i + 1;
    END;

    SET @cs = @cs + 1;
END;
GO

-- ============================================================
-- 3. NguoiDung — các vai trò khác (9 tài khoản/cơ sở) + Chủ tịch
-- ============================================================
DECLARE @pwd NVARCHAR(MAX) = N'PBKDF2.100000.KyXCnFpUo1bHxTDNWgjRRg==.rEaV8J40WGO8KnFojLN9RC9r7cRbSdkJTcL6sxDVqEQ=';
DECLARE @cs INT = 2;
WHILE @cs <= 11
BEGIN
    DECLARE @px NVARCHAR(10) = CASE @cs
        WHEN 2 THEN 'HN' WHEN 3 THEN 'HCM' WHEN 4 THEN 'DN'
        WHEN 5 THEN 'CT' WHEN 6 THEN 'TN' WHEN 7 THEN 'HP'
        WHEN 8 THEN 'DNA' WHEN 9 THEN 'BD' WHEN 10 THEN 'QN'
        ELSE 'HUE' END;

    IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE email = @px + N'_giaovu@lms.local')
        INSERT INTO dbo.NguoiDung (ma_don_vi, email, ho_ten, vai_tro_chinh, trang_thai, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
        VALUES (@cs, @px + N'_giaovu@lms.local', N'Giáo vụ ' + @px, N'nhan_vien', N'hoat_dong', @pwd, SYSUTCDATETIME(), 0, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE email = @px + N'_bgh@lms.local')
        INSERT INTO dbo.NguoiDung (ma_don_vi, email, ho_ten, vai_tro_chinh, trang_thai, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
        VALUES (@cs, @px + N'_bgh@lms.local', N'Ban Giám Hiệu ' + @px, N'hieu_truong', N'hoat_dong', @pwd, SYSUTCDATETIME(), 0, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE email = @px + N'_phuhuynh@lms.local')
        INSERT INTO dbo.NguoiDung (ma_don_vi, email, ho_ten, vai_tro_chinh, trang_thai, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
        VALUES (@cs, @px + N'_phuhuynh@lms.local', N'Phụ huynh ' + @px, N'phu_huynh', N'hoat_dong', @pwd, SYSUTCDATETIME(), 0, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE email = @px + N'_qtcs@lms.local')
        INSERT INTO dbo.NguoiDung (ma_don_vi, email, ho_ten, vai_tro_chinh, trang_thai, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
        VALUES (@cs, @px + N'_qtcs@lms.local', N'Quản trị cơ sở ' + @px, N'quan_tri_co_so', N'hoat_dong', @pwd, SYSUTCDATETIME(), 0, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE email = @px + N'_qtcscon@lms.local')
        INSERT INTO dbo.NguoiDung (ma_don_vi, email, ho_ten, vai_tro_chinh, trang_thai, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
        VALUES (@cs, @px + N'_qtcscon@lms.local', N'Quản trị cơ sở con ' + @px, N'quan_tri_co_so_con', N'hoat_dong', @pwd, SYSUTCDATETIME(), 0, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE email = @px + N'_admintc@lms.local')
        INSERT INTO dbo.NguoiDung (ma_don_vi, email, ho_ten, vai_tro_chinh, trang_thai, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
        VALUES (@cs, @px + N'_admintc@lms.local', N'Admin tài chính ' + @px, N'admin_tai_chinh', N'hoat_dong', @pwd, SYSUTCDATETIME(), 0, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE email = @px + N'_ketoan@lms.local')
        INSERT INTO dbo.NguoiDung (ma_don_vi, email, ho_ten, vai_tro_chinh, trang_thai, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
        VALUES (@cs, @px + N'_ketoan@lms.local', N'Kế toán cơ sở ' + @px, N'ke_toan_co_so', N'hoat_dong', @pwd, SYSUTCDATETIME(), 0, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE email = @px + N'_ketoantruong@lms.local')
        INSERT INTO dbo.NguoiDung (ma_don_vi, email, ho_ten, vai_tro_chinh, trang_thai, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
        VALUES (@cs, @px + N'_ketoantruong@lms.local', N'Kế toán trưởng cơ sở ' + @px, N'ke_toan_truong_co_so', N'hoat_dong', @pwd, SYSUTCDATETIME(), 0, 0);

    IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE email = @px + N'_hoidongnd@lms.local')
        INSERT INTO dbo.NguoiDung (ma_don_vi, email, ho_ten, vai_tro_chinh, trang_thai, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
        VALUES (@cs, @px + N'_hoidongnd@lms.local', N'Hội đồng nội dung ' + @px, N'hoidong_quanly_noidung', N'hoat_dong', @pwd, SYSUTCDATETIME(), 0, 0);

    SET @cs = @cs + 1;
END;

-- Chủ tịch hệ thống (1 tài khoản duy nhất, đặt tại cơ sở Hà Nội)
IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE email = N'chutich@lms.local')
    INSERT INTO dbo.NguoiDung (ma_don_vi, email, ho_ten, vai_tro_chinh, trang_thai, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
    VALUES (2, N'chutich@lms.local', N'Chủ tịch hệ thống', N'chu_tich', N'hoat_dong', @pwd, SYSUTCDATETIME(), 0, 0);
GO

-- ============================================================
-- 4. NguoiDung — Giảng viên: 20 GV/cơ sở, chia đều 9 chuyên ngành
--    Phân bố: CN1:3, CN2:3, CN3:2, CN4:2, CN5:2, CN6:2, CN7:2, CN8:2, CN9:2
-- ============================================================
DECLARE @pwd NVARCHAR(MAX) = N'PBKDF2.100000.KyXCnFpUo1bHxTDNWgjRRg==.rEaV8J40WGO8KnFojLN9RC9r7cRbSdkJTcL6sxDVqEQ=';
DECLARE @cs INT = 2;
WHILE @cs <= 11
BEGIN
    DECLARE @px NVARCHAR(10) = CASE @cs
        WHEN 2 THEN 'HN' WHEN 3 THEN 'HCM' WHEN 4 THEN 'DN'
        WHEN 5 THEN 'CT' WHEN 6 THEN 'TN' WHEN 7 THEN 'HP'
        WHEN 8 THEN 'DNA' WHEN 9 THEN 'BD' WHEN 10 THEN 'QN'
        ELSE 'HUE' END;

    -- MaChuyenNganh cho GV thứ k: mảng [1,1,1, 2,2,2, 3,3, 4,4, 5,5, 6,6, 7,7, 8,8, 9,9]
    DECLARE @gv INT = 1;
    WHILE @gv <= 20
    BEGIN
        DECLARE @cn INT = CASE
            WHEN @gv IN (1,2,3) THEN 1
            WHEN @gv IN (4,5,6) THEN 2
            WHEN @gv IN (7,8) THEN 3
            WHEN @gv IN (9,10) THEN 4
            WHEN @gv IN (11,12) THEN 5
            WHEN @gv IN (13,14) THEN 6
            WHEN @gv IN (15,16) THEN 7
            WHEN @gv IN (17,18) THEN 8
            ELSE 9 END;
        DECLARE @em NVARCHAR(100) = @px + N'_gv' + RIGHT('00' + CAST(@gv AS NVARCHAR), 2) + N'@lms.local';
        DECLARE @ht NVARCHAR(100) = N'GV ' + @px + N'.' + RIGHT('00' + CAST(@gv AS NVARCHAR), 2);

        IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE email = @em)
        BEGIN
            INSERT INTO dbo.NguoiDung (ma_don_vi, email, ho_ten, vai_tro_chinh, trang_thai, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
            VALUES (@cs, @em, @ht, N'giao_vien', N'hoat_dong', @pwd, SYSUTCDATETIME(), 0, 0);

            DECLARE @uid INT = SCOPE_IDENTITY();
            -- Gắn chuyên ngành (chuyên môn chính)
            INSERT INTO dbo.GiaoVienChuyenNganh (ma_giao_vien, ma_chuyen_nganh, la_chuyen_mon_chinh, muc_do_phu_hop, so_nam_kinh_nghiem, con_hoat_dong, ngay_tao)
            VALUES (@uid, @cn, 1, 75 + (@gv % 20), 2 + (@gv % 8), 1, SYSUTCDATETIME());
        END
        SET @gv = @gv + 1;
    END;

    SET @cs = @cs + 1;
END;
GO

-- ============================================================
-- 5. NguoiDung — Sinh viên: 100 SV/cơ sở (CNTT 34, TKDH 34, MKT 32)
--    Lớp: CNTT L01..L17 (mỗi lớp 2 SV), TKDH L01..L17, MKT L01..L16
-- ============================================================
DECLARE @pwd NVARCHAR(MAX) = N'PBKDF2.100000.KyXCnFpUo1bHxTDNWgjRRg==.rEaV8J40WGO8KnFojLN9RC9r7cRbSdkJTcL6sxDVqEQ=';
DECLARE @cs INT = 2;
WHILE @cs <= 11
BEGIN
    DECLARE @px NVARCHAR(10) = CASE @cs
        WHEN 2 THEN 'HN' WHEN 3 THEN 'HCM' WHEN 4 THEN 'DN'
        WHEN 5 THEN 'CT' WHEN 6 THEN 'TN' WHEN 7 THEN 'HP'
        WHEN 8 THEN 'DNA' WHEN 9 THEN 'BD' WHEN 10 THEN 'QN'
        ELSE 'HUE' END;

    DECLARE @sv INT = 1;
    WHILE @sv <= 100
    BEGIN
        -- Xác định ngành & lớp: 1-34 CNTT, 35-68 TKDH, 69-100 MKT
        DECLARE @nganh NVARCHAR(10);
        DECLARE @lopNo INT;
        DECLARE @trongLop INT;
        IF @sv <= 34
        BEGIN
            SET @nganh = 'CNTT';
            SET @lopNo = ((@sv - 1) / 2) + 1;   -- L01..L17
            SET @trongLop = ((@sv - 1) % 2) + 1;
        END
        ELSE IF @sv <= 68
        BEGIN
            SET @nganh = 'TKDH';
            SET @lopNo = ((@sv - 35) / 2) + 1;  -- L01..L17
            SET @trongLop = ((@sv - 35) % 2) + 1;
        END
        ELSE
        BEGIN
            SET @nganh = 'MKT';
            SET @lopNo = ((@sv - 69) / 2) + 1;  -- L01..L16
            SET @trongLop = ((@sv - 69) % 2) + 1;
        END

        DECLARE @maCodeLop NVARCHAR(30) = @px + N'_' + @nganh + RIGHT('00' + CAST(@lopNo AS NVARCHAR), 2);
        DECLARE @em NVARCHAR(100) = @px + N'_sv' + RIGHT('000' + CAST(@sv AS NVARCHAR), 3) + N'@lms.local';
        DECLARE @ht NVARCHAR(100) = N'SV ' + @px + N'.' + @nganh + N'.' + RIGHT('00' + CAST(@lopNo AS NVARCHAR), 2) + N'.' + CAST(@trongLop AS NVARCHAR);

        IF NOT EXISTS (SELECT 1 FROM dbo.NguoiDung WHERE email = @em)
        BEGIN
            DECLARE @lopId INT = (SELECT ma_lop FROM dbo.LopHanhChinh WHERE ma_code_lop = @maCodeLop AND ma_don_vi = @cs);
            INSERT INTO dbo.NguoiDung (ma_don_vi, email, ho_ten, vai_tro_chinh, ma_lop, trang_thai, nam_nhap_hoc, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
            VALUES (@cs, @em, @ht, N'hoc_sinh', @lopId, N'hoat_dong', 2026, @pwd, SYSUTCDATETIME(), 0, 0);
        END
        SET @sv = @sv + 1;
    END;

    SET @cs = @cs + 1;
END;
GO

-- ============================================================
-- 6. PhanQuyenNguoiDung — gán vai trò cho tất cả tài khoản mới
-- ============================================================
-- Vai trò khác theo vai_tro_chinh (1 tk/cơ sở/role)
INSERT INTO dbo.PhanQuyenNguoiDung (ma_nguoi_dung, ma_vai_tro, ngay_gan)
SELECT nd.ma_nguoi_dung, vt.ma_vai_tro, SYSUTCDATETIME()
FROM dbo.NguoiDung nd
JOIN dbo.VaiTro vt ON
    (nd.vai_tro_chinh = N'nhan_vien' AND vt.ma_code_vai_tro = N'nhan_vien') OR
    (nd.vai_tro_chinh = N'hieu_truong' AND vt.ma_code_vai_tro = N'hieu_truong') OR
    (nd.vai_tro_chinh = N'phu_huynh' AND vt.ma_code_vai_tro = N'phu_huynh') OR
    (nd.vai_tro_chinh = N'quan_tri_co_so' AND vt.ma_code_vai_tro = N'quan_tri_co_so') OR
    (nd.vai_tro_chinh = N'quan_tri_co_so_con' AND vt.ma_code_vai_tro = N'quan_tri_co_so_con') OR
    (nd.vai_tro_chinh = N'admin_tai_chinh' AND vt.ma_code_vai_tro = N'admin_tai_chinh') OR
    (nd.vai_tro_chinh = N'ke_toan_co_so' AND vt.ma_code_vai_tro = N'ke_toan_co_so') OR
    (nd.vai_tro_chinh = N'ke_toan_truong_co_so' AND vt.ma_code_vai_tro = N'ke_toan_truong_co_so') OR
    (nd.vai_tro_chinh = N'hoidong_quanly_noidung' AND vt.ma_code_vai_tro = N'hoidong_quanly_noidung') OR
    (nd.vai_tro_chinh = N'chu_tich' AND vt.ma_code_vai_tro = N'chu_tich')
WHERE nd.email LIKE N'%@lms.local'
  AND nd.email NOT IN (SELECT email FROM dbo.NguoiDung WHERE email IN
      (SELECT em FROM (VALUES (N'x')) x(em)) ) -- placeholder, không lọc thêm
  AND NOT EXISTS (
      SELECT 1 FROM dbo.PhanQuyenNguoiDung p
      WHERE p.ma_nguoi_dung = nd.ma_nguoi_dung AND p.ma_vai_tro = vt.ma_vai_tro
  );

-- Giảng viên (vai_tro_chinh = giao_vien, chưa có phân quyền)
INSERT INTO dbo.PhanQuyenNguoiDung (ma_nguoi_dung, ma_vai_tro, ngay_gan)
SELECT nd.ma_nguoi_dung, vt.ma_vai_tro, SYSUTCDATETIME()
FROM dbo.NguoiDung nd
JOIN dbo.VaiTro vt ON vt.ma_code_vai_tro = N'giao_vien'
WHERE nd.vai_tro_chinh = N'giao_vien'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.PhanQuyenNguoiDung p
      WHERE p.ma_nguoi_dung = nd.ma_nguoi_dung AND p.ma_vai_tro = vt.ma_vai_tro
  );

-- Sinh viên
INSERT INTO dbo.PhanQuyenNguoiDung (ma_nguoi_dung, ma_vai_tro, ngay_gan)
SELECT nd.ma_nguoi_dung, vt.ma_vai_tro, SYSUTCDATETIME()
FROM dbo.NguoiDung nd
JOIN dbo.VaiTro vt ON vt.ma_code_vai_tro = N'hoc_sinh'
WHERE nd.vai_tro_chinh = N'hoc_sinh'
  AND NOT EXISTS (
      SELECT 1 FROM dbo.PhanQuyenNguoiDung p
      WHERE p.ma_nguoi_dung = nd.ma_nguoi_dung AND p.ma_vai_tro = vt.ma_vai_tro
  );
GO

-- ============================================================
-- 7. GiaoVienMonHoc — gán môn học theo chuyên ngành của GV
--    CNTT (CN 1-3): môn 1-17 | TKDH (CN 4-6): môn 18-33 | MKT (CN 7-9): môn 34-49
-- ============================================================
DECLARE @cs INT = 2;
WHILE @cs <= 11
BEGIN
    DECLARE @gv INT = 1;
    WHILE @gv <= 20
    BEGIN
        DECLARE @cn INT = CASE
            WHEN @gv IN (1,2,3) THEN 1 WHEN @gv IN (4,5,6) THEN 2
            WHEN @gv IN (7,8) THEN 3 WHEN @gv IN (9,10) THEN 4
            WHEN @gv IN (11,12) THEN 5 WHEN @gv IN (13,14) THEN 6
            WHEN @gv IN (15,16) THEN 7 WHEN @gv IN (17,18) THEN 8
            ELSE 9 END;
        DECLARE @uid INT = (SELECT ma_nguoi_dung FROM dbo.NguoiDung
            WHERE email = (CASE @cs WHEN 2 THEN 'HN' WHEN 3 THEN 'HCM' WHEN 4 THEN 'DN'
                                   WHEN 5 THEN 'CT' WHEN 6 THEN 'TN' WHEN 7 THEN 'HP'
                                   WHEN 8 THEN 'DNA' WHEN 9 THEN 'BD' WHEN 10 THEN 'QN'
                                   ELSE 'HUE' END) + N'_gv' + RIGHT('00' + CAST(@gv AS NVARCHAR), 2) + N'@lms.local');

        -- Môn chính theo chuyên ngành
        DECLARE @mainSubject INT = CASE
            WHEN @cn IN (1,2,3) THEN 1 + ((@gv - 1) % 4)          -- CTDL101, COM101, COM102, COM103
            WHEN @cn IN (4,5,6) THEN 18 + ((@gv - 1) % 4)         -- DES101..DES104
            ELSE 34 + ((@gv - 1) % 4) END;                        -- MKT101..MKT104
        -- Môn phụ (cùng nhóm, dịch đi 4)
        DECLARE @secondSubject INT = CASE
            WHEN @cn IN (1,2,3) THEN 5 + ((@gv - 1) % 4)          -- WEB101, WEB102, DBI101, PRO101
            WHEN @cn IN (4,5,6) THEN 22 + ((@gv - 1) % 4)         -- DES105..DES108
            ELSE 38 + ((@gv - 1) % 4) END;                        -- MKT105..MKT108

        IF @uid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.GiaoVienMonHoc WHERE ma_giao_vien = @uid AND ma_mon_hoc = @mainSubject)
            INSERT INTO dbo.GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
            VALUES (@uid, @mainSubject, 85 + (@gv % 10), 0, 2 + (@gv % 8), 1, 1, SYSUTCDATETIME());

        IF @uid IS NOT NULL AND @secondSubject <= 49 AND NOT EXISTS (SELECT 1 FROM dbo.GiaoVienMonHoc WHERE ma_giao_vien = @uid AND ma_mon_hoc = @secondSubject)
            INSERT INTO dbo.GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
            VALUES (@uid, @secondSubject, 65 + (@gv % 10), 0, 1 + (@gv % 5), 0, 1, SYSUTCDATETIME());

        SET @gv = @gv + 1;
    END;
    SET @cs = @cs + 1;
END;
GO

-- ============================================================
-- 8. KhoaHoc — 1 khóa học cho mỗi lớp (50/cơ sở, tổng 500)
--    GV chính = GV cùng cơ sở & chuyên ngành tương ứng ngành lớp
-- ============================================================
DECLARE @cs INT = 2;
WHILE @cs <= 11
BEGIN
    DECLARE @px NVARCHAR(10) = CASE @cs
        WHEN 2 THEN 'HN' WHEN 3 THEN 'HCM' WHEN 4 THEN 'DN'
        WHEN 5 THEN 'CT' WHEN 6 THEN 'TN' WHEN 7 THEN 'HP'
        WHEN 8 THEN 'DNA' WHEN 9 THEN 'BD' WHEN 10 THEN 'QN'
        ELSE 'HUE' END;

    -- CNTT 17 lớp: môn luân phiên CTDL101/COM101/COM102/COM103
    DECLARE @i INT = 1;
    WHILE @i <= 17
    BEGIN
        DECLARE @maCodeLop NVARCHAR(30) = @px + N'_CNTT' + RIGHT('00' + CAST(@i AS NVARCHAR), 2);
        DECLARE @lopId INT = (SELECT ma_lop FROM dbo.LopHanhChinh WHERE ma_code_lop = @maCodeLop AND ma_don_vi = @cs);
        IF @lopId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.KhoaHoc WHERE ma_lop = @lopId AND ma_don_vi = @cs AND ma_hoc_ky = 2)
        BEGIN
            DECLARE @monHoc INT = 1 + ((@i - 1) % 4);
            DECLARE @tenMon NVARCHAR(100) = (SELECT ten_mon_hoc FROM dbo.DanhMucMonHoc WHERE ma_mon_hoc = @monHoc);
            DECLARE @gvId INT = (SELECT TOP 1 nd.ma_nguoi_dung
                FROM dbo.NguoiDung nd
                JOIN dbo.GiaoVienChuyenNganh gcn ON gcn.ma_giao_vien = nd.ma_nguoi_dung
                WHERE nd.ma_don_vi = @cs AND nd.vai_tro_chinh = N'giao_vien'
                  AND gcn.ma_chuyen_nganh IN (1,2,3)
                ORDER BY NEWID());
            INSERT INTO dbo.KhoaHoc (ma_don_vi, ma_giao_vien, ma_mon_hoc, ma_hoc_ky, ma_lop, SoBlockHoc, tieu_de, mo_ta, trang_thai, ngay_tao)
            VALUES (@cs, @gvId, @monHoc, 2, @lopId, 1,
                    @tenMon + N' - ' + (SELECT ten_lop FROM dbo.LopHanhChinh WHERE ma_lop = @lopId) + N' - Học kỳ 2 năm 2026',
                    N'Khóa học seed mở rộng toàn hệ thống (K2026).', N'da_xuat_ban', SYSUTCDATETIME());
        END
        SET @i = @i + 1;
    END;

    -- TKDH 17 lớp: môn luân phiên DES101/DES102/DES103/DES104
    SET @i = 1;
    WHILE @i <= 17
    BEGIN
        SET @maCodeLop = @px + N'_TKDH' + RIGHT('00' + CAST(@i AS NVARCHAR), 2);
        SET @lopId = (SELECT ma_lop FROM dbo.LopHanhChinh WHERE ma_code_lop = @maCodeLop AND ma_don_vi = @cs);
        IF @lopId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.KhoaHoc WHERE ma_lop = @lopId AND ma_don_vi = @cs AND ma_hoc_ky = 2)
        BEGIN
            SET @monHoc = 18 + ((@i - 1) % 4);
            SET @tenMon = (SELECT ten_mon_hoc FROM dbo.DanhMucMonHoc WHERE ma_mon_hoc = @monHoc);
            SET @gvId = (SELECT TOP 1 nd.ma_nguoi_dung
                FROM dbo.NguoiDung nd
                JOIN dbo.GiaoVienChuyenNganh gcn ON gcn.ma_giao_vien = nd.ma_nguoi_dung
                WHERE nd.ma_don_vi = @cs AND nd.vai_tro_chinh = N'giao_vien'
                  AND gcn.ma_chuyen_nganh IN (4,5,6)
                ORDER BY NEWID());
            INSERT INTO dbo.KhoaHoc (ma_don_vi, ma_giao_vien, ma_mon_hoc, ma_hoc_ky, ma_lop, SoBlockHoc, tieu_de, mo_ta, trang_thai, ngay_tao)
            VALUES (@cs, @gvId, @monHoc, 2, @lopId, 1,
                    @tenMon + N' - ' + (SELECT ten_lop FROM dbo.LopHanhChinh WHERE ma_lop = @lopId) + N' - Học kỳ 2 năm 2026',
                    N'Khóa học seed mở rộng toàn hệ thống (K2026).', N'da_xuat_ban', SYSUTCDATETIME());
        END
        SET @i = @i + 1;
    END;

    -- MKT 16 lớp: môn luân phiên MKT101/MKT102/MKT103/MKT104
    SET @i = 1;
    WHILE @i <= 16
    BEGIN
        SET @maCodeLop = @px + N'_MKT' + RIGHT('00' + CAST(@i AS NVARCHAR), 2);
        SET @lopId = (SELECT ma_lop FROM dbo.LopHanhChinh WHERE ma_code_lop = @maCodeLop AND ma_don_vi = @cs);
        IF @lopId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.KhoaHoc WHERE ma_lop = @lopId AND ma_don_vi = @cs AND ma_hoc_ky = 2)
        BEGIN
            SET @monHoc = 34 + ((@i - 1) % 4);
            SET @tenMon = (SELECT ten_mon_hoc FROM dbo.DanhMucMonHoc WHERE ma_mon_hoc = @monHoc);
            SET @gvId = (SELECT TOP 1 nd.ma_nguoi_dung
                FROM dbo.NguoiDung nd
                JOIN dbo.GiaoVienChuyenNganh gcn ON gcn.ma_giao_vien = nd.ma_nguoi_dung
                WHERE nd.ma_don_vi = @cs AND nd.vai_tro_chinh = N'giao_vien'
                  AND gcn.ma_chuyen_nganh IN (7,8,9)
                ORDER BY NEWID());
            INSERT INTO dbo.KhoaHoc (ma_don_vi, ma_giao_vien, ma_mon_hoc, ma_hoc_ky, ma_lop, SoBlockHoc, tieu_de, mo_ta, trang_thai, ngay_tao)
            VALUES (@cs, @gvId, @monHoc, 2, @lopId, 1,
                    @tenMon + N' - ' + (SELECT ten_lop FROM dbo.LopHanhChinh WHERE ma_lop = @lopId) + N' - Học kỳ 2 năm 2026',
                    N'Khóa học seed mở rộng toàn hệ thống (K2026).', N'da_xuat_ban', SYSUTCDATETIME());
        END
        SET @i = @i + 1;
    END;

    SET @cs = @cs + 1;
END;
GO

-- ============================================================
-- 9. Tóm tắt kết quả
-- ============================================================
SELECT N'Đơn vị' AS [Mục], COUNT(*) AS [Số lượng] FROM dbo.DonVi WHERE cap_don_vi = N'co_so'
UNION ALL SELECT N'Chuyên ngành theo cơ sở', COUNT(*) FROM dbo.ChuyenNganhTheoCoSo
UNION ALL SELECT N'Lớp hành chính (mới)', COUNT(*) FROM dbo.LopHanhChinh WHERE ma_code_lop LIKE N'%_CNTT%' OR ma_code_lop LIKE N'%_TKDH%' OR ma_code_lop LIKE N'%_MKT%'
UNION ALL SELECT N'Giảng viên (mới)', COUNT(*) FROM dbo.NguoiDung WHERE email LIKE N'%@lms.local' AND vai_tro_chinh = N'giao_vien' AND email NOT LIKE N'p12test_%'
UNION ALL SELECT N'Sinh viên (mới)', COUNT(*) FROM dbo.NguoiDung WHERE vai_tro_chinh = N'hoc_sinh' AND email LIKE N'%@lms.local' AND email NOT LIKE N'p12test_%'
UNION ALL SELECT N'Vai trò khác (mới)', COUNT(*) FROM dbo.NguoiDung WHERE vai_tro_chinh IN (N'nhan_vien',N'hieu_truong',N'phu_huynh',N'quan_tri_co_so',N'quan_tri_co_so_con',N'admin_tai_chinh',N'ke_toan_co_so',N'ke_toan_truong_co_so',N'hoidong_quanly_noidung',N'chu_tich') AND email LIKE N'%@lms.local'
UNION ALL SELECT N'Phân quyền', COUNT(*) FROM dbo.PhanQuyenNguoiDung
UNION ALL SELECT N'GV - Chuyên ngành', COUNT(*) FROM dbo.GiaoVienChuyenNganh
UNION ALL SELECT N'GV - Môn học', COUNT(*) FROM dbo.GiaoVienMonHoc
UNION ALL SELECT N'Khóa học (mới)', COUNT(*) FROM dbo.KhoaHoc
ORDER BY [Mục];
GO
