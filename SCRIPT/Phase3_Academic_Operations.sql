USE LMS;
GO

SET NOCOUNT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();

PRINT N'--- BẮT ĐẦU PHASE 3: ACADEMIC OPERATIONS ---';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==========================================
    -- PHẦN 1: MỞ KHÓA HỌC (LMS) VÀ PHÂN CÔNG GIẢNG VIÊN
    -- ==========================================

    -- LẤY ID CÁC DANH MỤC CẦN THIẾT
    DECLARE @SE INT, @GD INT, @DM INT;
    SELECT @SE = ma_chuyen_nganh FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Kỹ thuật phần mềm';
    SELECT @GD = ma_chuyen_nganh FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Thiết kế đồ họa';
    SELECT @DM = ma_chuyen_nganh FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Digital Marketing';

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
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    SELECT ma_giao_vien, @COM101, 5, 10, 3, 1, 1, @CurrentDate FROM GiaoVienChuyenNganh WHERE ma_chuyen_nganh = @SE
      AND NOT EXISTS (SELECT 1 FROM GiaoVienMonHoc gm WHERE gm.ma_giao_vien = GiaoVienChuyenNganh.ma_giao_vien AND gm.ma_mon_hoc = @COM101);
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    SELECT ma_giao_vien, @DBI202, 5, 8, 3, 1, 1, @CurrentDate FROM GiaoVienChuyenNganh WHERE ma_chuyen_nganh = @SE
      AND NOT EXISTS (SELECT 1 FROM GiaoVienMonHoc gm WHERE gm.ma_giao_vien = GiaoVienChuyenNganh.ma_giao_vien AND gm.ma_mon_hoc = @DBI202);
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    SELECT ma_giao_vien, @WEB104, 5, 5, 2, 1, 1, @CurrentDate FROM GiaoVienChuyenNganh WHERE ma_chuyen_nganh = @SE
      AND NOT EXISTS (SELECT 1 FROM GiaoVienMonHoc gm WHERE gm.ma_giao_vien = GiaoVienChuyenNganh.ma_giao_vien AND gm.ma_mon_hoc = @WEB104);
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    SELECT ma_giao_vien, @UIX101, 5, 12, 4, 1, 1, @CurrentDate FROM GiaoVienChuyenNganh WHERE ma_chuyen_nganh = @GD
      AND NOT EXISTS (SELECT 1 FROM GiaoVienMonHoc gm WHERE gm.ma_giao_vien = GiaoVienChuyenNganh.ma_giao_vien AND gm.ma_mon_hoc = @UIX101);
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    SELECT ma_giao_vien, @MKT101, 5, 15, 5, 1, 1, @CurrentDate FROM GiaoVienChuyenNganh WHERE ma_chuyen_nganh = @DM
      AND NOT EXISTS (SELECT 1 FROM GiaoVienMonHoc gm WHERE gm.ma_giao_vien = GiaoVienChuyenNganh.ma_giao_vien AND gm.ma_mon_hoc = @MKT101);

    BEGIN TRY EXEC('UPDATE GiaoVienMonHoc SET con_hoat_dong = 1 WHERE con_hoat_dong IS NULL OR con_hoat_dong = 0;'); END TRY BEGIN CATCH END CATCH;

    PRINT N'- Đang mở Khóa học (LMS) cho tất cả Lớp Hành chính...';
    DECLARE @CampusId INT;
    DECLARE curCS CURSOR LOCAL FOR SELECT ma_don_vi FROM DonVi WHERE cap_don_vi = 'co_so';
    OPEN curCS;
    FETCH NEXT FROM curCS INTO @CampusId;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @TermId INT;
        SELECT @TermId = ma_hoc_ky FROM HocKy WHERE ma_don_vi = @CampusId AND nam_hoc = '2026' AND thu_tu_trong_nam = 1;

        IF @CampusId IS NOT NULL AND @TermId IS NOT NULL
        BEGIN
            WITH Classes AS (SELECT ma_lop, ten_lop, ROW_NUMBER() OVER(ORDER BY ma_lop) AS rn FROM LopHanhChinh WHERE ma_don_vi = @CampusId),
            TeachersCOM AS (SELECT gm.ma_giao_vien, ROW_NUMBER() OVER(ORDER BY NEWID()) AS rn FROM GiaoVienMonHoc gm JOIN NguoiDung u ON gm.ma_giao_vien = u.ma_nguoi_dung WHERE gm.ma_mon_hoc = @COM101 AND u.ma_don_vi = @CampusId)
            INSERT INTO KhoaHoc (ma_don_vi, ma_giao_vien, ma_hoc_ky, ma_lop, ma_mon_hoc, tieu_de, trang_thai, SoBlockHoc, ngay_tao)
            SELECT @CampusId, (SELECT TOP 1 ma_giao_vien FROM TeachersCOM t WHERE t.rn = (c.rn % (SELECT COUNT(*) FROM TeachersCOM)) + 1), @TermId, c.ma_lop, @COM101, N'Nhập môn lập trình - ' + c.ten_lop, 'da_xuat_ban', 1, @CurrentDate FROM Classes c
            WHERE NOT EXISTS (SELECT 1 FROM KhoaHoc k WHERE k.ma_lop = c.ma_lop AND k.ma_mon_hoc = @COM101);

            WITH Classes AS (SELECT ma_lop, ten_lop, ROW_NUMBER() OVER(ORDER BY ma_lop) AS rn FROM LopHanhChinh WHERE ma_don_vi = @CampusId),
            TeachersDBI AS (SELECT gm.ma_giao_vien, ROW_NUMBER() OVER(ORDER BY NEWID()) AS rn FROM GiaoVienMonHoc gm JOIN NguoiDung u ON gm.ma_giao_vien = u.ma_nguoi_dung WHERE gm.ma_mon_hoc = @DBI202 AND u.ma_don_vi = @CampusId)
            INSERT INTO KhoaHoc (ma_don_vi, ma_giao_vien, ma_hoc_ky, ma_lop, ma_mon_hoc, tieu_de, trang_thai, SoBlockHoc, ngay_tao)
            SELECT @CampusId, (SELECT TOP 1 ma_giao_vien FROM TeachersDBI t WHERE t.rn = (c.rn % (SELECT COUNT(*) FROM TeachersDBI)) + 1), @TermId, c.ma_lop, @DBI202, N'Hệ quản trị CSDL - ' + c.ten_lop, 'da_xuat_ban', 1, @CurrentDate FROM Classes c
            WHERE NOT EXISTS (SELECT 1 FROM KhoaHoc k WHERE k.ma_lop = c.ma_lop AND k.ma_mon_hoc = @DBI202);
        END
        FETCH NEXT FROM curCS INTO @CampusId;
    END
    CLOSE curCS;
    DEALLOCATE curCS;

    -- ==========================================
    -- PHẦN 2: LỊCH HỌC, ĐIỂM DANH, CHẤM ĐIỂM
    -- ==========================================
    
    PRINT N'- Đang khởi tạo Ca học và Cấu hình điểm...';
    DELETE FROM CaHoc; DBCC CHECKIDENT ('CaHoc', RESEED, 0);
    -- Cột buoi: constraint chỉ nhận 'sang', 'chieu', 'toi'
    INSERT INTO CaHoc (ten_ca, buoi, gio_bat_dau, gio_ket_thuc, thu_tu, con_hoat_dong) VALUES 
        (N'Ca 1 (Sáng)',  'sang',  '07:30', '09:30', 1, 1),
        (N'Ca 2 (Sáng)',  'sang',  '09:45', '11:45', 2, 1),
        (N'Ca 3 (Chiều)', 'chieu', '13:00', '15:00', 3, 1),
        (N'Ca 4 (Chiều)', 'chieu', '15:15', '17:15', 4, 1);

    INSERT INTO CauHinhDiemMonHoc (ma_mon_hoc, ma_hoc_ky, nguong_dat, ti_le_chuyen_can_toi_thieu, trong_so_qua_trinh, trong_so_giua_ky, trong_so_cuoi_ky)
    SELECT m.ma_mon_hoc, h.ma_hoc_ky, 5.0, 80.0, 40.0, 30.0, 30.0
    FROM DanhMucMonHoc m CROSS JOIN HocKy h WHERE m.ma_code_mon_hoc IN ('COM101', 'DBI202') AND h.nam_hoc = '2026'
    AND NOT EXISTS (SELECT 1 FROM CauHinhDiemMonHoc c WHERE c.ma_mon_hoc = m.ma_mon_hoc AND c.ma_hoc_ky = h.ma_hoc_ky);

    PRINT N'- Đang lên Thời khóa biểu và phát sinh Buổi học...';
    -- trang_thai ThoiKhoaBieu: constraint nhận 'nhap' (default theo model)
    INSERT INTO ThoiKhoaBieu (ma_khoa_hoc, ma_phong, ma_ca_hoc, thu_trong_tuan, ngay_bat_dau, ngay_ket_thuc, trang_thai, ngay_tao)
    SELECT 
        k.ma_khoa_hoc,
        (SELECT TOP 1 ma_phong FROM PhongHoc WHERE ma_don_vi = k.ma_don_vi AND trang_thai_phong = 'hoat_dong' ORDER BY NEWID()),
        (SELECT TOP 1 ma_ca_hoc FROM CaHoc ORDER BY NEWID()),
        (k.ma_khoa_hoc % 5) + 2,  -- thu 2 den thu 6
        h.ngay_bat_dau,
        h.ngay_ket_thuc,
        'nhap',
        @CurrentDate
    FROM KhoaHoc k JOIN HocKy h ON k.ma_hoc_ky = h.ma_hoc_ky
    WHERE NOT EXISTS (SELECT 1 FROM ThoiKhoaBieu tkb WHERE tkb.ma_khoa_hoc = k.ma_khoa_hoc);

    -- Sinh 10 buổi học cho mỗi khóa học (1 buổi/tuần x 10 tuần)
    -- trang_thai_diem_danh trong BuoiHoc không có constraint, dùng 'da_chot'
    -- trang_thai_buoi không có constraint, dùng 'da_dien_ra'
    -- NgayHoc phải là DateOnly -> CAST sang DATE
    WITH Numbers AS (
        SELECT TOP 10 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N 
        FROM master.dbo.spt_values
    )
    INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_buoi, trang_thai_diem_danh, ngay_tao)
    SELECT 
        tkb.ma_tkb, tkb.ma_khoa_hoc, tkb.ma_phong, tkb.ma_ca_hoc,
        k.ma_giao_vien,
        CAST(DATEADD(DAY, (n.N - 1) * 7, tkb.ngay_bat_dau) AS DATE),
        'da_dien_ra',
        'da_khoa',
        @CurrentDate
    FROM ThoiKhoaBieu tkb 
    JOIN KhoaHoc k ON tkb.ma_khoa_hoc = k.ma_khoa_hoc 
    CROSS JOIN Numbers n
    WHERE tkb.ngay_bat_dau IS NOT NULL
      AND NOT EXISTS (
        SELECT 1 FROM BuoiHoc b 
        WHERE b.ma_tkb = tkb.ma_tkb 
          AND b.ngay_hoc = CAST(DATEADD(DAY, (n.N - 1) * 7, tkb.ngay_bat_dau) AS DATE)
      );

    PRINT N'- Đang quét Điểm danh Sinh viên...';
    -- trang_thai DiemDanh constraint: 'co_mat', 'vang', 'di_muon', 'co_phep'
    -- he_so_vang NOT NULL, ghi_nhan_luc NOT NULL
    INSERT INTO DiemDanh (ma_buoi_hoc, ma_don_vi, ma_hoc_sinh, trang_thai, nguoi_ghi_nhan, ghi_nhan_luc, he_so_vang)
    SELECT 
        b.ma_buoi_hoc, 
        k.ma_don_vi, 
        s.ma_nguoi_dung,
        CASE 
            WHEN (b.ma_buoi_hoc + s.ma_nguoi_dung) % 100 < 85 THEN 'co_mat' 
            WHEN (b.ma_buoi_hoc + s.ma_nguoi_dung) % 100 < 95 THEN 'vang'
            ELSE 'di_muon' 
        END,
        b.ma_giao_vien,
        @CurrentDate,
        CASE WHEN (b.ma_buoi_hoc + s.ma_nguoi_dung) % 100 BETWEEN 85 AND 94 THEN 1 ELSE 0 END
    FROM BuoiHoc b 
    JOIN KhoaHoc k ON b.ma_khoa_hoc = k.ma_khoa_hoc
    JOIN NguoiDung s ON s.ma_lop = k.ma_lop AND s.vai_tro_chinh = 'hoc_sinh'
    WHERE NOT EXISTS (SELECT 1 FROM DiemDanh dd WHERE dd.ma_buoi_hoc = b.ma_buoi_hoc AND dd.ma_hoc_sinh = s.ma_nguoi_dung);

    PRINT N'- Đang tính toán và sinh Điểm số tổng kết...';
    -- trang_thai DiemSo constraint: 'dat', 'rot', 'chua_hoan_thanh', 'cho_hoan_thanh_bo_sung'
    -- ly_do_rot: constraint ISJSON -> phải là NULL hoặc chuỗi JSON hợp lệ
    -- da_khoa: NOT NULL
    INSERT INTO DiemSo (ma_don_vi, ma_hoc_ky, ma_hoc_sinh, ma_mon_hoc, nam_nhap_hoc, diem_qua_trinh, diem_giua_ky, diem_cuoi_ky, gpa_mon_hoc, trang_thai, ly_do_rot, da_khoa)
    SELECT 
        k.ma_don_vi, k.ma_hoc_ky, s.ma_nguoi_dung, k.ma_mon_hoc, 2024,
        CAST((s.ma_nguoi_dung % 6) + 4.0 AS DECIMAL(4,2)),
        CAST((s.ma_nguoi_dung % 5) + 5.0 AS DECIMAL(4,2)),
        CAST((s.ma_nguoi_dung % 7) + 3.0 AS DECIMAL(4,2)),
        CAST(((s.ma_nguoi_dung % 6) + 4.0) * 0.4 + ((s.ma_nguoi_dung % 5) + 5.0) * 0.3 + ((s.ma_nguoi_dung % 7) + 3.0) * 0.3 AS DECIMAL(4,2)),
        CASE WHEN (((s.ma_nguoi_dung % 6) + 4.0) * 0.4 + ((s.ma_nguoi_dung % 5) + 5.0) * 0.3 + ((s.ma_nguoi_dung % 7) + 3.0) * 0.3) >= 5.0 
             THEN 'dat' ELSE 'rot' END,
        -- ly_do_rot phải là JSON hoặc NULL (constraint ISJSON)
        CASE WHEN (((s.ma_nguoi_dung % 6) + 4.0) * 0.4 + ((s.ma_nguoi_dung % 5) + 5.0) * 0.3 + ((s.ma_nguoi_dung % 7) + 3.0) * 0.3) < 5.0 
             THEN N'{"ly_do": "Điểm tổng kết dưới 5.0"}' ELSE NULL END,
        1  -- da_khoa = 1 (đã khóa điểm cuối kỳ)
    FROM KhoaHoc k JOIN NguoiDung s ON s.ma_lop = k.ma_lop AND s.vai_tro_chinh = 'hoc_sinh'
    WHERE NOT EXISTS (SELECT 1 FROM DiemSo d WHERE d.ma_hoc_sinh = s.ma_nguoi_dung AND d.ma_mon_hoc = k.ma_mon_hoc AND d.ma_hoc_ky = k.ma_hoc_ky);

    COMMIT TRANSACTION;
    PRINT N'--- HOÀN THÀNH TOÀN BỘ PHASE 3 (GỘP) THÀNH CÔNG ---';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'!!! CÓ LỖI XẢY RA TRONG PHASE 3 !!!';
    PRINT ERROR_MESSAGE();
END CATCH
GO
