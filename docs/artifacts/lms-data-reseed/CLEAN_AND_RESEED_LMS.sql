-- ====================================================================
-- SCRIPT XÓA SẠCH DỮ LIỆU BẢNG ỨNG DỤNG THEO THỨ TỰ KHÓA NGOẠI (FK)
-- Database: LMS (Local SQL Server)
-- Note: Giữ nguyên __EFMigrationsHistory và cấu trúc DB snapshot
-- ====================================================================

USE [LMS];
GO

BEGIN TRANSACTION;
BEGIN TRY

    PRINT N'1. Xóa dữ liệu các bảng Thi, Kỳ thi và Đề kiểm tra...';
    DELETE FROM [XuLyViPhamThi];
    DELETE FROM [NhatKyViPhamThi];
    DELETE FROM [DiemDanhThi];
    DELETE FROM [BienBanThi];
    DELETE FROM [ThiSinhCaThi];
    DELETE FROM [PhanCongGiamThi];
    DELETE FROM [PhienThiHocSinh];
    DELETE FROM [CaThi];
    DELETE FROM [LichThiTong];
    DELETE FROM [CauHoiDeKiemTra];
    DELETE FROM [DeKiemTra];
    DELETE FROM [CauHoi];
    DELETE FROM [KyThi];

    PRINT N'2. Xóa dữ liệu Điểm số, Đánh giá và Chuyên cần...';
    DELETE FROM [DiemSo];
    DELETE FROM [NhatKyThayDoiDiem];
    DELETE FROM [YeuCauSuaDiem];
    DELETE FROM [YeuCauMoKhoaDiemDanh];
    DELETE FROM [LoaiDauDiemQuaTrinh];
    DELETE FROM [CauHinhDauDiemQuaTrinh];
    DELETE FROM [CauHinhDiemMonHoc];
    DELETE FROM [DangKyHocPhan];
    DELETE FROM [DanhGiaGiaoVien];
    DELETE FROM [NopBaiDanhGia];
    DELETE FROM [CauHoiDanhGia];
    DELETE FROM [DiemDanh];
    DELETE FROM [BuoiHoc];
    DELETE FROM [ThoiKhoaBieu];
    DELETE FROM [YeuCauDoiLich];

    PRINT N'3. Xóa dữ liệu Bài học, Bài tập và Nội dung...';
    DELETE FROM [BaiNop];
    DELETE FROM [BaiTap];
    DELETE FROM [TienDoBaiHoc];
    DELETE FROM [BinhLuan];
    DELETE FROM [BaiHocNoiDung];
    DELETE FROM [BaiHoc];
    DELETE FROM [Chuong];

    PRINT N'4. Xóa dữ liệu Học phí, Hóa đơn và Giao dịch...';
    DELETE FROM [GiaoDich];
    DELETE FROM [HoaDon];
    DELETE FROM [CauHinhHocPhiChuongTrinh];
    DELETE FROM [YeuCauHoanPhi];
    DELETE FROM [TaiKhoanNhanTien];

    PRINT N'5. Xóa dữ liệu Báo cáo, Đơn từ, Khen thưởng và Cảnh báo...';
    DELETE FROM [BaoCaoRuiRoRotMon];
    DELETE FROM [DanhSachRuiRoRotMon];
    DELETE FROM [BaoCaoRuiRoVang];
    DELETE FROM [BaoCaoSuDungPhong];
    DELETE FROM [XuatBaoCao];
    DELETE FROM [NhatKyDuyetDon];
    DELETE FROM [DonTu];
    DELETE FROM [HoSoKyLuat];
    DELETE FROM [KhenThuong];
    DELETE FROM [CauHinhKhenThuong];
    DELETE FROM [TinNhanHoTro];
    DELETE FROM [PhieuHoTro];
    DELETE FROM [ThongBaoNguoiNhan];
    DELETE FROM [ThongBaoHenGio];
    DELETE FROM [ThongBao];
    DELETE FROM [MauThongBao];
    DELETE FROM [TuyChonThongBao];
    DELETE FROM [NhatKyThongBao];
    DELETE FROM [CanhBaoBaoMat];
    DELETE FROM [CanhBaoDaoVan];

    PRINT N'6. Xóa dữ liệu Khóa học, Lớp HP và Phân công...';
    DELETE FROM [KhoaHoc];
    DELETE FROM [LopHocPhan];
    DELETE FROM [GiaoVienMonHoc];
    DELETE FROM [GiaoVienChuyenNganh];
    DELETE FROM [GiaoVienNguyenVongCaDay];
    DELETE FROM [GiaoVienNguyenVongHocKy];
    IF OBJECT_ID('GiaiDoanDangKy', 'U') IS NOT NULL DELETE FROM [GiaiDoanDangKy];

    PRINT N'7. Xóa dữ liệu Môn học, Đề cương và Chương trình...';
    DELETE FROM [DeCuongMonHoc];
    DELETE FROM [MonHocTrongChuongTrinh];
    DELETE FROM [MonHocTienQuyet];
    DELETE FROM [DanhMucMonHoc];
    DELETE FROM [LienKetPhuHuynh];
    DELETE FROM [TokenLamMoi];
    DELETE FROM [PhanQuyenNguoiDung];
    DELETE FROM [AnhChupPhanTich];
    DELETE FROM [NhatKyKiemToan];
    DELETE FROM [PasswordResetOtps];
    UPDATE [NguoiDung] SET [ma_lop] = NULL;
    UPDATE [LopHanhChinh] SET [ma_giao_vien_chu_nhiem] = NULL;
    DELETE FROM [NguoiDung];
    DELETE FROM [LopHanhChinh];
    DELETE FROM [ChuongTrinhHocKy];
    DELETE FROM [ChuongTrinhDaoTao];
    DELETE FROM [ChuyenNganhTheoCoSo];
    DELETE FROM [ChuyenNganh];
    DELETE FROM [NganhDaoTao];
    DELETE FROM [KhoaTuyenSinh];
    DELETE FROM [Block];
    DELETE FROM [HocKy];

    PRINT N'8. Xóa dữ liệu Cơ sở vật chất và Phòng học...';
    DELETE FROM [ThietBiPhong];
    DELETE FROM [DatPhong];
    DELETE FROM [PhongHoc];
    DELETE FROM [Tang];
    DELETE FROM [ToaNha];
    DELETE FROM [CaHoc];

    PRINT N'9. Xóa dữ liệu Vai trò và Đơn vị...';
    DELETE FROM [VaiTro];
    DELETE FROM [DonVi];

    COMMIT TRANSACTION;
    PRINT N'=== ĐÃ XÓA SẠCH DỮ LIỆU CÁC BẢNG ỨNG DỤNG THÀNH CÔNG (TRANSACTION COMMITTED) ===';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT N'ERROR DETECTED: ROLLBACK TRANSACTION!';
    THROW;
END CATCH;
GO
