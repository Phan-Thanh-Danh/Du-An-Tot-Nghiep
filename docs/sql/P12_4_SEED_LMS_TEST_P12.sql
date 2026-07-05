-- ============================================================
-- P12.4 — Seed LMS_TEST_P12 for Smart Timetable API Smoke
-- Branch: feature/p12-3-smart-timetable-stress-smoke
-- Date: 2026-07-05
-- Database: LMS_TEST_P12 (DELL\SQLEXPRESS02)
-- Password for all test users: Test@123
-- NOTE: All column names use snake_case (actual DB columns)
-- ============================================================

-- ============================================================
-- Helper: verify all required tables exist
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DonVi')
    THROW 50000, N'Missing required tables. Run dotnet ef database update first.', 1;
GO

-- ============================================================
-- 1. DonVi (Campus)
-- ============================================================
SET IDENTITY_INSERT dbo.DonVi ON;
MERGE dbo.DonVi AS t
USING (SELECT 1 AS mid, NULL AS m_cha, N'Cơ sở Test P12' AS ten, N'co_so' AS cap, 1 AS hd) AS s
ON (t.ma_don_vi = s.mid)
WHEN NOT MATCHED THEN INSERT (ma_don_vi, ma_don_vi_cha, ten_don_vi, cap_don_vi, con_hoat_dong, ngay_tao)
VALUES (s.mid, s.m_cha, s.ten, s.cap, s.hd, SYSUTCDATETIME());
SET IDENTITY_INSERT dbo.DonVi OFF;
GO

-- ============================================================
-- 2. VaiTro (Roles) — Note: ma_vai_tro is NOT identity
-- ============================================================
MERGE dbo.VaiTro AS t
USING (VALUES
    (1, N'sieu_quan_tri', N'Siêu quản trị'),
    (2, N'quan_tri', N'Quản trị hệ thống'),
    (3, N'giao_vien', N'Giảng viên'),
    (4, N'hoc_sinh', N'Sinh viên'),
    (5, N'nhan_vien', N'Giáo vụ'),
    (6, N'hieu_truong', N'Ban Giám Hiệu')
) AS s(mid, code, ten)
ON (t.ma_vai_tro = s.mid)
WHEN NOT MATCHED THEN INSERT (ma_vai_tro, ma_code_vai_tro, ten_vai_tro)
VALUES (s.mid, s.code, s.ten);

-- ============================================================
-- 3. HocKy (Academic Term)
-- ============================================================
SET IDENTITY_INSERT dbo.HocKy ON;
MERGE dbo.HocKy AS t
USING (SELECT
    1 AS mid, 1 AS dvid, N'HK2_2026_P12TEST' AS code, N'Học kỳ 2 năm 2026 (Test P12)' AS ten,
    CAST('2026-05-01' AS DATE) AS bd, CAST('2026-08-31' AS DATE) AS kt,
    N'2026' AS nam, 2 AS thu_tu,
    CAST('2026-09-05' AS DATE) AS bd5, 0 AS khoa, 24 AS tctd, CAST('2026-05-15' AS DATE) AS han
) AS s
ON (t.ma_hoc_ky = s.mid)
WHEN NOT MATCHED THEN INSERT (ma_hoc_ky, ma_don_vi, ma_code_hoc_ky, ten_hoc_ky, ngay_bat_dau, ngay_ket_thuc, nam_hoc, thu_tu_trong_nam, ngay_ket_thuc_block5, da_khoa, so_tin_chi_toi_da, han_rut_mon)
VALUES (s.mid, s.dvid, s.code, s.ten, s.bd, s.kt, s.nam, s.thu_tu, s.bd5, s.khoa, s.tctd, s.han);
SET IDENTITY_INSERT dbo.HocKy OFF;
GO

-- ============================================================
-- 4. CaHoc (Shifts)
-- ============================================================
SET IDENTITY_INSERT dbo.CaHoc ON;
MERGE dbo.CaHoc AS t
USING (VALUES
    (1, N'Ca 1 (Sáng)', N'sang', CAST('07:00' AS TIME), CAST('09:30' AS TIME), 1, 1),
    (2, N'Ca 2 (Sáng)', N'sang', CAST('09:45' AS TIME), CAST('12:15' AS TIME), 2, 1),
    (3, N'Ca 3 (Chiều)', N'chieu', CAST('13:00' AS TIME), CAST('15:30' AS TIME), 3, 1),
    (4, N'Ca 4 (Chiều)', N'chieu', CAST('15:45' AS TIME), CAST('18:15' AS TIME), 4, 1),
    (5, N'Ca 5 (Tối) - Inactive', N'toi', CAST('18:30' AS TIME), CAST('21:00' AS TIME), 5, 0)
) AS s(mid, ten, buoi, gbd, gkt, tt, hd)
ON (t.ma_ca_hoc = s.mid)
WHEN NOT MATCHED THEN INSERT (ma_ca_hoc, ten_ca, buoi, gio_bat_dau, gio_ket_thuc, thu_tu, con_hoat_dong)
VALUES (s.mid, s.ten, s.buoi, s.gbd, s.gkt, s.tt, s.hd);
SET IDENTITY_INSERT dbo.CaHoc OFF;
GO

-- ============================================================
-- 5. ToaNha (Building)
-- ============================================================
SET IDENTITY_INSERT dbo.ToaNha ON;
MERGE dbo.ToaNha AS t
USING (SELECT 1 AS mid, 1 AS dvid, N'P12-A' AS code, N'Tòa A (Test P12)' AS ten, NULL AS dc, 3 AS st, 1 AS hd) AS s
ON (t.ma_toa_nha = s.mid)
WHEN NOT MATCHED THEN INSERT (ma_toa_nha, ma_don_vi, ma_code_toa_nha, ten_toa_nha, dia_chi, so_tang, con_hoat_dong, ngay_tao)
VALUES (s.mid, s.dvid, s.code, s.ten, s.dc, s.st, s.hd, SYSUTCDATETIME());
SET IDENTITY_INSERT dbo.ToaNha OFF;
GO

-- ============================================================
-- 6. Tang (Floor)
-- ============================================================
SET IDENTITY_INSERT dbo.Tang ON;
MERGE dbo.Tang AS t
USING (VALUES
    (1, 1, N'Tầng 1', 1, NULL, 1),
    (2, 1, N'Tầng 2', 2, NULL, 1)
) AS s(mid, tn, ten, tt, mo, hd)
ON (t.ma_tang = s.mid)
WHEN NOT MATCHED THEN INSERT (ma_tang, ma_toa_nha, ten_tang, thu_tu_tang, mo_ta, con_hoat_dong)
VALUES (s.mid, s.tn, s.ten, s.tt, s.mo, s.hd);
SET IDENTITY_INSERT dbo.Tang OFF;
GO

-- ============================================================
-- 7. PhongHoc (Rooms)
-- ============================================================
SET IDENTITY_INSERT dbo.PhongHoc ON;
MERGE dbo.PhongHoc AS t
USING (VALUES
    (1,  1, 1, 1, N'P12-P01', N'Phòng 01', 40, N'ly_thuyet', N'hoat_dong', NULL),
    (2,  1, 1, 1, N'P12-P02', N'Phòng 02', 40, N'ly_thuyet', N'hoat_dong', NULL),
    (3,  1, 1, 1, N'P12-P03', N'Phòng 03', 40, N'ly_thuyet', N'hoat_dong', NULL),
    (4,  1, 1, 1, N'P12-P04', N'Phòng 04', 40, N'ly_thuyet', N'hoat_dong', NULL),
    (5,  1, 1, 1, N'P12-P05', N'Phòng 05', 40, N'ly_thuyet', N'hoat_dong', NULL),
    (6,  1, 1, 1, N'P12-P06', N'Phòng 06', 40, N'ly_thuyet', N'hoat_dong', NULL),
    (7,  1, 1, 1, N'P12-LAB1', N'Lab 01', 35, N'lab', N'hoat_dong', NULL),
    (8,  1, 1, 1, N'P12-LAB2', N'Lab 02', 35, N'lab', N'hoat_dong', NULL),
    (9,  1, 1, 1, N'P12-P09', N'Phòng 09 (Nhỏ)', 20, N'ly_thuyet', N'hoat_dong', N'Phòng nhỏ cho lớp ít'),
    (10, 1, 1, 1, N'P12-P10', N'Phòng 10 (Inactive)', 40, N'ly_thuyet', N'ngung_hoat_dong', N'Phòng inactive test block')
) AS s(mid, dvid, tn, tang, code, ten, sc, loai, tt, gc)
ON (t.ma_phong = s.mid)
WHEN NOT MATCHED THEN INSERT (ma_phong, ma_don_vi, ma_toa_nha, ma_tang, ma_code_phong, ten_phong, suc_chua, loai_phong, trang_thai_phong, ghi_chu)
VALUES (s.mid, s.dvid, s.tn, s.tang, s.code, s.ten, s.sc, s.loai, s.tt, s.gc);
SET IDENTITY_INSERT dbo.PhongHoc OFF;
GO

-- ============================================================
-- 8. DanhMucMonHoc (Subjects)
-- ============================================================
SET IDENTITY_INSERT dbo.DanhMucMonHoc ON;
MERGE dbo.DanhMucMonHoc AS t
USING (VALUES
    (1, N'CSHARP', N'Lập trình C#', 3, 1),
    (2, N'SQL101', N'SQL Server', 3, 1),
    (3, N'WEB101', N'Thiết kế Web', 3, 1),
    (4, N'PSHOP', N'Photoshop', 3, 1)
) AS s(mid, code, ten, tc, hd)
ON (t.ma_mon_hoc = s.mid)
WHEN NOT MATCHED THEN INSERT (ma_mon_hoc, ma_code_mon_hoc, ten_mon_hoc, so_tin_chi, con_hoat_dong)
VALUES (s.mid, s.code, s.ten, s.tc, s.hd);
SET IDENTITY_INSERT dbo.DanhMucMonHoc OFF;
GO

-- ============================================================
-- 9. LopHanhChinh (Classes)
-- ============================================================
SET IDENTITY_INSERT dbo.LopHanhChinh ON;
MERGE dbo.LopHanhChinh AS t
USING (VALUES
    (1,  1, N'P12_LOP01', N'Lớp P12.01 - CNTT',  NULL, NULL, 2026, 1),
    (2,  1, N'P12_LOP02', N'Lớp P12.02 - CNTT',  NULL, NULL, 2026, 1),
    (3,  1, N'P12_LOP03', N'Lớp P12.03 - CNTT',  NULL, NULL, 2026, 1),
    (4,  1, N'P12_LOP04', N'Lớp P12.04 - CNTT',  NULL, NULL, 2026, 1),
    (5,  1, N'P12_LOP05', N'Lớp P12.05 - CNTT',  NULL, NULL, 2026, 1),
    (6,  1, N'P12_LOP06', N'Lớp P12.06 - CNTT',  NULL, NULL, 2026, 1),
    (7,  1, N'P12_LOP07', N'Lớp P12.07 - TKDH',  NULL, NULL, 2026, 1),
    (8,  1, N'P12_LOP08', N'Lớp P12.08 - TKDH',  NULL, NULL, 2026, 1),
    (9,  1, N'P12_LOP09', N'Lớp P12.09 - CNTT (Ít)',  NULL, NULL, 2026, 1),
    (10, 1, N'P12_LOP10', N'Lớp P12.10 - CNTT (Đông)', NULL, NULL, 2026, 1)
) AS s(mid, dvid, code, ten, gvcn, ct, nh, hd)
ON (t.ma_lop = s.mid)
WHEN NOT MATCHED THEN INSERT (ma_lop, ma_don_vi, ma_code_lop, ten_lop, ma_giao_vien_chu_nhiem, ma_chuong_trinh, nam_nhap_hoc, con_hoat_dong)
VALUES (s.mid, s.dvid, s.code, s.ten, s.gvcn, s.ct, s.nh, s.hd);
SET IDENTITY_INSERT dbo.LopHanhChinh OFF;
GO

-- ============================================================
-- 10. NguoiDung (Users) + Students
-- Password hash for "Test@123"
-- ============================================================
DECLARE @pwd NVARCHAR(MAX) = N'PBKDF2.100000.f+H7yfqvHWe52DMplQT89A==.JoMAVXk00JZD2JXNw5SL0OdmfM03d3DTZQDlaRU2xfw=';

SET IDENTITY_INSERT dbo.NguoiDung ON;

-- Teachers (IDs 1-9)
MERGE dbo.NguoiDung AS t
USING (VALUES
    (1,  1, N'p12test_teacher01@lms.local', N'Nguyễn Văn An', N'giao_vien', NULL, NULL, N'hoat_dong', NULL, @pwd),
    (2,  1, N'p12test_teacher02@lms.local', N'Trần Thị Bình', N'giao_vien', NULL, NULL, N'hoat_dong', NULL, @pwd),
    (3,  1, N'p12test_teacher03@lms.local', N'Phạm Văn Cường', N'giao_vien', NULL, NULL, N'hoat_dong', NULL, @pwd),
    (4,  1, N'p12test_teacher04@lms.local', N'Đỗ Thị Dung', N'giao_vien', NULL, NULL, N'hoat_dong', NULL, @pwd),
    (5,  1, N'p12test_teacher05@lms.local', N'Lê Văn Em', N'giao_vien', NULL, NULL, N'hoat_dong', NULL, @pwd),
    (6,  1, N'p12test_teacher06@lms.local', N'Hoàng Thị Phương', N'giao_vien', NULL, NULL, N'hoat_dong', NULL, @pwd),
    (7,  1, N'p12test_teacher07@lms.local', N'Ngô Văn Giàu', N'giao_vien', NULL, NULL, N'hoat_dong', NULL, @pwd),
    (8,  1, N'p12test_teacher08@lms.local', N'Dương Thị Hạnh', N'giao_vien', NULL, NULL, N'hoat_dong', NULL, @pwd),
    (9,  1, N'p12test_teacher09_inactive@lms.local', N'Võ Văn Inactive', N'giao_vien', NULL, NULL, N'bi_khoa', NULL, @pwd),
    -- Staff (GiaoVu)
    (10, 1, N'p12test_staff01@lms.local', N'Phạm Thị Giáo Vụ', N'nhan_vien', NULL, NULL, N'hoat_dong', NULL, @pwd)
) AS s(mid, dvid, email, hoten, vaitro, lop, sdt, trangthai, nh, hash)
ON (t.ma_nguoi_dung = s.mid)
WHEN NOT MATCHED THEN INSERT (ma_nguoi_dung, ma_don_vi, email, ho_ten, vai_tro_chinh, ma_lop, so_dien_thoai, trang_thai, nam_nhap_hoc, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
VALUES (s.mid, s.dvid, s.email, s.hoten, s.vaitro, s.lop, s.sdt, s.trangthai, s.nh, s.hash, SYSUTCDATETIME(), 0, 0);

-- Students (IDs 11-310, 300 students, ~30 per class)
DECLARE @i INT = 11;
WHILE @i <= 310
BEGIN
    DECLARE @ml INT = ((@i - 11) / 30) + 1;
    DECLARE @stt INT = (@i - 11) % 30 + 1;
    DECLARE @em NVARCHAR(100) = N'p12test_student' + RIGHT('000' + CAST(@i AS NVARCHAR), 3) + N'@lms.local';
    DECLARE @ht NVARCHAR(100) = N'SV P12.' + RIGHT('00' + CAST(@ml AS NVARCHAR), 2) + N'.' + RIGHT('00' + CAST(@stt AS NVARCHAR), 2);

    MERGE dbo.NguoiDung AS t
    USING (SELECT @i AS mid, 1 AS dvid, @em AS em, @ht AS ht, N'hoc_sinh' AS vt, @ml AS lop, @pwd AS hash) AS s
    ON (t.ma_nguoi_dung = s.mid)
    WHEN NOT MATCHED THEN INSERT (ma_nguoi_dung, ma_don_vi, email, ho_ten, vai_tro_chinh, ma_lop, trang_thai, nam_nhap_hoc, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
    VALUES (s.mid, s.dvid, s.em, s.ht, s.vt, s.lop, N'hoat_dong', 2026, s.hash, SYSUTCDATETIME(), 0, 0);

    SET @i = @i + 1;
END;

-- SuperAdmin (ID 311)
MERGE dbo.NguoiDung AS t
USING (SELECT 311 AS mid, 1 AS dvid, N'superadmin@lms.local' AS em, N'Super Admin' AS ht, N'sieu_quan_tri' AS vt, NULL AS lop, @pwd AS hash) AS s
ON (t.ma_nguoi_dung = s.mid)
WHEN NOT MATCHED THEN INSERT (ma_nguoi_dung, ma_don_vi, email, ho_ten, vai_tro_chinh, ma_lop, trang_thai, nam_nhap_hoc, mat_khau_hash, ngay_tao, so_lan_sai_mat_khau, dang_nhap_lan_dau)
VALUES (s.mid, s.dvid, s.em, s.ht, s.vt, s.lop, N'hoat_dong', 2026, s.hash, SYSUTCDATETIME(), 0, 0);

SET IDENTITY_INSERT dbo.NguoiDung OFF;
GO

-- ============================================================
-- 11. PhanQuyenNguoiDung for staff and superadmin
-- ============================================================
MERGE dbo.PhanQuyenNguoiDung AS t
USING (VALUES (10, 5), (311, 1)) AS s(uid, rid)
ON (t.ma_nguoi_dung = s.uid AND t.ma_vai_tro = s.rid)
WHEN NOT MATCHED THEN INSERT (ma_nguoi_dung, ma_vai_tro)
VALUES (s.uid, s.rid);
GO

-- ============================================================
-- 12. GiaoVienMonHoc (Teacher-Subject capability)
-- ============================================================
MERGE dbo.GiaoVienMonHoc AS t
USING (VALUES
    (1, 1, 90, 5, 3, 1, 1),
    (1, 2, 70, 3, 2, 0, 1),
    (2, 1, 85, 4, 2, 1, 1),
    (2, 2, 75, 2, 1, 0, 1),
    (3, 2, 95, 6, 4, 1, 1),
    (3, 3, 65, 2, 1, 0, 1),
    (4, 2, 80, 4, 3, 1, 1),
    (4, 3, 70, 3, 2, 0, 1),
    (5, 3, 90, 5, 3, 1, 1),
    (5, 1, 60, 1, 1, 0, 1),
    (6, 3, 85, 4, 3, 1, 1),
    (6, 1, 65, 2, 1, 0, 1),
    (7, 4, 95, 7, 5, 1, 1),
    (8, 4, 80, 4, 3, 1, 1)
) AS s(gv, mh, md, lan, nam, chinh, hd)
ON (t.ma_giao_vien = s.gv AND t.ma_mon_hoc = s.mh)
WHEN NOT MATCHED THEN INSERT (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
VALUES (s.gv, s.mh, s.md, s.lan, s.nam, s.chinh, s.hd, SYSUTCDATETIME());
GO

-- ============================================================
-- 13. KhoaHoc (Courses) — 20 courses
-- ============================================================
SET IDENTITY_INSERT dbo.KhoaHoc ON;
MERGE dbo.KhoaHoc AS t
USING (VALUES
    (1,  1, 1, 1, 1, 1, NULL, N'C# Cho Lớp P12.01', NULL, N'nhap'),
    (2,  1, 1, 1, 1, 2, NULL, N'C# Cho Lớp P12.02', NULL, N'nhap'),
    (3,  1, 2, 1, 1, 3, NULL, N'C# Cho Lớp P12.03', NULL, N'nhap'),
    (4,  1, 2, 1, 1, 4, NULL, N'C# Cho Lớp P12.04', NULL, N'nhap'),
    (5,  1, 1, 1, 1, 5, NULL, N'C# Cho Lớp P12.05', NULL, N'nhap'),
    (6,  1, 3, 2, 1, 1, NULL, N'SQL Cho Lớp P12.01', NULL, N'nhap'),
    (7,  1, 3, 2, 1, 2, NULL, N'SQL Cho Lớp P12.02', NULL, N'nhap'),
    (8,  1, 4, 2, 1, 3, NULL, N'SQL Cho Lớp P12.03', NULL, N'nhap'),
    (9,  1, 4, 2, 1, 4, NULL, N'SQL Cho Lớp P12.04', NULL, N'nhap'),
    (10, 1, 3, 2, 1, 5, NULL, N'SQL Cho Lớp P12.05', NULL, N'nhap'),
    (11, 1, 5, 3, 1, 6, NULL, N'Web Cho Lớp P12.06', NULL, N'nhap'),
    (12, 1, 5, 3, 1, 7, NULL, N'Web Cho Lớp P12.07', NULL, N'nhap'),
    (13, 1, 6, 3, 1, 8, NULL, N'Web Cho Lớp P12.08', NULL, N'nhap'),
    (14, 1, 6, 3, 1, 9, NULL, N'Web Cho Lớp P12.09', NULL, N'nhap'),
    (15, 1, 5, 3, 1, 10, NULL, N'Web Cho Lớp P12.10', NULL, N'nhap'),
    (16, 1, 7, 4, 1, 6, NULL, N'Photoshop Cho Lớp P12.06', NULL, N'nhap'),
    (17, 1, 7, 4, 1, 7, NULL, N'Photoshop Cho Lớp P12.07', NULL, N'nhap'),
    (18, 1, 8, 4, 1, 8, NULL, N'Photoshop Cho Lớp P12.08', NULL, N'nhap'),
    (19, 1, 8, 4, 1, 9, NULL, N'Photoshop Cho Lớp P12.09', NULL, N'nhap'),
    (20, 1, 7, 4, 1, 10, NULL, N'Photoshop Cho Lớp P12.10', NULL, N'nhap')
) AS s(mid, dvid, gv, mh, hk, lop, lhp, td, mo, tt)
ON (t.ma_khoa_hoc = s.mid)
WHEN NOT MATCHED THEN INSERT (ma_khoa_hoc, ma_don_vi, ma_giao_vien, ma_mon_hoc, ma_hoc_ky, ma_lop, ma_lop_hoc_phan, tieu_de, mo_ta, trang_thai, ngay_tao)
VALUES (s.mid, s.dvid, s.gv, s.mh, s.hk, s.lop, s.lhp, s.td, s.mo, s.tt, SYSUTCDATETIME());
SET IDENTITY_INSERT dbo.KhoaHoc OFF;
GO

-- ============================================================
-- Summary
-- ============================================================
SELECT '[SEED COMPLETE]' AS Message;
SELECT N'DonVi' AS [Table], COUNT(*) AS [Rows] FROM dbo.DonVi
UNION ALL SELECT N'VaiTro', COUNT(*) FROM dbo.VaiTro
UNION ALL SELECT N'HocKy', COUNT(*) FROM dbo.HocKy
UNION ALL SELECT N'CaHoc', COUNT(*) FROM dbo.CaHoc
UNION ALL SELECT N'ToaNha', COUNT(*) FROM dbo.ToaNha
UNION ALL SELECT N'Tang', COUNT(*) FROM dbo.Tang
UNION ALL SELECT N'PhongHoc', COUNT(*) FROM dbo.PhongHoc
UNION ALL SELECT N'DanhMucMonHoc', COUNT(*) FROM dbo.DanhMucMonHoc
UNION ALL SELECT N'NguoiDung', COUNT(*) FROM dbo.NguoiDung
UNION ALL SELECT N'LopHanhChinh', COUNT(*) FROM dbo.LopHanhChinh
UNION ALL SELECT N'GiaoVienMonHoc', COUNT(*) FROM dbo.GiaoVienMonHoc
UNION ALL SELECT N'KhoaHoc', COUNT(*) FROM dbo.KhoaHoc
ORDER BY [Table];
GO
