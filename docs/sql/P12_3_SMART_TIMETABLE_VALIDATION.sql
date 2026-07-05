-- ============================================================
-- P12.3 Smart Timetable — SQL Validation Queries
-- Branch: feature/p12-3-smart-timetable-stress-smoke
-- Date: 2026-07-05
-- Database: LMS_TEST_P12
-- Description:
--   These queries validate that the Smart Timetable Engine
--   (OccupationMap + SmartTimetableService) produces correct
--   output with no conflicts, capacity violations, or
--   inactive-resource assignments.
--
-- Usage:
--   Run after each Publish operation to verify integrity.
--   Expect 0 rows returned for all queries.
-- ============================================================

-- ============================================================
-- Query 1: Weekly teacher conflict (ThoiKhoaBieu level)
-- Finds teachers assigned to 2+ different courses on the same
-- (MaHocKy, ThuTrongTuan, MaCaHoc) combination.
-- ============================================================
SELECT
    kh.MaHocKy,
    tkb.ThuTrongTuan,
    tkb.MaCaHoc,
    kh.MaGiaoVien,
    COUNT(*) AS SoLich,
    STRING_AGG(CONCAT('TKB#', tkb.MaTkb, ':', kh.TieuDe), '; ') AS DanhSach
FROM ThoiKhoaBieu tkb
JOIN KhoaHoc kh ON kh.MaKhoaHoc = tkb.MaKhoaHoc
WHERE tkb.TrangThai <> 'da_huy'
GROUP BY kh.MaHocKy, tkb.ThuTrongTuan, tkb.MaCaHoc, kh.MaGiaoVien
HAVING COUNT(*) > 1
ORDER BY SoLich DESC;

-- ============================================================
-- Query 2: Weekly class conflict (ThoiKhoaBieu level)
-- Finds classes assigned to 2+ different courses on the same
-- (MaHocKy, ThuTrongTuan, MaCaHoc).
-- ============================================================
SELECT
    kh.MaHocKy,
    tkb.ThuTrongTuan,
    tkb.MaCaHoc,
    kh.MaLop,
    COUNT(*) AS SoLich,
    STRING_AGG(CONCAT('TKB#', tkb.MaTkb, ':', kh.TieuDe), '; ') AS DanhSach
FROM ThoiKhoaBieu tkb
JOIN KhoaHoc kh ON kh.MaKhoaHoc = tkb.MaKhoaHoc
WHERE tkb.TrangThai <> 'da_huy'
GROUP BY kh.MaHocKy, tkb.ThuTrongTuan, tkb.MaCaHoc, kh.MaLop
HAVING COUNT(*) > 1
ORDER BY SoLich DESC;

-- ============================================================
-- Query 3: Weekly room conflict (ThoiKhoaBieu level)
-- Finds rooms assigned to 2+ different courses on the same
-- (MaHocKy, ThuTrongTuan, MaCaHoc).
-- ============================================================
SELECT
    kh.MaHocKy,
    tkb.ThuTrongTuan,
    tkb.MaCaHoc,
    tkb.MaPhong,
    COUNT(*) AS SoLich,
    STRING_AGG(CONCAT('TKB#', tkb.MaTkb, ':', kh.TieuDe), '; ') AS DanhSach
FROM ThoiKhoaBieu tkb
JOIN KhoaHoc kh ON kh.MaKhoaHoc = tkb.MaKhoaHoc
WHERE tkb.TrangThai <> 'da_huy'
GROUP BY kh.MaHocKy, tkb.ThuTrongTuan, tkb.MaCaHoc, tkb.MaPhong
HAVING COUNT(*) > 1
ORDER BY SoLich DESC;

-- ============================================================
-- Query 4: Session — effective teacher conflict (BuoiHoc level)
-- Finds teachers (including substitutes) assigned to 2+
-- sessions on the same (NgayHoc, MaCaHoc).
-- ============================================================
SELECT
    bh.NgayHoc,
    bh.MaCaHoc,
    COALESCE(bh.MaGiaoVienDayThay, bh.MaGiaoVien) AS EffectiveTeacherId,
    COUNT(*) AS SoBuoi,
    STRING_AGG(CONCAT('BH#', bh.MaBuoiHoc), '; ') AS DanhSach
FROM BuoiHoc bh
WHERE bh.TrangThaiBuoi <> 'da_huy'
GROUP BY bh.NgayHoc, bh.MaCaHoc, COALESCE(bh.MaGiaoVienDayThay, bh.MaGiaoVien)
HAVING COUNT(*) > 1
ORDER BY SoBuoi DESC;

-- ============================================================
-- Query 5: Session — class conflict (BuoiHoc level)
-- Finds classes assigned to 2+ sessions on same (NgayHoc, MaCaHoc).
-- ============================================================
SELECT
    bh.NgayHoc,
    bh.MaCaHoc,
    kh.MaLop,
    COUNT(*) AS SoBuoi,
    STRING_AGG(CONCAT('BH#', bh.MaBuoiHoc), '; ') AS DanhSach
FROM BuoiHoc bh
JOIN KhoaHoc kh ON kh.MaKhoaHoc = bh.MaKhoaHoc
WHERE bh.TrangThaiBuoi <> 'da_huy'
GROUP BY bh.NgayHoc, bh.MaCaHoc, kh.MaLop
HAVING COUNT(*) > 1
ORDER BY SoBuoi DESC;

-- ============================================================
-- Query 6: Session — room conflict (BuoiHoc level)
-- Finds rooms used by 2+ sessions on same (NgayHoc, MaCaHoc).
-- ============================================================
SELECT
    bh.NgayHoc,
    bh.MaCaHoc,
    bh.MaPhong,
    COUNT(*) AS SoBuoi,
    STRING_AGG(CONCAT('BH#', bh.MaBuoiHoc), '; ') AS DanhSach
FROM BuoiHoc bh
WHERE bh.TrangThaiBuoi <> 'da_huy'
GROUP BY bh.NgayHoc, bh.MaCaHoc, bh.MaPhong
HAVING COUNT(*) > 1
ORDER BY SoBuoi DESC;

-- ============================================================
-- Query 7: Capacity violation
-- Finds ThoiKhoaBieu where the room's SucChua (capacity)
-- is less than the estimated class size.
-- Estimate: count of NguoiDung with VaiTroChinh = 'Student' and MaLop matching.
-- ============================================================
SELECT
    tkb.MaTkb,
    kh.MaKhoaHoc,
    kh.TieuDe,
    kh.MaLop,
    tkb.MaPhong,
    p.TenPhong,
    p.SucChua,
    COUNT(nd.MaNguoiDung) AS SiSoUocTinh,
    CASE
        WHEN COUNT(nd.MaNguoiDung) > p.SucChua THEN N'VƯỢT QUÁ SỨC CHỨA'
        ELSE N'OK'
    END AS TrangThai
FROM ThoiKhoaBieu tkb
JOIN KhoaHoc kh ON kh.MaKhoaHoc = tkb.MaKhoaHoc
JOIN PhongHoc p ON p.MaPhong = tkb.MaPhong
LEFT JOIN NguoiDung nd ON nd.MaLop = kh.MaLop AND nd.VaiTroChinh = 'Student'
WHERE tkb.TrangThai <> 'da_huy'
GROUP BY tkb.MaTkb, kh.MaKhoaHoc, kh.TieuDe, kh.MaLop, tkb.MaPhong, p.TenPhong, p.SucChua
HAVING COUNT(nd.MaNguoiDung) > p.SucChua;

-- ============================================================
-- Query 8: Inactive room usage
-- Finds ThoiKhoaBieu or BuoiHoc using a room that is not
-- in 'hoat_dong' status.
-- ============================================================
-- TKB level
SELECT
    'TKB' AS Source,
    tkb.MaTkb AS Id,
    tkb.MaPhong,
    p.TenPhong,
    p.TrangThaiPhong
FROM ThoiKhoaBieu tkb
JOIN PhongHoc p ON p.MaPhong = tkb.MaPhong
WHERE tkb.TrangThai <> 'da_huy' AND p.TrangThaiPhong <> 'hoat_dong'

UNION ALL

-- BuoiHoc level
SELECT
    'BuoiHoc' AS Source,
    bh.MaBuoiHoc AS Id,
    bh.MaPhong,
    p.TenPhong,
    p.TrangThaiPhong
FROM BuoiHoc bh
JOIN PhongHoc p ON p.MaPhong = bh.MaPhong
WHERE bh.TrangThaiBuoi <> 'da_huy' AND p.TrangThaiPhong <> 'hoat_dong';

-- ============================================================
-- Query 9: Inactive shift usage
-- Finds ThoiKhoaBieu or BuoiHoc using a shift where
-- ConHoatDong = 0.
-- ============================================================
-- TKB level
SELECT
    'TKB' AS Source,
    tkb.MaTkb AS Id,
    tkb.MaCaHoc,
    c.TenCa,
    c.ConHoatDong
FROM ThoiKhoaBieu tkb
JOIN CaHoc c ON c.MaCaHoc = tkb.MaCaHoc
WHERE tkb.TrangThai <> 'da_huy' AND c.ConHoatDong = 0

UNION ALL

-- BuoiHoc level
SELECT
    'BuoiHoc' AS Source,
    bh.MaBuoiHoc AS Id,
    bh.MaCaHoc,
    c.TenCa,
    c.ConHoatDong
FROM BuoiHoc bh
JOIN CaHoc c ON c.MaCaHoc = bh.MaCaHoc
WHERE bh.TrangThaiBuoi <> 'da_huy' AND c.ConHoatDong = 0;

-- ============================================================
-- Query 10: Duplicate ThoiKhoaBieu
-- Finds duplicate (MaKhoaHoc, ThuTrongTuan, MaCaHoc) entries
-- that are not canceled.
-- ============================================================
SELECT
    MaKhoaHoc,
    ThuTrongTuan,
    MaCaHoc,
    COUNT(*) AS SoBanGhi,
    STRING_AGG(CONCAT('TKB#', MaTkb, '[', TrangThai, ']'), '; ') AS DanhSach
FROM ThoiKhoaBieu
WHERE TrangThai <> 'da_huy'
GROUP BY MaKhoaHoc, ThuTrongTuan, MaCaHoc
HAVING COUNT(*) > 1;
