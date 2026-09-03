USE LMS;
GO

SET NOCOUNT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();

PRINT N'========================================================================';
PRINT N'=== BẮT ĐẦU: PHÁT SINH VÀ CẬP NHẬT ĐIỂM CHO TẤT CẢ KHÓA HỌC / HỌC SINH ===';
PRINT N'========================================================================';

BEGIN TRY
    BEGIN TRANSACTION;

    -- =========================================================================
    -- BƯỚC 1: ĐỒNG BỘ ma_hoc_ky CHO CÁC KHÓA HỌC NẾU ĐANG BỊ NULL
    -- =========================================================================
    PRINT N'[1/5] Kiểm tra và gán ma_hoc_ky cho các Khóa học chưa có học kỳ...';

    UPDATE k
    SET k.ma_hoc_ky = ISNULL(
        (SELECT TOP 1 hk.ma_hoc_ky 
         FROM HocKy hk 
         WHERE (hk.ma_don_vi = k.ma_don_vi OR hk.ma_don_vi = 3) 
           AND hk.nam_hoc = '2026' 
           AND hk.thu_tu_trong_nam = 1 
         ORDER BY hk.ma_hoc_ky),
        ISNULL(
            (SELECT TOP 1 hk.ma_hoc_ky 
             FROM HocKy hk 
             WHERE hk.ma_don_vi = k.ma_don_vi 
             ORDER BY hk.ngay_bat_dau DESC),
            ISNULL((SELECT TOP 1 hk.ma_hoc_ky FROM HocKy hk ORDER BY hk.ma_hoc_ky), 1)
        )
    )
    FROM KhoaHoc k
    WHERE k.ma_hoc_ky IS NULL;

    PRINT N'      -> Đã đồng bộ xong học kỳ cho các khóa học.';

    -- =========================================================================
    -- BƯỚC 2: PHÁT SINH VÀ CẬP NHẬT BẢNG DiemSo CHO TẤT CẢ HỌC SINH VÀ MÔN HỌC
    -- (Tuân thủ nghiêm ngặt ràng buộc UQ_DiemSo_1, CHECK CONSTRAINT điểm 0-10, ISJSON ly_do_rot)
    -- =========================================================================
    PRINT N'[2/5] Đang tính toán và chèn bảng DiemSo cho tất cả sinh viên...';

    ;WITH RawStudentCourseList AS (
        SELECT 
            s.ma_nguoi_dung AS MaHocSinh,
            k.ma_mon_hoc AS MaMonHoc,
            k.ma_hoc_ky AS MaHocKy,
            ISNULL(s.ma_don_vi, k.ma_don_vi) AS MaDonVi,
            ISNULL(s.nam_nhap_hoc, 2024) AS NamNhapHoc,
            k.ma_khoa_hoc AS MaKhoaHoc,
            -- Ngăn ngừa trùng lặp nếu 1 lớp có nhiều hơn 1 khóa học cùng môn và học kỳ
            ROW_NUMBER() OVER (
                PARTITION BY s.ma_nguoi_dung, k.ma_mon_hoc, k.ma_hoc_ky 
                ORDER BY k.ma_khoa_hoc
            ) AS rn
        FROM KhoaHoc k
        JOIN NguoiDung s ON s.ma_lop = k.ma_lop
        WHERE s.vai_tro_chinh IN ('hoc_sinh', 'Student')
          AND k.ma_hoc_ky IS NOT NULL
    ),
    StudentCourseList AS (
        SELECT 
            MaHocSinh,
            MaMonHoc,
            MaHocKy,
            MaDonVi,
            NamNhapHoc,
            -- Tạo số giả lập ngẫu nhiên nhưng ổn định (deterministic random) từ 0.000 đến 0.999
            (ABS(CHECKSUM(CAST(MaHocSinh * 7919 + MaMonHoc * 104729 + MaKhoaHoc * 31 AS VARCHAR(50)))) % 1000) / 1000.0 AS Rnd
        FROM RawStudentCourseList
        WHERE rn = 1
    ),
    CalculatedGrades AS (
        SELECT 
            MaHocSinh,
            MaMonHoc,
            MaHocKy,
            MaDonVi,
            NamNhapHoc,
            -- Điểm Quá Trình (4.0 -> 9.8)
            CASE 
                WHEN Rnd < 0.12 THEN CAST(4.0 + (Rnd / 0.12) * 1.5 AS DECIMAL(5,2))
                WHEN Rnd < 0.35 THEN CAST(6.0 + ((Rnd - 0.12) / 0.23) * 1.5 AS DECIMAL(5,2))
                WHEN Rnd < 0.80 THEN CAST(7.5 + ((Rnd - 0.35) / 0.45) * 1.5 AS DECIMAL(5,2))
                ELSE CAST(9.0 + ((Rnd - 0.80) / 0.20) * 0.9 AS DECIMAL(5,2))
            END AS DiemQT,
            -- Điểm Giữa Kỳ (3.5 -> 9.8)
            CASE 
                WHEN Rnd < 0.12 THEN CAST(3.5 + (Rnd / 0.12) * 1.5 AS DECIMAL(5,2))
                WHEN Rnd < 0.35 THEN CAST(5.5 + ((Rnd - 0.12) / 0.23) * 1.5 AS DECIMAL(5,2))
                WHEN Rnd < 0.80 THEN CAST(7.0 + ((Rnd - 0.35) / 0.45) * 1.5 AS DECIMAL(5,2))
                ELSE CAST(8.5 + ((Rnd - 0.80) / 0.20) * 1.3 AS DECIMAL(5,2))
            END AS DiemGK,
            -- Điểm Cuối Kỳ (2.5 -> 9.9)
            CASE 
                WHEN Rnd < 0.12 THEN CAST(2.5 + (Rnd / 0.12) * 1.8 AS DECIMAL(5,2))
                WHEN Rnd < 0.35 THEN CAST(5.0 + ((Rnd - 0.12) / 0.23) * 1.5 AS DECIMAL(5,2))
                WHEN Rnd < 0.80 THEN CAST(7.0 + ((Rnd - 0.35) / 0.45) * 1.5 AS DECIMAL(5,2))
                ELSE CAST(8.5 + ((Rnd - 0.80) / 0.20) * 1.4 AS DECIMAL(5,2))
            END AS DiemCK
        FROM StudentCourseList
    ),
    FinalGrades AS (
        SELECT 
            MaHocSinh,
            MaMonHoc,
            MaHocKy,
            MaDonVi,
            NamNhapHoc,
            DiemQT,
            DiemGK,
            DiemCK,
            -- GPA = DiemQT * 0.3 + DiemGK * 0.2 + DiemCK * 0.5
            CAST(ROUND(DiemQT * 0.3 + DiemGK * 0.2 + DiemCK * 0.5, 2) AS DECIMAL(5,2)) AS GpaCalc
        FROM CalculatedGrades
    )
    -- 2.1: Chèn mới cho những sinh viên chưa có bản ghi trong DiemSo
    INSERT INTO DiemSo (
        ma_don_vi, 
        ma_hoc_ky, 
        ma_hoc_sinh, 
        ma_mon_hoc, 
        nam_nhap_hoc, 
        diem_qua_trinh, 
        diem_giua_ky, 
        diem_cuoi_ky, 
        gpa_mon_hoc, 
        trang_thai, 
        ly_do_rot, 
        da_khoa
    )
    SELECT 
        fg.MaDonVi,
        fg.MaHocKy,
        fg.MaHocSinh,
        fg.MaMonHoc,
        fg.NamNhapHoc,
        fg.DiemQT,
        fg.DiemGK,
        fg.DiemCK,
        fg.GpaCalc,
        CASE WHEN fg.GpaCalc >= 5.0 THEN 'dat' ELSE 'rot' END,
        CASE WHEN fg.GpaCalc < 5.0 THEN N'{"reason":"Không đạt điểm thi kết thúc môn"}' ELSE NULL END,
        0 -- da_khoa = 0 (cho phép giảng viên chỉnh sửa)
    FROM FinalGrades fg
    WHERE NOT EXISTS (
        SELECT 1 FROM DiemSo d 
        WHERE d.ma_hoc_sinh = fg.MaHocSinh 
          AND d.ma_mon_hoc = fg.MaMonHoc 
          AND d.ma_hoc_ky = fg.MaHocKy
    );

    PRINT N'      -> Đã thêm mới các bản ghi DiemSo còn thiếu.';

    -- 2.2: Cập nhật lại những bản ghi DiemSo đã tồn tại nhưng điểm đang bị NULL hoặc 0
    UPDATE d
    SET 
        d.diem_qua_trinh = ISNULL(d.diem_qua_trinh, CAST((d.ma_hoc_sinh % 5) + 5.5 AS DECIMAL(5,2))),
        d.diem_giua_ky   = ISNULL(d.diem_giua_ky,   CAST((d.ma_hoc_sinh % 4) + 6.0 AS DECIMAL(5,2))),
        d.diem_cuoi_ky   = ISNULL(d.diem_cuoi_ky,   CAST((d.ma_hoc_sinh % 6) + 4.5 AS DECIMAL(5,2))),
        d.gpa_mon_hoc    = CASE 
            WHEN d.gpa_mon_hoc > 0 THEN d.gpa_mon_hoc
            ELSE CAST(ROUND(
                ISNULL(d.diem_qua_trinh, (d.ma_hoc_sinh % 5) + 5.5) * 0.3 + 
                ISNULL(d.diem_giua_ky, (d.ma_hoc_sinh % 4) + 6.0) * 0.2 + 
                ISNULL(d.diem_cuoi_ky, (d.ma_hoc_sinh % 6) + 4.5) * 0.5, 2) AS DECIMAL(5,2))
        END,
        d.trang_thai     = CASE 
            WHEN ISNULL(d.gpa_mon_hoc, 0) >= 5.0 THEN 'dat' 
            ELSE 'rot' 
        END
    FROM DiemSo d
    WHERE d.diem_qua_trinh IS NULL 
       OR d.diem_giua_ky IS NULL 
       OR d.diem_cuoi_ky IS NULL 
       OR d.gpa_mon_hoc = 0;

    PRINT N'      -> Đã chuẩn hóa lại các bản ghi DiemSo có điểm rỗng.';

    -- =========================================================================
    -- BƯỚC 3: TẠO BUỔI HỌC (BuoiHoc) VÀ ĐIỂM DANH (DiemDanh) ĐỂ CỘT CHUYÊN CẦN CÓ ĐIỂM
    -- =========================================================================
    PRINT N'[3/5] Đang tạo Buổi học và Dữ liệu điểm danh (Chuyên cần)...';

    -- Lấy mã phòng và mã ca học an toàn từ DB
    DECLARE @DefaultPhong INT, @DefaultCa INT, @DefaultTeacher INT;
    SELECT TOP 1 @DefaultPhong = ma_phong FROM PhongHoc ORDER BY ma_phong;
    SELECT TOP 1 @DefaultCa = ma_ca_hoc FROM CaHoc ORDER BY ma_ca_hoc;
    SELECT TOP 1 @DefaultTeacher = ma_nguoi_dung FROM NguoiDung WHERE vai_tro_chinh = 'giao_vien' ORDER BY ma_nguoi_dung;

    -- 3.1: Đảm bảo có TKB mẫu cho các khóa học chưa có TKB
    INSERT INTO ThoiKhoaBieu (ma_khoa_hoc, ma_phong, ma_ca_hoc, thu_trong_tuan, ngay_bat_dau, ngay_ket_thuc, trang_thai, ngay_tao)
    SELECT 
        k.ma_khoa_hoc,
        ISNULL((SELECT TOP 1 ma_phong FROM PhongHoc WHERE ma_don_vi = k.ma_don_vi AND trang_thai_phong = 'hoat_dong'), @DefaultPhong),
        ISNULL((SELECT TOP 1 ma_ca_hoc FROM CaHoc WHERE con_hoat_dong = 1), @DefaultCa),
        (k.ma_khoa_hoc % 5) + 2,
        CAST('2026-01-05' AS DATE),
        CAST('2026-04-30' AS DATE),
        'nhap',
        @CurrentDate
    FROM KhoaHoc k
    WHERE NOT EXISTS (SELECT 1 FROM ThoiKhoaBieu tkb WHERE tkb.ma_khoa_hoc = k.ma_khoa_hoc);

    -- 3.2: Tạo 10 buổi học đã diễn ra cho mỗi khóa học (chỉ lấy 1 TKB đại diện)
    ;WITH DistinctTKB AS (
        SELECT ma_khoa_hoc, ma_tkb, ma_phong, ma_ca_hoc,
               ROW_NUMBER() OVER (PARTITION BY ma_khoa_hoc ORDER BY ma_tkb) AS rn
        FROM ThoiKhoaBieu
    ),
    Numbers AS (
        SELECT 1 AS N UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5
        UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9 UNION ALL SELECT 10
    )
    INSERT INTO BuoiHoc (ma_tkb, ma_khoa_hoc, ma_phong, ma_ca_hoc, ma_giao_vien, ngay_hoc, trang_thai_buoi, trang_thai_diem_danh, ngay_tao)
    SELECT 
        tkb.ma_tkb,
        k.ma_khoa_hoc,
        tkb.ma_phong,
        tkb.ma_ca_hoc,
        ISNULL(k.ma_giao_vien, @DefaultTeacher),
        CAST(DATEADD(DAY, (num.N - 1) * 7, '2026-01-05') AS DATE),
        'da_dien_ra',
        'da_khoa',
        @CurrentDate
    FROM KhoaHoc k
    JOIN DistinctTKB tkb ON tkb.ma_khoa_hoc = k.ma_khoa_hoc AND tkb.rn = 1
    CROSS JOIN Numbers num
    WHERE NOT EXISTS (
        SELECT 1 FROM BuoiHoc b 
        WHERE b.ma_khoa_hoc = k.ma_khoa_hoc 
          AND b.ngay_hoc = CAST(DATEADD(DAY, (num.N - 1) * 7, '2026-01-05') AS DATE)
    );

    -- 3.3: Chèn điểm danh (90% có mặt, 10% đi muộn/vắng)
    INSERT INTO DiemDanh (ma_buoi_hoc, ma_don_vi, ma_hoc_sinh, trang_thai, nguoi_ghi_nhan, ghi_nhan_luc, he_so_vang)
    SELECT 
        b.ma_buoi_hoc,
        k.ma_don_vi,
        s.ma_nguoi_dung,
        CASE 
            WHEN (b.ma_buoi_hoc + s.ma_nguoi_dung) % 10 < 8 THEN 'co_mat'
            WHEN (b.ma_buoi_hoc + s.ma_nguoi_dung) % 10 = 8 THEN 'di_muon'
            ELSE 'vang'
        END,
        ISNULL(k.ma_giao_vien, @DefaultTeacher),
        @CurrentDate,
        CASE WHEN (b.ma_buoi_hoc + s.ma_nguoi_dung) % 10 = 9 THEN 1.0 ELSE 0.0 END
    FROM BuoiHoc b
    JOIN KhoaHoc k ON b.ma_khoa_hoc = k.ma_khoa_hoc
    JOIN NguoiDung s ON s.ma_lop = k.ma_lop AND s.vai_tro_chinh IN ('hoc_sinh', 'Student')
    WHERE b.trang_thai_buoi = 'da_dien_ra'
      AND NOT EXISTS (
        SELECT 1 FROM DiemDanh dd 
        WHERE dd.ma_buoi_hoc = b.ma_buoi_hoc AND dd.ma_hoc_sinh = s.ma_nguoi_dung
    );

    PRINT N'      -> Đã khởi tạo xong dữ liệu Chuyên cần cho tất cả các lớp.';

    -- =========================================================================
    -- BƯỚC 4: TẠO BÀI NỘP (BaiNop) ĐỂ CỘT "BÀI TẬP & THỰC HÀNH" CÓ ĐIỂM
    -- =========================================================================
    PRINT N'[4/5] Đang tạo Bài nộp (BaiNop) cho các bài tập của môn học...';

    INSERT INTO BaiNop (ma_bai_tap, ma_hoc_sinh, url_tap_tin, so_lan_nop, nop_tre, diem_so, thoi_diem_nop, da_cong_bo)
    SELECT 
        bt.ma_bai_tap,
        s.ma_nguoi_dung,
        'https://storage.lms.local/submissions/assignment_' + CAST(bt.ma_bai_tap AS VARCHAR) + '.zip',
        1,
        0,
        CAST((s.ma_nguoi_dung % 4) + 6.5 AS DECIMAL(5,2)), -- Điểm 6.5 đến 9.5
        @CurrentDate,
        1
    FROM BaiTap bt
    JOIN KhoaHoc k ON k.ma_mon_hoc = bt.ma_mon_hoc
    JOIN NguoiDung s ON s.ma_lop = k.ma_lop AND s.vai_tro_chinh IN ('hoc_sinh', 'Student')
    WHERE NOT EXISTS (
        SELECT 1 FROM BaiNop bn 
        WHERE bn.ma_bai_tap = bt.ma_bai_tap AND bn.ma_hoc_sinh = s.ma_nguoi_dung
    );

    PRINT N'      -> Đã tạo xong dữ liệu Bài tập & Thực hành.';

    -- =========================================================================
    -- BƯỚC 5: TẠO PHIÊN THI (PhienThiHocSinh) ĐỂ CỘT "KIỂM TRA & QUIZ" CÓ ĐIỂM
    -- =========================================================================
    PRINT N'[5/5] Đang tạo Phiên thi (PhienThiHocSinh) cho các đề kiểm tra/quiz...';

    INSERT INTO PhienThiHocSinh (
        ma_de_kiem_tra, 
        ma_hoc_sinh, 
        bat_dau_luc, 
        nop_luc, 
        trang_thai_luong, 
        diem_tu_dong, 
        diem_cuoi_cung, 
        lan_thu, 
        ngay_cap_nhat
    )
    SELECT 
        dkt.ma_de_kiem_tra,
        s.ma_nguoi_dung,
        DATEADD(MINUTE, -45, @CurrentDate),
        @CurrentDate,
        'da_dung',
        CAST((s.ma_nguoi_dung % 5) + 6.0 AS DECIMAL(5,2)), -- Điểm 6.0 đến 10.0
        CAST((s.ma_nguoi_dung % 5) + 6.0 AS DECIMAL(5,2)),
        1,
        @CurrentDate
    FROM DeKiemTra dkt
    JOIN KhoaHoc k ON k.ma_mon_hoc = dkt.ma_mon_hoc
    JOIN NguoiDung s ON s.ma_lop = k.ma_lop AND s.vai_tro_chinh IN ('hoc_sinh', 'Student')
    WHERE NOT EXISTS (
        SELECT 1 FROM PhienThiHocSinh pts 
        WHERE pts.ma_de_kiem_tra = dkt.ma_de_kiem_tra AND pts.ma_hoc_sinh = s.ma_nguoi_dung
    );

    PRINT N'      -> Đã tạo xong dữ liệu Kiểm tra & Quiz.';

    COMMIT TRANSACTION;
    PRINT N'========================================================================';
    PRINT N'=== THÀNH CÔNG: TẤT CẢ SINH VIÊN ĐÃ ĐƯỢC CẬP NHẬT ĐẦY ĐỦ ĐIỂM SỐ! ===';
    PRINT N'========================================================================';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'!!! CÓ LỖI XẢY RA KHI THÊM ĐIỂM SỐ !!!';
    PRINT ERROR_MESSAGE();
END CATCH;
GO
