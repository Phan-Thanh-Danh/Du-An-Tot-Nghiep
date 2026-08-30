USE LMS;
GO

SET NOCOUNT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();

PRINT N'--- BẮT ĐẦU PHASE 5: NỘI DUNG MÔN HỌC (LMS) ---';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==========================================
    -- 1. TẠO HỌC LIỆU DÙNG CHUNG (KHÔNG LẶP THEO CƠ SỞ)
    -- ==========================================
    PRINT N'>> Tạo Đề cương, Chương, Bài học, Nội dung cho các Môn học...';

    -- Lấy 5 môn học chính
    DECLARE @Subjects TABLE (MaMon INT, Code NVARCHAR(50), Ten NVARCHAR(255));
    INSERT INTO @Subjects 
    SELECT ma_mon_hoc, ma_code_mon_hoc, ten_mon_hoc FROM DanhMucMonHoc 
    WHERE ma_code_mon_hoc IN ('COM101', 'DBI202', 'WEB104', 'UIX101', 'MKT101');

    -- a. Đề cương (DeCuongMonHoc)
    INSERT INTO DeCuongMonHoc (ma_mon_hoc, ma_chuyen_nganh, version, ten_syllabus, trang_thai, mo_ta)
    SELECT s.MaMon, c.ma_chuyen_nganh, 'v1.0', N'Đề cương ' + s.Ten, 'active', N'Đề cương chuẩn năm 2026'
    FROM @Subjects s
    JOIN ChuyenNganh c ON (
        (s.Code IN ('COM101', 'DBI202', 'WEB104') AND c.ten_chuyen_nganh = N'Kỹ thuật phần mềm') OR
        (s.Code = 'UIX101' AND c.ten_chuyen_nganh = N'Thiết kế đồ họa') OR
        (s.Code = 'MKT101' AND c.ten_chuyen_nganh = N'Digital Marketing')
    )
    WHERE NOT EXISTS (SELECT 1 FROM DeCuongMonHoc d WHERE d.ma_mon_hoc = s.MaMon AND d.ma_chuyen_nganh = c.ma_chuyen_nganh);

    -- b. Chương (Chuong) - 5 chương mỗi môn
    WITH Numbers AS (SELECT TOP 5 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values)
    INSERT INTO Chuong (ma_mon_hoc, tieu_de, thu_tu, ngay_tao)
    SELECT s.MaMon, N'Chương ' + CAST(n.N AS NVARCHAR) + N': Kiến thức cốt lõi', n.N, @CurrentDate
    FROM @Subjects s CROSS JOIN Numbers n
    WHERE NOT EXISTS (SELECT 1 FROM Chuong c WHERE c.ma_mon_hoc = s.MaMon AND c.thu_tu = n.N);

    -- c. Bài học (BaiHoc) - 3 bài mỗi chương
    WITH Numbers AS (SELECT TOP 3 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values)
    INSERT INTO BaiHoc (ma_chuong, tieu_de, thu_tu, loai_bai_hoc, thoi_luong_giay, trang_thai)
    SELECT c.ma_chuong, N'Bài học ' + CAST(n.N AS NVARCHAR) + N' của ' + c.tieu_de, n.N, 'video', 3600, 'da_xuat_ban'
    FROM Chuong c CROSS JOIN Numbers n
    WHERE NOT EXISTS (SELECT 1 FROM BaiHoc b WHERE b.ma_chuong = c.ma_chuong AND b.thu_tu = n.N);

    -- d. Nội dung (BaiHocNoiDung) - 1 Video + 1 Tài liệu mỗi bài học
    INSERT INTO BaiHocNoiDung (ma_bai_hoc, tieu_de, loai_noi_dung, thu_tu, thoi_luong_giay, url_tap_tin, trang_thai)
    SELECT ma_bai_hoc, N'Video bài giảng', 'video', 1, 1800, 'https://cdn.lms.local/video.mp4', 'da_xuat_ban'
    FROM BaiHoc
    WHERE NOT EXISTS (SELECT 1 FROM BaiHocNoiDung nd WHERE nd.ma_bai_hoc = BaiHoc.ma_bai_hoc AND nd.loai_noi_dung = 'video');

    INSERT INTO BaiHocNoiDung (ma_bai_hoc, tieu_de, loai_noi_dung, thu_tu, thoi_luong_giay, url_tap_tin, trang_thai)
    SELECT ma_bai_hoc, N'Tài liệu đọc', 'tai_lieu', 2, 0, 'https://cdn.lms.local/doc.pdf', 'da_xuat_ban'
    FROM BaiHoc
    WHERE NOT EXISTS (SELECT 1 FROM BaiHocNoiDung nd WHERE nd.ma_bai_hoc = BaiHoc.ma_bai_hoc AND nd.loai_noi_dung = 'tai_lieu');

    -- ==========================================
    -- 2. TIẾN ĐỘ HỌC TẬP DÀNH CHO SINH VIÊN
    -- ==========================================
    PRINT N'>> Tạo Tiến độ học tập cho Sinh viên (Giới hạn data để tối ưu)...';

    -- Lấy 1 cơ sở ngẫu nhiên (hoặc cơ sở đầu tiên)
    DECLARE @Campus1 INT;
    SELECT TOP 1 @Campus1 = ma_don_vi FROM DonVi WHERE cap_don_vi = 'co_so' ORDER BY ma_don_vi;

    DECLARE @CourseCOM101 INT;
    SELECT TOP 1 @CourseCOM101 = ma_khoa_hoc FROM KhoaHoc 
    WHERE ma_don_vi = @Campus1 AND tieu_de LIKE N'Nhập môn lập trình%';

    IF @CourseCOM101 IS NOT NULL
    BEGIN
        -- a. Tiến độ Nội dung
        INSERT INTO TienDoNoiDungHocTap (ma_hoc_sinh, ma_noi_dung, phan_tram_tien_do, phan_tram_cuon_lon_nhat, so_giay_da_xac_nhan, trang_thai, lan_tuong_tac_cuoi)
        SELECT 
            u.ma_nguoi_dung,
            nd.ma_noi_dung,
            CASE WHEN (u.ma_nguoi_dung % 10) < 6 THEN 100.0 
                 WHEN (u.ma_nguoi_dung % 10) < 8 THEN 50.0  
                 ELSE 0.0 END,                              
            CASE WHEN (u.ma_nguoi_dung % 10) < 6 THEN 100.0 ELSE 50.0 END,
            1800,
            CASE WHEN (u.ma_nguoi_dung % 10) < 6 THEN 'hoan_thanh' 
                 WHEN (u.ma_nguoi_dung % 10) < 8 THEN 'dang_hoc' 
                 ELSE 'chua_bat_dau' END,
            @CurrentDate
        FROM NguoiDung u
        JOIN KhoaHoc kh ON u.ma_lop = kh.ma_lop
        JOIN Chuong c ON kh.ma_mon_hoc = c.ma_mon_hoc
        JOIN BaiHoc bh ON c.ma_chuong = bh.ma_chuong
        JOIN BaiHocNoiDung nd ON bh.ma_bai_hoc = nd.ma_bai_hoc
        WHERE kh.ma_khoa_hoc = @CourseCOM101 AND u.vai_tro_chinh = 'hoc_sinh'
        AND NOT EXISTS (SELECT 1 FROM TienDoNoiDungHocTap t WHERE t.ma_hoc_sinh = u.ma_nguoi_dung AND t.ma_noi_dung = nd.ma_noi_dung);

        -- b. Tiến độ Bài học (Tính trung bình từ Nội dung)
        INSERT INTO TienDoBaiHoc (ma_hoc_sinh, ma_bai_hoc, phan_tram_tien_do, hoan_thanh_luc)
        SELECT 
            u.ma_nguoi_dung,
            bh.ma_bai_hoc,
            CASE WHEN (u.ma_nguoi_dung % 10) < 6 THEN 100.0 
                 WHEN (u.ma_nguoi_dung % 10) < 8 THEN 50.0 
                 ELSE 0.0 END,
            CASE WHEN (u.ma_nguoi_dung % 10) < 6 THEN @CurrentDate ELSE NULL END
        FROM NguoiDung u
        JOIN KhoaHoc kh ON u.ma_lop = kh.ma_lop
        JOIN Chuong c ON kh.ma_mon_hoc = c.ma_mon_hoc
        JOIN BaiHoc bh ON c.ma_chuong = bh.ma_chuong
        WHERE kh.ma_khoa_hoc = @CourseCOM101 AND u.vai_tro_chinh = 'hoc_sinh'
        AND NOT EXISTS (SELECT 1 FROM TienDoBaiHoc t WHERE t.ma_hoc_sinh = u.ma_nguoi_dung AND t.ma_bai_hoc = bh.ma_bai_hoc);
    END

    -- ==========================================
    -- 3. LOG PHIÊN HỌC & BÌNH LUẬN (GIỚI HẠN ~100 DÒNG)
    -- ==========================================
    PRINT N'>> Tạo Log Phiên học & Bình luận (~100 dòng)...';

    -- Lấy TOP 100 Sinh viên đang học để tạo Log Phiên
    INSERT INTO PhienHocNoiDung (ma_hoc_sinh, ma_noi_dung, session_token, bat_dau_luc, ket_thuc_luc, dia_chi_ip_hash, user_agent_hash, nhip_tim_cuoi_luc, vi_tri_video_cuoi_giay, trang_thai)
    SELECT TOP 100 
        ma_hoc_sinh, 
        ma_noi_dung, 
        NEWID(), 
        DATEADD(MINUTE, -30, @CurrentDate), 
        @CurrentDate, 
        '192.168.1.x', 
        'Chrome', 
        @CurrentDate, 
        900, 
        'da_ket_thuc'
    FROM TienDoNoiDungHocTap WHERE phan_tram_tien_do > 0;

    -- Lấy TOP 100 Bài học có tiến độ để thả Bình luận
    INSERT INTO BinhLuan (ma_bai_hoc, ma_nguoi_dung, noi_dung, giay_trong_video, ngay_tao)
    SELECT TOP 100 
        ma_bai_hoc, 
        ma_hoc_sinh, 
        N'Thầy ơi cho em hỏi đoạn video này giải thích thêm được không ạ?', 
        300, 
        @CurrentDate
    FROM TienDoBaiHoc WHERE phan_tram_tien_do > 0 ORDER BY NEWID();

    COMMIT TRANSACTION;
    PRINT N'--- HOÀN THÀNH TOÀN BỘ PHASE 5 THÀNH CÔNG ---';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'!!! CÓ LỖI XẢY RA TRONG PHASE 5 !!!';
    PRINT ERROR_MESSAGE();
END CATCH
GO
