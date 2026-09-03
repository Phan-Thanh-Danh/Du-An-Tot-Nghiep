USE [LMS];
GO

SET NOCOUNT ON;

DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();

PRINT N'======================================================================';
PRINT N'--- BẮT ĐẦU SEED BỔ SUNG ĐẦY ĐỦ CƠ SỞ DỮ LIỆU: MÔN HỌC, KHUNG CTĐT & HỌC LIỆU ---';
PRINT N'======================================================================';

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==================================================================
    -- 1. ĐẢM BẢO CHUYÊN NGÀNH & NGÀNH ĐÀO TẠO ĐẦY ĐỦ
    -- ==================================================================
    PRINT N'1. Kiểm tra và đồng bộ Ngành, Chuyên ngành...';

    DECLARE @NganhCNTT INT, @NganhTKDH INT, @NganhMKT INT;
    SELECT @NganhCNTT = ma_nganh FROM NganhDaoTao WHERE ma_code_nganh = 'CNTT';
    SELECT @NganhTKDH = ma_nganh FROM NganhDaoTao WHERE ma_code_nganh = 'TKDH';
    SELECT @NganhMKT  = ma_nganh FROM NganhDaoTao WHERE ma_code_nganh = 'MKT';

    -- Đảm bảo có chuyên ngành chuẩn
    IF NOT EXISTS (SELECT 1 FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Kỹ thuật phần mềm')
        INSERT INTO ChuyenNganh (ma_nganh, ten_chuyen_nganh, con_hoat_dong, ngay_tao) VALUES (@NganhCNTT, N'Kỹ thuật phần mềm', 1, @CurrentDate);

    IF NOT EXISTS (SELECT 1 FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Thiết kế đồ họa')
        INSERT INTO ChuyenNganh (ma_nganh, ten_chuyen_nganh, con_hoat_dong, ngay_tao) VALUES (@NganhTKDH, N'Thiết kế đồ họa', 1, @CurrentDate);

    IF NOT EXISTS (SELECT 1 FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Digital Marketing')
        INSERT INTO ChuyenNganh (ma_nganh, ten_chuyen_nganh, con_hoat_dong, ngay_tao) VALUES (@NganhMKT, N'Digital Marketing', 1, @CurrentDate);

    DECLARE @ChuyenNganhSE INT, @ChuyenNganhGD INT, @ChuyenNganhDM INT;
    SELECT TOP 1 @ChuyenNganhSE = ma_chuyen_nganh FROM ChuyenNganh WHERE ten_chuyen_nganh IN (N'Kỹ thuật phần mềm', N'Phát triển phần mềm') ORDER BY ma_chuyen_nganh;
    SELECT TOP 1 @ChuyenNganhGD = ma_chuyen_nganh FROM ChuyenNganh WHERE ten_chuyen_nganh IN (N'Thiết kế đồ họa', N'Thiết kế UI/UX', N'Đồ họa truyền thông') ORDER BY ma_chuyen_nganh;
    SELECT TOP 1 @ChuyenNganhDM = ma_chuyen_nganh FROM ChuyenNganh WHERE ten_chuyen_nganh = N'Digital Marketing';

    -- Đảm bảo ChuyenNganhTheoCoSo có liên kết
    INSERT INTO ChuyenNganhTheoCoSo (ma_don_vi, ma_chuyen_nganh, con_hoat_dong, trang_thai)
    SELECT d.ma_don_vi, c.ma_chuyen_nganh, 1, 'active'
    FROM DonVi d CROSS JOIN ChuyenNganh c
    WHERE d.cap_don_vi = 'co_so'
      AND NOT EXISTS (
          SELECT 1 FROM ChuyenNganhTheoCoSo cs WHERE cs.ma_don_vi = d.ma_don_vi AND cs.ma_chuyen_nganh = c.ma_chuyen_nganh
      );

    -- ==================================================================
    -- 2. ĐẢM BẢO QUY ĐỔI TÍN CHỈ
    -- ==================================================================
    PRINT N'2. Kiểm tra bảng Quy đổi tín chỉ (QuyDoiTinChi)...';
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 2)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (2, 1, 2, 1);
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 3)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (3, 1, 3, 1);
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 4)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (4, 2, 2, 1);
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 5)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (5, 2, 3, 1);
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 9)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (9, 2, 4, 1);
    IF NOT EXISTS (SELECT 1 FROM QuyDoiTinChi WHERE so_tin_chi = 12)
        INSERT INTO QuyDoiTinChi (so_tin_chi, so_block_hoc, so_buoi_moi_tuan, so_ca_moi_buoi) VALUES (12, 2, 4, 1);

    -- ==================================================================
    -- 3. NẠP TOÀN BỘ 87 MÔN HỌC CHUẨN HÓA (DanhMucMonHoc)
    -- ==================================================================
    PRINT N'3. Nạp và đồng bộ toàn bộ Danh mục môn học chuẩn hóa...';

    DECLARE @StandardSubjects TABLE (
        code NVARCHAR(50), 
        ten NVARCHAR(255), 
        tc INT, 
        nganh INT, 
        chuyenNganh INT
    );

    INSERT INTO @StandardSubjects VALUES
        -- Môn đại cương & Kỹ năng chung
        ('ENG101', N'Tiếng Anh căn bản 1', 3, NULL, NULL),
        ('ENG102', N'Tiếng Anh chuyên ngành 1', 3, NULL, NULL),
        ('ENG103', N'Tiếng Anh chuyên ngành 2', 3, NULL, NULL),
        ('SSG101', N'Kỹ năng học tập & Làm việc nhóm', 3, NULL, NULL),
        ('ENT101', N'Khởi nghiệp đổi mới sáng tạo', 3, NULL, NULL),
        ('ETH301', N'Đạo đức nghề nghiệp & Pháp luật', 3, NULL, NULL),

        -- Khối Kỹ thuật phần mềm (SE) & CNTT
        ('COM101', N'Nhập môn lập trình', 3, @NganhCNTT, @ChuyenNganhSE),
        ('MAT101', N'Toán rời rạc & Đại số tuyến tính', 3, @NganhCNTT, @ChuyenNganhSE),
        ('PRF192', N'Kỹ thuật lập trình C/C++', 3, @NganhCNTT, @ChuyenNganhSE),
        ('CEA201', N'Kiến trúc máy tính & Hệ điều hành', 3, @NganhCNTT, @ChuyenNganhSE),
        ('DBI202', N'Hệ quản trị CSDL & SQL Server', 3, @NganhCNTT, @ChuyenNganhSE),
        ('WEB104', N'Thiết kế trang web (HTML5/CSS3/JS)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('PRO192', N'Lập trình hướng đối tượng với Java', 3, @NganhCNTT, @ChuyenNganhSE),
        ('CSD201', N'Cấu trúc dữ liệu & Giải thuật', 3, @NganhCNTT, @ChuyenNganhSE),
        ('NWC203', N'Mạng máy tính căn bản', 3, @NganhCNTT, @ChuyenNganhSE),
        ('WED201', N'Lập trình Frontend nâng cao (Vue.js/React)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('PRN211', N'Lập trình ứng dụng với C# .NET', 3, @NganhCNTT, @ChuyenNganhSE),
        ('SWP391', N'Dự án phần mềm thực chiến 1', 3, @NganhCNTT, @ChuyenNganhSE),
        ('MAS291', N'Xác suất thống kê cho CNTT', 3, @NganhCNTT, @ChuyenNganhSE),
        ('SWE201', N'Nhập môn Kỹ thuật phần mềm', 3, @NganhCNTT, @ChuyenNganhSE),
        ('PRN231', N'Xây dựng RESTful API với ASP.NET Core', 3, @NganhCNTT, @ChuyenNganhSE),
        ('SWT301', N'Kiểm thử phần mềm & Đảm bảo chất lượng (QA/QC)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('PRM392', N'Lập trình ứng dụng di động (Mobile App)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('SWR302', N'Phân tích thiết kế hệ thống & Yêu cầu phần mềm', 3, @NganhCNTT, @ChuyenNganhSE),
        ('PMG201', N'Quản lý dự án CNTT (Agile / Scrum)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('IOT102', N'Nhập môn Internet vạn vật (IoT)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('WDU301', N'Điện toán đám mây & DevOps (Docker, CI/CD)', 3, @NganhCNTT, @ChuyenNganhSE),
        ('AIL302', N'Trí tuệ nhân tạo & Machine Learning căn bản', 3, @NganhCNTT, @ChuyenNganhSE),
        ('SWP490', N'Dự án phần mềm thực chiến 2', 3, @NganhCNTT, @ChuyenNganhSE),
        ('SEC301', N'An toàn và Bảo mật thông tin ứng dụng', 3, @NganhCNTT, @ChuyenNganhSE),
        ('DBS301', N'Cơ sở dữ liệu NoSQL & Big Data', 3, @NganhCNTT, @ChuyenNganhSE),
        ('OJT401', N'Thực tập tốt nghiệp doanh nghiệp CNTT (OJT)', 9, @NganhCNTT, @ChuyenNganhSE),
        ('SEM401', N'Hội thảo chuyên đề công nghệ mới', 3, @NganhCNTT, @ChuyenNganhSE),
        ('CAP499', N'Đồ án tốt nghiệp Kỹ thuật phần mềm', 12, @NganhCNTT, @ChuyenNganhSE),
        ('SRE401', N'Vận hành hệ thống phần mềm & SRE', 3, @NganhCNTT, @ChuyenNganhSE),

        -- Khối Thiết kế Đồ họa (GD)
        ('UIX101', N'Thiết kế UI/UX căn bản', 3, @NganhTKDH, @ChuyenNganhGD),
        ('ART101', N'Mỹ thuật căn bản & Hình họa', 3, @NganhTKDH, @ChuyenNganhGD),
        ('COL101', N'Lý thuyết màu sắc & Bố cục thị giác', 3, @NganhTKDH, @ChuyenNganhGD),
        ('PSH101', N'Xử lý ảnh kỹ thuật số (Photoshop)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('ILL101', N'Thiết kế đồ họa vector (Illustrator)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('TYP101', N'Nghệ thuật chữ trong thiết kế (Typography)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('IND101', N'Dàn trang & Xuất bản điện tử (InDesign)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('FDM101', N'Nhiếp ảnh & Xử lý ánh sáng studio', 3, @NganhTKDH, @ChuyenNganhGD),
        ('BRD201', N'Thiết kế hệ thống nhận diện thương hiệu (Branding)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('PKG201', N'Thiết kế bao bì & Nhãn hiệu sản phẩm', 3, @NganhTKDH, @ChuyenNganhGD),
        ('AFX201', N'Đồ họa chuyển động 2D (After Effects)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('UIX201', N'Thiết kế trải nghiệm người dùng nâng cao (Figma)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('PRD201', N'Kỹ thuật in ấn và vật liệu mỹ thuật', 3, @NganhTKDH, @ChuyenNganhGD),
        ('MAX201', N'Đồ họa không gian 3D căn bản (Blender)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('VFX201', N'Kỹ xảo hình ảnh & Dựng video (Premiere Pro)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('GDP391', N'Đồ án thiết kế đồ họa thực chiến 1', 3, @NganhTKDH, @ChuyenNganhGD),
        ('ILL202', N'Minh họa kỹ thuật số (Digital Painting)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('ADR201', N'Nghệ thuật chỉ đạo hình ảnh (Art Direction)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('MAX301', N'Diễn hoạt & Tạo hình nhân vật 3D nâng cao', 3, @NganhTKDH, @ChuyenNganhGD),
        ('UIX301', N'Thiết kế Design System cho ứng dụng đa nền tảng', 3, @NganhTKDH, @ChuyenNganhGD),
        ('GDP491', N'Đồ án thiết kế đồ họa thực chiến 2', 3, @NganhTKDH, @ChuyenNganhGD),
        ('GMD201', N'Thiết kế mỹ thuật game (Game Art UI/Asset)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('CPY201', N'Sáng tạo nội dung thị giác (Visual Copywriting)', 3, @NganhTKDH, @ChuyenNganhGD),
        ('OJT402', N'Thực tập tốt nghiệp doanh nghiệp thiết kế (OJT)', 9, @NganhTKDH, @ChuyenNganhGD),
        ('POR401', N'Xây dựng Portfolio chuyên nghiệp & Personal Branding', 3, @NganhTKDH, @ChuyenNganhGD),
        ('CAP498', N'Đồ án tốt nghiệp Thiết kế đồ họa', 12, @NganhTKDH, @ChuyenNganhGD),
        ('EXH401', N'Tổ chức triển lãm & Trình bày đồ án', 3, @NganhTKDH, @ChuyenNganhGD),

        -- Khối Digital Marketing (DM)
        ('MKT101', N'Marketing căn bản', 3, @NganhMKT, @ChuyenNganhDM),
        ('ECO101', N'Kinh tế vi mô & Hành vi người tiêu dùng', 3, @NganhMKT, @ChuyenNganhDM),
        ('ICT101', N'Công nghệ thông tin ứng dụng trong kinh doanh', 3, @NganhMKT, @ChuyenNganhDM),
        ('STA101', N'Thống kê kinh doanh căn bản', 3, @NganhMKT, @ChuyenNganhDM),
        ('MKT201', N'Nhập môn Digital Marketing & E-Commerce', 3, @NganhMKT, @ChuyenNganhDM),
        ('CPY101', N'Sáng tạo nội dung tiếp thị (Content Marketing)', 3, @NganhMKT, @ChuyenNganhDM),
        ('SEO101', N'Tối ưu hóa công cụ tìm kiếm (SEO căn bản)', 3, @NganhMKT, @ChuyenNganhDM),
        ('SEM201', N'Quảng cáo tìm kiếm Google Ads (SEM)', 3, @NganhMKT, @ChuyenNganhDM),
        ('SMM201', N'Tiếp thị trên mạng xã hội (Social Media Marketing)', 3, @NganhMKT, @ChuyenNganhDM),
        ('EMA201', N'Tiếp thị tự động qua Email (Email Marketing Automation)', 3, @NganhMKT, @ChuyenNganhDM),
        ('VID201', N'Sản xuất video ngắn truyền thông (Short-form Video)', 3, @NganhMKT, @ChuyenNganhDM),
        ('MRK202', N'Nghiên cứu thị trường & Phân tích đối thủ', 3, @NganhMKT, @ChuyenNganhDM),
        ('MKA301', N'Phân tích dữ liệu tiếp thị (Google Analytics 4 / PowerBI)', 3, @NganhMKT, @ChuyenNganhDM),
        ('PPC301', N'Quảng cáo đa kênh trả phí (Meta Ads / TikTok Ads)', 3, @NganhMKT, @ChuyenNganhDM),
        ('DMP391', N'Dự án Digital Marketing thực chiến 1', 3, @NganhMKT, @ChuyenNganhDM),
        ('ECOM301', N'Quản trị vận hành sàn TMĐT (Shopee, TikTok Shop)', 3, @NganhMKT, @ChuyenNganhDM),
        ('CRM201', N'Quản trị quan hệ khách hàng (CRM & Chăm sóc số)', 3, @NganhMKT, @ChuyenNganhDM),
        ('PRM201', N'Truyền thông & Quan hệ công chúng trực tuyến (Online PR)', 3, @NganhMKT, @ChuyenNganhDM),
        ('GRH301', N'Tăng trưởng kinh doanh số (Growth Hacking & Phễu chuyển đổi)', 3, @NganhMKT, @ChuyenNganhDM),
        ('AIK301', N'Ứng dụng AI trong sáng tạo & Tối ưu hóa Marketing', 3, @NganhMKT, @ChuyenNganhDM),
        ('DMP491', N'Dự án Digital Marketing thực chiến 2', 3, @NganhMKT, @ChuyenNganhDM),
        ('INF201', N'Quản lý chiến dịch Influencer & KOL/KOC Marketing', 3, @NganhMKT, @ChuyenNganhDM),
        ('LAW201', N'Pháp luật trong thương mại điện tử & Bản quyền số', 3, @NganhMKT, @ChuyenNganhDM),
        ('OJT403', N'Thực tập tốt nghiệp doanh nghiệp Marketing (OJT)', 9, @NganhMKT, @ChuyenNganhDM),
        ('STR401', N'Hoạch định chiến lược Digital Marketing tổng thể', 3, @NganhMKT, @ChuyenNganhDM),
        ('CAP497', N'Đồ án tốt nghiệp Chiến lược Digital Marketing', 12, @NganhMKT, @ChuyenNganhDM),
        ('BDM401', N'Quản trị ngân sách & Đo lường hiệu quả ROI tiếp thị', 3, @NganhMKT, @ChuyenNganhDM);

    -- Nạp vào DanhMucMonHoc nếu chưa tồn tại
    INSERT INTO DanhMucMonHoc (ma_code_mon_hoc, ten_mon_hoc, so_tin_chi, con_hoat_dong, ma_nganh, ma_chuyen_nganh)
    SELECT s.code, s.ten, s.tc, 1, s.nganh, s.chuyenNganh
    FROM @StandardSubjects s
    WHERE NOT EXISTS (SELECT 1 FROM DanhMucMonHoc m WHERE m.ma_code_mon_hoc = s.code);

    -- Cập nhật lại số tín chỉ, chuyên ngành & con_hoat_dong
    UPDATE m
    SET m.ten_mon_hoc = s.ten,
        m.so_tin_chi = s.tc,
        m.ma_nganh = ISNULL(s.nganh, m.ma_nganh),
        m.ma_chuyen_nganh = ISNULL(s.chuyenNganh, m.ma_chuyen_nganh),
        m.con_hoat_dong = 1
    FROM DanhMucMonHoc m
    JOIN @StandardSubjects s ON m.ma_code_mon_hoc = s.code;

    PRINT N'  [OK] Đã nạp thành công toàn bộ môn học chuẩn hóa!';

    -- ==================================================================
    -- 4. LIÊN KẾT MÔN HỌC - CHUYÊN NGÀNH (MonHocChuyenNganh)
    -- ==================================================================
    PRINT N'4. Liên kết môn học vào MonHocChuyenNganh...';

    INSERT INTO MonHocChuyenNganh (ma_mon_hoc, ma_chuyen_nganh)
    SELECT m.ma_mon_hoc, @ChuyenNganhSE
    FROM DanhMucMonHoc m
    WHERE (m.ma_chuyen_nganh = @ChuyenNganhSE OR m.ma_nganh = @NganhCNTT OR m.ma_code_mon_hoc IN ('ENG101', 'ENG102', 'ENG103', 'SSG101', 'ENT101', 'ETH301'))
      AND NOT EXISTS (SELECT 1 FROM MonHocChuyenNganh mc WHERE mc.ma_mon_hoc = m.ma_mon_hoc AND mc.ma_chuyen_nganh = @ChuyenNganhSE);

    INSERT INTO MonHocChuyenNganh (ma_mon_hoc, ma_chuyen_nganh)
    SELECT m.ma_mon_hoc, @ChuyenNganhGD
    FROM DanhMucMonHoc m
    WHERE (m.ma_chuyen_nganh = @ChuyenNganhGD OR m.ma_nganh = @NganhTKDH OR m.ma_code_mon_hoc IN ('ENG101', 'ENG102', 'ENG103', 'SSG101', 'ENT101', 'ETH301', 'WEB104'))
      AND NOT EXISTS (SELECT 1 FROM MonHocChuyenNganh mc WHERE mc.ma_mon_hoc = m.ma_mon_hoc AND mc.ma_chuyen_nganh = @ChuyenNganhGD);

    INSERT INTO MonHocChuyenNganh (ma_mon_hoc, ma_chuyen_nganh)
    SELECT m.ma_mon_hoc, @ChuyenNganhDM
    FROM DanhMucMonHoc m
    WHERE (m.ma_chuyen_nganh = @ChuyenNganhDM OR m.ma_nganh = @NganhMKT OR m.ma_code_mon_hoc IN ('ENG101', 'ENG102', 'ENG103', 'SSG101', 'ENT101', 'ETH301', 'WEB104', 'PSH101'))
      AND NOT EXISTS (SELECT 1 FROM MonHocChuyenNganh mc WHERE mc.ma_mon_hoc = m.ma_mon_hoc AND mc.ma_chuyen_nganh = @ChuyenNganhDM);

    -- ==================================================================
    -- 5. ĐỒNG BỘ CHƯƠNG TRÌNH ĐÀO TẠO & MÔN HỌC (MonHocTrongChuongTrinh)
    -- ==================================================================
    PRINT N'5. Gắn môn học vào khung chương trình đào tạo...';

    -- Khóa tuyển sinh K20 / K2026
    DECLARE @K20 INT;
    SELECT TOP 1 @K20 = ma_khoa_tuyen_sinh FROM KhoaTuyenSinh ORDER BY ma_khoa_tuyen_sinh;

    -- Đảm bảo có 3 CTĐT chuẩn
    DECLARE @CtdtSE INT, @CtdtGD INT, @CtdtDM INT;

    SELECT TOP 1 @CtdtSE = ma_chuong_trinh FROM ChuongTrinhDaoTao WHERE ma_code_chuong_trinh IN ('CTDT_SE_K20', 'CT_CNTT_K2026');
    IF @CtdtSE IS NULL
    BEGIN
        INSERT INTO ChuongTrinhDaoTao (ma_code_chuong_trinh, ten_chuong_trinh, ma_chuyen_nganh, ma_khoa_tuyen_sinh, so_hoc_ky, thoi_gian_dao_tao_thang, tong_tin_chi_yeu_cau, version, trang_thai, con_hoat_dong, ngay_tao)
        VALUES ('CTDT_SE_K20', N'Chương trình Kỹ thuật phần mềm K20', @ChuyenNganhSE, @K20, 7, 28, 120, '2026.1', 'active', 1, @CurrentDate);
        SET @CtdtSE = SCOPE_IDENTITY();
    END

    SELECT TOP 1 @CtdtGD = ma_chuong_trinh FROM ChuongTrinhDaoTao WHERE ma_code_chuong_trinh IN ('CTDT_GD_K20', 'CT_TKDH_K2026');
    IF @CtdtGD IS NULL
    BEGIN
        INSERT INTO ChuongTrinhDaoTao (ma_code_chuong_trinh, ten_chuong_trinh, ma_chuyen_nganh, ma_khoa_tuyen_sinh, so_hoc_ky, thoi_gian_dao_tao_thang, tong_tin_chi_yeu_cau, version, trang_thai, con_hoat_dong, ngay_tao)
        VALUES ('CTDT_GD_K20', N'Chương trình Thiết kế đồ họa K20', @ChuyenNganhGD, @K20, 7, 28, 120, '2026.1', 'active', 1, @CurrentDate);
        SET @CtdtGD = SCOPE_IDENTITY();
    END

    SELECT TOP 1 @CtdtDM = ma_chuong_trinh FROM ChuongTrinhDaoTao WHERE ma_code_chuong_trinh IN ('CTDT_DM_K20', 'CT_MKT_K2026');
    IF @CtdtDM IS NULL
    BEGIN
        INSERT INTO ChuongTrinhDaoTao (ma_code_chuong_trinh, ten_chuong_trinh, ma_chuyen_nganh, ma_khoa_tuyen_sinh, so_hoc_ky, thoi_gian_dao_tao_thang, tong_tin_chi_yeu_cau, version, trang_thai, con_hoat_dong, ngay_tao)
        VALUES ('CTDT_DM_K20', N'Chương trình Digital Marketing K20', @ChuyenNganhDM, @K20, 7, 28, 120, '2026.1', 'active', 1, @CurrentDate);
        SET @CtdtDM = SCOPE_IDENTITY();
    END

    DECLARE @ProgramCurriculum TABLE (
        ctdtId INT,
        monCode NVARCHAR(50),
        hocKy INT,
        loai NVARCHAR(30),
        thuTu INT
    );

    -- 5.1 SE
    INSERT INTO @ProgramCurriculum VALUES
        (@CtdtSE, 'COM101', 1, 'bat_buoc', 1),
        (@CtdtSE, 'ENG101', 1, 'bat_buoc', 2),
        (@CtdtSE, 'MAT101', 1, 'bat_buoc', 3),
        (@CtdtSE, 'PRF192', 1, 'bat_buoc', 4),
        (@CtdtSE, 'SSG101', 1, 'bat_buoc', 5),
        (@CtdtSE, 'CEA201', 1, 'bat_buoc', 6),
        (@CtdtSE, 'DBI202', 2, 'bat_buoc', 1),
        (@CtdtSE, 'WEB104', 2, 'bat_buoc', 2),
        (@CtdtSE, 'ENG102', 2, 'bat_buoc', 3),
        (@CtdtSE, 'PRO192', 2, 'bat_buoc', 4),
        (@CtdtSE, 'CSD201', 2, 'bat_buoc', 5),
        (@CtdtSE, 'NWC203', 2, 'bat_buoc', 6),
        (@CtdtSE, 'WED201', 3, 'bat_buoc', 1),
        (@CtdtSE, 'PRN211', 3, 'bat_buoc', 2),
        (@CtdtSE, 'SWP391', 3, 'bat_buoc', 3),
        (@CtdtSE, 'MAS291', 3, 'bat_buoc', 4),
        (@CtdtSE, 'SWE201', 3, 'bat_buoc', 5),
        (@CtdtSE, 'ENG103', 3, 'bat_buoc', 6),
        (@CtdtSE, 'PRN231', 4, 'bat_buoc', 1),
        (@CtdtSE, 'SWT301', 4, 'bat_buoc', 2),
        (@CtdtSE, 'PRM392', 4, 'bat_buoc', 3),
        (@CtdtSE, 'SWR302', 4, 'bat_buoc', 4),
        (@CtdtSE, 'PMG201', 4, 'bat_buoc', 5),
        (@CtdtSE, 'IOT102', 4, 'tu_chon',  6),
        (@CtdtSE, 'WDU301', 5, 'bat_buoc', 1),
        (@CtdtSE, 'AIL302', 5, 'bat_buoc', 2),
        (@CtdtSE, 'SWP490', 5, 'bat_buoc', 3),
        (@CtdtSE, 'SEC301', 5, 'bat_buoc', 4),
        (@CtdtSE, 'DBS301', 5, 'tu_chon',  5),
        (@CtdtSE, 'ENT101', 5, 'bat_buoc', 6),
        (@CtdtSE, 'OJT401', 6, 'bat_buoc', 1),
        (@CtdtSE, 'SEM401', 6, 'bat_buoc', 2),
        (@CtdtSE, 'ETH301', 6, 'bat_buoc', 3),
        (@CtdtSE, 'CAP499', 7, 'bat_buoc', 1),
        (@CtdtSE, 'SRE401', 7, 'bat_buoc', 2);

    -- 5.2 GD
    INSERT INTO @ProgramCurriculum VALUES
        (@CtdtGD, 'UIX101', 1, 'bat_buoc', 1),
        (@CtdtGD, 'ART101', 1, 'bat_buoc', 2),
        (@CtdtGD, 'COL101', 1, 'bat_buoc', 3),
        (@CtdtGD, 'PSH101', 1, 'bat_buoc', 4),
        (@CtdtGD, 'ENG101', 1, 'bat_buoc', 5),
        (@CtdtGD, 'SSG101', 1, 'bat_buoc', 6),
        (@CtdtGD, 'ILL101', 2, 'bat_buoc', 1),
        (@CtdtGD, 'TYP101', 2, 'bat_buoc', 2),
        (@CtdtGD, 'IND101', 2, 'bat_buoc', 3),
        (@CtdtGD, 'FDM101', 2, 'bat_buoc', 4),
        (@CtdtGD, 'ENG102', 2, 'bat_buoc', 5),
        (@CtdtGD, 'WEB104', 2, 'bat_buoc', 6),
        (@CtdtGD, 'BRD201', 3, 'bat_buoc', 1),
        (@CtdtGD, 'PKG201', 3, 'bat_buoc', 2),
        (@CtdtGD, 'AFX201', 3, 'bat_buoc', 3),
        (@CtdtGD, 'UIX201', 3, 'bat_buoc', 4),
        (@CtdtGD, 'PRD201', 3, 'bat_buoc', 5),
        (@CtdtGD, 'ENG103', 3, 'bat_buoc', 6),
        (@CtdtGD, 'MAX201', 4, 'bat_buoc', 1),
        (@CtdtGD, 'VFX201', 4, 'bat_buoc', 2),
        (@CtdtGD, 'GDP391', 4, 'bat_buoc', 3),
        (@CtdtGD, 'MKT101', 4, 'bat_buoc', 4),
        (@CtdtGD, 'ILL202', 4, 'tu_chon',  5),
        (@CtdtGD, 'ADR201', 4, 'bat_buoc', 6),
        (@CtdtGD, 'MAX301', 5, 'bat_buoc', 1),
        (@CtdtGD, 'UIX301', 5, 'bat_buoc', 2),
        (@CtdtGD, 'GDP491', 5, 'bat_buoc', 3),
        (@CtdtGD, 'GMD201', 5, 'tu_chon',  4),
        (@CtdtGD, 'CPY201', 5, 'bat_buoc', 5),
        (@CtdtGD, 'ENT101', 5, 'bat_buoc', 6),
        (@CtdtGD, 'OJT402', 6, 'bat_buoc', 1),
        (@CtdtGD, 'POR401', 6, 'bat_buoc', 2),
        (@CtdtGD, 'ETH301', 6, 'bat_buoc', 3),
        (@CtdtGD, 'CAP498', 7, 'bat_buoc', 1),
        (@CtdtGD, 'EXH401', 7, 'bat_buoc', 2);

    -- 5.3 DM
    INSERT INTO @ProgramCurriculum VALUES
        (@CtdtDM, 'MKT101', 1, 'bat_buoc', 1),
        (@CtdtDM, 'ECO101', 1, 'bat_buoc', 2),
        (@CtdtDM, 'ICT101', 1, 'bat_buoc', 3),
        (@CtdtDM, 'STA101', 1, 'bat_buoc', 4),
        (@CtdtDM, 'ENG101', 1, 'bat_buoc', 5),
        (@CtdtDM, 'SSG101', 1, 'bat_buoc', 6),
        (@CtdtDM, 'MKT201', 2, 'bat_buoc', 1),
        (@CtdtDM, 'CPY101', 2, 'bat_buoc', 2),
        (@CtdtDM, 'SEO101', 2, 'bat_buoc', 3),
        (@CtdtDM, 'PSH101', 2, 'bat_buoc', 4),
        (@CtdtDM, 'ENG102', 2, 'bat_buoc', 5),
        (@CtdtDM, 'WEB104', 2, 'bat_buoc', 6),
        (@CtdtDM, 'SEM201', 3, 'bat_buoc', 1),
        (@CtdtDM, 'SMM201', 3, 'bat_buoc', 2),
        (@CtdtDM, 'EMA201', 3, 'bat_buoc', 3),
        (@CtdtDM, 'VID201', 3, 'bat_buoc', 4),
        (@CtdtDM, 'MRK202', 3, 'bat_buoc', 5),
        (@CtdtDM, 'ENG103', 3, 'bat_buoc', 6),
        (@CtdtDM, 'MKA301', 4, 'bat_buoc', 1),
        (@CtdtDM, 'PPC301', 4, 'bat_buoc', 2),
        (@CtdtDM, 'DMP391', 4, 'bat_buoc', 3),
        (@CtdtDM, 'ECOM301', 4, 'bat_buoc', 4),
        (@CtdtDM, 'CRM201', 4, 'bat_buoc', 5),
        (@CtdtDM, 'PRM201', 4, 'tu_chon',  6),
        (@CtdtDM, 'GRH301', 5, 'bat_buoc', 1),
        (@CtdtDM, 'AIK301', 5, 'bat_buoc', 2),
        (@CtdtDM, 'DMP491', 5, 'bat_buoc', 3),
        (@CtdtDM, 'INF201', 5, 'bat_buoc', 4),
        (@CtdtDM, 'LAW201', 5, 'tu_chon',  5),
        (@CtdtDM, 'ENT101', 5, 'bat_buoc', 6),
        (@CtdtDM, 'OJT403', 6, 'bat_buoc', 1),
        (@CtdtDM, 'STR401', 6, 'bat_buoc', 2),
        (@CtdtDM, 'ETH301', 6, 'bat_buoc', 3),
        (@CtdtDM, 'CAP497', 7, 'bat_buoc', 1),
        (@CtdtDM, 'BDM401', 7, 'bat_buoc', 2);

    INSERT INTO MonHocTrongChuongTrinh (ma_chuong_trinh, ma_mon_hoc, hoc_ky_du_kien, so_tin_chi, loai_mon_hoc, bat_buoc, thu_tu, con_hoat_dong, ngay_tao)
    SELECT pc.ctdtId, m.ma_mon_hoc, pc.hocKy, m.so_tin_chi, pc.loai, 
           CASE WHEN pc.loai = 'bat_buoc' THEN 1 ELSE 0 END, pc.thuTu, 1, @CurrentDate
    FROM @ProgramCurriculum pc
    JOIN DanhMucMonHoc m ON m.ma_code_mon_hoc = pc.monCode
    WHERE pc.ctdtId IS NOT NULL
      AND NOT EXISTS (
          SELECT 1 FROM MonHocTrongChuongTrinh tc WHERE tc.ma_chuong_trinh = pc.ctdtId AND tc.ma_mon_hoc = m.ma_mon_hoc
      );

    -- ==================================================================
    -- 6. THIẾT LẬP MÔN HỌC TIÊN QUYẾT (MonHocTienQuyet)
    -- ==================================================================
    PRINT N'6. Thiết lập môn học tiên quyết (MonHocTienQuyet)...';

    DECLARE @Prerequisites TABLE (mon NVARCHAR(50), tienQuyet NVARCHAR(50));
    INSERT INTO @Prerequisites VALUES
        ('PRO192', 'PRF192'),
        ('CSD201', 'PRO192'),
        ('DBI202', 'COM101'),
        ('WED201', 'WEB104'),
        ('PRN211', 'PRO192'),
        ('PRN231', 'PRN211'),
        ('PRN231', 'DBI202'),
        ('SWP391', 'WED201'),
        ('SWP391', 'DBI202'),
        ('SWP490', 'SWP391'),
        ('PRM392', 'PRO192'),
        ('WDU301', 'NWC203'),
        ('CAP499', 'OJT401'),
        ('ILL101', 'ART101'),
        ('PSH101', 'COL101'),
        ('BRD201', 'ILL101'),
        ('UIX201', 'UIX101'),
        ('AFX201', 'PSH101'),
        ('MAX301', 'MAX201'),
        ('GDP491', 'GDP391'),
        ('CAP498', 'OJT402'),
        ('MKT201', 'MKT101'),
        ('SEO101', 'MKT201'),
        ('SEM201', 'SEO101'),
        ('PPC301', 'SEM201'),
        ('MKA301', 'STA101'),
        ('DMP491', 'DMP391'),
        ('CAP497', 'OJT403');

    INSERT INTO MonHocTienQuyet (ma_mon_hoc, ma_mon_tien_quyet)
    SELECT m1.ma_mon_hoc, m2.ma_mon_hoc
    FROM @Prerequisites p
    JOIN DanhMucMonHoc m1 ON m1.ma_code_mon_hoc = p.mon
    JOIN DanhMucMonHoc m2 ON m2.ma_code_mon_hoc = p.tienQuyet
    WHERE NOT EXISTS (
        SELECT 1 FROM MonHocTienQuyet tq WHERE tq.ma_mon_hoc = m1.ma_mon_hoc AND tq.ma_mon_tien_quyet = m2.ma_mon_hoc
    );

    -- ==================================================================
    -- 7. CẤP QUYỀN GIẢNG DẠY CHO GIẢNG VIÊN (GiaoVienMonHoc)
    -- ==================================================================
    PRINT N'7. Cấp quyền giảng dạy cho giảng viên theo chuyên ngành...';

    -- SE
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    SELECT gvcn.ma_giao_vien, m.ma_mon_hoc, 5, 8, 3, 1, 1, @CurrentDate
    FROM GiaoVienChuyenNganh gvcn
    CROSS JOIN DanhMucMonHoc m
    WHERE gvcn.ma_chuyen_nganh = @ChuyenNganhSE
      AND (m.ma_chuyen_nganh = @ChuyenNganhSE OR m.ma_nganh = @NganhCNTT OR m.ma_code_mon_hoc IN ('ENG101', 'ENG102', 'ENG103', 'SSG101', 'ENT101', 'ETH301'))
      AND NOT EXISTS (
          SELECT 1 FROM GiaoVienMonHoc gm WHERE gm.ma_giao_vien = gvcn.ma_giao_vien AND gm.ma_mon_hoc = m.ma_mon_hoc
      );

    -- GD
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    SELECT gvcn.ma_giao_vien, m.ma_mon_hoc, 5, 8, 3, 1, 1, @CurrentDate
    FROM GiaoVienChuyenNganh gvcn
    CROSS JOIN DanhMucMonHoc m
    WHERE gvcn.ma_chuyen_nganh = @ChuyenNganhGD
      AND (m.ma_chuyen_nganh = @ChuyenNganhGD OR m.ma_nganh = @NganhTKDH OR m.ma_code_mon_hoc IN ('ENG101', 'ENG102', 'ENG103', 'SSG101', 'ENT101', 'ETH301'))
      AND NOT EXISTS (
          SELECT 1 FROM GiaoVienMonHoc gm WHERE gm.ma_giao_vien = gvcn.ma_giao_vien AND gm.ma_mon_hoc = m.ma_mon_hoc
      );

    -- DM
    INSERT INTO GiaoVienMonHoc (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    SELECT gvcn.ma_giao_vien, m.ma_mon_hoc, 5, 8, 3, 1, 1, @CurrentDate
    FROM GiaoVienChuyenNganh gvcn
    CROSS JOIN DanhMucMonHoc m
    WHERE gvcn.ma_chuyen_nganh = @ChuyenNganhDM
      AND (m.ma_chuyen_nganh = @ChuyenNganhDM OR m.ma_nganh = @NganhMKT OR m.ma_code_mon_hoc IN ('ENG101', 'ENG102', 'ENG103', 'SSG101', 'ENT101', 'ETH301'))
      AND NOT EXISTS (
          SELECT 1 FROM GiaoVienMonHoc gm WHERE gm.ma_giao_vien = gvcn.ma_giao_vien AND gm.ma_mon_hoc = m.ma_mon_hoc
      );

    -- ==================================================================
    -- 8. NẠP HỌC LIỆU LMS (DeCuongMonHoc, Chuong, BaiHoc, BaiHocNoiDung)
    -- ==================================================================
    PRINT N'8. Khởi tạo Đề cương, Chương, Bài học và Nội dung LMS...';

    DECLARE @LmsSubjects TABLE (Code NVARCHAR(50), Ten NVARCHAR(255));
    INSERT INTO @LmsSubjects VALUES
        ('COM101', N'Nhập môn lập trình'),
        ('DBI202', N'Hệ quản trị CSDL & SQL Server'),
        ('WEB104', N'Thiết kế trang web (HTML5/CSS3/JS)'),
        ('UIX101', N'Thiết kế UI/UX căn bản'),
        ('MKT101', N'Marketing căn bản'),
        ('PRF192', N'Kỹ thuật lập trình C/C++'),
        ('PRO192', N'Lập trình hướng đối tượng với Java'),
        ('CSD201', N'Cấu trúc dữ liệu & Giải thuật'),
        ('WED201', N'Lập trình Frontend nâng cao (Vue.js/React)'),
        ('PRN211', N'Lập trình ứng dụng với C# .NET'),
        ('PRN231', N'Xây dựng RESTful API với ASP.NET Core'),
        ('SWT301', N'Kiểm thử phần mềm & Đảm bảo chất lượng (QA/QC)'),
        ('SWP391', N'Dự án phần mềm thực chiến 1'),
        ('PSH101', N'Xử lý ảnh kỹ thuật số (Photoshop)'),
        ('ILL101', N'Thiết kế đồ họa vector (Illustrator)'),
        ('SEO101', N'Tối ưu hóa công cụ tìm kiếm (SEO căn bản)'),
        ('ENG101', N'Tiếng Anh căn bản 1');

    -- 8.1 DeCuongMonHoc
    INSERT INTO DeCuongMonHoc (ma_mon_hoc, ma_chuyen_nganh, ten_syllabus, version, trang_thai, bat_buoc, con_hoat_dong, ngay_tao)
    SELECT m.ma_mon_hoc, ISNULL(m.ma_chuyen_nganh, @ChuyenNganhSE), N'Đề cương chi tiết môn ' + m.ten_mon_hoc, 'v1.0', 'active', 1, 1, @CurrentDate
    FROM DanhMucMonHoc m
    JOIN @LmsSubjects s ON m.ma_code_mon_hoc = s.Code
    WHERE NOT EXISTS (SELECT 1 FROM DeCuongMonHoc d WHERE d.ma_mon_hoc = m.ma_mon_hoc);

    -- 8.2 Chuong (5 chương mỗi môn)
    WITH Numbers AS (SELECT TOP 5 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values)
    INSERT INTO Chuong (ma_mon_hoc, tieu_de, thu_tu, da_an, ngay_tao)
    SELECT m.ma_mon_hoc, 
           CASE n.N
             WHEN 1 THEN N'Chương 1: Tổng quan và Kiến thức nền tảng'
             WHEN 2 THEN N'Chương 2: Cú pháp và Kỹ thuật cốt lõi'
             WHEN 3 THEN N'Chương 3: Thực hành ứng dụng nâng cao'
             WHEN 4 THEN N'Chương 4: Tối ưu hóa và Xử lý lỗi'
             WHEN 5 THEN N'Chương 5: Dự án tổng hợp và Đánh giá'
           END, 
           n.N, 0, @CurrentDate
    FROM DanhMucMonHoc m
    JOIN @LmsSubjects s ON m.ma_code_mon_hoc = s.Code
    CROSS JOIN Numbers n
    WHERE NOT EXISTS (SELECT 1 FROM Chuong c WHERE c.ma_mon_hoc = m.ma_mon_hoc AND c.thu_tu = n.N);

    -- 8.3 BaiHoc (3 bài mỗi chương)
    WITH Numbers AS (SELECT TOP 3 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS N FROM master.dbo.spt_values)
    INSERT INTO BaiHoc (ma_chuong, tieu_de, loai_bai_hoc, thu_tu, da_an, trang_thai, ngay_tao)
    SELECT c.ma_chuong, 
           N'Bài ' + CAST(c.thu_tu AS NVARCHAR) + N'.' + CAST(n.N AS NVARCHAR) + N': ' +
           CASE n.N
             WHEN 1 THEN N'Lý thuyết trọng tâm'
             WHEN 2 THEN N'Ví dụ minh họa thực tế'
             WHEN 3 THEN N'Thực hành & Bài tập củng cố'
           END, 
           'video', n.N, 0, 'da_xuat_ban', @CurrentDate
    FROM Chuong c
    JOIN DanhMucMonHoc m ON c.ma_mon_hoc = m.ma_mon_hoc
    JOIN @LmsSubjects s ON m.ma_code_mon_hoc = s.Code
    CROSS JOIN Numbers n
    WHERE NOT EXISTS (SELECT 1 FROM BaiHoc b WHERE b.ma_chuong = c.ma_chuong AND b.thu_tu = n.N);

    -- 8.4 BaiHocNoiDung (1 Video + 1 Tài liệu PDF mỗi bài học)
    INSERT INTO BaiHocNoiDung (ma_bai_hoc, loai_noi_dung, thu_tu, thoi_luong_giay, url_tap_tin, trang_thai, ngay_tao)
    SELECT b.ma_bai_hoc, 'video', 1, 1800, 'https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4', 'da_xuat_ban', @CurrentDate
    FROM BaiHoc b
    WHERE NOT EXISTS (SELECT 1 FROM BaiHocNoiDung nd WHERE nd.ma_bai_hoc = b.ma_bai_hoc AND nd.loai_noi_dung = 'video');

    INSERT INTO BaiHocNoiDung (ma_bai_hoc, loai_noi_dung, thu_tu, thoi_luong_giay, url_tap_tin, trang_thai, ngay_tao)
    SELECT b.ma_bai_hoc, 'tai_lieu', 2, 0, 'https://cdn.lms.local/documents/course_materials.pdf', 'da_xuat_ban', @CurrentDate
    FROM BaiHoc b
    WHERE NOT EXISTS (SELECT 1 FROM BaiHocNoiDung nd WHERE nd.ma_bai_hoc = b.ma_bai_hoc AND nd.loai_noi_dung = 'tai_lieu');

    -- ==================================================================
    -- 9. NẠP NGÂN HÀNG CÂU HỎI (CauHoi) PHỤC VỤ TẠO QUIZ (20+ CÂU/MÔN)
    -- ==================================================================
    PRINT N'9. Nạp ngân hàng câu hỏi chuẩn hóa (CauHoi) cho các môn học...';

    DECLARE @AdminUserId INT;
    SELECT TOP 1 @AdminUserId = ma_nguoi_dung FROM NguoiDung WHERE vai_tro_chinh IN ('quan_tri', 'HoiDongQuanLyNoiDung', 'ContentCouncil', 'SuperAdmin') ORDER BY ma_nguoi_dung;
    IF @AdminUserId IS NULL SELECT TOP 1 @AdminUserId = ma_nguoi_dung FROM NguoiDung ORDER BY ma_nguoi_dung;

    -- Bảng tạm câu hỏi mẫu
    DECLARE @SampleQuestions TABLE (
        subCode NVARCHAR(50),
        idx INT,
        noiDung NVARCHAR(500),
        optA NVARCHAR(255),
        optB NVARCHAR(255),
        optC NVARCHAR(255),
        optD NVARCHAR(255),
        dapAn NVARCHAR(10),
        doKho NVARCHAR(20),
        giaiThich NVARCHAR(500)
    );

    -- COM101: Nhập môn lập trình (20 câu)
    INSERT INTO @SampleQuestions VALUES
    ('COM101', 1, N'Biến trong lập trình được sử dụng để làm gì?', N'Lưu trữ dữ liệu trong bộ nhớ', N'Hiển thị kết quả ra màn hình', N'Thực hiện vòng lặp', N'Khai báo thư viện', 'A', 'de', N'Biến là vùng nhớ có tên dùng để lưu trữ dữ liệu tạm thời trong khi chương trình thực thi.'),
    ('COM101', 2, N'Kiểu dữ liệu nào sau đây dùng để lưu số nguyên trong C/C++?', N'int', N'float', N'double', N'char', 'A', 'de', N'int là kiểu dữ liệu số nguyên tiêu chuẩn.'),
    ('COM101', 3, N'Toán tử nào dùng để so sánh bằng trong hầu hết các ngôn ngữ lập trình hiện đại?', N'==', N'=', N'===', N':=', 'A', 'de', N'Toán tử == dùng để so sánh bằng, còn = là toán tử gán.'),
    ('COM101', 4, N'Cấu trúc điều khiển nào được dùng để thực hiện lựa chọn giữa nhiều trường hợp?', N'switch-case', N'while', N'for', N'do-while', 'A', 'de', N'switch-case cho phép rẽ nhánh nhiều trường hợp dựa trên giá trị của biểu thức.'),
    ('COM101', 5, N'Vòng lặp nào luôn thực hiện khối lệnh ít nhất một lần trước khi kiểm tra điều kiện?', N'do-while', N'while', N'for', N'foreach', 'A', 'trung_binh', N'do-while kiểm tra điều kiện ở cuối vòng lặp nên luôn chạy ít nhất 1 lần.'),
    ('COM101', 6, N'Mảng (Array) trong lập trình là gì?', N'Tập hợp các phần tử có cùng kiểu dữ liệu', N'Tập hợp các hàm khác nhau', N'Một kiểu dữ liệu không lưu trữ được gì', N'Một biến đặc biệt chỉ lưu số thực', 'A', 'de', N'Mảng là tập hợp liên tiếp các ô nhớ chứa các phần tử có cùng kiểu dữ liệu.'),
    ('COM101', 7, N'Hàm (Function) có lợi ích lớn nhất là gì?', N'Tái sử dụng mã nguồn và chia nhỏ chương trình', N'Tăng dung lượng tệp thực thi', N'Làm cho code chạy chậm hơn', N'Bắt buộc phải có trong mọi ngôn ngữ', 'A', 'de', N'Hàm giúp module hóa chương trình và tránh lặp code (DRY principle).'),
    ('COM101', 8, N'Tham số hình thức (Formal Parameter) là gì?', N'Tham số được khai báo trong định nghĩa hàm', N'Giá trị thực tế truyền vào hàm khi gọi', N'Một biến toàn cục', N'Hằng số trong chương trình', 'A', 'trung_binh', N'Tham số hình thức nằm ở chữ ký hàm, nhận giá trị từ đối số khi hàm được gọi.'),
    ('COM101', 9, N'Con trỏ (Pointer) trong C dùng để lưu trữ cái gì?', N'Địa chỉ của ô nhớ', N'Giá trị chuỗi ký tự', N'Dung lượng RAM', N'Mã nhị phân của CPU', 'A', 'kho', N'Con trỏ là biến lưu trữ địa chỉ vùng nhớ của một biến khác.'),
    ('COM101', 10, N'Thuật toán là gì?', N'Dãy các bước xác định để giải quyết một bài toán', N'Một phần mềm chạy trên máy tính', N'Một ngôn ngữ lập trình', N'Một linh kiện phần cứng', 'A', 'de', N'Thuật toán là tập hữu hạn các chỉ thị từng bước để đạt được kết quả mong muốn.'),
    ('COM101', 11, N'Độ phức tạp thời gian O(1) nghĩa là gì?', N'Thời gian thực thi không phụ thuộc vào kích thước dữ liệu đầu vào', N'Thời gian chạy mất 1 giây', N'Chương trình chỉ chạy 1 dòng lệnh', N'Thuật toán chạy chậm nhất', 'A', 'trung_binh', N'O(1) là độ phức tạp hằng số, thời gian thực hiện cố định.'),
    ('COM101', 12, N'Ký hiệu nào thường dùng để kết thúc một câu lệnh trong C/C++/Java/C#?', N';', N':', N'.', N'#', 'A', 'de', N'Dấu chấm phẩy ; là dấu kết thúc câu lệnh.'),
    ('COM101', 13, N'Trong biểu thức `a % b`, toán tử `%` thực hiện phép toán nào?', N'Chia lấy phần dư', N'Chia lấy phần nguyên', N'Tính phần trăm', N'Nhân lũy thừa', 'A', 'de', N'% là toán tử modulo, trả về phần dư của phép chia nguyên.'),
    ('COM101', 14, N'Đệ quy (Recursion) là kỹ thuật gì?', N'Hàm tự gọi lại chính nó', N'Hàm gọi hàm khác vô tận', N'Vòng lặp không có điều kiện dừng', N'Khai báo biến trong cấu trúc', 'A', 'kho', N'Hàm đệ quy gọi lại chính nó với bài toán con nhỏ hơn kèm điều kiện cơ sở dừng.'),
    ('COM101', 15, N'Lỗi tràn bộ nhớ đệm (Buffer Overflow) thường xảy ra khi nào?', N'Ghi dữ liệu vượt quá kích thước vùng nhớ đã cấp phát', N'CPU quá nóng', N'Không đóng tệp sau khi đọc', N'Khai báo quá ít biến', 'A', 'kho', N'Buffer overflow xảy ra khi ghi đè ra ngoài biên mảng/vùng nhớ cho phép.'),
    ('COM101', 16, N'Giá trị của biểu thức logic `!(true && false)` là gì?', N'true', N'false', N'null', N'undefined', 'A', 'de', N'true && false là false, phủ định !false là true.'),
    ('COM101', 17, N'Hằng số (Constant) trong lập trình là gì?', N'Giá trị không thể thay đổi sau khi được khởi tạo', N'Biến có thể đổi giá trị liên tục', N'Một loại hàm đặc biệt', N'Tệp cấu hình của hệ thống', 'A', 'de', N'Hằng số (const) giữ nguyên giá trị trong suốt quá trình chạy chương trình.'),
    ('COM101', 18, N'Phạm vi biến (Scope) cục bộ có đặc điểm gì?', N'Chỉ có thể truy cập được bên trong khối lệnh khai báo nó', N'Có thể truy cập ở mọi tệp nguồn', N'Tồn tại mãi sau khi chương trình tắt', N'Tự động đồng bộ lên cơ sở dữ liệu', 'A', 'trung_binh', N'Biến cục bộ (local scope) chỉ tồn tại và nhìn thấy được trong khối/hàm chứa nó.'),
    ('COM101', 19, N'Khi chia số nguyên 7 / 2 trong C, kết quả nhận được là bao nhiêu?', N'3', N'3.5', N'4', N'0', 'A', 'de', N'Phép chia 2 số nguyên trong C cho kết quả là phần nguyên (7/2 = 3).'),
    ('COM101', 20, N'Hệ thống đếm nhị phân sử dụng các chữ số nào?', N'0 và 1', N'0 đến 9', N'0 đến 7', N'0 đến F', 'A', 'de', N'Hệ nhị phân (Binary) chỉ sử dụng 2 ký hiệu 0 và 1.');

    -- DBI202: Hệ quản trị CSDL & SQL Server (20 câu)
    INSERT INTO @SampleQuestions VALUES
    ('DBI202', 1, N'Khóa chính (Primary Key) trong bảng quan hệ có đặc điểm nào?', N'Duy nhất và không được chứa giá trị NULL', N'Có thể trùng lặp giá trị', N'Có thể chứa nhiều giá trị NULL', N'Chỉ áp dụng cho cột chuỗi ký tự', 'A', 'de', N'Primary Key định danh duy nhất mỗi hàng và NOT NULL.'),
    ('DBI202', 2, N'Khóa ngoại (Foreign Key) dùng để làm gì?', N'Thiết lập mối quan hệ và toàn vẹn tham chiếu giữa 2 bảng', N'Tăng tốc độ hiển thị hình ảnh', N'Mã hóa mật khẩu người dùng', N'Tự động tăng giá trị số', 'A', 'de', N'Khóa ngoại tham chiếu đến khóa chính bảng khác để đảm bảo toàn vẹn dữ liệu.'),
    ('DBI202', 3, N'Lệnh SQL nào dùng để trích xuất dữ liệu từ một hoặc nhiều bảng?', N'SELECT', N'INSERT', N'UPDATE', N'DROP', 'A', 'de', N'SELECT là câu lệnh DQL cốt lõi để truy vấn dữ liệu.'),
    ('DBI202', 4, N'Mệnh đề nào dùng để lọc kết quả theo điều kiện trong câu lệnh SELECT?', N'WHERE', N'GROUP BY', N'ORDER BY', N'HAVING', 'A', 'de', N'WHERE dùng để lọc các dòng thỏa mãn điều kiện.'),
    ('DBI202', 5, N'Sự khác nhau giữa WHERE và HAVING là gì?', N'WHERE lọc trước khi gom nhóm, HAVING lọc sau khi GROUP BY', N'WHERE chỉ dùng cho số, HAVING dùng cho chuỗi', N'HAVING không thể dùng kèm hàm tổng hợp', N'Không có sự khác nhau nào', 'A', 'trung_binh', N'HAVING áp dụng điều kiện lọc trên các nhóm kết quả sau GROUP BY.'),
    ('DBI202', 6, N'Phép kết nối INNER JOIN trả về kết quả như thế nào?', N'Chỉ các dòng có sự khớp dữ liệu giữa 2 bảng', N'Tất cả các dòng của bảng bên trái', N'Tất cả các dòng của bảng bên phải', N'Tích Đề-các của 2 bảng', 'A', 'de', N'INNER JOIN chỉ lấy các bản ghi có khóa thỏa mãn điều kiện kết nối ở cả 2 bảng.'),
    ('DBI202', 7, N'Lệnh nào dùng để xóa vĩnh viễn cấu trúc của một bảng khỏi CSDL?', N'DROP TABLE', N'DELETE FROM', N'TRUNCATE TABLE', N'REMOVE TABLE', 'A', 'de', N'DROP TABLE xóa cả schema lẫn dữ liệu của bảng.'),
    ('DBI202', 8, N'Mục đích chính của việc tạo Index trong SQL Server là gì?', N'Tăng tốc độ truy vấn tìm kiếm dữ liệu', N'Giảm dung lượng ổ đĩa lưu trữ', N'Tự động sao lưu dữ liệu mỗi ngày', N'Ngăn chặn người dùng đăng nhập', 'A', 'trung_binh', N'Index giúp CSDL tìm kiếm dữ liệu nhanh hơn mà không cần quét toàn bảng.'),
    ('DBI202', 9, N'Tính chất ACID trong giao dịch (Transaction) gồm những gì?', N'Atomicity, Consistency, Isolation, Durability', N'Accuracy, Control, Integrity, Data', N'Access, Connection, Index, Database', N'Authentication, Cryptography, Identity, Directory', 'A', 'trung_binh', N'ACID là 4 thuộc tính nền tảng đảm bảo tính tin cậy của transaction.'),
    ('DBI202', 10, N'Mức độ chuẩn hóa 1NF yêu cầu điều gì?', N'Các thuộc tính phải chứa giá trị nguyên tử (Atomic values)', N'Không có phụ thuộc bắc cầu', N'Mọi thuộc tính không khóa phụ thuộc hoàn toàn vào khóa chính', N'Bảng phải có ít nhất 10 cột', 'A', 'kho', N'Chuẩn 1NF yêu cầu mỗi ô dữ liệu chỉ chứa một giá trị đơn, không chứa mảng/danh sách.'),
    ('DBI202', 11, N'Hàm tổng hợp nào dùng để đếm số lượng dòng thỏa mãn điều kiện?', N'COUNT()', N'SUM()', N'AVG()', N'MAX()', 'A', 'de', N'COUNT() đếm số bản ghi.'),
    ('DBI202', 12, N'Từ khóa DISTINCT trong câu lệnh SELECT dùng để làm gì?', N'Loại bỏ các dòng có giá trị trùng lặp trong kết quả', N'Sắp xếp dữ liệu tăng dần', N'Chuyển chữ hoa thành chữ thường', N'Giới hạn số dòng trả về là 10', 'A', 'de', N'DISTINCT lọc bỏ các bản ghi trùng lặp.'),
    ('DBI202', 13, N'Trong SQL Server, Stored Procedure là gì?', N'Tập hợp các câu lệnh SQL được biên dịch và lưu trữ trên server', N'Tệp tin sao lưu cơ sở dữ liệu', N'Một loại bảng tạm thời', N'Hàm chỉ thực thi ở phía client', 'A', 'trung_binh', N'Stored Procedure là chương trình con lưu trên DB, giúp tái sử dụng và tăng hiệu năng.'),
    ('DBI202', 14, N'Trigger trong SQL Server được kích hoạt khi nào?', N'Tự động kích hoạt khi có sự kiện INSERT, UPDATE, hoặc DELETE', N'Khi người dùng nhấn F5', N'Khi khởi động lại máy chủ', N'Khi backup dữ liệu thành công', 'A', 'trung_binh', N'Trigger là thủ tục đặc biệt tự động chạy khi dữ liệu của bảng bị thay đổi.'),
    ('DBI202', 15, N'Kiểu dữ liệu NVARCHAR(100) khác VARCHAR(100) ở điểm nào?', N'NVARCHAR hỗ trợ lưu ký tự Unicode (tiếng Việt có dấu)', N'VARCHAR lưu được nhiều ký tự hơn', N'NVARCHAR chỉ lưu được số', N'VARCHAR bắt buộc phải có độ dài 100 ký tự', 'A', 'de', N'Tiền tố N (National) cho phép lưu bảng mã Unicode chuẩn.'),
    ('DBI202', 16, N'Lệnh COMMIT trong Transaction dùng để làm gì?', N'Xác nhận và lưu vĩnh viễn các thay đổi vào CSDL', N'Hủy bỏ toàn bộ các câu lệnh vừa thực thi', N'Tạo điểm lưu trữ checkpoint tạm thời', N'Đóng kết nối cơ sở dữ liệu', 'A', 'de', N'COMMIT xác nhận transaction thành công.'),
    ('DBI202', 17, N'Lệnh ROLLBACK trong Transaction dùng để làm gì?', N'Khôi phục dữ liệu về trạng thái trước khi bắt đầu transaction', N'Lưu dữ liệu sang ổ đĩa thứ hai', N'Khóa bảng vĩnh viễn', N'Xóa sạch nhật ký giao dịch (Log file)', 'A', 'de', N'ROLLBACK hủy bỏ các thay đổi của transaction khi xảy ra lỗi.'),
    ('DBI202', 18, N'VIEW trong CSDL là gì?', N'Bảng ảo được định nghĩa dựa trên một câu truy vấn SELECT', N'Một cửa sổ giao diện người dùng', N'Một bảng vật lý lưu trên ổ đĩa SSD', N'Hình ảnh chụp màn hình dữ liệu', 'A', 'de', N'VIEW là bảng ảo không chứa dữ liệu vật lý riêng mà trích xuất từ bảng gốc.'),
    ('DBI202', 19, N'Ràng buộc UNIQUE khác với PRIMARY KEY ở điểm nào?', N'UNIQUE cho phép chứa một giá trị NULL, còn PRIMARY KEY thì không', N'UNIQUE chỉ tạo được trên một cột duy nhất', N'PRIMARY KEY không tự động tạo Index', N'UNIQUE không thể dùng làm điều kiện JOIN', 'A', 'kho', N'UNIQUE đảm bảo không trùng giá trị nhưng cho phép 1 giá trị NULL trong SQL Server.'),
    ('DBI202', 20, N'Tấn công SQL Injection có thể được phòng tránh hiệu quả nhất bằng cách nào?', N'Sử dụng Parameterized Queries (Tham số hóa truy vấn)', N'Tắt kết nối Internet của database', N'Xóa các bảng quan trọng', N'Đổi tên người dùng sa thành admin', 'A', 'trung_binh', N'Parameterized query phân tách mã lệnh và dữ liệu đầu vào, ngăn chặn injection.');

    -- WEB104: Thiết kế trang web (HTML5/CSS3/JS) (20 câu)
    INSERT INTO @SampleQuestions VALUES
    ('WEB104', 1, N'Thẻ HTML nào dùng để tạo liên kết siêu văn bản (Hyperlink)?', N'<a>', N'<link>', N'<href>', N'<url>', 'A', 'de', N'Thẻ <a> với thuộc tính href tạo liên kết sang trang khác.'),
    ('WEB104', 2, N'Thuộc tính nào trong CSS được dùng để thay đổi màu chữ của phần tử?', N'color', N'background-color', N'text-color', N'font-color', 'A', 'de', N'color quy định màu sắc văn bản.'),
    ('WEB104', 3, N'Trong mô hình hộp (Box Model) của CSS, thứ tự từ trong ra ngoài là gì?', N'Content -> Padding -> Border -> Margin', N'Content -> Border -> Padding -> Margin', N'Margin -> Border -> Padding -> Content', N'Padding -> Content -> Margin -> Border', 'A', 'de', N'Mô hình hộp tiêu chuẩn gồm nội dung, đệm trong, đường viền, lề ngoài.'),
    ('WEB104', 4, N'Thuộc tính CSS nào cho phép dàn layout dạng lưới 2 chiều hiện đại?', N'display: grid', N'display: flex', N'display: block', N'display: inline', 'A', 'de', N'CSS Grid được thiết kế chuyên biệt cho layout lưới 2 chiều dòng và cột.'),
    ('WEB104', 5, N'Hàm nào trong JavaScript dùng để in thông tin ra cửa sổ Developer Console?', N'console.log()', N'print()', N'echo()', N'System.out.println()', 'A', 'de', N'console.log() ghi log ra tab Console của DevTools.'),
    ('WEB104', 6, N'Từ khóa nào trong ES6 dùng để khai báo một biến có phạm vi khối và có thể gán lại giá trị?', N'let', N'const', N'var', N'static', 'A', 'de', N'let có block scope và cho phép gán lại giá trị.'),
    ('WEB104', 7, N'Cú pháp nào dùng để chọn một phần tử theo id trong DOM bằng JavaScript?', N'document.getElementById()', N'document.selectId()', N'document.findElement()', N'document.queryId()', 'A', 'de', N'getElementById tìm phần tử qua thuộc tính id duy nhất.'),
    ('WEB104', 8, N'Đơn vị CSS nào có kích thước tỉ lệ tương đối theo kích thước font chữ của phần tử gốc html (Root)?', N'rem', N'em', N'px', N'%', 'A', 'trung_binh', N'rem = Root EM, tính dựa theo font-size của thẻ <html>.'),
    ('WEB104', 9, N'Sự kiện JavaScript nào xảy ra khi người dùng nhấp chuột vào một phần tử giao diện?', N'click', N'hover', N'keydown', N'submit', 'A', 'de', N'Sự kiện click kích hoạt khi bấm chuột trái.'),
    ('WEB104', 10, N'Phương thức Array nào trong JavaScript biến đổi từng phần tử và trả về một mảng mới có cùng độ dài?', N'map()', N'filter()', N'reduce()', N'forEach()', 'A', 'trung_binh', N'map() duyệt qua các phần tử và trả về mảng kết quả mới.'),
    ('WEB104', 11, N'Thẻ HTML5 nào dùng để nhúng video trực tiếp vào trang web mà không cần Flash?', N'<video>', N'<movie>', N'<media>', N'<film>', 'A', 'de', N'HTML5 cung cấp thẻ chuẩn <video>.'),
    ('WEB104', 12, N'Trong CSS Flexbox, thuộc tính nào căn chỉnh các phần tử con dọc theo trục chính (Main Axis)?', N'justify-content', N'align-items', N'flex-direction', N'align-content', 'A', 'trung_binh', N'justify-content căn chỉnh item theo trục chính (main axis).'),
    ('WEB104', 13, N'Trong JavaScript, kiểu dữ liệu nào đại diện cho giá trị chưa được định nghĩa?', N'undefined', N'null', N'NaN', N'void', 'A', 'de', N'undefined là kiểu dữ liệu của biến đã khai báo nhưng chưa được gán giá trị.'),
    ('WEB104', 14, N'Giao thức truyền thông nào là giao thức bảo mật có mã hóa SSL/TLS cho web?', N'HTTPS', N'HTTP', N'FTP', N'SMTP', 'A', 'de', N'HTTPS (HTTP Secure) mã hóa lưu lượng mạng bằng chứng chỉ TLS.'),
    ('WEB104', 15, N'Phương thức fetch() trong JavaScript trả về đối tượng nào?', N'Promise', N'Callback', N'Array', N'String', 'A', 'trung_binh', N'fetch API trả về một Promise đại diện cho kết quả bất đồng bộ.'),
    ('WEB104', 16, N'JSON là viết tắt của cụm từ nào?', N'JavaScript Object Notation', N'Java Source Open Network', N'JavaScript Online Navigation', N'Joint System Online Network', 'A', 'de', N'JSON là định dạng trao đổi dữ liệu tiêu chuẩn.'),
    ('WEB104', 17, N'Thẻ meta nào bắt buộc phải có để trang web hiển thị responsive đúng tỉ lệ trên màn hình di động?', N'<meta name="viewport" content="width=device-width, initial-scale=1.0">', N'<meta name="responsive" content="true">', N'<meta name="mobile" content="yes">', N'<meta name="screen" content="auto">', 'A', 'de', N'Thẻ meta viewport thiết lập kích thước viewport tương thích với màn hình thiết bị.'),
    ('WEB104', 18, N'Toán tử so sánh nghiêm ngặt `===` trong JavaScript khác `==` ở điểm nào?', N'So sánh cả giá trị lẫn kiểu dữ liệu mà không tự động ép kiểu', N'Chạy nhanh hơn gấp 3 lần', N'Chỉ dùng để so sánh chuỗi', N'Tự động ép kiểu chuỗi sang số', 'A', 'trung_binh', N'=== (strict equality) không thực hiện type coercion.'),
    ('WEB104', 19, N'Thuộc tính CSS `z-index` chỉ có tác dụng khi phần tử có giá trị `position` nào?', N'relative, absolute, fixed hoặc sticky', N'static', N'inherit', N'none', 'A', 'kho', N'z-index chỉ ảnh hưởng đến các phần tử được định vị (positioned elements).'),
    ('WEB104', 20, N'Cơ chế LocalStorage trong trình duyệt có đặc điểm gì?', N'Dữ liệu được lưu vô thời hạn trên client cho đến khi bị xóa chủ động', N'Dữ liệu bị xóa ngay khi đóng tab', N'Dữ liệu tự động gửi lên server mỗi request', N'Chỉ lưu tối đa được 100 bytes', 'A', 'trung_binh', N'LocalStorage lưu trữ trên client không có thời hạn hết hạn tự động.');

    -- PRF192: Kỹ thuật lập trình C/C++ (15 câu)
    INSERT INTO @SampleQuestions VALUES
    ('PRF192', 1, N'Kích thước của kiểu dữ liệu char trong C tiêu chuẩn là bao nhiêu byte?', N'1 byte', N'2 bytes', N'4 bytes', N'8 bytes', 'A', 'de', N'char luôn có kích thước là 1 byte trong chuẩn C.'),
    ('PRF192', 2, N'Hàm nào trong thư viện stdio.h dùng để đọc dữ liệu có định dạng từ bàn phím?', N'scanf()', N'printf()', N'gets()', N'puts()', 'A', 'de', N'scanf() đọc dữ liệu theo định dạng.'),
    ('PRF192', 3, N'Ký tự kết thúc chuỗi (Null-terminator) trong C là gì?', N'\\0', N'\\n', N'\\t', N'\\r', 'A', 'de', N'Chuỗi trong C kết thúc bằng byte 0 (\\0).'),
    ('PRF192', 4, N'Toán tử nào dùng để lấy địa chỉ của một biến?', N'&', N'*', N'->', N'.', 'A', 'de', N'Toán tử & (address-of) lấy địa chỉ ô nhớ của biến.'),
    ('PRF192', 5, N'Hàm nào trong stdlib.h dùng để cấp phát bộ nhớ động?', N'malloc()', N'free()', N'alloc()', N'new', 'A', 'trung_binh', N'malloc cấp phát bộ nhớ động trên Heap.'),
    ('PRF192', 6, N'Hàm nào dùng để giải phóng vùng nhớ được cấp phát bằng malloc?', N'free()', N'delete', N'release()', N'clear()', 'A', 'de', N'free() giải phóng bộ nhớ Heap đã cấp phát.'),
    ('PRF192', 7, N'Từ khóa struct trong C dùng để định nghĩa cái gì?', N'Kiểu dữ liệu cấu trúc tự định nghĩa', N'Một hàm đặc biệt', N'Một con trỏ hàm', N'Một thư viện hệ thống', 'A', 'de', N'struct nhóm các biến có kiểu dữ liệu khác nhau thành một thực thể.'),
    ('PRF192', 8, N'Toán tử -> trong C dùng khi nào?', N'Truy cập thành viên của struct thông qua con trỏ', N'Khai báo hàm', N'Gán giá trị cho mảng', N'Ép kiểu dữ liệu', 'A', 'trung_binh', N'ptr->member tương đương (*ptr).member.'),
    ('PRF192', 9, N'Điều gì xảy ra khi không giải phóng bộ nhớ sau khi dùng malloc?', N'Gây ra hiện tượng rò rỉ bộ nhớ (Memory Leak)', N'Máy tính tự động tắt nguồn', N'Dữ liệu tự động lưu vào ổ cứng', N'Trình biên dịch báo lỗi cú pháp', 'A', 'trung_binh', N'Memory Leak làm cạn kiệt RAM theo thời gian.'),
    ('PRF192', 10, N'Chỉ thị tiền xử lý nào dùng để đưa tệp tiêu đề (Header file) vào chương trình?', N'#include', N'#define', N'#ifdef', N'#pragma', 'A', 'de', N'#include chèn nội dung file header vào mã nguồn.'),
    ('PRF192', 11, N'Trong C++, toán tử nào dùng để cấp phát động cho một đối tượng?', N'new', N'malloc', N'create', N'make', 'A', 'de', N'C++ sử dụng toán tử new để cấp phát và gọi constructor.'),
    ('PRF192', 12, N'Trong C++, toán tử nào dùng để giải phóng bộ nhớ cấp phát bằng new?', N'delete', N'free', N'remove', N'drop', 'A', 'de', N'delete gọi destructor và giải phóng bộ nhớ.'),
    ('PRF192', 13, N'Tham chiếu (Reference) trong C++ có đặc điểm gì so với con trỏ?', N'Là bí danh của biến và không thể NULL', N'Có thể trỏ sang đối tượng khác sau khi khởi tạo', N'Chiếm 16 bytes trong RAM', N'Không cần khởi tạo khi khai báo', 'A', 'kho', N'Reference là alias của biến gốc và phải gán ngay khi khai báo.'),
    ('PRF192', 14, N'Từ khóa const đặt trước tham số hàm có ý nghĩa gì?', N'Hàm không được phép thay đổi giá trị của tham số đó', N'Tham số bắt buộc phải là số', N'Tham số tự động tăng lên 1', N'Hàm chạy ở chế độ đa luồng', 'A', 'de', N'const bảo vệ biến không bị ghi đè/sửa đổi.'),
    ('PRF192', 15, N'Hàm main() trong chuẩn C/C++ trả về kiểu dữ liệu gì?', N'int', N'void', N'char', N'float', 'A', 'de', N'int main() trả về mã thoát exit code cho hệ điều hành.');

    -- PRO192: Lập trình hướng đối tượng với Java (15 câu)
    INSERT INTO @SampleQuestions VALUES
    ('PRO192', 1, N'Bốn tính chất cốt lõi của lập trình hướng đối tượng (OOP) là gì?', N'Đóng gói, Kế thừa, Đa hình, Trừu tượng', N'Tuần tự, Rẽ nhánh, Vòng lặp, Hàm', N'Biến, Mảng, Con trỏ, Cấu trúc', N'Đọc, Ghi, Mở, Đóng', 'A', 'de', N'Encapsulation, Inheritance, Polymorphism, Abstraction.'),
    ('PRO192', 2, N'Từ khóa nào trong Java dùng để kế thừa một lớp cha?', N'extends', N'implements', N'inherits', N'super', 'A', 'de', N'extends dùng cho kế thừa lớp.'),
    ('PRO192', 3, N'Từ khóa nào trong Java dùng để hiện thực hóa một Interface?', N'implements', N'extends', N'interface', N'abstract', 'A', 'de', N'implements dùng để triển khai interface.'),
    ('PRO192', 4, N'Constructor (Hàm khởi tạo) trong Java có đặc điểm nào?', N'Cùng tên với lớp và không có kiểu trả về', N'Có kiểu trả về là void', N'Tên bắt đầu bằng chữ new', N'Bắt buộc phải là private', 'A', 'de', N'Constructor trùng tên với Class và không có return type.'),
    ('PRO192', 5, N'Từ khóa super trong Java dùng để làm gì?', N'Tham chiếu đến các thành viên hoặc constructor của lớp cha', N'Khai báo lớp trừu tượng', N'Ngăn chặn kế thừa', N'Tạo đối tượng mới', 'A', 'trung_binh', N'super trỏ đến trực tiếp superclass.'),
    ('PRO192', 6, N'Lớp gốc của tất cả các lớp trong Java là lớp nào?', N'java.lang.Object', N'java.lang.Class', N'java.lang.System', N'java.lang.String', 'A', 'de', N'Object là root class trong hệ thống phân cấp của Java.'),
    ('PRO192', 7, N'Từ khóa final áp dụng cho một lớp có ý nghĩa gì?', N'Lớp đó không thể bị kế thừa', N'Lớp đó không thể tạo đối tượng', N'Mọi phương thức đều là trừu tượng', N'Lớp đó tự động bị xóa sau khi chạy', 'A', 'trung_binh', N'final class không thể có subclass.'),
    ('PRO192', 8, N'Nạp chồng phương thức (Method Overloading) là gì?', N'Các phương thức cùng tên nhưng khác nhau về danh sách tham số', N'Phương thức con ghi đè phương thức cha', N'Phương thức có nhiều kiểu trả về cùng lúc', N'Gọi phương thức liên tục không ngừng', 'A', 'trung_binh', N'Overloading diễn ra trong cùng 1 class với chữ ký tham số khác nhau.'),
    ('PRO192', 9, N'Ghi đè phương thức (Method Overriding) là gì?', N'Lớp con định nghĩa lại phương thức đã có ở lớp cha', N'Khai báo lại biến trong hàm', N'Hai phương thức khác tên cùng chức năng', N'Nạp chồng toán tử', 'A', 'de', N'Overriding thể hiện tính đa hình lúc chạy (Runtime polymorphism).'),
    ('PRO192', 10, N'Garbage Collection trong máy ảo Java (JVM) có chức năng gì?', N'Tự động thu hồi vùng nhớ của các đối tượng không còn được tham chiếu', N'Xóa các file rác trên ổ cứng', N'Kiểm tra lỗi chính tả trong code', N'Tối ưu hóa đường truyền mạng', 'A', 'de', N'Garbage Collector tự động quản lý bộ nhớ Heap.'),
    ('PRO192', 11, N'Khối lệnh nào trong xử lý ngoại lệ try-catch luôn được thực thi dù có lỗi hay không?', N'finally', N'catch', N'throw', N'throws', 'A', 'de', N'finally luôn chạy để dọn dẹp tài nguyên (close file, connection).'),
    ('PRO192', 12, N'Chuỗi String trong Java có đặc tính nào nổi bật?', N'Bất biến (Immutable)', N'Có thể thay đổi độ dài tùy ý', N'Lưu trữ trên Stack', N'Không phân biệt chữ hoa chữ thường', 'A', 'kho', N'String trong Java là immutable, thay đổi chuỗi sẽ sinh đối tượng mới.'),
    ('PRO192', 13, N'Giao diện List trong Java Collections Framework khác Set ở điểm nào?', N'List cho phép phần tử trùng lặp và giữ thứ tự thêm vào', N'Set cho phép trùng lặp', N'List không thể chứa số', N'Set chạy chậm hơn 100 lần', 'A', 'trung_binh', N'List có thứ tự và cho phép duplicate; Set không duplicate.'),
    ('PRO192', 14, N'Từ khóa static trong định nghĩa biến có ý nghĩa gì?', N'Biến thuộc về lớp và được chia sẻ chung cho mọi đối tượng', N'Biến không thể thay đổi giá trị', N'Biến chỉ dùng được trong hàm main', N'Biến tự động mã hóa', 'A', 'de', N'static variable thuộc class-level, chia sẻ chung cho tất cả instance.'),
    ('PRO192', 15, N'Đóng gói (Encapsulation) thường được thực hiện như thế nào trong Java?', N'Khai báo các thuộc tính private và cung cấp getter/setter public', N'Để tất cả thuộc tính là public', N'Không viết phương thức nào trong class', N'Đặt tất cả code vào hàm main', 'A', 'de', N'Ẩn giấu trạng thái đối tượng qua private field và điều khiển truy cập qua getter/setter.');

    -- CSD201: Cấu trúc dữ liệu & Giải thuật (15 câu)
    INSERT INTO @SampleQuestions VALUES
    ('CSD201', 1, N'Ngăn xếp (Stack) hoạt động theo nguyên lý nào?', N'LIFO (Last In First Out)', N'FIFO (First In First Out)', N'LILO (Last In Last Out)', N'Ngẫu nhiên', 'A', 'de', N'Stack vào sau ra trước (LIFO).'),
    ('CSD201', 2, N'Hàng đợi (Queue) hoạt động theo nguyên lý nào?', N'FIFO (First In First Out)', N'LIFO (Last In First Out)', N'Priority Only', N'Hỗn loạn', 'A', 'de', N'Queue vào trước ra trước (FIFO).'),
    ('CSD201', 3, N'Độ phức tạp thời gian trung bình của thuật toán tìm kiếm nhị phân (Binary Search) là gì?', N'O(log n)', N'O(n)', N'O(n^2)', N'O(1)', 'A', 'de', N'Binary Search chia đôi không gian tìm kiếm mỗi bước, đạt O(log n).'),
    ('CSD201', 4, N'Điều kiện tiên quyết để áp dụng thuật toán tìm kiếm nhị phân là gì?', N'Dãy dữ liệu đã được sắp xếp', N'Dãy dữ liệu phải là số thực', N'Kích thước dữ liệu phải nhỏ hơn 100', N'Dữ liệu lưu trong bảng băm', 'A', 'de', N'Mảng phải được sort trước khi áp dụng Binary Search.'),
    ('CSD201', 5, N'Danh sách liên kết đơn (Singly Linked List) có ưu điểm gì so với Mảng tĩnh?', N'Chèn và xóa phần tử linh hoạt không cần dời các phần tử khác', N'Truy cập ngẫu nhiên qua chỉ số O(1)', N'Tiết kiệm bộ nhớ hơn mảng', N'Không bao giờ bị lỗi tràn bộ nhớ', 'A', 'trung_binh', N'Linked list chỉ cần đổi con trỏ node kế tiếp khi insert/delete.'),
    ('CSD201', 6, N'Độ phức tạp trường hợp xấu nhất của thuật toán sắp xếp nổi bọt (Bubble Sort) là gì?', N'O(n^2)', N'O(n log n)', N'O(n)', N'O(1)', 'A', 'de', N'Bubble sort duyệt 2 vòng lặp lồng nhau nên xấu nhất là O(n^2).'),
    ('CSD201', 7, N'Thuật toán sắp xếp nào hoạt động theo chiến lược Chia để trị (Divide and Conquer)?', N'Merge Sort', N'Bubble Sort', N'Insertion Sort', N'Selection Sort', 'A', 'trung_binh', N'Merge Sort chia nhỏ mảng rồi gộp lại.'),
    ('CSD201', 8, N'Trong cây nhị phân tìm kiếm (BST), phần tử ở nhánh con bên trái luôn có giá trị như thế nào so với nút gốc?', N'Nhỏ hơn nút gốc', N'Lớn hơn nút gốc', N'Bằng nút gốc', N'Gấp đôi nút gốc', 'A', 'de', N'BST quy định left < root < right.'),
    ('CSD201', 9, N'Duyệt cây theo thứ tự giữa (In-order Traversal) trên cây BST cho kết quả như thế nào?', N'Dãy các phần tử được sắp xếp tăng dần', N'Dãy các phần tử giảm dần', N'Thứ tự ngẫu nhiên', N'Chỉ in ra các lá của cây', 'A', 'kho', N'In-order duyệt Left -> Root -> Right sinh ra dãy có thứ tự tăng dần.'),
    ('CSD201', 10, N'Bảng băm (Hash Table) sử dụng hàm băm (Hash Function) để làm gì?', N'Ánh xạ khóa (Key) thành chỉ số vị trí lưu trữ trong mảng', N'Mã hóa mật khẩu 2 chiều', N'Sắp xếp chuỗi ký tự theo alphabet', N'Nén dữ liệu giảm dung lượng', 'A', 'trung_binh', N'Hash function tính toán index từ key để đạt truy cập O(1).'),
    ('CSD201', 11, N'Xử lý xung đột (Collision) trong Bảng băm thường dùng phương pháp nào?', N'Chaining (Nối chuỗi) hoặc Open Addressing (Dò mở)', N'Xóa bỏ khóa bị trùng', N'Tăng gấp đôi tốc độ CPU', N'Dừng chương trình', 'A', 'kho', N'Separate chaining và open addressing là 2 kỹ thuật xử lý đụng độ phổ biến.'),
    ('CSD201', 12, N'Thuật toán Dijkstra dùng để giải quyết bài toán nào trên Đồ thị?', N'Tìm đường đi ngắn nhất từ một đỉnh nguồn đến các đỉnh còn lại', N'Tìm cây khung nhỏ nhất (MST)', N'Tìm chu trình Euler', N'Tô màu đồ thị', 'A', 'kho', N'Dijkstra tìm single-source shortest path cho đồ thị có trọng số không âm.'),
    ('CSD201', 13, N'Thuật toán BFS (Breadth-First Search) duyệt đồ thị theo cơ chế nào?', N'Duyệt theo chiều rộng dùng Queue', N'Duyệt theo chiều sâu dùng Stack', N'Duyệt ngẫu nhiên dùng mảng', N'Duyệt ngược từ đích về nguồn', 'A', 'trung_binh', N'BFS mở rộng các đỉnh lân cận cùng cấp trước bằng hàng đợi Queue.'),
    ('CSD201', 14, N'Cây cân bằng AVL là cây nhị phân tìm kiếm có đặc điểm gì?', N'Chênh lệch chiều cao giữa 2 cây con của mọi nút không quá 1', N'Tất cả các lá đều nằm ở cùng một độ sâu', N'Mỗi nút có đúng 3 con', N'Chiều cao luôn bằng số nút', 'A', 'kho', N'AVL tree tự cân bằng qua phép quay khi hệ số cân bằng vượt quá [-1, 1].'),
    ('CSD201', 15, N'Độ phức tạp không gian (Space Complexity) của thuật toán là gì?', N'Lượng bộ nhớ mà thuật toán sử dụng phụ thuộc vào kích thước đầu vào', N'Thời gian chạy tính bằng mili-giây', N'Kích thước file mã nguồn', N'Dung lượng bộ nhớ cache của CPU', 'A', 'de', N'Space complexity đo lường mức độ chiếm dụng RAM của thuật toán.');

    -- PRN211: Lập trình ứng dụng với C# .NET (15 câu)
    INSERT INTO @SampleQuestions VALUES
    ('PRN211', 1, N'Trong C#, từ khóa nào dùng để định nghĩa một thuộc tính (Property) tự động?', N'{ get; set; }', N'{ read; write; }', N'{ return; assign; }', N'{ val; setVal; }', 'A', 'de', N'Auto-implemented properties có cú pháp { get; set; }.'),
    ('PRN211', 2, N'Lớp trừu tượng (Abstract Class) trong C# khác Interface ở điểm nào?', N'Abstract class có thể chứa định nghĩa phương thức có thân hàm', N'Interface có thể chứa constructor', N'Một class có thể kế thừa nhiều Abstract class', N'Không có điểm khác nhau nào', 'A', 'trung_binh', N'Abstract class hỗ trợ cả method implementation và field trạng thái.'),
    ('PRN211', 3, N'Từ khóa async/await trong C# dùng để làm gì?', N'Lập trình bất đồng bộ (Asynchronous Programming)', N'Tạo biến toàn cục', N'Quản lý ngoại lệ', N'Đóng kết nối cơ sở dữ liệu', 'A', 'de', N'async/await đơn giản hóa việc viết code bất đồng bộ không chặn luồng chính.'),
    ('PRN211', 4, N'LINQ trong C# là viết tắt của cụm từ nào?', N'Language Integrated Query', N'Local Information Network Queue', N'Logical Interface Node Query', N'Linked Internal Network Queue', 'A', 'de', N'LINQ tích hợp cú pháp truy vấn trực tiếp vào ngôn ngữ C#.'),
    ('PRN211', 5, N'Phương thức LINQ nào dùng để lọc các phần tử theo điều kiện?', N'Where()', N'Select()', N'OrderBy()', N'GroupBy()', 'A', 'de', N'Where() lọc các phần tử thỏa predicate.'),
    ('PRN211', 6, N'Khối lệnh using (...) trong C# đảm bảo điều gì cho đối tượng thực thi IDisposable?', N'Tự động gọi Dispose() khi kết thúc khối lệnh', N'Bảo mật mã hóa dữ liệu', N'Tự động đồng bộ lên cloud', N'Bỏ qua mọi ngoại lệ xảy ra', 'A', 'de', N'using statement gọi Dispose() giải phóng tài nguyên unmanaged.'),
    ('PRN211', 7, N'Delegate trong C# là gì?', N'Kiểu dữ liệu tham chiếu an toàn trỏ đến một hoặc nhiều hàm', N'Một biến kiểu số', N'Một thư viện đồ họa', N'Một tiến trình riêng biệt', 'A', 'trung_binh', N'Delegate là type-safe function pointer.'),
    ('PRN211', 8, N'Kiểu dữ liệu nullable trong C# được khai báo bằng ký hiệu nào sau tên kiểu?', N'?', N'!', N'*', N'&', 'A', 'de', N'Ví dụ int? cho phép lưu cả số nguyên lẫn giá trị null.'),
    ('PRN211', 9, N'Sự khác biệt chính giữa Value Type và Reference Type trong C# là gì?', N'Value Type lưu trên Stack, Reference Type lưu trên Heap', N'Value Type không thể chứa số', N'Reference Type luôn có kích thước cố định 1 byte', N'Value Type tự động bị mã hóa', 'A', 'trung_binh', N'Value types cấp phát trên stack, reference types cấp phát trên heap.'),
    ('PRN211', 10, N'Trong Entity Framework Core, DbContext đại diện cho cái gì?', N'Phiên làm việc với cơ sở dữ liệu và quản lý các entity', N'Một bảng đơn lẻ trong DB', N'Một chuỗi kết nối duy nhất', N'Một tệp tin cấu hình JSON', 'A', 'de', N'DbContext quản lý kết nối, mapping, change tracking và lưu dữ liệu xuống DB.'),
    ('PRN211', 11, N'Từ khóa sealed áp dụng cho một lớp trong C# có ý nghĩa gì?', N'Ngăn chặn các lớp khác kế thừa từ lớp này', N'Không cho phép tạo đối tượng', N'Mọi thành viên đều là private', N'Lớp chỉ chạy được trên hệ điều hành Windows', 'A', 'trung_binh', N'sealed class tương đương final class trong Java.'),
    ('PRN211', 12, N'Extension Method trong C# cho phép làm gì?', N'Thêm phương thức mới vào kiểu dữ liệu có sẵn mà không cần kế thừa', N'Đổi tên class thư viện', N'Tự động tăng tốc độ CPU', N'Xóa các phương thức không dùng', 'A', 'kho', N'Extension methods mở rộng tính năng của class có sẵn qua static method có từ khóa this.'),
    ('PRN211', 13, N'Trong C#, record type được sử dụng tối ưu nhất cho mục đích nào?', N'Mô hình dữ liệu bất biến (Immutable Data Modeling / DTO)', N'Xử lý đồ họa chuyển động', N'Giao tiếp với phần cứng máy in', N'Viết hệ điều hành', 'A', 'trung_binh', N'record cung cấp value-based equality và immutable properties.'),
    ('PRN211', 14, N'Collection Generic nào lưu trữ các cặp Khóa/Giá trị (Key/Value) trong C#?', N'Dictionary<TKey, TValue>', N'List<T>', N'HashSet<T>', N'Queue<T>', 'A', 'de', N'Dictionary<TKey, TValue> ánh xạ key sang value với tốc độ tìm kiếm O(1).'),
    ('PRN211', 15, N'Dependency Injection (DI) trong ASP.NET Core mang lại lợi ích gì?', N'Giảm sự phụ thuộc cứng giữa các thành phần và tăng khả năng kiểm thử', N'Làm cho code dài gấp đôi', N'Bắt buộc dùng SQL Server', N'Chỉ hoạt động khi có kết nối Internet', 'A', 'trung_binh', N'DI tách rời phụ thuộc (loose coupling), giúp mã nguồn dễ bảo trì và viết Unit Test.');

    -- UIX101: Thiết kế UI/UX căn bản (15 câu)
    INSERT INTO @SampleQuestions VALUES
    ('UIX101', 1, N'UI là viết tắt của thuật ngữ nào trong thiết kế sản phẩm số?', N'User Interface', N'User Information', N'Unified Integration', N'Universal Interaction', 'A', 'de', N'UI là User Interface - Giao diện người dùng.'),
    ('UIX101', 2, N'UX là viết tắt của thuật ngữ nào?', N'User Experience', N'User Examination', N'Universal Extension', N'Unit Execution', 'A', 'de', N'UX là User Experience - Trải nghiệm người dùng.'),
    ('UIX101', 3, N'Mục tiêu chính của thiết kế UX là gì?', N'Đảm bảo sản phẩm dễ sử dụng, hữu ích và mang lại trải nghiệm hài lòng cho người dùng', N'Vẽ thật nhiều màu sắc sặc sỡ', N'Viết mã nguồn backend nhanh nhất', N'Tạo hiệu ứng động phức tạp', 'A', 'de', N'UX tập trung vào sự thuận tiện, hiệu quả và cảm xúc của người dùng khi tương tác.'),
    ('UIX101', 4, N'Wireframe trong quy trình thiết kế là gì?', N'Bản phác thảo khung bố cục tĩnh mô tả cấu trúc giao diện trước khi thêm chi tiết đồ họa', N'Mã nguồn CSS của trang', N'Bản hợp đồng ký với khách hàng', N'Ảnh chụp màn hình sản phẩm cuối', 'A', 'de', N'Wireframe thể hiện cấu trúc phân cấp và vị trí các thành phần.'),
    ('UIX101', 5, N'Độ tương phản (Contrast) giữa chữ và nền có vai trò gì quan trọng nhất?', N'Giúp người dùng đọc nội dung dễ dàng và tăng tính tiếp cận (Accessibility)', N'Làm cho trang web tải nhanh hơn', N'Tiết kiệm pin cho điện thoại', N'Tránh bị vi phạm bản quyền', 'A', 'de', N'Contrast chuẩn WCAG đảm bảo khả năng đọc cho mọi đối tượng người dùng.'),
    ('UIX101', 6, N'Nguyên tắc thiết kế Mobile-First có nghĩa là gì?', N'Thiết kế giao diện cho màn hình di động trước rồi mới mở rộng sang máy tính', N'Chỉ làm ứng dụng cho điện thoại', N'Bắt buộc người dùng phải mua điện thoại mới', N'Không bao giờ làm phiên bản desktop', 'A', 'de', N'Mobile-first ưu tiên tối ưu nội dung cốt lõi cho màn hình nhỏ.'),
    ('UIX101', 7, N'Hệ màu RGB thường được dùng cho mục đích nào?', N'Hiển thị trên các màn hình kỹ thuật số', N'In ấn trên giấy và bao bì', N'Vẽ tranh sơn dầu', N'Khắc laser', 'A', 'de', N'RGB là hệ màu cộng phát xạ ánh sáng trên màn hình số.'),
    ('UIX101', 8, N'Hệ màu CMYK được dùng chuyên biệt cho lĩnh vực nào?', N'In ấn công nghiệp và xuất bản', N'Màn hình OLED', N'Thiết kế trang web', N'Quay video Youtube', 'A', 'de', N'CMYK là hệ màu trừ dùng trong mực in ấn.'),
    ('UIX101', 9, N'Call-to-Action (CTA) trên trang web thường là phần tử nào?', N'Nút bấm kêu gọi hành động (Mua ngay, Đăng ký, Tải về)', N'Thanh cuộn chuột', N'Logo trường học', N'Địa chỉ ở chân trang', 'A', 'de', N'CTA điều hướng người dùng thực hiện hành vi chuyển đổi mong muốn.'),
    ('UIX101', 10, N'Khoảng trắng (White Space / Negative Space) trong thiết kế có tác dụng gì?', N'Giúp bố cục thoáng đãng, phân tách thông tin và hướng sự chú ý của mắt', N'Là khoảng trống bị lỗi do thiếu nội dung', N'Làm cho website bị xấu đi', N'Gây tốn dung lượng máy chủ', 'A', 'trung_binh', N'White space tăng tính thẩm mỹ và định hướng thị giác.'),
    ('UIX101', 11, N'Thử nghiệm khả năng sử dụng (Usability Testing) nhằm mục đích gì?', N'Quan sát người dùng thật tương tác để tìm ra rào cản và lỗi trải nghiệm', N'Kiểm tra tốc độ của CPU', N'Đo lường điện năng tiêu thụ', N'Xếp hạng SEO trang web', 'A', 'trung_binh', N'Usability testing giúp phát hiện các điểm đau (pain points) của người dùng.'),
    ('UIX101', 12, N'Typography trong thiết kế giao diện đề cập đến điều gì?', N'Nghệ thuật sắp đặt và trình bày chữ viết (Font, kích thước, khoảng cách)', N'Vẽ hình minh họa động vật', N'Màu sắc của banner', N'Tạo hiệu ứng âm thanh', 'A', 'de', N'Typography quyết định tính đọc được và phong cách nhận diện văn bản.'),
    ('UIX101', 13, N'Trong Figma, tính năng Auto Layout giúp làm gì?', N'Tạo các khung linh hoạt tự động co giãn kích thước theo nội dung', N'Tự động đổi màu ngẫu nhiên', N'Xuất mã nguồn C#', N'Tự động viết nội dung quảng cáo', 'A', 'trung_binh', N'Auto Layout trong Figma xây dựng giao diện responsive tương tự Flexbox.'),
    ('UIX101', 14, N'Luật Hick (Hick''s Law) trong tâm lý học UX phát biểu điều gì?', N'Thời gian ra quyết định tăng lên khi số lượng lựa chọn tăng', N'Càng nhiều nút bấm thì người dùng càng thích', N'Giao diện nên có tối thiểu 20 màu sắc', N'Người dùng luôn đọc hết mọi từ trên trang', 'A', 'kho', N'Giảm bớt sự lựa chọn giúp người dùng ra quyết định nhanh hơn.'),
    ('UIX101', 15, N'Affordance của một phần tử giao diện là gì?', N'Đặc tính trực quan gợi ý cho người dùng biết cách tương tác với nó', N'Giá tiền để mua phần tử đó', N'Tốc độ tải xuống của hình ảnh', N'Mã số nhận diện trong CSS', 'A', 'kho', N'Ví dụ nút bấm có bóng đổ và viền gợi ý rằng nó có thể bấm được.');

    -- MKT101: Marketing căn bản (15 câu)
    INSERT INTO @SampleQuestions VALUES
    ('MKT101', 1, N'Mô hình 4P truyền thống trong Marketing Mix gồm các yếu tố nào?', N'Product, Price, Place, Promotion', N'People, Process, Physical, Plan', N'Public, Profit, Power, Performance', N'Packet, Policy, Partner, Position', 'A', 'de', N'4P gồm Sản phẩm, Giá cả, Phân phối, và Xúc tiến bán hàng.'),
    ('MKT101', 2, N'Mô hình SWOT phân tích các yếu tố nào của doanh nghiệp?', N'Strengths, Weaknesses, Opportunities, Threats', N'Sales, Workflow, Orders, Targets', N'Staff, Website, Operations, Technology', N'System, Warehouse, Output, Tax', 'A', 'de', N'SWOT phân tích Điểm mạnh, Điểm yếu, Cơ hội, và Thách thức.'),
    ('MKT101', 3, N'Khái niệm Khách hàng mục tiêu (Target Audience) là gì?', N'Nhóm khách hàng cụ thể có nhu cầu và đặc điểm phù hợp mà sản phẩm hướng tới', N'Tất cả mọi người trên thế giới', N'Những người đang làm việc tại công ty đối thủ', N'Nhà cung cấp nguyên vật liệu', 'A', 'de', N'Target audience là đối tượng mục tiêu của chiến dịch tiếp thị.'),
    ('MKT101', 4, N'Phân khúc thị trường (Market Segmentation) là quá trình gì?', N'Chia thị trường lớn thành các nhóm nhỏ có đặc điểm tiêu dùng tương đồng', N'Bán hết toàn bộ hàng hóa với giá rẻ', N'Xây dựng thêm nhà máy sản xuất', N'Sa thải nhân viên kinh doanh', 'A', 'de', N'Phân đoạn thị trường theo nhân khẩu học, địa lý, tâm lý, hoặc hành vi.'),
    ('MKT101', 5, N'Chỉ số ROI (Return on Investment) trong Marketing đo lường cái gì?', N'Hiệu quả sinh lời của chi phí đầu tư tiếp thị', N'Số lượng người truy cập website', N'Thời gian chạy chiến dịch', N'Mức độ hài lòng của nhân viên', 'A', 'de', N'ROI = (Lợi nhuận ròng / Chi phí đầu tư) * 100%.'),
    ('MKT101', 6, N'SEO (Search Engine Optimization) có mục đích chính là gì?', N'Tối ưu hóa website để đạt thứ hạng cao tự nhiên trên trang kết quả tìm kiếm', N'Trả tiền mua quảng cáo banner', N'Gửi thư rác qua email', N'Chạy quảng cáo truyền hình', 'A', 'de', N'SEO gia tăng lưu lượng truy cập tự nhiên (Organic Traffic).'),
    ('MKT101', 7, N'Phễu Marketing (Marketing Funnel) tiêu chuẩn theo mô hình AIDA gồm các giai đoạn nào?', N'Attention, Interest, Desire, Action', N'Access, Information, Delivery, Acceptance', N'Aim, Investment, Deal, Account', N'Ask, Inspect, Decide, Agree', 'A', 'trung_binh', N'AIDA: Chú ý -> Hứng thú -> Mong muốn -> Hành động mua.'),
    ('MKT101', 8, N'Khái niệm USP (Unique Selling Proposition) của sản phẩm là gì?', N'Đặc điểm bán hàng độc nhất giúp phân biệt sản phẩm với đối thủ cạnh tranh', N'Giá bán sỉ thấp nhất thị trường', N'Mã số đăng ký kinh doanh của công ty', N'Tên của giám đốc marketing', 'A', 'de', N'USP nêu bật lợi ích khác biệt vượt trội mà đối thủ không có.'),
    ('MKT101', 9, N'Tiếp thị nội dung (Content Marketing) tập trung vào điều gì?', N'Tạo và phân phối nội dung có giá trị, phù hợp để thu hút và giữ chân khách hàng', N'In tờ rơi phát ở ngã tư', N'Gọi điện thoại làm phiền khách hàng', N'Giảm giá sản phẩm liên tục', 'A', 'de', N'Content marketing nuôi dưỡng mối quan hệ bền vững với khách hàng qua giá trị thông tin.'),
    ('MKT101', 10, N'Chỉ số CTR (Click-Through Rate) đo lường điều gì?', N'Tỷ lệ người dùng nhấp vào liên kết/quảng cáo so với tổng số lần hiển thị', N'Tỷ lệ hoàn tiền sản phẩm', N'Tốc độ tải trang web', N'Số lượng bình luận trên mạng xã hội', 'A', 'trung_binh', N'CTR = (Số click / Số lần hiển thị) * 100%.'),
    ('MKT101', 11, N'KOL (Key Opinion Leader) trong truyền thông là ai?', N'Những người có kiến thức chuyên môn và tầm ảnh hưởng lớn trong một lĩnh vực', N'Khách hàng mới mua hàng lần đầu', N'Lập trình viên viết phần mềm', N'Kế toán trưởng của doanh nghiệp', 'A', 'de', N'KOL có độ tin cậy và định hướng quan điểm trong cộng đồng.'),
    ('MKT101', 12, N'Branding (Xây dựng thương hiệu) có ý nghĩa như thế nào đối với doanh nghiệp?', N'Định vị hình ảnh, bản sắc và uy tín của doanh nghiệp trong tâm trí khách hàng', N'Chỉ đơn giản là vẽ một cái logo đẹp', N'Tăng giá bán sản phẩm lên gấp 10 lần', N'Không mang lại lợi ích tài chính nào', 'A', 'de', N'Thương hiệu tạo ra tài sản vô hình và lợi thế cạnh tranh lâu dài.'),
    ('MKT101', 13, N'Chiến lược giá thâm nhập thị trường (Penetration Pricing) là gì?', N'Đặt giá ban đầu thấp để nhanh chóng chiếm lĩnh thị phần lớn', N'Đặt giá rất cao để thể hiện đẳng cấp sang trọng', N'Bán theo giá niêm yết của nhà nước', N'Tặng miễn phí mãi mãi', 'A', 'trung_binh', N'Penetration pricing thu hút khách hàng dùng thử nhờ mức giá hấp dẫn ban đầu.'),
    ('MKT101', 14, N'Chỉ số CAC (Customer Acquisition Cost) biểu thị điều gì?', N'Chi phí trung bình để có được một khách hàng mới', N'Giá bán lẻ của sản phẩm', N'Tiền lương của đội ngũ chăm sóc khách hàng', N'Thuế thu nhập doanh nghiệp', 'A', 'trung_binh', N'CAC = Tổng chi phí tiếp thị & bán hàng / Số khách hàng mới thu được.'),
    ('MKT101', 15, N'Tỷ lệ chuyển đổi (Conversion Rate) trong E-commerce được tính như thế nào?', N'(Số lượt mua hàng / Tổng số lượt truy cập) * 100%', N'Số sản phẩm bị trả lại', N'Tổng doanh thu chia cho số ngày trong tháng', N'Số lượng email gửi đi thành công', 'A', 'de', N'Conversion rate đo lường tỷ lệ khách truy cập chuyển hóa thành người mua hàng.');

    -- ENG101: Tiếng Anh căn bản 1 (15 câu)
    INSERT INTO @SampleQuestions VALUES
    ('ENG101', 1, N'Choose the correct form of the verb: "She ______ to school every morning."', N'goes', N'go', N'going', N'gone', 'A', 'de', N'Chủ ngữ ngôi thứ 3 số ít "She" ở thì hiện tại đơn đi với động từ "goes".'),
    ('ENG101', 2, N'What is the past tense of the verb "buy"?', N'bought', N'buyed', N'buying', N'buys', 'A', 'de', N'Buy là động từ bất quy tắc, quá khứ là bought.'),
    ('ENG101', 3, N'Choose the correct preposition: "I was born ______ July 15th."', N'on', N'in', N'at', N'for', 'A', 'de', N'Dùng giới từ "on" trước ngày cụ thể.'),
    ('ENG101', 4, N'Which word is a synonym of "happy"?', N'joyful', N'sad', N'angry', N'tired', 'A', 'de', N'Joyful có nghĩa là vui vẻ, đồng nghĩa với happy.'),
    ('ENG101', 5, N'Complete the sentence: "There are ______ apples on the table."', N'some', N'any', N'much', N'a', 'A', 'de', N'"some" dùng trong câu khẳng định với danh từ đếm được số nhiều.'),
    ('ENG101', 6, N'Choose the correct plural form of "child":', N'children', N'childs', N'childrens', N'childes', 'A', 'de', N'Danh từ số nhiều bất quy tắc của child là children.'),
    ('ENG101', 7, N'Complete the sentence: "He is very interested ______ learning foreign languages."', N'in', N'on', N'at', N'about', 'A', 'de', N'Cấu trúc cố định: be interested in something.'),
    ('ENG101', 8, N'Which sentence is grammatically correct?', N'They don''t like spicy food.', N'They doesn''t like spicy food.', N'They not like spicy food.', N'They no like spicy food.', 'A', 'de', N'Chủ ngữ "They" dùng trợ động từ phủ định "don''t".'),
    ('ENG101', 9, N'What is the opposite of the adjective "expensive"?', N'cheap', N'costly', N'valuable', N'precious', 'A', 'de', N'Trái nghĩa với expensive (đắt) là cheap (rẻ).'),
    ('ENG101', 10, N'Choose the correct pronoun: "Peter and Mary are doctors. ______ work in a hospital."', N'They', N'We', N'You', N'He', 'A', 'de', N'Đại từ nhân xưng thay thế cho Peter and Mary là They.'),
    ('ENG101', 11, N'Complete the question: "______ do you live?" - "In Hanoi."', N'Where', N'When', N'Who', N'Why', 'A', 'de', N'Hỏi về nơi chốn dùng từ để hỏi Where.'),
    ('ENG101', 12, N'Choose the correct comparative: "This smartphone is ______ than that one."', N'more expensive', N'expensiver', N'most expensive', 'as expensive', 'A', 'de', N'Tính từ dài expensive có dạng so sánh hơn là more expensive.'),
    ('ENG101', 13, N'Choose the correct modal verb: "You ______ wear a helmet when riding a motorbike."', N'must', N'may', N'might', N'could', 'A', 'de', N'Must diễn tả quy định bắt buộc.'),
    ('ENG101', 14, N'Complete the sentence: "If it rains tomorrow, we ______ stay at home."', N'will', N'would', N'had', N'did', 'A', 'de', N'Câu điều kiện loại 1: If + hiện tại đơn, tương lai đơn (will + V).'),
    ('ENG101', 15, N'What does the idiom "piece of cake" mean?', N'Very easy', N'Very delicious', N'Very expensive', N'Very difficult', 'A', 'de', N'"A piece of cake" là thành ngữ chỉ một việc rất dễ dàng.');

    -- Nạp câu hỏi vào bảng CauHoi
    DECLARE @QSubCode NVARCHAR(50), @QIdx INT, @QNoiDung NVARCHAR(500), @QOptA NVARCHAR(255), @QOptB NVARCHAR(255), @QOptC NVARCHAR(255), @QOptD NVARCHAR(255), @QDapAn NVARCHAR(10), @QDoKho NVARCHAR(20), @QGiaiThich NVARCHAR(500);
    DECLARE curQ CURSOR LOCAL FOR 
        SELECT subCode, idx, noiDung, optA, optB, optC, optD, dapAn, doKho, giaiThich 
        FROM @SampleQuestions;

    OPEN curQ;
    FETCH NEXT FROM curQ INTO @QSubCode, @QIdx, @QNoiDung, @QOptA, @QOptB, @QOptC, @QOptD, @QDapAn, @QDoKho, @QGiaiThich;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @QMaMon INT;
        SELECT @QMaMon = ma_mon_hoc FROM DanhMucMonHoc WHERE ma_code_mon_hoc = @QSubCode;

        IF @QMaMon IS NOT NULL
        BEGIN
            -- Tạo chuỗi JSON Choices & Đáp án
            DECLARE @CleanOptA NVARCHAR(500) = REPLACE(REPLACE(@QOptA, '\', '\\'), '"', '\"');
            DECLARE @CleanOptB NVARCHAR(500) = REPLACE(REPLACE(@QOptB, '\', '\\'), '"', '\"');
            DECLARE @CleanOptC NVARCHAR(500) = REPLACE(REPLACE(@QOptC, '\', '\\'), '"', '\"');
            DECLARE @CleanOptD NVARCHAR(500) = REPLACE(REPLACE(@QOptD, '\', '\\'), '"', '\"');
            DECLARE @QLuaChonJson NVARCHAR(MAX) = N'[{"key":"A","text":"' + @CleanOptA + N'"},{"key":"B","text":"' + @CleanOptB + N'"},{"key":"C","text":"' + @CleanOptC + N'"},{"key":"D","text":"' + @CleanOptD + N'"}]';
            DECLARE @QDapAnJson NVARCHAR(50) = N'["' + @QDapAn + N'"]';

            IF NOT EXISTS (SELECT 1 FROM CauHoi WHERE ma_mon_hoc = @QMaMon AND noi_dung = @QNoiDung)
            BEGIN
                INSERT INTO CauHoi (ma_mon_hoc, nguoi_tao, loai_cau_hoi, noi_dung, kieu_lua_chon, lua_chon, dap_an_dung, do_kho, giai_thich_dap_an, con_hoat_dong, ngay_tao)
                VALUES (@QMaMon, @AdminUserId, 'trac_nghiem', @QNoiDung, 'single', @QLuaChonJson, @QDapAnJson, @QDoKho, @QGiaiThich, 1, @CurrentDate);
            END
        END

        FETCH NEXT FROM curQ INTO @QSubCode, @QIdx, @QNoiDung, @QOptA, @QOptB, @QOptC, @QOptD, @QDapAn, @QDoKho, @QGiaiThich;
    END

    CLOSE curQ;
    DEALLOCATE curQ;

    PRINT N'  [OK] Đã hoàn tất nạp ngân hàng câu hỏi!';

    COMMIT TRANSACTION;

    -- Thống kê kết quả
    DECLARE @TotalSubjectsFinal INT, @TotalChuongFinal INT, @TotalBaiHocFinal INT, @TotalNoiDungFinal INT, @TotalCauHoiFinal INT;
    SELECT @TotalSubjectsFinal = COUNT(*) FROM DanhMucMonHoc;
    SELECT @TotalChuongFinal = COUNT(*) FROM Chuong;
    SELECT @TotalBaiHocFinal = COUNT(*) FROM BaiHoc;
    SELECT @TotalNoiDungFinal = COUNT(*) FROM BaiHocNoiDung;
    SELECT @TotalCauHoiFinal = COUNT(*) FROM CauHoi;

    PRINT N'======================================================================';
    PRINT N'--- BÁO CÁO KẾT QUẢ ĐỒNG BỘ CƠ SỞ DỮ LIỆU THÀNH CÔNG ---';
    PRINT N'- Tổng số môn học trong DanhMucMonHoc: ' + CAST(@TotalSubjectsFinal AS NVARCHAR);
    PRINT N'- Tổng số chương học trong Chuong: ' + CAST(@TotalChuongFinal AS NVARCHAR);
    PRINT N'- Tổng số bài học trong BaiHoc: ' + CAST(@TotalBaiHocFinal AS NVARCHAR);
    PRINT N'- Tổng số nội dung bài học trong BaiHocNoiDung: ' + CAST(@TotalNoiDungFinal AS NVARCHAR);
    PRINT N'- Tổng số câu hỏi ngân hàng trong CauHoi: ' + CAST(@TotalCauHoiFinal AS NVARCHAR);
    PRINT N'======================================================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT N'!!! LỖI KHI SEED DỮ LIỆU !!!';
    PRINT ERROR_MESSAGE();
END CATCH
GO
