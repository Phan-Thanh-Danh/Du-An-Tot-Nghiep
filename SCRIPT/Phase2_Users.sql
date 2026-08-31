USE LMS;
GO

SET NOCOUNT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();
DECLARE @DefaultPasswordHash NVARCHAR(MAX) = 'PBKDF2.100000.E08Uerno/mWBsiCCQpVZuQ==.H6STe78bQZXHynCV5JXHIBve2jSLcXJt1/INXAUWF/4='; -- Hash cho '123456'

PRINT N'--- BẮT ĐẦU PHASE 2: TẠO TÀI KHOẢN (MẬT KHẨU: 123456) ---';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==========================================
    -- 0. SEED BẢNG VAI TRÒ & QUYỀN HẠN (RBAC)
    -- ==========================================
    IF NOT EXISTS (SELECT 1 FROM VaiTro WHERE ma_code_vai_tro = 'sieu_quan_tri')
    BEGIN
        INSERT INTO VaiTro (ma_vai_tro, ma_code_vai_tro, ten_vai_tro) VALUES
            (1,  'sieu_quan_tri',           N'Siêu quản trị'),
            (2,  'quan_tri',                N'Quản trị'),
            (3,  'quan_tri_co_so',          N'Quản trị cơ sở'),
            (4,  'quan_tri_co_so_con',      N'Quản trị cơ sở con'),
            (5,  'hieu_truong',             N'Hiệu trưởng'),
            (6,  'chu_tich',               N'Chủ tịch hội đồng'),
            (7,  'giao_vien',              N'Giảng viên'),
            (8,  'nhan_vien',              N'Nhân viên giáo vụ'),
            (9,  'hoc_sinh',               N'Sinh viên'),
            (10, 'phu_huynh',              N'Phụ huynh'),
            (11, 'hoidong_quanly_noidung', N'Hội đồng quản lý nội dung'),
            (12, 'admin_tai_chinh',        N'Admin tài chính'),
            (13, 'ke_toan_co_so',          N'Kế toán cơ sở'),
            (14, 'ke_toan_truong_co_so',   N'Kế toán trưởng cơ sở');
    END

    -- Seed Danh mục Quyền hạn (QuyenHan)
    DECLARE @Perms TABLE (code NVARCHAR(100), ten NVARCHAR(200), mod NVARCHAR(50), act NVARCHAR(50), mota NVARCHAR(500));
    INSERT INTO @Perms VALUES
        ('training.read', N'Xem chương trình đào tạo & môn học', 'training', 'read', N'Cho phép xem môn học và khung chương trình'),
        ('training.create', N'Tạo mới môn học & CTĐT', 'training', 'create', N'Cho phép tạo môn học mới'),
        ('training.update', N'Chỉnh sửa môn học', 'training', 'update', N'Cho phép chỉnh sửa môn học'),
        ('training.delete', N'Xóa môn học', 'training', 'delete', N'Cho phép xóa môn học'),
        ('training.manage_curriculum', N'Quản lý đề cương & CTĐT', 'training', 'approve', N'Cho phép duyệt CTĐT'),
        ('schedules.read', N'Xem thời khóa biểu & lịch học', 'schedules', 'read', N'Cho phép xem lịch học và phòng học'),
        ('schedules.create', N'Xếp lịch học', 'schedules', 'create', N'Cho phép tạo TKB'),
        ('schedules.update', N'Điều chỉnh lịch học', 'schedules', 'update', N'Cho phép đổi ca học, đổi phòng'),
        ('schedules.delete', N'Hủy lịch học', 'schedules', 'delete', N'Cho phép hủy TKB'),
        ('schedules.approve', N'Phê duyệt TKB', 'schedules', 'approve', N'Cho phép BGH duyệt TKB'),
        ('exams.read', N'Xem bảng điểm & lịch thi', 'exams', 'read', N'Cho phép xem điểm số và lịch thi'),
        ('exams.create', N'Tạo ca thi & đề thi', 'exams', 'create', N'Cho phép tạo ca thi'),
        ('exams.update', N'Nhập & sửa điểm', 'exams', 'update', N'Cho phép nhập điểm thi'),
        ('exams.delete', N'Hủy ca thi', 'exams', 'delete', N'Cho phép xóa ca thi'),
        ('exams.grade', N'Chấm bài & tổng kết GPA', 'exams', 'update', N'Cho phép chấm bài thi'),
        ('exams.unlock_grade', N'Mở khóa bảng điểm', 'exams', 'approve', N'Cho phép duyệt mở khóa điểm'),
        ('requests.read', N'Xem đơn từ', 'requests', 'read', N'Cho phép xem đơn từ sinh viên'),
        ('requests.create', N'Tạo & gửi đơn', 'requests', 'create', N'Cho phép gửi đơn xin nghỉ, phúc khảo'),
        ('requests.update', N'Xử lý đơn từ', 'requests', 'update', N'Cho phép tiếp nhận xử lý đơn'),
        ('requests.delete', N'Hủy đơn từ', 'requests', 'delete', N'Cho phép xóa đơn từ'),
        ('requests.process', N'Phê duyệt đơn từ', 'requests', 'approve', N'Cho phép duyệt hoặc từ chối đơn'),
        ('accounts.read', N'Xem tài khoản', 'accounts', 'read', N'Cho phép xem tài khoản'),
        ('accounts.create', N'Tạo tài khoản', 'accounts', 'create', N'Cho phép tạo tài khoản'),
        ('accounts.update', N'Chỉnh sửa tài khoản', 'accounts', 'update', N'Cho phép sửa tài khoản'),
        ('accounts.delete', N'Xóa tài khoản', 'accounts', 'delete', N'Cho phép xóa tài khoản'),
        ('campus.read', N'Xem cơ sở', 'campus', 'read', N'Cho phép xem cơ sở'),
        ('campus.create', N'Tạo cơ sở', 'campus', 'create', N'Cho phép tạo cơ sở'),
        ('campus.update', N'Sửa cơ sở', 'campus', 'update', N'Cho phép sửa cơ sở'),
        ('campus.delete', N'Xóa cơ sở', 'campus', 'delete', N'Cho phép xóa cơ sở'),
        ('finance.read', N'Xem tài chính học phí', 'finance', 'read', N'Cho phép xem học phí'),
        ('finance.create', N'Tạo giao dịch học phí', 'finance', 'create', N'Cho phép tạo hóa đơn'),
        ('finance.update', N'Sửa giao dịch học phí', 'finance', 'update', N'Cho phép cập nhật giao dịch'),
        ('finance.delete', N'Hủy giao dịch học phí', 'finance', 'delete', N'Cho phép hủy giao dịch'),
        ('reports.read', N'Xem báo cáo học vụ', 'reports', 'read', N'Cho phép xem biểu đồ và báo cáo GPA'),
        ('reports.export', N'Xuất báo cáo', 'reports', 'export', N'Cho phép xuất file báo cáo'),
        ('reports.ai_analysis', N'Phân tích AI', 'reports', 'approve', N'Cho phép xem phân tích nguy cơ rớt môn');

    INSERT INTO QuyenHan (ma_code, ten_quyen_han, module, action, mo_ta)
    SELECT code, ten, mod, act, mota FROM @Perms p
    WHERE NOT EXISTS (SELECT 1 FROM QuyenHan q WHERE q.ma_code = p.code);

    -- Gán quyền cho các Vai Trò (VaiTroQuyenHan)
    -- Sinh viên (hoc_sinh - role 9): xem học tập, TKB, kết quả thi, gửi đơn từ
    INSERT INTO VaiTroQuyenHan (ma_vai_tro, ma_quyen_han, ngay_cap)
    SELECT 9, q.ma_quyen_han, @CurrentDate FROM QuyenHan q
    WHERE q.ma_code IN ('training.read', 'schedules.read', 'exams.read', 'requests.read', 'requests.create')
    AND NOT EXISTS (SELECT 1 FROM VaiTroQuyenHan v WHERE v.ma_vai_tro = 9 AND v.ma_quyen_han = q.ma_quyen_han);

    -- Giảng viên (giao_vien - role 7)
    INSERT INTO VaiTroQuyenHan (ma_vai_tro, ma_quyen_han, ngay_cap)
    SELECT 7, q.ma_quyen_han, @CurrentDate FROM QuyenHan q
    WHERE q.ma_code IN ('training.read', 'schedules.read', 'exams.read', 'exams.update', 'exams.grade', 'requests.read', 'requests.update', 'requests.create', 'reports.read')
    AND NOT EXISTS (SELECT 1 FROM VaiTroQuyenHan v WHERE v.ma_vai_tro = 7 AND v.ma_quyen_han = q.ma_quyen_han);

    -- Nhân viên giáo vụ (nhan_vien - role 8)
    INSERT INTO VaiTroQuyenHan (ma_vai_tro, ma_quyen_han, ngay_cap)
    SELECT 8, q.ma_quyen_han, @CurrentDate FROM QuyenHan q
    WHERE q.ma_code IN ('training.read', 'training.create', 'training.update', 'schedules.read', 'schedules.create', 'schedules.update', 'exams.read', 'exams.create', 'requests.read', 'requests.update', 'requests.process', 'reports.read', 'reports.export')
    AND NOT EXISTS (SELECT 1 FROM VaiTroQuyenHan v WHERE v.ma_vai_tro = 8 AND v.ma_quyen_han = q.ma_quyen_han);

    -- Ban giám hiệu (hieu_truong - role 5)
    INSERT INTO VaiTroQuyenHan (ma_vai_tro, ma_quyen_han, ngay_cap)
    SELECT 5, q.ma_quyen_han, @CurrentDate FROM QuyenHan q
    WHERE q.ma_code IN ('training.read', 'training.manage_curriculum', 'schedules.read', 'schedules.approve', 'exams.read', 'exams.unlock_grade', 'requests.read', 'requests.process', 'reports.read', 'reports.export', 'reports.ai_analysis')
    AND NOT EXISTS (SELECT 1 FROM VaiTroQuyenHan v WHERE v.ma_vai_tro = 5 AND v.ma_quyen_han = q.ma_quyen_han);

    -- Quản trị hệ thống (sieu_quan_tri: 1, quan_tri: 2, quan_tri_co_so: 3) -> Toàn quyền
    INSERT INTO VaiTroQuyenHan (ma_vai_tro, ma_quyen_han, ngay_cap)
    SELECT v.ma_vai_tro, q.ma_quyen_han, @CurrentDate 
    FROM VaiTro v CROSS JOIN QuyenHan q
    WHERE v.ma_vai_tro IN (1, 2, 3)
    AND NOT EXISTS (SELECT 1 FROM VaiTroQuyenHan x WHERE x.ma_vai_tro = v.ma_vai_tro AND x.ma_quyen_han = q.ma_quyen_han);

    PRINT N'  [OK] VaiTro, QuyenHan & VaiTroQuyenHan da seed day du';

    -- ==========================================
    -- 0.5 TẠO TÀI KHOẢN CẤP CAO (TOÀN TRƯỜNG)
    -- ==========================================
    DECLARE @RootId INT;
    SELECT TOP 1 @RootId = ma_don_vi FROM DonVi WHERE cap_don_vi = 'root';

    IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE email = 'superadmin@aet.local')
    BEGIN
        INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, mat_khau_hash, vai_tro_chinh, trang_thai, dang_nhap_lan_dau, ngay_tao)
        VALUES ('superadmin@aet.local', N'Siêu Quản Trị Hệ Thống', @RootId, @DefaultPasswordHash, 'sieu_quan_tri', 'hoat_dong', 0, @CurrentDate);
    END

    IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE email = 'hdqlnd@aet.local')
    BEGIN
        INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, mat_khau_hash, vai_tro_chinh, trang_thai, dang_nhap_lan_dau, ngay_tao)
        VALUES ('hdqlnd@aet.local', N'Hội Đồng Quản Lý Nội Dung', @RootId, @DefaultPasswordHash, 'hoidong_quanly_noidung', 'hoat_dong', 0, @CurrentDate);
    END

    -- Gán quyền cho các tài khoản cấp cao
    INSERT INTO PhanQuyenNguoiDung (ma_nguoi_dung, ma_vai_tro)
    SELECT nd.ma_nguoi_dung, vt.ma_vai_tro
    FROM NguoiDung nd JOIN VaiTro vt ON vt.ma_code_vai_tro = nd.vai_tro_chinh
    WHERE nd.email IN ('superadmin@aet.local', 'hdqlnd@aet.local')
      AND NOT EXISTS (SELECT 1 FROM PhanQuyenNguoiDung pq WHERE pq.ma_nguoi_dung = nd.ma_nguoi_dung);

    -- ==========================================
    -- Lặp qua từng cơ sở (cap_don_vi = 'co_so')
    -- ==========================================
    DECLARE @CampusId INT;
    DECLARE @CampusTen NVARCHAR(255);
    DECLARE @CampusIdx INT = 0; -- dùng để tạo email unique
    
    DECLARE curCS CURSOR LOCAL FOR
        SELECT ma_don_vi, ten_don_vi FROM DonVi WHERE cap_don_vi = 'co_so' ORDER BY ma_don_vi;
    OPEN curCS;
    FETCH NEXT FROM curCS INTO @CampusId, @CampusTen;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @CampusIdx += 1;
        PRINT N'--- Dang xu ly: ' + @CampusTen + N' (ID=' + CAST(@CampusId AS NVARCHAR) + N') ---';

        -- ==========================================
        -- 1. TẠO 1000 GIẢNG VIÊN (Teacher)
        -- ==========================================
        PRINT N'  - Tao 1000 Giang vien...';
        INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, mat_khau_hash, vai_tro_chinh, trang_thai, dang_nhap_lan_dau, ngay_tao)
        SELECT TOP 1000
            'gv' + CAST(N AS NVARCHAR) + '.cs' + CAST(@CampusIdx AS NVARCHAR) + '@aet.local',
            N'Giảng viên ' + CAST(N AS NVARCHAR),
            @CampusId,
            @DefaultPasswordHash,
            'giao_vien',
            'hoat_dong',
            0,
            @CurrentDate
        FROM (SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N
              FROM master.dbo.spt_values a CROSS JOIN master.dbo.spt_values b) AS Nums
        WHERE NOT EXISTS (
            SELECT 1 FROM NguoiDung x 
            WHERE x.email = 'gv' + CAST(N AS NVARCHAR) + '.cs' + CAST(@CampusIdx AS NVARCHAR) + '@aet.local'
        );

        -- ==========================================
        -- 2. TẠO LỚP HÀNH CHÍNH (50 Lớp chia cho 3 CTĐT)
        -- ==========================================
        PRINT N'  - Tao 50 Lop Hanh chinh (SE, GD, DM)...';
        DECLARE @GVCN TABLE (id INT, row_num INT);
        INSERT INTO @GVCN (id, row_num)
        SELECT TOP 50 ma_nguoi_dung, ROW_NUMBER() OVER(ORDER BY ma_nguoi_dung)
        FROM NguoiDung WHERE ma_don_vi = @CampusId AND vai_tro_chinh = 'giao_vien' ORDER BY NEWID();

        DECLARE @CtdtSE_Id INT, @CtdtGD_Id INT, @CtdtDM_Id INT;
        SELECT @CtdtSE_Id = ma_chuong_trinh FROM ChuongTrinhDaoTao WHERE ma_code_chuong_trinh = 'CTDT_SE_K20';
        SELECT @CtdtGD_Id = ma_chuong_trinh FROM ChuongTrinhDaoTao WHERE ma_code_chuong_trinh = 'CTDT_GD_K20';
        SELECT @CtdtDM_Id = ma_chuong_trinh FROM ChuongTrinhDaoTao WHERE ma_code_chuong_trinh = 'CTDT_DM_K20';

        INSERT INTO LopHanhChinh (ma_code_lop, ten_lop, ma_don_vi, ma_chuong_trinh, ma_giao_vien_chu_nhiem, nam_nhap_hoc, si_so_du_kien, con_hoat_dong)
        SELECT
            CASE 
                WHEN Nums.N <= 25 THEN 'SE' + CAST(@CampusIdx AS NVARCHAR) + RIGHT('00' + CAST(Nums.N AS NVARCHAR), 2)
                WHEN Nums.N <= 40 THEN 'GD' + CAST(@CampusIdx AS NVARCHAR) + RIGHT('00' + CAST(Nums.N - 25 AS NVARCHAR), 2)
                ELSE 'DM' + CAST(@CampusIdx AS NVARCHAR) + RIGHT('00' + CAST(Nums.N - 40 AS NVARCHAR), 2)
            END,
            CASE 
                WHEN Nums.N <= 25 THEN N'Lớp SE' + RIGHT('00' + CAST(Nums.N AS NVARCHAR), 2) + N' - ' + @CampusTen
                WHEN Nums.N <= 40 THEN N'Lớp GD' + RIGHT('00' + CAST(Nums.N - 25 AS NVARCHAR), 2) + N' - ' + @CampusTen
                ELSE N'Lớp DM' + RIGHT('00' + CAST(Nums.N - 40 AS NVARCHAR), 2) + N' - ' + @CampusTen
            END,
            @CampusId,
            CASE 
                WHEN Nums.N <= 25 THEN ISNULL(@CtdtSE_Id, (SELECT TOP 1 ma_chuong_trinh FROM ChuongTrinhDaoTao))
                WHEN Nums.N <= 40 THEN ISNULL(@CtdtGD_Id, (SELECT TOP 1 ma_chuong_trinh FROM ChuongTrinhDaoTao))
                ELSE ISNULL(@CtdtDM_Id, (SELECT TOP 1 ma_chuong_trinh FROM ChuongTrinhDaoTao))
            END,
            g.id,
            2024,
            40,
            1
        FROM (SELECT TOP 50 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values) AS Nums
        JOIN @GVCN g ON Nums.N = g.row_num
        WHERE NOT EXISTS (
            SELECT 1 FROM LopHanhChinh x 
            WHERE x.ma_don_vi = @CampusId
              AND x.ma_code_lop = CASE 
                    WHEN Nums.N <= 25 THEN 'SE' + CAST(@CampusIdx AS NVARCHAR) + RIGHT('00' + CAST(Nums.N AS NVARCHAR), 2)
                    WHEN Nums.N <= 40 THEN 'GD' + CAST(@CampusIdx AS NVARCHAR) + RIGHT('00' + CAST(Nums.N - 25 AS NVARCHAR), 2)
                    ELSE 'DM' + CAST(@CampusIdx AS NVARCHAR) + RIGHT('00' + CAST(Nums.N - 40 AS NVARCHAR), 2)
                END
        );

        DELETE FROM @GVCN;

        -- ==========================================
        -- 3. TẠO 2000 SINH VIÊN (Student)
        -- ==========================================
        PRINT N'  - Tao 2000 Sinh vien...';
        DECLARE @Classes TABLE (id INT, row_num INT);
        INSERT INTO @Classes (id, row_num)
        SELECT ma_lop, ROW_NUMBER() OVER(ORDER BY ma_lop) FROM LopHanhChinh WHERE ma_don_vi = @CampusId;

        INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, ma_lop, mat_khau_hash, vai_tro_chinh, nam_nhap_hoc, trang_thai, dang_nhap_lan_dau, ngay_tao)
        SELECT TOP 2000
            'sv' + CAST(N AS NVARCHAR) + '.cs' + CAST(@CampusIdx AS NVARCHAR) + '@aet.local',
            N'Sinh viên ' + CAST(N AS NVARCHAR),
            @CampusId,
            c.id,
            @DefaultPasswordHash,
            'hoc_sinh',
            2024,
            'hoat_dong',
            0,
            @CurrentDate
        FROM (SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N
              FROM master.dbo.spt_values a CROSS JOIN master.dbo.spt_values b) AS Nums
        JOIN @Classes c ON c.row_num = ((Nums.N - 1) % 50) + 1
        WHERE NOT EXISTS (
            SELECT 1 FROM NguoiDung x 
            WHERE x.email = 'sv' + CAST(N AS NVARCHAR) + '.cs' + CAST(@CampusIdx AS NVARCHAR) + '@aet.local'
        );

        DELETE FROM @Classes;

        -- ==========================================
        -- 4. TẠO CÁC VAI TRÒ KHÁC (GiaoVu, Admin, BGH)
        -- ==========================================
        PRINT N'  - Tao Giao vu & Admin...';

        -- 5 Giáo vụ
        INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, mat_khau_hash, vai_tro_chinh, trang_thai, dang_nhap_lan_dau, ngay_tao)
        SELECT TOP 5
            'giaovu' + CAST(N AS NVARCHAR) + '.cs' + CAST(@CampusIdx AS NVARCHAR) + '@aet.local',
            N'Giáo vụ ' + CAST(N AS NVARCHAR) + N' - ' + @CampusTen,
            @CampusId, @DefaultPasswordHash, 'nhan_vien', 'hoat_dong', 0, @CurrentDate
        FROM (SELECT TOP 5 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values) AS Nums
        WHERE NOT EXISTS (
            SELECT 1 FROM NguoiDung x 
            WHERE x.email = 'giaovu' + CAST(N AS NVARCHAR) + '.cs' + CAST(@CampusIdx AS NVARCHAR) + '@aet.local'
        );

        -- 1 Admin Cơ sở
        IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE email = 'admin.cs' + CAST(@CampusIdx AS NVARCHAR) + '@aet.local')
            INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, mat_khau_hash, vai_tro_chinh, trang_thai, dang_nhap_lan_dau, ngay_tao)
            VALUES (
                'admin.cs' + CAST(@CampusIdx AS NVARCHAR) + '@aet.local',
                N'Admin - ' + @CampusTen,
                @CampusId, @DefaultPasswordHash, 'quan_tri_co_so', 'hoat_dong', 0, @CurrentDate
            );

        -- 1 BGH (Hiệu trưởng)
        IF NOT EXISTS (SELECT 1 FROM NguoiDung WHERE email = 'hieupho.cs' + CAST(@CampusIdx AS NVARCHAR) + '@aet.local')
            INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, mat_khau_hash, vai_tro_chinh, trang_thai, dang_nhap_lan_dau, ngay_tao)
            VALUES (
                'hieupho.cs' + CAST(@CampusIdx AS NVARCHAR) + '@aet.local',
                N'Hiệu trưởng - ' + @CampusTen,
                @CampusId, @DefaultPasswordHash, 'hieu_truong', 'hoat_dong', 0, @CurrentDate
            );

        -- ==========================================
        -- 5. TẠO PHỤ HUYNH & Liên kết
        -- ==========================================
        PRINT N'  - Tao Phu huynh & lien ket...';
        DECLARE @Students TABLE (id INT, row_num INT);
        INSERT INTO @Students (id, row_num)
        SELECT TOP 1000 ma_nguoi_dung, ROW_NUMBER() OVER(ORDER BY ma_nguoi_dung)
        FROM NguoiDung WHERE ma_don_vi = @CampusId AND vai_tro_chinh = 'hoc_sinh';

        INSERT INTO NguoiDung (email, ho_ten, ma_don_vi, mat_khau_hash, vai_tro_chinh, trang_thai, dang_nhap_lan_dau, ngay_tao)
        SELECT TOP 1000
            'ph' + CAST(N AS NVARCHAR) + '.cs' + CAST(@CampusIdx AS NVARCHAR) + '@aet.local',
            N'Phụ huynh ' + CAST(N AS NVARCHAR),
            @CampusId, @DefaultPasswordHash, 'phu_huynh', 'hoat_dong', 0, @CurrentDate
        FROM (SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N
              FROM master.dbo.spt_values a CROSS JOIN master.dbo.spt_values b) AS Nums
        WHERE NOT EXISTS (
            SELECT 1 FROM NguoiDung x 
            WHERE x.email = 'ph' + CAST(N AS NVARCHAR) + '.cs' + CAST(@CampusIdx AS NVARCHAR) + '@aet.local'
        );

        DECLARE @Parents TABLE (id INT, row_num INT);
        INSERT INTO @Parents (id, row_num)
        SELECT TOP 1000 ma_nguoi_dung, ROW_NUMBER() OVER(ORDER BY ma_nguoi_dung)
        FROM NguoiDung WHERE ma_don_vi = @CampusId AND vai_tro_chinh = 'phu_huynh'
        ORDER BY ma_nguoi_dung DESC;

        INSERT INTO LienKetPhuHuynh (ma_phu_huynh, ma_hoc_sinh, quyen_xem, trang_thai)
        SELECT p.id, s.id, '["Diem", "DiemDanh", "HocPhi"]', 'hoat_dong'
        FROM @Parents p
        JOIN @Students s ON p.row_num = s.row_num;

        DELETE FROM @Students;
        DELETE FROM @Parents;

        -- ==========================================
        -- 6. GÁN VAI TRÒ VÀO PhanQuyenNguoiDung
        -- ==========================================
        PRINT N'  - Gan vai tro PhanQuyenNguoiDung...';
        INSERT INTO PhanQuyenNguoiDung (ma_nguoi_dung, ma_vai_tro)
        SELECT nd.ma_nguoi_dung, vt.ma_vai_tro
        FROM NguoiDung nd
        JOIN VaiTro vt ON vt.ma_code_vai_tro = nd.vai_tro_chinh
        WHERE nd.ma_don_vi = @CampusId
          AND NOT EXISTS (
              SELECT 1 FROM PhanQuyenNguoiDung pq
              WHERE pq.ma_nguoi_dung = nd.ma_nguoi_dung AND pq.ma_vai_tro = vt.ma_vai_tro
          );

        FETCH NEXT FROM curCS INTO @CampusId, @CampusTen;
    END

    CLOSE curCS;
    DEALLOCATE curCS;

    COMMIT TRANSACTION;
    PRINT N'--- HOÀN THÀNH PHASE 2 THÀNH CÔNG ---';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'!!! LỖI TRONG PHASE 2 !!!';
    PRINT ERROR_MESSAGE();
END CATCH
GO
