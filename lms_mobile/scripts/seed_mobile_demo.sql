SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF DB_NAME() <> N'LMS_MobileDemo'
        THROW 51000, N'Script này chỉ được phép chạy trên database LMS_MobileDemo.', 1;

    DECLARE @StudentPasswordHash nvarchar(max) = (
        SELECT mat_khau_hash
        FROM dbo.NguoiDung
        WHERE email = N'p12test_student011@lms.local'
    );
    DECLARE @ParentPasswordHash nvarchar(max) = (
        SELECT mat_khau_hash
        FROM dbo.NguoiDung
        WHERE email = N'p15test_parent01@lms.local'
    );

    IF @StudentPasswordHash IS NULL OR @ParentPasswordHash IS NULL
        THROW 51001, N'Không tìm thấy tài khoản chuẩn để kế thừa mật khẩu demo.', 1;

    DECLARE @Specializations TABLE (
        major_code nvarchar(20) NOT NULL,
        specialization_name nvarchar(255) NOT NULL,
        description nvarchar(max) NULL,
        program_code nvarchar(100) NOT NULL,
        program_name nvarchar(255) NOT NULL,
        source_program_code nvarchar(100) NOT NULL,
        class_code nvarchar(50) NOT NULL,
        class_name nvarchar(255) NOT NULL
    );

    INSERT INTO @Specializations
        (major_code, specialization_name, description, program_code, program_name,
         source_program_code, class_code, class_name)
    VALUES
        (N'CNTT', N'Trí tuệ nhân tạo ứng dụng', N'Chuyên ngành demo mobile về AI ứng dụng.',
         N'CT_AI_K2026', N'Chương trình Trí tuệ nhân tạo ứng dụng K2026', N'CT_CNTT_K2026',
         N'AI1901', N'AI1901 - Trí tuệ nhân tạo ứng dụng K2026'),
        (N'CNTT', N'An toàn thông tin', N'Chuyên ngành demo mobile về bảo mật và an toàn hệ thống.',
         N'CT_ATTT_K2026', N'Chương trình An toàn thông tin K2026', N'CT_CNTT_K2026',
         N'ATTT1901', N'ATTT1901 - An toàn thông tin K2026'),
        (N'TKDH', N'Thiết kế đồ họa số', N'Chuyên ngành demo mobile về thiết kế số và truyền thông đa phương tiện.',
         N'CT_TKSO_K2026', N'Chương trình Thiết kế đồ họa số K2026', N'CT_TKDH_K2026',
         N'TKSO1901', N'TKSO1901 - Thiết kế đồ họa số K2026'),
        (N'MKT', N'Thương mại điện tử', N'Chuyên ngành demo mobile kết hợp marketing và thương mại điện tử.',
         N'CT_TMDT_K2026', N'Chương trình Thương mại điện tử K2026', N'CT_MKT_K2026',
         N'TMDT1901', N'TMDT1901 - Thương mại điện tử K2026');

    INSERT INTO dbo.ChuyenNganh (ma_nganh, ten_chuyen_nganh, mo_ta, con_hoat_dong)
    SELECT n.ma_nganh, s.specialization_name, s.description, 1
    FROM @Specializations s
    JOIN dbo.NganhDaoTao n ON n.ma_code_nganh = s.major_code
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.ChuyenNganh cn
        WHERE cn.ma_nganh = n.ma_nganh
          AND cn.ten_chuyen_nganh = s.specialization_name
    );

    INSERT INTO dbo.ChuongTrinhDaoTao (
        ma_chuyen_nganh, ma_khoa_tuyen_sinh, ma_code_chuong_trinh,
        ten_chuong_trinh, version, so_hoc_ky, thoi_gian_dao_tao_thang,
        tong_tin_chi_yeu_cau, so_tin_chi_toi_thieu_moi_ky,
        so_tin_chi_toi_da_moi_ky, trang_thai, mo_ta,
        nguon_chuong_trinh_id, ghi_chu_thay_doi, ngay_hieu_luc,
        con_hoat_dong
    )
    SELECT
        cn.ma_chuyen_nganh, source.ma_khoa_tuyen_sinh, s.program_code,
        s.program_name, source.version, source.so_hoc_ky,
        source.thoi_gian_dao_tao_thang, source.tong_tin_chi_yeu_cau,
        source.so_tin_chi_toi_thieu_moi_ky, source.so_tin_chi_toi_da_moi_ky,
        N'active', s.description, source.ma_chuong_trinh,
        N'Mobile demo seed', source.ngay_hieu_luc, 1
    FROM @Specializations s
    JOIN dbo.NganhDaoTao n ON n.ma_code_nganh = s.major_code
    JOIN dbo.ChuyenNganh cn
      ON cn.ma_nganh = n.ma_nganh
     AND cn.ten_chuyen_nganh = s.specialization_name
    JOIN dbo.ChuongTrinhDaoTao source
      ON source.ma_code_chuong_trinh = s.source_program_code
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.ChuongTrinhDaoTao ct
        WHERE ct.ma_code_chuong_trinh = s.program_code
    );

    INSERT INTO dbo.LopHanhChinh (
        ma_don_vi, ma_code_lop, ten_lop, ma_chuong_trinh,
        nam_nhap_hoc, con_hoat_dong, si_so_du_kien
    )
    SELECT 3, s.class_code, s.class_name, ct.ma_chuong_trinh, 2026, 1, 30
    FROM @Specializations s
    JOIN dbo.ChuongTrinhDaoTao ct
      ON ct.ma_code_chuong_trinh = s.program_code
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.LopHanhChinh l WHERE l.ma_code_lop = s.class_code
    );

    DECLARE @Students TABLE (
        email nvarchar(255) NOT NULL,
        full_name nvarchar(255) NOT NULL,
        class_code nvarchar(50) NOT NULL,
        phone nvarchar(20) NULL
    );

    INSERT INTO @Students (email, full_name, class_code, phone)
    VALUES
        (N'mobile.student01@lms.local', N'Nguyễn Minh Anh', N'SD1901', N'0901000001'),
        (N'mobile.student02@lms.local', N'Trần Gia Bảo', N'SD1902', N'0901000002'),
        (N'mobile.student03@lms.local', N'Lê Hoàng Duy', N'AI1901', N'0901000003'),
        (N'mobile.student04@lms.local', N'Phạm Khánh Linh', N'AI1901', N'0901000004'),
        (N'mobile.student05@lms.local', N'Võ Quốc Huy', N'ATTT1901', N'0901000005'),
        (N'mobile.student06@lms.local', N'Đặng Ngọc Mai', N'ATTT1901', N'0901000006'),
        (N'mobile.student07@lms.local', N'Bùi Thanh Tùng', N'TKDH1901', N'0901000007'),
        (N'mobile.student08@lms.local', N'Đỗ Mỹ Duyên', N'TKSO1901', N'0901000008'),
        (N'mobile.student09@lms.local', N'Huỳnh Đức Long', N'MKT1901', N'0901000009'),
        (N'mobile.student10@lms.local', N'Ngô Thảo Vy', N'MKT1902', N'0901000010'),
        (N'mobile.student11@lms.local', N'Phan Nhật Nam', N'TMDT1901', N'0901000011'),
        (N'mobile.student12@lms.local', N'Trương Hà My', N'TMDT1901', N'0901000012');

    INSERT INTO dbo.NguoiDung (
        ma_don_vi, email, ho_ten, vai_tro_chinh, ma_lop,
        so_dien_thoai, trang_thai, nam_nhap_hoc, mat_khau_hash,
        so_lan_sai_mat_khau, dang_nhap_lan_dau
    )
    SELECT 3, s.email, s.full_name, N'hoc_sinh', l.ma_lop,
           s.phone, N'hoat_dong', 2026, @StudentPasswordHash, 0, 0
    FROM @Students s
    JOIN dbo.LopHanhChinh l ON l.ma_code_lop = s.class_code
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.NguoiDung u WHERE u.email = s.email
    );

    DECLARE @Parents TABLE (
        email nvarchar(255) NOT NULL,
        full_name nvarchar(255) NOT NULL,
        phone nvarchar(20) NULL
    );

    INSERT INTO @Parents (email, full_name, phone)
    VALUES
        (N'mobile.parent01@lms.local', N'Nguyễn Văn Hùng', N'0912000001'),
        (N'mobile.parent02@lms.local', N'Trần Thị Thu', N'0912000002'),
        (N'mobile.parent03@lms.local', N'Lê Quốc Tuấn', N'0912000003'),
        (N'mobile.parent04@lms.local', N'Phạm Ngọc Lan', N'0912000004'),
        (N'mobile.parent05@lms.local', N'Võ Minh Đức', N'0912000005'),
        (N'mobile.parent06@lms.local', N'Trương Thanh Hà', N'0912000006');

    INSERT INTO dbo.NguoiDung (
        ma_don_vi, email, ho_ten, vai_tro_chinh, so_dien_thoai,
        trang_thai, mat_khau_hash, so_lan_sai_mat_khau, dang_nhap_lan_dau
    )
    SELECT 3, p.email, p.full_name, N'phu_huynh', p.phone,
           N'hoat_dong', @ParentPasswordHash, 0, 0
    FROM @Parents p
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.NguoiDung u WHERE u.email = p.email
    );

    DECLARE @Links TABLE (
        parent_email nvarchar(255) NOT NULL,
        student_email nvarchar(255) NOT NULL
    );

    -- Chuyển hai liên kết demo cũ sang sinh viên thuộc chuyên ngành mới.
    UPDATE link
    SET ma_hoc_sinh = replacement.ma_nguoi_dung
    FROM dbo.LienKetPhuHuynh link
    JOIN dbo.NguoiDung parent
      ON parent.ma_nguoi_dung = link.ma_phu_huynh
     AND parent.email = N'p15test_parent01@lms.local'
    JOIN dbo.NguoiDung current_student
      ON current_student.ma_nguoi_dung = link.ma_hoc_sinh
     AND current_student.email = N'mobile.student01@lms.local'
    JOIN dbo.NguoiDung replacement
      ON replacement.email = N'mobile.student03@lms.local'
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.LienKetPhuHuynh existing
        WHERE existing.ma_phu_huynh = parent.ma_nguoi_dung
          AND existing.ma_hoc_sinh = replacement.ma_nguoi_dung
    );

    UPDATE link
    SET ma_hoc_sinh = replacement.ma_nguoi_dung
    FROM dbo.LienKetPhuHuynh link
    JOIN dbo.NguoiDung parent
      ON parent.ma_nguoi_dung = link.ma_phu_huynh
     AND parent.email = N'p15test_parent01@lms.local'
    JOIN dbo.NguoiDung current_student
      ON current_student.ma_nguoi_dung = link.ma_hoc_sinh
     AND current_student.email = N'mobile.student02@lms.local'
    JOIN dbo.NguoiDung replacement
      ON replacement.email = N'mobile.student05@lms.local'
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.LienKetPhuHuynh existing
        WHERE existing.ma_phu_huynh = parent.ma_nguoi_dung
          AND existing.ma_hoc_sinh = replacement.ma_nguoi_dung
    );

    INSERT INTO @Links (parent_email, student_email)
    VALUES
        (N'p15test_parent01@lms.local', N'p12test_student011@lms.local'),
        (N'p15test_parent01@lms.local', N'mobile.student03@lms.local'),
        (N'p15test_parent01@lms.local', N'mobile.student05@lms.local'),
        (N'p15test_parent01@lms.local', N'mobile.student08@lms.local'),
        (N'p15test_parent01@lms.local', N'mobile.student11@lms.local'),
        (N'parent01@lms.local', N'student01@edulms.local'),
        (N'parent01@lms.local', N'student.tkdh01@lms.local'),
        (N'mobile.parent01@lms.local', N'mobile.student01@lms.local'),
        (N'mobile.parent01@lms.local', N'mobile.student02@lms.local'),
        (N'mobile.parent02@lms.local', N'mobile.student03@lms.local'),
        (N'mobile.parent02@lms.local', N'mobile.student04@lms.local'),
        (N'mobile.parent03@lms.local', N'mobile.student05@lms.local'),
        (N'mobile.parent03@lms.local', N'mobile.student06@lms.local'),
        (N'mobile.parent04@lms.local', N'mobile.student07@lms.local'),
        (N'mobile.parent04@lms.local', N'mobile.student08@lms.local'),
        (N'mobile.parent05@lms.local', N'mobile.student09@lms.local'),
        (N'mobile.parent05@lms.local', N'mobile.student10@lms.local'),
        (N'mobile.parent06@lms.local', N'mobile.student11@lms.local'),
        (N'mobile.parent06@lms.local', N'mobile.student12@lms.local');

    INSERT INTO dbo.LienKetPhuHuynh (
        ma_phu_huynh, ma_hoc_sinh, quyen_xem, trang_thai, lien_ket_luc
    )
    SELECT parent.ma_nguoi_dung, student.ma_nguoi_dung,
           N'{"grades":true,"attendance":true,"tuition":true,"schedule":true}',
           N'hoat_dong', SYSUTCDATETIME()
    FROM @Links link
    JOIN dbo.NguoiDung parent ON parent.email = link.parent_email
    JOIN dbo.NguoiDung student ON student.email = link.student_email
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.LienKetPhuHuynh existing
        WHERE existing.ma_phu_huynh = parent.ma_nguoi_dung
          AND existing.ma_hoc_sinh = student.ma_nguoi_dung
    );

    DECLARE @InvoiceStudents TABLE (student_id int PRIMARY KEY);
    INSERT INTO @InvoiceStudents (student_id)
    SELECT u.ma_nguoi_dung
    FROM dbo.NguoiDung u
    WHERE u.email IN (
        N'p12test_student011@lms.local', N'student.cntt01@lms.local',
        N'student01@edulms.local', N'student.tkdh01@lms.local',
        N'student.mkt01@lms.local'
    ) OR u.email LIKE N'mobile.student%@lms.local';

    DECLARE @SemesterOneId int = (
        SELECT TOP (1) ma_hoc_ky FROM dbo.HocKy WHERE ma_code_hoc_ky = N'HK1_2026'
    );
    DECLARE @SemesterTwoId int = (
        SELECT TOP (1) ma_hoc_ky FROM dbo.HocKy WHERE ma_code_hoc_ky = N'HK2_2026'
    );

    IF @SemesterOneId IS NULL OR @SemesterTwoId IS NULL
        THROW 51002, N'Không tìm thấy học kỳ HK1_2026/HK2_2026 để tạo hóa đơn demo.', 1;

    INSERT INTO dbo.HoaDon (
        ma_don_vi, ma_hoc_sinh, ma_hoc_ky, ma_hoa_don_code,
        loai_hoa_don, so_tien, giam_tru, da_thanh_toan,
        trang_thai, han_thanh_toan, ghi_chu
    )
    SELECT 3, s.student_id, @SemesterOneId,
           CONCAT(N'MOB-HK1-', s.student_id), N'hoc_phi',
           9000000, 0, 9000000, N'da_thanh_toan',
           CONVERT(date, '2026-04-15'), N'Hóa đơn đã thanh toán - Mobile demo seed'
    FROM @InvoiceStudents s
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.HoaDon h
        WHERE h.ma_hoc_sinh = s.student_id
          AND h.ma_hoc_ky = @SemesterOneId
          AND h.loai_hoa_don = N'hoc_phi'
    );

    INSERT INTO dbo.HoaDon (
        ma_don_vi, ma_hoc_sinh, ma_hoc_ky, ma_hoa_don_code,
        loai_hoa_don, so_tien, giam_tru, da_thanh_toan,
        trang_thai, han_thanh_toan, ghi_chu
    )
    SELECT 3, s.student_id, @SemesterTwoId,
           CONCAT(N'MOB-HK2-', s.student_id), N'hoc_phi',
           9500000,
           CASE WHEN s.student_id % 4 = 0 THEN 500000 ELSE 0 END,
           CASE WHEN s.student_id % 3 = 0 THEN 2500000 ELSE 0 END,
           CASE WHEN s.student_id % 3 = 0
                THEN N'thanh_toan_mot_phan'
                ELSE N'chua_thanh_toan' END,
           CONVERT(date, '2026-08-20'), N'Hóa đơn cần thanh toán - Mobile demo seed'
    FROM @InvoiceStudents s
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.HoaDon h
        WHERE h.ma_hoc_sinh = s.student_id
          AND h.ma_hoc_ky = @SemesterTwoId
          AND h.loai_hoa_don = N'hoc_phi'
    );

    -- Tạo nhiều trạng thái công nợ để kiểm tra badge và số tiền còn lại.
    UPDATE invoice
    SET giam_tru = CASE WHEN demo.student_number % 3 = 0 THEN 500000 ELSE 0 END,
        da_thanh_toan = CASE WHEN demo.student_number % 4 = 1 THEN 2500000 ELSE 0 END,
        trang_thai = CASE
            WHEN demo.student_number % 4 = 0 THEN N'qua_han'
            WHEN demo.student_number % 4 = 1 THEN N'thanh_toan_mot_phan'
            ELSE N'chua_thanh_toan'
        END,
        han_thanh_toan = CASE
            WHEN demo.student_number % 4 = 0 THEN CONVERT(date, '2026-07-15')
            ELSE CONVERT(date, '2026-08-20')
        END,
        ghi_chu = N'Hóa đơn nhiều trạng thái - Mobile demo seed'
    FROM dbo.HoaDon invoice
    JOIN dbo.NguoiDung student ON student.ma_nguoi_dung = invoice.ma_hoc_sinh
    CROSS APPLY (
        SELECT TRY_CONVERT(int, RIGHT(LEFT(student.email, CHARINDEX('@', student.email) - 1), 2))
    ) demo(student_number)
    WHERE invoice.ma_hoc_ky = @SemesterTwoId
      AND invoice.loai_hoa_don = N'hoc_phi'
      AND student.email LIKE N'mobile.student%@lms.local'
      AND demo.student_number IS NOT NULL;

    DECLARE @GradeStudents TABLE (
        student_id int PRIMARY KEY,
        email nvarchar(255) NOT NULL,
        track nvarchar(20) NOT NULL,
        student_number int NOT NULL
    );

    INSERT INTO @GradeStudents (student_id, email, track, student_number)
    SELECT student.ma_nguoi_dung, student.email,
           CASE
               WHEN class.ma_code_lop LIKE N'TK%' THEN N'design'
               WHEN class.ma_code_lop LIKE N'MKT%' OR class.ma_code_lop LIKE N'TMDT%' THEN N'marketing'
               ELSE N'tech'
           END,
           TRY_CONVERT(int, RIGHT(LEFT(student.email, CHARINDEX('@', student.email) - 1), 2))
    FROM dbo.NguoiDung student
    JOIN dbo.LopHanhChinh class ON class.ma_lop = student.ma_lop
    WHERE student.email LIKE N'mobile.student%@lms.local';

    DECLARE @TrackSubjects TABLE (
        track nvarchar(20) NOT NULL,
        semester_id int NOT NULL,
        position int NOT NULL,
        subject_code nvarchar(50) NOT NULL
    );

    INSERT INTO @TrackSubjects (track, semester_id, position, subject_code)
    VALUES
        (N'tech', @SemesterOneId, 1, N'COM101'),
        (N'tech', @SemesterOneId, 2, N'CTDL101'),
        (N'tech', @SemesterOneId, 3, N'COM102'),
        (N'tech', @SemesterTwoId, 1, N'SEC101'),
        (N'tech', @SemesterTwoId, 2, N'API101'),
        (N'tech', @SemesterTwoId, 3, N'MOB101'),
        (N'design', @SemesterOneId, 1, N'DES101'),
        (N'design', @SemesterOneId, 2, N'DES102'),
        (N'design', @SemesterOneId, 3, N'DES103'),
        (N'design', @SemesterTwoId, 1, N'DES106'),
        (N'design', @SemesterTwoId, 2, N'DES111'),
        (N'design', @SemesterTwoId, 3, N'DES112'),
        (N'marketing', @SemesterOneId, 1, N'MKT101'),
        (N'marketing', @SemesterOneId, 2, N'MKT102'),
        (N'marketing', @SemesterOneId, 3, N'MKT103'),
        (N'marketing', @SemesterTwoId, 1, N'MKT105'),
        (N'marketing', @SemesterTwoId, 2, N'MKT107'),
        (N'marketing', @SemesterTwoId, 3, N'MKT109');

    INSERT INTO dbo.DiemSo (
        ma_don_vi, ma_hoc_sinh, ma_mon_hoc, ma_hoc_ky,
        diem_qua_trinh, diem_giua_ky, diem_cuoi_ky,
        gpa_mon_hoc, trang_thai, da_khoa, ly_do_rot, nam_nhap_hoc
    )
    SELECT 3, student.student_id, subject.ma_mon_hoc, track_subject.semester_id,
           CASE WHEN score.gpa + 0.4 > 10 THEN 10 ELSE score.gpa + 0.4 END,
           score.gpa,
           CASE WHEN score.gpa - 0.2 < 0 THEN 0 ELSE score.gpa - 0.2 END,
           score.gpa,
           CASE WHEN score.gpa >= 5 THEN N'dat' ELSE N'rot' END,
           1,
           CASE WHEN score.gpa < 5
                THEN N'{"reason":"Điểm tổng kết dưới 5 - nợ môn","demo":true}'
                ELSE NULL END,
           2026
    FROM @GradeStudents student
    JOIN @TrackSubjects track_subject ON track_subject.track = student.track
    JOIN dbo.DanhMucMonHoc subject
      ON subject.ma_code_mon_hoc = track_subject.subject_code
    CROSS APPLY (
        SELECT CAST(CASE student.student_number % 4
            WHEN 3 THEN 8.2 + (track_subject.position * 0.25)
            WHEN 0 THEN 7.0 + (track_subject.position * 0.20)
            WHEN 2 THEN 5.4 + (track_subject.position * 0.25)
            ELSE CASE track_subject.position
                WHEN 1 THEN 6.1
                WHEN 2 THEN 3.4
                ELSE 6.8
            END
        END AS decimal(4,2))
    ) score(gpa)
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.DiemSo existing
        WHERE existing.ma_hoc_sinh = student.student_id
          AND existing.ma_mon_hoc = subject.ma_mon_hoc
          AND existing.ma_hoc_ky = track_subject.semester_id
    );

    -- Mỗi sinh viên demo có đủ: có mặt, đi muộn, có phép và vắng.
    INSERT INTO dbo.DiemDanh (
        ma_don_vi, ma_buoi_hoc, ma_hoc_sinh, trang_thai,
        nguoi_ghi_nhan, ghi_nhan_luc, he_so_vang
    )
    SELECT 3, session.ma_buoi_hoc, student.student_id,
           CASE (student.student_number + session.ma_buoi_hoc) % 5
               WHEN 0 THEN N'vang'
               WHEN 1 THEN N'di_muon'
               WHEN 2 THEN N'co_phep'
               ELSE N'co_mat'
           END,
           session.ma_giao_vien, SYSUTCDATETIME(),
           CASE WHEN (student.student_number + session.ma_buoi_hoc) % 5 = 0 THEN 1 ELSE 0 END
    FROM @GradeStudents student
    CROSS JOIN (
        SELECT TOP (5) ma_buoi_hoc, ma_giao_vien
        FROM dbo.BuoiHoc
        WHERE trang_thai_buoi = N'da_dien_ra'
        ORDER BY ma_buoi_hoc
    ) session
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.DiemDanh existing
        WHERE existing.ma_buoi_hoc = session.ma_buoi_hoc
          AND existing.ma_hoc_sinh = student.student_id
    );

    -- Lịch sử đóng tiền thành công tương ứng hóa đơn HK1 đã thanh toán.
    INSERT INTO dbo.GiaoDich (
        ma_hoa_don, ma_tham_chieu_noi_bo, so_tien,
        loai_giao_dich, trang_thai, nha_cung_cap_thanh_toan,
        noi_dung_chuyen_khoan, ngay_thanh_toan,
        ma_nguoi_thuc_hien, chu_thich
    )
    SELECT invoice.ma_hoa_don, CONCAT(N'MOB-PAID-', invoice.ma_hoa_don),
           invoice.da_thanh_toan, N'thanh_toan_hoc_phi', N'thanh_cong',
           N'payos', CONCAT(N'LMS DEMO ', invoice.ma_hoa_don),
           DATEADD(day, -20, SYSUTCDATETIME()), invoice.ma_hoc_sinh,
           N'Giao dịch đã đóng - Mobile demo seed'
    FROM dbo.HoaDon invoice
    WHERE invoice.ma_hoc_ky = @SemesterOneId
      AND invoice.trang_thai = N'da_thanh_toan'
      AND NOT EXISTS (
          SELECT 1 FROM dbo.GiaoDich payment
          WHERE payment.ma_tham_chieu_noi_bo = CONCAT(N'MOB-PAID-', invoice.ma_hoa_don)
      );

    COMMIT TRANSACTION;

    SELECT
        (SELECT COUNT(*) FROM dbo.NguoiDung WHERE vai_tro_chinh = N'hoc_sinh') AS StudentCount,
        (SELECT COUNT(*) FROM dbo.NguoiDung WHERE vai_tro_chinh = N'phu_huynh') AS ParentCount,
        (SELECT COUNT(*) FROM dbo.ChuyenNganh) AS SpecializationCount,
        (SELECT COUNT(*) FROM dbo.LienKetPhuHuynh WHERE trang_thai = N'hoat_dong') AS ActiveParentLinks,
        (SELECT COUNT(*) FROM dbo.HoaDon) AS InvoiceCount,
        (SELECT COUNT(*) FROM dbo.DiemSo WHERE ma_hoc_sinh IN (SELECT student_id FROM @GradeStudents)) AS DemoGradeCount,
        (SELECT COUNT(*) FROM dbo.DiemDanh WHERE ma_hoc_sinh IN (SELECT student_id FROM @GradeStudents)) AS DemoAttendanceCount;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
