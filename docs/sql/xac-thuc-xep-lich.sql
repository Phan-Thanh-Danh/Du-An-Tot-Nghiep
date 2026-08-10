-- ============================================================
-- P17 Smart Timetable GA — Xác thực bản nháp thuật toán di truyền
-- Database: LMS (SQL Server)
-- Ngày: 2026-08-07
-- Mô tả:
--   Chạy sau khi sinh bản nháp bằng GA (Smart Timetable → Tạo thông minh)
--   để kiểm tra tính toàn vẹn của ScheduleDraftItem:
--     + Không trùng giờ giữa giáo viên / lớp / phòng
--     + Số buổi/tuần khớp QuyDoiTinChi (SoTinChi → SoBuoiMoiTuan)
--     + Không vượt sức chứa phòng
--     + Thống kê phân bố slot theo thứ/ca
-- Cách dùng:
--   1. Thay {draftId} bên dưới bằng DraftId bản nháp muốn xác thực
--      (lấy từ response API /api/thoi-khoa-bieu/generate hoặc bảng
--      ScheduleGenerationJob).
--   2. Chạy từng khối SELECT trong SSMS.
--   Kỳ vọng: các truy vấn "cần bằng 0" trả về 0 dòng.
-- ============================================================

DECLARE @draftId UNIQUEIDENTIFIER = 'REPLACE_WITH_DRAFT_ID';
DECLARE @maJob INT = (SELECT TOP 1 ma_job FROM ScheduleGenerationJob WHERE draft_id = @draftId);

-- ============================================================
-- 0. Tổng quan bản nháp (job + GA params + kết quả)
-- ============================================================
SELECT
    ma_job,
    draft_id,
    ma_hoc_ky,
    ma_don_vi,
    trang_thai,
    tong_course       AS TongKhoaHoc,
    so_xep_duoc       AS XepDuoc,
    so_khong_xep_duoc AS KhongXepDuoc,
    score             AS DiemTB,
    tom_tat_json      AS TomTatGA,
    ngay_tao
FROM ScheduleGenerationJob
WHERE ma_job = @maJob;

-- ============================================================
-- 1. Xung đột giáo viên (cùng GV, cùng thứ + ca) — cần 0 dòng
-- ============================================================
SELECT
    a.thu_trong_tuan,
    a.ma_ca_hoc,
    k1.ma_giao_vien,
    COUNT(*) AS SoKhoaTrung
FROM ScheduleDraftItem a
JOIN KhoaHoc k1 ON k1.ma_khoa_hoc = a.ma_khoa_hoc
JOIN ScheduleDraftItem b ON b.ma_draft_item > a.ma_draft_item
     AND b.ma_ca_hoc = a.ma_ca_hoc
     AND b.thu_trong_tuan = a.thu_trong_tuan
JOIN KhoaHoc k2 ON k2.ma_khoa_hoc = b.ma_khoa_hoc
WHERE a.ma_job = @maJob
  AND a.trang_thai = 'xep_duoc'
  AND b.trang_thai = 'xep_duoc'
  AND k1.ma_giao_vien = k2.ma_giao_vien
GROUP BY a.thu_trong_tuan, a.ma_ca_hoc, k1.ma_giao_vien;

-- ============================================================
-- 2. Xung đột lớp hành chính (cùng lớp, cùng thứ + ca) — cần 0 dòng
-- ============================================================
SELECT
    a.thu_trong_tuan,
    a.ma_ca_hoc,
    k1.ma_lop,
    COUNT(*) AS SoKhoaTrung
FROM ScheduleDraftItem a
JOIN KhoaHoc k1 ON k1.ma_khoa_hoc = a.ma_khoa_hoc
JOIN ScheduleDraftItem b ON b.ma_draft_item > a.ma_draft_item
     AND b.ma_ca_hoc = a.ma_ca_hoc
     AND b.thu_trong_tuan = a.thu_trong_tuan
JOIN KhoaHoc k2 ON k2.ma_khoa_hoc = b.ma_khoa_hoc
WHERE a.ma_job = @maJob
  AND a.trang_thai = 'xep_duoc'
  AND b.trang_thai = 'xep_duoc'
  AND k1.ma_lop = k2.ma_lop
GROUP BY a.thu_trong_tuan, a.ma_ca_hoc, k1.ma_lop;

-- ============================================================
-- 3. Xung đột phòng học (cùng phòng, cùng thứ + ca) — cần 0 dòng
-- ============================================================
SELECT
    a.thu_trong_tuan,
    a.ma_ca_hoc,
    a.ma_phong,
    COUNT(*) AS SoKhoaTrung
FROM ScheduleDraftItem a
JOIN ScheduleDraftItem b ON b.ma_draft_item > a.ma_draft_item
     AND b.ma_ca_hoc = a.ma_ca_hoc
     AND b.thu_trong_tuan = a.thu_trong_tuan
     AND b.ma_phong = a.ma_phong
WHERE a.ma_job = @maJob
  AND a.trang_thai = 'xep_duoc'
  AND b.trang_thai = 'xep_duoc'
GROUP BY a.thu_trong_tuan, a.ma_ca_hoc, a.ma_phong;

-- ============================================================
-- 4. Cùng khóa học, cùng thứ + ca + phòng — cần 0 dòng
-- ============================================================
SELECT
    a.ma_khoa_hoc,
    a.thu_trong_tuan,
    a.ma_ca_hoc,
    a.ma_phong,
    COUNT(*) AS SoSlotTrung
FROM ScheduleDraftItem a
JOIN ScheduleDraftItem b ON b.ma_draft_item > a.ma_draft_item
     AND b.ma_khoa_hoc = a.ma_khoa_hoc
     AND b.ma_ca_hoc = a.ma_ca_hoc
     AND b.thu_trong_tuan = a.thu_trong_tuan
WHERE a.ma_job = @maJob
  AND a.trang_thai = 'xep_duoc'
  AND b.trang_thai = 'xep_duoc'
GROUP BY a.ma_khoa_hoc, a.thu_trong_tuan, a.ma_ca_hoc, a.ma_phong;

-- ============================================================
-- 5. Số buổi/tuần mỗi khóa khớp QuyDoiTinChi
--    (SoTinChi → SoBuoiMoiTuan) — trả về các khóa LỆCH kỳ vọng
-- ============================================================
SELECT
    k.ma_khoa_hoc,
    dm.so_tin_chi,
    qd.so_buoi_moi_tuan AS KyVong,
    COUNT(i.ma_draft_item) AS ThucTe,
    CASE WHEN COUNT(i.ma_draft_item) = qd.so_buoi_moi_tuan THEN N'OK' ELSE N'LỆCH' END AS TrangThai
FROM KhoaHoc k
JOIN DanhMucMonHoc dm ON dm.ma_mon_hoc = k.ma_mon_hoc
JOIN QuyDoiTinChi qd ON qd.so_tin_chi = dm.so_tin_chi
LEFT JOIN ScheduleDraftItem i
    ON i.ma_khoa_hoc = k.ma_khoa_hoc
   AND i.ma_job = @maJob
   AND i.trang_thai = 'xep_duoc'
WHERE k.ma_hoc_ky = (SELECT TOP 1 ma_hoc_ky FROM ScheduleGenerationJob WHERE ma_job = @maJob)
  AND k.ma_don_vi = (SELECT TOP 1 ma_don_vi FROM ScheduleGenerationJob WHERE ma_job = @maJob)
GROUP BY k.ma_khoa_hoc, dm.so_tin_chi, qd.so_buoi_moi_tuan
HAVING COUNT(i.ma_draft_item) <> qd.so_buoi_moi_tuan;

-- ============================================================
-- 6. Sức chứa phòng vs sĩ số ước tính lớp — cần 0 dòng
-- ============================================================
SELECT
    i.ma_draft_item,
    k.ma_khoa_hoc,
    k.ma_lop,
    i.ma_phong,
    p.suc_chua,
    COUNT(nd.ma_nguoi_dung) AS SiSoUocTinh,
    N'VƯỢT SỨC CHỨA' AS TrangThai
FROM ScheduleDraftItem i
JOIN KhoaHoc k ON k.ma_khoa_hoc = i.ma_khoa_hoc
JOIN PhongHoc p ON p.ma_phong = i.ma_phong
LEFT JOIN NguoiDung nd ON nd.ma_lop = k.ma_lop AND nd.vai_tro_chinh = 'hoc_sinh'
WHERE i.ma_job = @maJob
  AND i.trang_thai = 'xep_duoc'
GROUP BY i.ma_draft_item, k.ma_khoa_hoc, k.ma_lop, i.ma_phong, p.suc_chua
HAVING COUNT(nd.ma_nguoi_dung) > p.suc_chua;

-- ============================================================
-- 7. Phân bố slot theo (thứ, ca) — tham khảo tính trải lịch
-- ============================================================
SELECT
    thu_trong_tuan AS Thu,
    ma_ca_hoc      AS Ca,
    COUNT(*)       AS SoSlot
FROM ScheduleDraftItem
WHERE ma_job = @maJob
  AND trang_thai = 'xep_duoc'
GROUP BY thu_trong_tuan, ma_ca_hoc
ORDER BY thu_trong_tuan, ma_ca_hoc;

-- ============================================================
-- 8. Thống kê trùng thứ trong tuần mỗi khóa
--    (càng ít dòng → phân bố ngày càng tốt; 0 = hoàn hảo)
-- ============================================================
SELECT
    ma_khoa_hoc,
    thu_trong_tuan,
    COUNT(*) AS SoSlotCungNgay
FROM ScheduleDraftItem
WHERE ma_job = @maJob
  AND trang_thai = 'xep_duoc'
GROUP BY ma_khoa_hoc, thu_trong_tuan
HAVING COUNT(*) > 1;

-- ============================================================
-- 9. Khóa không xếp được — danh sách + lý do
-- ============================================================
SELECT
    i.ma_khoa_hoc,
    k.tieu_de,
    i.loi_json AS Loi,
    i.trang_thai
FROM ScheduleDraftItem i
JOIN KhoaHoc k ON k.ma_khoa_hoc = i.ma_khoa_hoc
WHERE i.ma_job = @maJob
  AND i.trang_thai = 'khong_xep_duoc';
