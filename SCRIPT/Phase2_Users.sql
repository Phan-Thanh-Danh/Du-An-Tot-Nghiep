USE LMS;
GO

SET NOCOUNT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();
DECLARE @DefaultPasswordHash NVARCHAR(MAX) = 'PBKDF2.100000.E08Uerno/mWBsiCCQpVZuQ==.H6STe78bQZXHynCV5JXHIBve2jSLcXJt1/INXAUWF/4='; -- Hash cho '123456'

PRINT N'--- BẮT ĐẦU PHASE 2: TẠO TÀI KHOẢN (MẬT KHẨU: 123456) ---';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==========================================
    -- 0. SEED BẢNG VAI TRÒ (VaiTro)
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

    PRINT N'  [OK] VaiTro da seed';

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
    
    DECLARE curCS CURSOR FOR
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
        -- 2. TẠO LỚP HÀNH CHÍNH (50 Lớp)
        -- ==========================================
        PRINT N'  - Tao 50 Lop Hanh chinh...';
        DECLARE @GVCN TABLE (id INT, row_num INT);
        INSERT INTO @GVCN (id, row_num)
        SELECT TOP 50 ma_nguoi_dung, ROW_NUMBER() OVER(ORDER BY ma_nguoi_dung)
        FROM NguoiDung WHERE ma_don_vi = @CampusId AND vai_tro_chinh = 'giao_vien' ORDER BY NEWID();

        DECLARE @ChuongTrinhId INT;
        SELECT TOP 1 @ChuongTrinhId = ma_chuong_trinh FROM ChuongTrinhDaoTao;

        INSERT INTO LopHanhChinh (ma_code_lop, ten_lop, ma_don_vi, ma_chuong_trinh, ma_giao_vien_chu_nhiem, nam_nhap_hoc, si_so_du_kien, con_hoat_dong)
        SELECT
            'SE' + CAST(@CampusIdx AS NVARCHAR) + RIGHT('00' + CAST(N AS NVARCHAR), 2),
            N'Lớp ' + CAST(N AS NVARCHAR) + N' - ' + @CampusTen,
            @CampusId,
            @ChuongTrinhId,
            g.id,
            2024,
            35,
            1
        FROM (SELECT TOP 50 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values) AS Nums
        JOIN @GVCN g ON Nums.N = g.row_num
        WHERE NOT EXISTS (
            SELECT 1 FROM LopHanhChinh x 
            WHERE x.ma_code_lop = 'SE' + CAST(@CampusIdx AS NVARCHAR) + RIGHT('00' + CAST(N AS NVARCHAR), 2)
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
