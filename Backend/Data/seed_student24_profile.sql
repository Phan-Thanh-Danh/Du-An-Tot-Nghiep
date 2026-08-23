SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

SET IDENTITY_INSERT [dbo].[MauBangKhen] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[MauBangKhen] WHERE ma_mau_bang_khen = 1)
BEGIN
    INSERT INTO [dbo].[MauBangKhen] (ma_mau_bang_khen, ten_mau, loai_mau, file_nen_url, chieu_rong, chieu_cao, huong_giay, cau_hinh_json, con_hoat_dong, nguoi_tao, ngay_tao, ngay_cap_nhat)
    VALUES (1, N'Mẫu Giấy Khen Học Kỳ Xuất Sắc', 'TOP_100_HOC_KY', '', 1123, 794, 'A4_NGANG', '{"mode":"html","html":"<div class=\"cert\"><h1>GIẤY KHEN</h1><p>Trao tặng: {{hoTen}}</p><p>MSSV: {{mssv}}</p><p>Danh hiệu: {{danhHieu}}</p></div>","css":".cert{text-align:center;padding:40px;}"}', 1, 1, GETUTCDATE(), GETUTCDATE());
END;
SET IDENTITY_INSERT [dbo].[MauBangKhen] OFF;

SET IDENTITY_INSERT [dbo].[DotKhenThuong] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[DotKhenThuong] WHERE ma_dot_khen_thuong = 1)
BEGIN
    INSERT INTO [dbo].[DotKhenThuong] (ma_dot_khen_thuong, ma_hoc_ky, ma_don_vi, ten_dot, loai_dot, so_luong_toi_da, tieu_chi_xet_json, ma_mau_bang_khen, trang_thai, nguoi_tao, ngay_tao, ngay_cong_bo)
    VALUES (1, 1, 3, N'Đợt Khen Thưởng Sinh Viên Tiêu Biểu Kỳ 1 2026', 'TOP_100_HOC_KY', 100, '{"minGpa":3.5}', 1, 'da_cong_bo', 1, GETUTCDATE(), GETUTCDATE());
END;
SET IDENTITY_INSERT [dbo].[DotKhenThuong] OFF;

IF NOT EXISTS (SELECT 1 FROM [dbo].[KhenThuong] WHERE ma_hoc_sinh = 24)
BEGIN
    INSERT INTO [dbo].[KhenThuong] (ma_dot_khen_thuong, ma_hoc_sinh, ma_hoc_ky, ma_don_vi, loai_khen_thuong, url_chung_tu, danh_hieu_snapshot, ho_ten_snapshot, mssv_snapshot, ten_hoc_ky_snapshot, diem_xet, gpa_dat_duoc, xep_hang, ma_mau_bang_khen, trang_thai, ngay_cap, cap_luc, da_huy, so_lan_sinh_pdf)
    VALUES (1, 24, 1, 3, 'TOP_100_HOC_KY', '', N'Sinh Viên Xuất Sắc Kỳ 1 2026', N'P12 Test Sinh Viên', N'SV00024', N'Học kỳ 1 năm 2026', 3.85, 3.85, 1, 1, 'da_cap', CAST(GETUTCDATE() AS DATE), GETUTCDATE(), 0, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[HoSoKyLuat] WHERE ma_hoc_sinh = 24)
BEGIN
    INSERT INTO [dbo].[HoSoKyLuat] (ma_hoc_sinh, ma_don_vi, ma_hoc_ky, tieu_de, loai_ky_luat, muc_do_vi_pham, hinh_thuc_xu_ly, mo_ta, trang_thai, ngay_vi_pham, ngay_duyet, ngay_tao, da_go_ky_luat, nguoi_tao)
    VALUES (24, 3, 1, N'Nhắc nhở nề nếp trang phục', 'HocVu', N'nhe', N'nhac_nho', N'Đi học không đeo thẻ sinh viên', 'dang_hieu_luc', CAST(GETUTCDATE() AS DATE), GETUTCDATE(), GETUTCDATE(), 0, 1);
END;

UPDATE [dbo].[NguoiDung] SET so_dien_thoai = '0987654321' WHERE ma_nguoi_dung = 24;

IF NOT EXISTS (SELECT 1 FROM [dbo].[LienKetPhuHuynh] WHERE ma_hoc_sinh = 24)
BEGIN
    INSERT INTO [dbo].[LienKetPhuHuynh] (ma_phu_huynh, ma_hoc_sinh, quyen_xem, trang_thai, lien_ket_luc)
    VALUES (29, 24, '{"grades":true,"attendance":true,"finance":true,"schedule":true}', 'hoat_dong', GETUTCDATE());
END;
