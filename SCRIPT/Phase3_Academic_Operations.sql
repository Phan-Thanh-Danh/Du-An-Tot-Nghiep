USE LMS;
GO

SET NOCOUNT ON;

DECLARE @N INT = 5; -- Số cơ sở
DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();

PRINT N'--- BẮT ĐẦU PHASE 3: ACADEMIC OPERATIONS ---';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==========================================
    -- PHẦN 1: MỞ KHÓA HỌC (LMS) VÀ PHÂN CÔNG GIẢNG VIÊN
    -- ==========================================

    -- LẤY ID CÁC DANH MỤC CẦN THIẾT
    DECLARE @SE INT, @GD INT, @DM INT;
    SELECT @SE = ma_chuyen_nganh FROM ChuyenNganh WHERE ma_code_chuyen_nganh = 'SE';
    SELECT @GD = ma_chuyen_nganh FROM ChuyenNganh WHERE ma_code_chuyen_nganh = 'GD';
    SELECT @DM = ma_chuyen_nganh FROM ChuyenNganh WHERE ma_code_chuyen_nganh = 'DM';

    DECLARE @COM101 INT, @DBI202 INT, @WEB104 INT, @UIX101 INT, @MKT101 INT;
    SELECT @COM101 = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'COM101';
    SELECT @DBI202 = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'DBI202';
    SELECT @WEB104 = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'WEB104';
    SELECT @UIX101 = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'UIX101';
    SELECT @MKT101 = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = 'MKT101';

    PRINT N'- Đang phân bổ 5000 Giảng viên vào Chuyên ngành...';
    WITH TeacherRanked AS (
        SELECT ma_nguoi_dung, ma_don_vi, ROW_NUMBER() OVER(PARTITION BY ma_don_vi ORDER BY ma_nguoi_dung) AS rn
        FROM NguoiDung WHERE vai_tro_chinh = 'giao_vien'
    )
    INSERT INTO GiaoVienChuyenNganh (ma_giao_vien, ma_chuyen_nganh, la_chuyen_mon_chinh, muc_do_phu_hop, so_nam_kinh_nghiem, con_hoat_dong, ngay_tao)
    SELECT ma_nguoi_dung, CASE WHEN rn % 3 = 0 THEN @SE WHEN rn % 3 = 1 THEN @GD ELSE @DM END, 1, 5, 3, 1, @CurrentDate
    FROM TeacherRanked
    WHERE NOT EXISTS (SELECT 1 FROM GiaoVienChuyenNganh gvcn WHERE gvcn.ma_giao_vien = TeacherRanked.ma_nguoi_dung);

    PRINT N'- Đang cấp quyền dạy môn học (GiaoVienMonHoc)...';
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, ngay_tao)
    SELECT ma_giao_vien, @COM101, 5, 10, 3, @CurrentDate FROM GiaoVienChuyenNganh WHERE ma_chuyen_nganh = @SE;
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, ngay_tao)
    SELECT ma_giao_vien, @DBI202, 5, 8, 3, @CurrentDate FROM GiaoVienChuyenNganh WHERE ma_chuyen_nganh = @SE;
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, ngay_tao)
    SELECT ma_giao_vien, @WEB104, 5, 5, 2, @CurrentDate FROM GiaoVienChuyenNganh WHERE ma_chuyen_nganh = @SE;
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, ngay_tao)
    SELECT ma_giao_vien, @UIX101, 5, 12, 4, @CurrentDate FROM GiaoVienChuyenNganh WHERE ma_chuyen_nganh = @GD;
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, ngay_tao)
    SELECT ma_giao_vien, @MKT101, 5, 15, 5, @CurrentDate FROM GiaoVienChuyenNganh WHERE ma_chuyen_nganh = @DM;

    BEGIN TRY EXEC('UPDATE GiaoVienMonHoc SET con_hoat_dong = 1 WHERE con_hoat_dong IS NULL OR con_hoat_dong = 0;'); END TRY BEGIN CATCH END CATCH;

    PRINT N'- Đang mở Khóa học (LMS) cho tất cả Lớp Hành chính...';
    DECLARE @i INT = 1;
    WHILE @i <= @N
    BEGIN
        DECLARE @CampusCode NVARCHAR(50) = 'CAMPUS_AET_' + CAST(@i AS NVARCHAR);
        DECLARE @CampusId INT;
        SELECT @CampusId = ma_don_vi FROM DonVi WHERE ten_don_vi = N'Trường AET Cơ sở ' \+ CAST\(@i AS NVARCHAR\) AND cap_don_vi = 'co_so';
        DECLARE @TermId INT;
        SELECT @TermId = ma_hoc_ky FROM HocKy WHERE ma_don_vi = @CampusId AND nam_hoc = '2026' AND thu_tu_trong_nam = 1;

        IF @CampusId IS NOT NULL AND @TermId IS NOT NULL
        BEGIN
            WITH Classes AS (SELECT ma_lop, ten_lop, ROW_NUMBER() OVER(ORDER BY ma_lop) AS rn FROM LopHanhChinh WHERE ma_don_vi = @CampusId),
            TeachersCOM AS (SELECT gm.ma_giao_vien, ROW_NUMBER() OVER(ORDER BY NEWID()) AS rn FROM GiaoVienMonHoc gm JOIN NguoiDung u ON gm.ma_giao_vien = u.ma_nguoi_dung WHERE gm.ma_mon_hoc = @COM101 AND u.ma_don_vi = @CampusId)
            INSERT INTO KhoaHoc (ma_don_vi, ma_giao_vien, ma_hoc_ky, ma_lop, ma_mon_hoc, tieu_de, trang_thai, SoBlockHoc, ngay_tao)
            SELECT @CampusId, (SELECT TOP 1 ma_giao_vien FROM TeachersCOM t WHERE t.rn = (c.rn % (SELECT COUNT(*) FROM TeachersCOM)) + 1), @TermId, c.ma_lop, @COM101, N'Nhập môn lập trình - ' + c.ten_lop, 'dang_mo', 1, @CurrentDate FROM Classes c;

            WITH Classes AS (SELECT ma_lop, ten_lop, ROW_NUMBER() OVER(ORDER BY ma_lop) AS rn FROM LopHanhChinh WHERE ma_don_vi = @CampusId),
            TeachersDBI AS (SELECT gm.ma_giao_vien, ROW_NUMBER() OVER(ORDER BY NEWID()) AS rn FROM GiaoVienMonHoc gm JOIN NguoiDung u ON gm.ma_giao_vien = u.ma_nguoi_dung WHERE gm.ma_mon_hoc = @DBI202 AND u.ma_don_vi = @CampusId)
            INSERT INTO KhoaHoc (ma_don_vi, ma_giao_vien, ma_hoc_ky, ma_lop, ma_mon_hoc, tieu_de, trang_thai, SoBlockHoc, ngay_tao)
            SELECT @CampusId, (SELECT TOP 1 ma_giao_vien FROM TeachersDBI t WHERE t.rn = (c.rn % (SELECT COUNT(*) FROM TeachersDBI)) + 1), @TermId, c.ma_lop, @DBI202, N'Hệ quản trị CSDL - ' + c.ten_lop, 'dang_mo', 1, @CurrentDate FROM Classes c;
        END
        SET @i += 1;
    END

    -- ==========================================
    -- PHẦN 2: LỊCH HỌC, ĐIỂM DANH, CHẤM ĐIỂM
    -- ==========================================
    
    PRINT N'- Đang khởi tạo Ca học và Cấu hình điểm...';
    DELETE FROM CaHoc; DBCC CHECKIDENT ('CaHoc', RESEED, 0);
    INSERT INTO CaHoc (ten_ca, gio_bat_dau, gio_ket_thuc, thu_tu, con_hoat_dong) VALUES 
        (N'Ca 1 (Sáng)', '07:30', '09:30', 1, 1), (N'Ca 2 (Sáng)', '09:45', '11:45', 2, 1),
        (N'Ca 3 (Chiều)', '13:00', '15:00', 3, 1), (N'Ca 4 (Chiều)', '15:15', '17:15', 4, 1);

    INSERT INTO CauHinhDiemMonHoc (ma_mon_hoc, ma_hoc_ky, nguong_dat, ti_le_chuyen_can_toi_thieu, trong_so_qua_trinh, trong_so_giua_ky, trong_so_cuoi_ky)
    SELECT m.ma_mon_hoc, h.ma_hoc_ky, 5.0, 80.0, 40.0, 30.0, 30.0
    FROM DanhMucMonHoc m CROSS JOIN HocKy h WHERE m.ma_code_mon_hoc IN ('COM101', 'DBI202') AND h.nam_hoc = '2026'
    AND NOT EXISTS (SELECT 1 FROM CauHinhDiemMonHoc c WHERE c.ma_mon_hoc = m.ma_mon_hoc AND c.ma_hoc_ky = h.ma_hoc_ky);

    PRINT N'- Đang lên Thời khóa biểu và phát sinh Buổi học...';
    INSERT INTO ThoiKhoaBieu (ma_khoa_hoc, ma_phong, ma_ca_hoc, thu_trong_tuan, ngay_bat_dau, ngay_ket_thuc, trang_thai, ngay_tao)
    SELECT k.ma_khoa_hoc, (SELECT TOP 1 ma_phong FROM PhongHoc p JOIN ToaNha t ON p.ma_toa_nha = t.ma_toa_nha WHERE t.ma_don_vi = k.ma_don_vi ORDER BY NEWID()),
        (SELECT TOP 1 ma_ca_hoc FROM CaHoc ORDER BY NEWID()), (k.ma_khoa_hoc % 6) + 2, h.ngay_bat_dau, h.ngay_ket_thuc, 'dang_hoat_dong', @CurrentDate
    FROM KhoaHoc k JOIN HocKy h ON k.ma_hoc_ky = h.ma_hoc_ky;

    WITH Numbers AS (SELECT TOP 10 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values)
    INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_buoi, trang_thai_diem_danh, ngay_tao)
    SELECT tkb.ma_tkb, tkb.ma_khoa_hoc, tkb.ma_phong, tkb.ma_ca_hoc, k.ma_giao_vien, DATEADD(DAY, (n.N - 1) * 7, tkb.ngay_bat_dau), 'da_dien_ra', 'da_chot', @CurrentDate
    FROM ThoiKhoaBieu tkb JOIN KhoaHoc k ON tkb.ma_khoa_hoc = k.ma_khoa_hoc CROSS JOIN Numbers n;

    PRINT N'- Đang quét Điểm danh Sinh viên (Dự kiến ~300.000 dòng)...';
    INSERT INTO DiemDanh (ma_buoi_hoc, ma_don_vi, ma_hoc_sinh, trang_thai, nguoi_ghi_nhan)
    SELECT b.ma_buoi_hoc, k.ma_don_vi, s.ma_nguoi_dung,
        CASE WHEN (b.ma_buoi_hoc + s.ma_nguoi_dung) % 100 < 85 THEN 'co_mat' WHEN (b.ma_buoi_hoc + s.ma_nguoi_dung) % 100 < 95 THEN 'vang_mat' ELSE 'di_muon' END,
        b.ma_giao_vien
    FROM BuoiHoc b JOIN KhoaHoc k ON b.ma_khoa_hoc = k.ma_khoa_hoc
    JOIN NguoiDung s ON s.ma_lop = k.ma_lop AND s.vai_tro_chinh = 'hoc_sinh';

    PRINT N'- Đang tính toán và sinh Điểm số tổng kết...';
    INSERT INTO DiemSo (ma_don_vi, ma_hoc_ky, ma_hoc_sinh, ma_mon_hoc, nam_nhap_hoc, diem_qua_trinh, diem_giua_ky, diem_cuoi_ky, gpa_mon_hoc, trang_thai, ly_do_rot)
    SELECT k.ma_don_vi, k.ma_hoc_ky, s.ma_nguoi_dung, k.ma_mon_hoc, 2024,
        (s.ma_nguoi_dung % 6) + 4.0, (s.ma_nguoi_dung % 5) + 5.0, (s.ma_nguoi_dung % 7) + 3.0,
        ((s.ma_nguoi_dung % 6) + 4.0) * 0.4 + ((s.ma_nguoi_dung % 5) + 5.0) * 0.3 + ((s.ma_nguoi_dung % 7) + 3.0) * 0.3,
        CASE WHEN (((s.ma_nguoi_dung % 6) + 4.0) * 0.4 + ((s.ma_nguoi_dung % 5) + 5.0) * 0.3 + ((s.ma_nguoi_dung % 7) + 3.0) * 0.3) >= 5.0 THEN 'dat' ELSE 'rot' END,
        CASE WHEN (((s.ma_nguoi_dung % 6) + 4.0) * 0.4 + ((s.ma_nguoi_dung % 5) + 5.0) * 0.3 + ((s.ma_nguoi_dung % 7) + 3.0) * 0.3) < 5.0 THEN N'Điểm tổng kết dưới 5.0' ELSE NULL END
    FROM KhoaHoc k JOIN NguoiDung s ON s.ma_lop = k.ma_lop AND s.vai_tro_chinh = 'hoc_sinh';

    COMMIT TRANSACTION;
    PRINT N'--- HOÀN THÀNH TOÀN BỘ PHASE 3 (GỘP) THÀNH CÔNG ---';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'!!! CÓ LỖI XẢY RA TRONG PHASE 3 !!!';
    PRINT ERROR_MESSAGE();
END CATCH
GO

