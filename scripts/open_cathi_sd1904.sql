USE LMS;
GO
SET NOCOUNT ON;

-- ============================================================
-- Mở ca thi cho sinh viên lớp SD1904
-- Ca thi 9: Kỹ năng học tập (GEN101)
-- Ca thi 10: Nhập môn lập trình (COM101)
--
-- Nguyên nhân báo "Nội dung này sẽ mở ở Học kỳ 1 năm 2026 - Block 1":
--   ResolveStudentExamAccessStatus (Backend/Services/Exam/ExamService.cs:1816-1821)
--   trả 'future_locked' khi CaThi.TrangThai != 'dang_thi'.
--   StartExamAsync (ExamService.cs:1387) cũng chặn khi TrangThai != 'dang_thi'.
--   => Cần chuyển trang_thai sang 'dang_thi' và đảm bảo thời gian
--      ThoiGianBatDau <= now <= ThoiGianKetThuc.
-- ============================================================

-- Bước 1: Chuyển trạng thái + chỉnh thời gian cho ca thi Kỹ năng học tập (9)
UPDATE CaThi
SET trang_thai = 'dang_thi',
    ngay_thi = CAST(GETDATE() AS DATE),
    thoi_gian_bat_dau = DATEADD(HOUR, -1, GETDATE()),
    thoi_gian_ket_thuc = DATEADD(HOUR, 3, GETDATE())
WHERE ma_ca_thi = 9;

-- Bước 2: Chuyển trạng thái + chỉnh thời gian cho ca thi Nhập môn lập trình (10)
UPDATE CaThi
SET trang_thai = 'dang_thi',
    ngay_thi = CAST(GETDATE() AS DATE),
    thoi_gian_bat_dau = DATEADD(HOUR, -1, GETDATE()),
    thoi_gian_ket_thuc = DATEADD(HOUR, 3, GETDATE())
WHERE ma_ca_thi = 10;

-- Bước 3: Đồng bộ lịch thi tổng (LichThiTong) sang đang mở để khớp trạng thái
UPDATE l
SET l.trang_thai = 'da_gui_ve_co_so'
FROM LichThiTong l
JOIN CaThi c ON c.ma_lich_thi_tong = l.ma_lich_thi_tong
WHERE c.ma_ca_thi IN (9, 10) AND l.trang_thai <> 'da_gui_ve_co_so';

-- Kiểm tra kết quả
SELECT c.ma_ca_thi, c.ten_ca_thi, c.trang_thai,
       c.thoi_gian_bat_dau, c.thoi_gian_ket_thuc,
       m.ten_mon_hoc
FROM CaThi c
JOIN LichThiTong l ON l.ma_lich_thi_tong = c.ma_lich_thi_tong
JOIN DanhMucMonHoc m ON m.ma_mon_hoc = l.ma_mon_hoc
WHERE c.ma_ca_thi IN (9, 10);
GO
