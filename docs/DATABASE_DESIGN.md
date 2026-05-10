# Database Design

## Tổng Quan Database

Database dùng SQL Server và được ánh xạ bằng EF Core trong `Backend/Data/ApplicationDbContext.cs`. Schema hiện khá rộng, phục vụ LMS và quản lý học vụ: người dùng, tổ chức, lớp học, khóa học, bài học, bài tập, thi, điểm danh, điểm số, thông báo, hỗ trợ, tài chính, báo cáo và AI.

Tài liệu này mô tả theo nhóm bảng để AI agent nắm context. Chi tiết cột/constraint xem `ApplicationDbContext.cs`, migrations và tài liệu SQL trong `Tài Liệu/LMS_SQL_Server_DDL_FULL_COMPLETED.sql` nếu cần.

## Nhóm Bảng Người Dùng/Phân Quyền

- `NguoiDung`: tài khoản người dùng, email, mật khẩu hash, vai trò chính, trạng thái, cơ sở (`MaDonVi`), thông tin đăng nhập.
- `VaiTro`: danh mục vai trò.
- `PhanQuyenNguoiDung`: liên kết người dùng với vai trò/đơn vị.
- `TokenLamMoi`: refresh token hash, thời hạn và trạng thái thu hồi.
- `LienKetPhuHuynh`: liên kết phụ huynh với học sinh.

Vai trò database như `hoc_sinh`, `giao_vien`, `quan_tri` được map sang role API trong `AuthRoles`.

## Nhóm Bảng Tổ Chức/Lớp/Phòng

- `DonVi`: cây tổ chức/cơ sở/cơ sở con.
- `LopHanhChinh`: lớp hành chính.
- `LopHocPhan`: lớp học phần.
- `PhongHoc`: phòng học.
- `ThietBiPhong`: thiết bị phòng học.
- `ThoiKhoaBieu`: lịch học.
- `BuoiHoc`: buổi học cụ thể.
- `DatPhong`: đặt phòng.

Các bảng học vụ thường có `MaDonVi`, cần chú ý phân quyền theo campus scope.

## Nhóm Bảng Khóa Học/Bài Học

- `DanhMucMonHoc`: danh mục môn học.
- `MonHocTienQuyet`: quan hệ môn tiên quyết.
- `KhoaHoc`: khóa học theo môn/giáo viên/cơ sở.
- `Chuong`: chương trong khóa học.
- `BaiHoc`: bài học video/pdf/văn bản/trắc nghiệm, có trường `TomTatAi`.
- `TienDoBaiHoc`: tiến độ học sinh theo bài học.
- `BinhLuan`: bình luận trong bài học.

API cho nhóm này hiện là dự kiến, chưa thấy controller.

## Nhóm Bảng Bài Tập/Bài Nộp

- `BaiTap`: bài tập trong khóa học, hạn nộp, số lần nộp, trạng thái.
- `BaiNop`: bài nộp của học sinh, file, điểm, nhận xét, điểm AI đề xuất.
- `CanhBaoDaoVan`: cảnh báo đạo văn cho bài nộp.

API cho nhóm này hiện là dự kiến, chưa thấy controller.

## Nhóm Bảng Thi/Quiz

- `DeKiemTra`: đề kiểm tra/quiz/exam.
- `CauHoi`: ngân hàng câu hỏi.
- `CauHoiDeKiemTra`: liên kết câu hỏi với đề.
- `PhienThiHocSinh`: phiên làm bài của học sinh, câu trả lời JSON, nhật ký vi phạm.

API cho nhóm này hiện là dự kiến, chưa thấy controller.

## Nhóm Bảng Điểm Danh/Điểm Số

- `DiemDanh`: trạng thái điểm danh theo buổi học/học sinh.
- `YeuCauMoKhoaDiemDanh`: yêu cầu mở khóa điểm danh.
- `DiemSo`: điểm quá trình, giữa kỳ, cuối kỳ, GPA môn học, trạng thái.
- `CauHinhDiemMonHoc`: trọng số điểm theo môn.
- `YeuCauSuaDiem`: yêu cầu sửa điểm.
- `NhatKyThayDoiDiem`: audit thay đổi điểm.
- `DangKyHocPhan`, `GiaiDoanDangKy`: đăng ký học phần.

API cho nhóm này hiện là dự kiến, chưa thấy controller.

## Nhóm Bảng Thông Báo/Hỗ Trợ

- `ThongBao`: thông báo.
- `ThongBaoHenGio`: thông báo lên lịch.
- `MauThongBao`: mẫu thông báo.
- `NhatKyThongBao`: log gửi thông báo.
- `TuyChonThongBao`: tùy chọn nhận thông báo.
- `PhieuHoTro`: ticket hỗ trợ.
- `TinNhanHoTro`: tin nhắn trong ticket.
- `CauHoiThuongGap`: FAQ.
- `DonTu`, `NhatKyDuyetDon`: đơn từ và quy trình duyệt.

API cho nhóm này hiện là dự kiến, chưa thấy controller.

## Nhóm Bảng Báo Cáo/AI/Audit

- `BaoCaoRuiRoVang`: báo cáo rủi ro vắng.
- `BaoCaoRuiRoRotMon`: báo cáo rủi ro rớt môn.
- `DanhSachRuiRoRotMon`: danh sách học sinh có nguy cơ.
- `BaoCaoSuDungPhong`: báo cáo sử dụng phòng.
- `AnhChupPhanTich`: snapshot chỉ số phân tích.
- `XuatBaoCao`: yêu cầu export báo cáo.
- `NhatKyKiemToan`: audit log hệ thống.
- `CanhBaoBaoMat`: cảnh báo bảo mật.
- `DanhGiaGiaoVien`, `CauHoiDanhGia`, `NopBaiDanhGia`: đánh giá giáo viên, có trường phân tích AI.

API/report/AI hiện là dự kiến, chưa thấy controller.

## Nhóm Bảng Tài Chính/Kỷ Luật/Khen Thưởng

- `HoaDon`: hóa đơn học phí.
- `GiaoDich`: giao dịch thanh toán.
- `YeuCauHoanPhi`: yêu cầu hoàn phí.
- `HoSoKyLuat`: hồ sơ kỷ luật.
- `KhenThuong`, `CauHinhKhenThuong`: khen thưởng.
- `YeuCauDoiLich`: yêu cầu đổi lịch.

Các module này có model/database nhưng API hiện là dự kiến.

## Lưu Ý Cho Agent

- Không đổi tên bảng/cột/entity nếu chưa có migration và yêu cầu rõ.
- Không giả định mọi bảng đã có API.
- Với dữ liệu theo cơ sở, kiểm tra `MaDonVi` và role/campus scope.
- JSON column đã có check constraint `ISJSON` ở nhiều bảng; cần serialize/validate đúng khi dùng.
