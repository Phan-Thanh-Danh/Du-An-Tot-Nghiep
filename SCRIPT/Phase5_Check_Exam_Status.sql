USE LMS;
GO

SET NOCOUNT ON;

PRINT N'--- KIỂM TRA TRẠNG THÁI CA THI / ĐỀ THI / THÍ SINH ---';

-- 1. Kiểm tra nhanh ca test COM101-SE18.
SELECT
    c.ma_ca_thi,
    c.ghi_chu,
    c.trang_thai AS trang_thai_ca_thi,
    c.thoi_gian_bat_dau,
    c.thoi_gian_ket_thuc,
    c.ma_lich_thi_tong,
    l.trang_thai AS trang_thai_lich_thi,
    l.ma_de_kiem_tra,
    d.tieu_de AS ten_de_thi,
    d.trang_thai AS trang_thai_de_thi,
    d.trang_thai_duyet,
    c.ma_don_vi,
    c.ma_phong,
    CASE
        WHEN c.trang_thai = 'dang_thi' AND d.trang_thai = 'dang_mo'
            THEN N'OK: Ca và đề đang mở'
        WHEN c.trang_thai = 'dang_thi' AND d.trang_thai <> 'dang_mo'
            THEN N'LOI: Ca dang_thi nhưng đề chưa dang_mo'
        WHEN c.trang_thai <> 'dang_thi'
            THEN N'LOI: Ca chưa dang_thi'
        WHEN d.ma_de_kiem_tra IS NULL
            THEN N'LOI: Lịch thi chưa nối đề thi'
        ELSE N'KIEM TRA THEM'
    END AS ket_luan_trang_thai
FROM CaThi c
LEFT JOIN LichThiTong l ON l.ma_lich_thi_tong = c.ma_lich_thi_tong
LEFT JOIN DeKiemTra d ON d.ma_de_kiem_tra = l.ma_de_kiem_tra
WHERE c.ghi_chu = N'Phase5_COM101_SE18_TEST';

-- 2. Thống kê danh sách thí sinh và điểm danh của ca test.
SELECT
    c.ma_ca_thi,
    COUNT(t.ma_thi_sinh_ca_thi) AS tong_thi_sinh,
    SUM(CASE WHEN t.trang_thai_du_thi = 'cho_thi' THEN 1 ELSE 0 END) AS cho_thi,
    SUM(CASE WHEN t.trang_thai_du_thi = 'duoc_thi' THEN 1 ELSE 0 END) AS duoc_thi,
    SUM(CASE WHEN t.trang_thai_du_thi = 'vang_thi' THEN 1 ELSE 0 END) AS vang_thi,
    SUM(CASE WHEN t.trang_thai_du_thi = 'dinh_chi' THEN 1 ELSE 0 END) AS dinh_chi,
    COUNT(dd.ma_diem_danh_thi) AS tong_ban_ghi_diem_danh,
    SUM(CASE WHEN dd.trang_thai_diem_danh = 'co_mat' THEN 1 ELSE 0 END) AS diem_danh_co_mat,
    SUM(CASE WHEN dd.trang_thai_diem_danh = 'vang_mat' THEN 1 ELSE 0 END) AS diem_danh_vang_mat
FROM CaThi c
LEFT JOIN ThiSinhCaThi t ON t.ma_ca_thi = c.ma_ca_thi
LEFT JOIN DiemDanhThi dd ON dd.ma_ca_thi = t.ma_ca_thi AND dd.ma_hoc_sinh = t.ma_hoc_sinh
WHERE c.ghi_chu = N'Phase5_COM101_SE18_TEST'
GROUP BY c.ma_ca_thi;

-- 3. Chi tiết sinh viên đang bị khóa hoặc chưa được điểm danh.
SELECT
    t.ma_ca_thi,
    t.ma_hoc_sinh,
    u.email,
    u.ho_ten,
    t.trang_thai_du_thi,
    dd.trang_thai_diem_danh,
    CASE
        WHEN t.trang_thai_du_thi = 'duoc_thi' THEN N'Được thi'
        WHEN t.trang_thai_du_thi = 'cho_thi' THEN N'Bị khóa: chưa được điểm danh'
        WHEN t.trang_thai_du_thi = 'vang_thi' THEN N'Bị khóa: vắng mặt'
        WHEN t.trang_thai_du_thi = 'dinh_chi' THEN N'Bị khóa: đình chỉ'
        ELSE N'Bị khóa: trạng thái khác'
    END AS ket_luan_thi_sinh
FROM ThiSinhCaThi t
INNER JOIN CaThi c ON c.ma_ca_thi = t.ma_ca_thi
INNER JOIN NguoiDung u ON u.ma_nguoi_dung = t.ma_hoc_sinh
LEFT JOIN DiemDanhThi dd ON dd.ma_ca_thi = t.ma_ca_thi AND dd.ma_hoc_sinh = t.ma_hoc_sinh
WHERE c.ghi_chu = N'Phase5_COM101_SE18_TEST'
ORDER BY u.email;

-- 4. Giám thị được phân công và trạng thái phân công.
SELECT
    p.ma_ca_thi,
    p.ma_giam_thi,
    u.email,
    u.ho_ten,
    p.vai_tro_giam_thi,
    p.trang_thai AS trang_thai_phan_cong
FROM PhanCongGiamThi p
INNER JOIN CaThi c ON c.ma_ca_thi = p.ma_ca_thi
INNER JOIN NguoiDung u ON u.ma_nguoi_dung = p.ma_giam_thi
WHERE c.ghi_chu = N'Phase5_COM101_SE18_TEST';

-- 5. Kiểm tra mọi ca Phase 5 có ca mở nhưng đề chưa mở hay không.
SELECT
    c.ma_ca_thi,
    c.ghi_chu,
    c.trang_thai AS trang_thai_ca_thi,
    d.ma_de_kiem_tra,
    d.trang_thai AS trang_thai_de_thi,
    l.trang_thai AS trang_thai_lich_thi
FROM CaThi c
LEFT JOIN LichThiTong l ON l.ma_lich_thi_tong = c.ma_lich_thi_tong
LEFT JOIN DeKiemTra d ON d.ma_de_kiem_tra = l.ma_de_kiem_tra
WHERE c.ghi_chu LIKE N'Phase5_%'
  AND (c.trang_thai = 'dang_thi' OR d.trang_thai <> 'dang_mo' OR d.ma_de_kiem_tra IS NULL)
ORDER BY c.ma_ca_thi;

PRINT N'--- KỲ VỌNG ĐỂ SINH VIÊN VÀO THI ---';
PRINT N'CaThi.trang_thai = dang_thi';
PRINT N'DeKiemTra.trang_thai = dang_mo';
PRINT N'ThiSinhCaThi.trang_thai_du_thi = duoc_thi';
PRINT N'DiemDanhThi.trang_thai_diem_danh = co_mat';
GO
