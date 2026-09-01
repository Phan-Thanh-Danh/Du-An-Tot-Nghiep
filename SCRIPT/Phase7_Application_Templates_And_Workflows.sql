USE LMS;
GO

SET NOCOUNT ON;
DECLARE @CurrentDate DATETIME2 = SYSUTCDATETIME();

PRINT N'--- BẮT ĐẦU CÀI ĐẶT MẪU ĐƠN TỪ & QUY TRÌNH DUYỆT (PHASE 7) ---';

BEGIN TRY
    BEGIN TRANSACTION;

    -- =========================================================================
    -- PHẦN 1: TẠO 11 MẪU ĐƠN TỪ CHUẨN TRONG MauDonTu (snake_case columns)
    -- =========================================================================
    PRINT N'- Đang khởi tạo 11 Mẫu đơn từ chuẩn trong MauDonTu...';

    -- 1. Đơn xin bảo lưu kết quả học tập (bao_luu)
    IF NOT EXISTS (SELECT 1 FROM MauDonTu WHERE loai_don = 'bao_luu')
    BEGIN
        INSERT INTO MauDonTu (loai_don, ten_mau, phien_ban, bat_buoc_minh_chung, so_tep_toi_da, dung_luong_tep_toi_da_byte, tong_dung_luong_toi_da_byte, sla_gio, dang_hoat_dong, ngay_tao, ngay_cap_nhat, cau_hinh_json)
        VALUES (
            'bao_luu',
            N'Đơn xin bảo lưu kết quả học tập',
            1,
            0,
            2,
            5242880,
            10485760,
            72,
            1,
            @CurrentDate,
            @CurrentDate,
            N'{"fields":[{"key":"student_info","type":"studentInfo","label":"Thông tin sinh viên","readonly":true},{"key":"hoc_ky_bao_luu","type":"select","label":"Học kỳ xin bảo lưu","required":true,"autoFill":"studentSemesters","options":[{"value":"auto","label":"Đang tải danh sách học kỳ..."}]},{"key":"reason_type","type":"select","label":"Lý do bảo lưu chính","required":true,"options":[{"value":"Sức khỏe","label":"Vấn đề sức khỏe điều trị dài hạn"},{"value":"Gia đình","label":"Hoàn cảnh gia đình đặc biệt"},{"value":"Nghĩa vụ quân sự","label":"Thực hiện nghĩa vụ quân sự"},{"value":"Tài chính","label":"Lý do kinh tế cá nhân"},{"value":"Khác","label":"Lý do cá nhân khác"}]},{"key":"thoi_luong_du_kien","type":"number","label":"Thời gian bảo lưu (tháng)","required":true,"min":1,"max":12},{"key":"reason_detail","type":"textarea","label":"Chi tiết lý do và nguyện vọng","required":true,"maxLength":2000},{"key":"dia_chi_lien_he","type":"text","label":"Địa chỉ tạm trú liên hệ","required":true,"maxLength":200},{"key":"so_dien_thoai","type":"tel","label":"Số điện thoại liên hệ","required":true},{"key":"email_lien_he","type":"email","label":"Email sinh viên","required":true,"autoFill":"studentEmail"}]}'
        );
    END

    -- 2. Đơn xin chuyển ngành / chuyên ngành (chuyen_nganh)
    IF NOT EXISTS (SELECT 1 FROM MauDonTu WHERE loai_don = 'chuyen_nganh')
    BEGIN
        INSERT INTO MauDonTu (loai_don, ten_mau, phien_ban, bat_buoc_minh_chung, so_tep_toi_da, dung_luong_tep_toi_da_byte, tong_dung_luong_toi_da_byte, sla_gio, dang_hoat_dong, ngay_tao, ngay_cap_nhat, cau_hinh_json)
        VALUES (
            'chuyen_nganh',
            N'Đơn xin chuyển ngành / chuyên ngành',
            1,
            0,
            2,
            5242880,
            10485760,
            72,
            1,
            @CurrentDate,
            @CurrentDate,
            N'{"fields":[{"key":"student_info","type":"studentInfo","label":"Thông tin sinh viên","readonly":true},{"key":"target_major_id","type":"select","label":"Ngành đào tạo muốn chuyển đến","required":true,"autoFill":"majors"},{"key":"target_specialization_id","type":"select","label":"Chuyên ngành muốn chuyển đến","required":true,"autoFill":"specializationsByMajor","dependsOn":"target_major_id"},{"key":"reason","type":"textarea","label":"Lý do xin chuyển ngành","required":true,"maxLength":1000},{"key":"gpa_hien_tai","type":"number","label":"GPA tích lũy hiện tại","required":false,"readonly":true},{"key":"so_dien_thoai","type":"tel","label":"Số điện thoại","required":true},{"key":"email_lien_he","type":"email","label":"Email","required":true,"autoFill":"studentEmail"}]}'
        );
    END

    -- 3. Đơn xin chuyển cơ sở đào tạo (chuyen_co_so)
    IF NOT EXISTS (SELECT 1 FROM MauDonTu WHERE loai_don = 'chuyen_co_so')
    BEGIN
        INSERT INTO MauDonTu (loai_don, ten_mau, phien_ban, bat_buoc_minh_chung, so_tep_toi_da, dung_luong_tep_toi_da_byte, tong_dung_luong_toi_da_byte, sla_gio, dang_hoat_dong, ngay_tao, ngay_cap_nhat, cau_hinh_json)
        VALUES (
            'chuyen_co_so',
            N'Đơn xin chuyển cơ sở đào tạo',
            1,
            0,
            2,
            5242880,
            10485760,
            72,
            1,
            @CurrentDate,
            @CurrentDate,
            N'{"fields":[{"key":"student_info","type":"studentInfo","label":"Thông tin sinh viên","readonly":true},{"key":"ma_don_vi_mong_muon","type":"select","label":"Cơ sở muốn chuyển đến","required":true,"autoFill":"campuses"},{"key":"ma_hoc_ky","type":"select","label":"Học kỳ bắt đầu chuyển","required":true,"autoFill":"availableSemesters"},{"key":"ly_do","type":"textarea","label":"Lý do chuyển cơ sở","required":true,"maxLength":1000},{"key":"dia_chi_moi","type":"text","label":"Địa chỉ cư trú tại cơ sở mới","required":true,"maxLength":200},{"key":"so_dien_thoai","type":"tel","label":"Số điện thoại","required":true},{"key":"email_lien_he","type":"email","label":"Email","required":true,"autoFill":"studentEmail"}]}'
        );
    END

    -- 4. Đơn xin nghỉ phép tạm thời (nghi_phep)
    IF NOT EXISTS (SELECT 1 FROM MauDonTu WHERE loai_don = 'nghi_phep')
    BEGIN
        INSERT INTO MauDonTu (loai_don, ten_mau, phien_ban, bat_buoc_minh_chung, so_tep_toi_da, dung_luong_tep_toi_da_byte, tong_dung_luong_toi_da_byte, sla_gio, dang_hoat_dong, ngay_tao, ngay_cap_nhat, cau_hinh_json)
        VALUES (
            'nghi_phep',
            N'Đơn xin nghỉ phép tạm thời',
            1,
            1,
            3,
            5242880,
            15728640,
            24,
            1,
            @CurrentDate,
            @CurrentDate,
            N'{"fields":[{"key":"student_info","type":"studentInfo","label":"Thông tin sinh viên","readonly":true},{"key":"from_date","type":"date","label":"Nghỉ từ ngày","required":true},{"key":"to_date","type":"date","label":"Đến ngày","required":true},{"key":"reason","type":"textarea","label":"Lý do xin nghỉ học","required":true,"maxLength":1000},{"key":"mon_hoc_anh_huong","type":"text","label":"Các môn học/buổi học bị ảnh hưởng","required":false},{"key":"contact_address","type":"text","label":"Địa chỉ liên hệ","required":true},{"key":"phone","type":"tel","label":"Số điện thoại","required":true}]}'
        );
    END

    -- 5. Đơn xin phúc tra / phúc khảo điểm (phuc_tra_diem)
    IF NOT EXISTS (SELECT 1 FROM MauDonTu WHERE loai_don = 'phuc_tra_diem')
    BEGIN
        INSERT INTO MauDonTu (loai_don, ten_mau, phien_ban, bat_buoc_minh_chung, so_tep_toi_da, dung_luong_tep_toi_da_byte, tong_dung_luong_toi_da_byte, sla_gio, dang_hoat_dong, ngay_tao, ngay_cap_nhat, cau_hinh_json)
        VALUES (
            'phuc_tra_diem',
            N'Đơn xin phúc khảo điểm số',
            1,
            0,
            2,
            5242880,
            10485760,
            72,
            1,
            @CurrentDate,
            @CurrentDate,
            N'{"fields":[{"key":"student_info","type":"studentInfo","label":"Thông tin sinh viên","readonly":true},{"key":"ma_diem_so","type":"select","label":"Môn học cần phúc khảo","required":true,"autoFill":"availableRegradeScores"},{"key":"cot_diem","type":"select","label":"Đầu điểm cần phúc khảo","required":true,"options":[{"value":"diem_qua_trinh","label":"Điểm quá trình / Assignment"},{"value":"diem_giua_ky","label":"Điểm kiểm tra giữa kỳ"},{"value":"diem_cuoi_ky","label":"Điểm thi cuối kỳ (Final Exam)"},{"value":"gpa_mon_hoc","label":"Điểm tổng kết môn"}]},{"key":"diem_hien_tai","type":"number","label":"Điểm số hiện tại","readonly":true},{"key":"diem_mong_muon","type":"number","label":"Điểm tự đánh giá / mong muốn","required":false},{"key":"ly_do","type":"textarea","label":"Lý do xin phúc khảo bài làm","required":true,"maxLength":1000},{"key":"so_dien_thoai","type":"tel","label":"Số điện thoại liên hệ","required":true}]}'
        );
    END

    -- 6. Đơn xin cấp giấy xác nhận sinh viên (xac_nhan)
    IF NOT EXISTS (SELECT 1 FROM MauDonTu WHERE loai_don = 'xac_nhan')
    BEGIN
        INSERT INTO MauDonTu (loai_don, ten_mau, phien_ban, bat_buoc_minh_chung, so_tep_toi_da, dung_luong_tep_toi_da_byte, tong_dung_luong_toi_da_byte, sla_gio, dang_hoat_dong, ngay_tao, ngay_cap_nhat, cau_hinh_json)
        VALUES (
            'xac_nhan',
            N'Đơn xin cấp giấy xác nhận sinh viên',
            1,
            0,
            1,
            5242880,
            5242880,
            48,
            1,
            @CurrentDate,
            @CurrentDate,
            N'{"fields":[{"key":"student_info","type":"studentInfo","label":"Thông tin sinh viên","readonly":true},{"key":"confirmation_type","type":"select","label":"Mục đích cấp giấy xác nhận","required":true,"options":[{"value":"Xác nhận vay vốn sinh viên","label":"Vay vốn tín dụng học tập"},{"value":"Tạm hoãn nghĩa vụ quân sự","label":"Tạm hoãn nghĩa vụ quân sự"},{"value":"Làm thủ tục xin việc / thực tập","label":"Xin việc / Thực tập doanh nghiệp"},{"value":"Làm vé xe buýt / ưu đãi","label":"Làm vé tháng xe buýt / Ưu đãi"},{"value":"Xác nhận hưởng chính sách xã hội","label":"Hưởng chế độ chính sách / Miễn giảm"},{"value":"Khác","label":"Mục đích khác"}]},{"key":"copies","type":"number","label":"Số lượng bản cần cấp","required":true,"min":1,"max":5},{"key":"recipient","type":"text","label":"Cơ quan / Đơn vị nơi nhận giấy","required":true,"maxLength":200},{"key":"purpose_detail","type":"textarea","label":"Ghi chú chi tiết thêm (nếu có)","required":false,"maxLength":500},{"key":"so_dien_thoai","type":"tel","label":"Số điện thoại nhận thông báo","required":true}]}'
        );
    END

    -- 7. Đơn đăng ký thi lại / cải thiện điểm (thi_lai)
    IF NOT EXISTS (SELECT 1 FROM MauDonTu WHERE loai_don = 'thi_lai')
    BEGIN
        INSERT INTO MauDonTu (loai_don, ten_mau, phien_ban, bat_buoc_minh_chung, so_tep_toi_da, dung_luong_tep_toi_da_byte, tong_dung_luong_toi_da_byte, sla_gio, dang_hoat_dong, ngay_tao, ngay_cap_nhat, cau_hinh_json)
        VALUES (
            'thi_lai',
            N'Đơn đăng ký thi lại / cải thiện điểm',
            1,
            0,
            1,
            5242880,
            5242880,
            72,
            1,
            @CurrentDate,
            @CurrentDate,
            N'{"fields":[{"key":"student_info","type":"studentInfo","label":"Thông tin sinh viên","readonly":true},{"key":"course_id","type":"select","label":"Môn học đăng ký thi lại","required":true,"autoFill":"availableRetakeSubjects"},{"key":"exam_session_id","type":"select","label":"Đợt thi / Ca thi mong muốn","required":true,"autoFill":"availableExamSessions","dependsOn":"course_id"},{"key":"reason","type":"textarea","label":"Lý do đăng ký thi lại","required":false,"maxLength":500},{"key":"phone","type":"tel","label":"Số điện thoại","required":true},{"key":"email","type":"email","label":"Email liên hệ","required":true,"autoFill":"studentEmail"}]}'
        );
    END

    -- 8. Đơn xin thôi học & rút học bạ (rut_hoc)
    IF NOT EXISTS (SELECT 1 FROM MauDonTu WHERE loai_don = 'rut_hoc')
    BEGIN
        INSERT INTO MauDonTu (loai_don, ten_mau, phien_ban, bat_buoc_minh_chung, so_tep_toi_da, dung_luong_tep_toi_da_byte, tong_dung_luong_toi_da_byte, sla_gio, dang_hoat_dong, ngay_tao, ngay_cap_nhat, cau_hinh_json)
        VALUES (
            'rut_hoc',
            N'Đơn xin thôi học & rút học bạ',
            1,
            0,
            3,
            5242880,
            15728640,
            72,
            1,
            @CurrentDate,
            @CurrentDate,
            N'{"fields":[{"key":"student_info","type":"studentInfo","label":"Thông tin sinh viên","readonly":true},{"key":"reason","type":"textarea","label":"Lý do xin thôi học","required":true,"maxLength":1000},{"key":"documents","type":"select","label":"Hồ sơ đề nghị nhận lại","required":true,"options":[{"value":"Học bạ gốc THPT","label":"Học bạ gốc THPT"},{"value":"Bằng tốt nghiệp THPT","label":"Bằng tốt nghiệp THPT"},{"value":"Giấy chứng nhận tốt nghiệp tạm thời","label":"Giấy chứng nhận tốt nghiệp tạm thời"},{"value":"Toàn bộ hồ sơ nhập học","label":"Toàn bộ hồ sơ nhập học"}]},{"key":"receiver_type","type":"select","label":"Hình thức nhận hồ sơ","required":true,"options":[{"value":"Sinh viên trực tiếp nhận","label":"Sinh viên trực tiếp nhận tại trường"},{"value":"Người thân nhận thay (Ủy quyền)","label":"Người thân nhận thay (kèm giấy ủy quyền)"}]},{"key":"contact_address","type":"text","label":"Địa chỉ nhận thư / liên lạc","required":true},{"key":"contact_phone","type":"tel","label":"Số điện thoại","required":true}]}'
        );
    END

    -- 9. Đơn xin cấp chứng chỉ / bảng điểm tốt nghiệp (cap_chung_chi)
    IF NOT EXISTS (SELECT 1 FROM MauDonTu WHERE loai_don = 'cap_chung_chi')
    BEGIN
        INSERT INTO MauDonTu (loai_don, ten_mau, phien_ban, bat_buoc_minh_chung, so_tep_toi_da, dung_luong_tep_toi_da_byte, tong_dung_luong_toi_da_byte, sla_gio, dang_hoat_dong, ngay_tao, ngay_cap_nhat, cau_hinh_json)
        VALUES (
            'cap_chung_chi',
            N'Đơn xin cấp chứng chỉ & bảng điểm',
            1,
            0,
            1,
            5242880,
            5242880,
            48,
            1,
            @CurrentDate,
            @CurrentDate,
            N'{"fields":[{"key":"student_info","type":"studentInfo","label":"Thông tin sinh viên","readonly":true},{"key":"cert_type","type":"select","label":"Loại chứng chỉ / giấy tờ đề nghị cấp","required":true,"options":[{"value":"Bảng điểm tích lũy tạm thời","label":"Bảng điểm tích lũy học tập"},{"value":"Chứng chỉ chuẩn đầu ra Tiếng Anh","label":"Chứng chỉ chuẩn đầu ra Tiếng Anh"},{"value":"Chứng chỉ Tin học văn phòng","label":"Chứng chỉ Tin học văn phòng"},{"value":"Giấy chứng nhận hoàn thành CTĐT","label":"Giấy chứng nhận hoàn thành khóa học"}]},{"key":"copies","type":"number","label":"Số lượng bản in","required":true,"min":1,"max":5},{"key":"language","type":"select","label":"Ngôn ngữ bản in","required":true,"options":[{"value":"Tiếng Việt","label":"Bản tiếng Việt"},{"value":"Song ngữ Anh - Việt","label":"Bản Song ngữ Anh - Việt"}]},{"key":"recipient","type":"text","label":"Nơi gửi đến / Mục đích nộp","required":true},{"key":"phone","type":"tel","label":"Số điện thoại","required":true}]}'
        );
    END

    -- 10. Đơn xin chuyển trường (chuyen_truong)
    IF NOT EXISTS (SELECT 1 FROM MauDonTu WHERE loai_don = 'chuyen_truong')
    BEGIN
        INSERT INTO MauDonTu (loai_don, ten_mau, phien_ban, bat_buoc_minh_chung, so_tep_toi_da, dung_luong_tep_toi_da_byte, tong_dung_luong_toi_da_byte, sla_gio, dang_hoat_dong, ngay_tao, ngay_cap_nhat, cau_hinh_json)
        VALUES (
            'chuyen_truong',
            N'Đơn xin chuyển trường đào tạo',
            1,
            1,
            3,
            5242880,
            15728640,
            96,
            1,
            @CurrentDate,
            @CurrentDate,
            N'{"fields":[{"key":"student_info","type":"studentInfo","label":"Thông tin sinh viên","readonly":true},{"key":"target_university","type":"text","label":"Tên trường đại học chuyển đến","required":true,"maxLength":200},{"key":"target_major","type":"text","label":"Ngành học tại trường mới","required":true,"maxLength":150},{"key":"reason","type":"textarea","label":"Lý do xin chuyển trường","required":true,"maxLength":1000},{"key":"contact_address","type":"text","label":"Địa chỉ liên hệ","required":true},{"key":"phone","type":"tel","label":"Số điện thoại","required":true}]}'
        );
    END

    -- 11. Đơn xin hỗ trợ thủ tục học vụ khác (khac)
    IF NOT EXISTS (SELECT 1 FROM MauDonTu WHERE loai_don = 'khac')
    BEGIN
        INSERT INTO MauDonTu (loai_don, ten_mau, phien_ban, bat_buoc_minh_chung, so_tep_toi_da, dung_luong_tep_toi_da_byte, tong_dung_luong_toi_da_byte, sla_gio, dang_hoat_dong, ngay_tao, ngay_cap_nhat, cau_hinh_json)
        VALUES (
            'khac',
            N'Đơn đề nghị hỗ trợ học vụ khác',
            1,
            0,
            3,
            5242880,
            15728640,
            48,
            1,
            @CurrentDate,
            @CurrentDate,
            N'{"fields":[{"key":"student_info","type":"studentInfo","label":"Thông tin sinh viên","readonly":true},{"key":"title","type":"text","label":"Tiêu đề nội dung kiến nghị","required":true,"maxLength":200},{"key":"content","type":"textarea","label":"Nội dung trình bày chi tiết","required":true,"maxLength":2000},{"key":"propose","type":"textarea","label":"Đề xuất giải quyết nguyện vọng","required":false,"maxLength":1000},{"key":"phone","type":"tel","label":"Số điện thoại","required":true},{"key":"email","type":"email","label":"Email","required":true,"autoFill":"studentEmail"}]}'
        );
    END

    -- =========================================================================
    -- PHẦN 2: THIẾT LẬP QUY TRÌNH DUYỆT ĐA BƯỚC (QuyTrinhDonTu & BuocQuyTrinh)
    -- Chú ý: Bảng QuyTrinhDonTu & BuocQuyTrinh dùng cột PascalCase (MaQuyTrinh, LoaiDon, TenQuyTrinh...)
    -- =========================================================================
    PRINT N'- Đang thiết lập Quy trình duyệt đa bước trong QuyTrinhDonTu & BuocQuyTrinh...';

    DECLARE @AppTypes TABLE (loaiDon NVARCHAR(50), tenQuyTrinh NVARCHAR(100), sla NVARCHAR(50));
    INSERT INTO @AppTypes VALUES
        ('bao_luu', N'Quy trình xét duyệt bảo lưu kết quả học tập', '72h'),
        ('chuyen_nganh', N'Quy trình xét duyệt chuyển ngành học', '72h'),
        ('chuyen_co_so', N'Quy trình xét duyệt chuyển cơ sở đào tạo', '72h'),
        ('nghi_phep', N'Quy trình duyệt đơn nghỉ phép sinh viên', '24h'),
        ('phuc_tra_diem', N'Quy trình xử lý phúc khảo điểm thi', '72h'),
        ('xac_nhan', N'Quy trình cấp giấy xác nhận sinh viên', '48h'),
        ('thi_lai', N'Quy trình đăng ký thi lại & xếp ca thi', '72h'),
        ('rut_hoc', N'Quy trình giải quyết thôi học & trả hồ sơ', '72h'),
        ('cap_chung_chi', N'Quy trình cấp bảng điểm & chứng chỉ', '48h'),
        ('chuyen_truong', N'Quy trình chuyển trường & bàn giao sinh viên', '96h'),
        ('khac', N'Quy trình tiếp nhận & xử lý đơn học vụ khác', '48h');

    DECLARE @curLoaiDon NVARCHAR(50), @curTenQT NVARCHAR(100), @curSla NVARCHAR(50);
    DECLARE @qtCursor CURSOR;

    SET @qtCursor = CURSOR FOR
    SELECT loaiDon, tenQuyTrinh, sla FROM @AppTypes;

    OPEN @qtCursor;
    FETCH NEXT FROM @qtCursor INTO @curLoaiDon, @curTenQT, @curSla;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @MaQT INT = NULL;
        SELECT @MaQT = MaQuyTrinh FROM QuyTrinhDonTu WHERE LoaiDon = @curLoaiDon;

        IF @MaQT IS NULL
        BEGIN
            INSERT INTO QuyTrinhDonTu (LoaiDon, TenQuyTrinh, IsActive, SlaKhoangThoiGian)
            VALUES (@curLoaiDon, @curTenQT, 1, @curSla);
            SET @MaQT = SCOPE_IDENTITY();
        END

        IF @MaQT IS NOT NULL
        BEGIN
            -- Bước 1: Giáo vụ thẩm định hồ sơ
            IF NOT EXISTS (SELECT 1 FROM BuocQuyTrinh WHERE MaQuyTrinh = @MaQT AND ThuTu = 1)
            BEGIN
                INSERT INTO BuocQuyTrinh (MaQuyTrinh, ThuTu, TenBuoc, VaiTroXuLy, KieuBuoc, SlaKhoangThoiGian)
                VALUES (@MaQT, 1, N'Giáo vụ tiếp nhận & Thẩm định hồ sơ', 'giao_vu', 'phe_duyet', '24h');
            END

            -- Bước 2: Ban Giám Hiệu phê duyệt quyết định
            IF NOT EXISTS (SELECT 1 FROM BuocQuyTrinh WHERE MaQuyTrinh = @MaQT AND ThuTu = 2)
            BEGIN
                INSERT INTO BuocQuyTrinh (MaQuyTrinh, ThuTu, TenBuoc, VaiTroXuLy, KieuBuoc, SlaKhoangThoiGian)
                VALUES (@MaQT, 2, N'Ban Giám Hiệu xét duyệt quyết định', 'hieu_truong', 'phe_duyet', '48h');
            END

            -- Bước 3: Giáo vụ thực hiện và trả kết quả
            IF NOT EXISTS (SELECT 1 FROM BuocQuyTrinh WHERE MaQuyTrinh = @MaQT AND ThuTu = 3)
            BEGIN
                INSERT INTO BuocQuyTrinh (MaQuyTrinh, ThuTu, TenBuoc, VaiTroXuLy, KieuBuoc, SlaKhoangThoiGian)
                VALUES (@MaQT, 3, N'Hoàn tất thủ tục & Thông báo kết quả', 'giao_vu', 'thong_bao', '12h');
            END
        END

        FETCH NEXT FROM @qtCursor INTO @curLoaiDon, @curTenQT, @curSla;
    END

    CLOSE @qtCursor;
    DEALLOCATE @qtCursor;

    COMMIT TRANSACTION;
    PRINT N'=== HOÀN TẤT THIẾT LẬP 11 MẪU ĐƠN TỪ & QUY TRÌNH DUYỆT THÀNH CÔNG 100% ===';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    PRINT N'LỖI: ' + @ErrMsg;
END CATCH;
GO
