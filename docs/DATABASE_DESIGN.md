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
- `CaHoc`: danh mục ca học cố định, gồm tên ca, buổi, giờ bắt đầu/kết thúc, thứ tự hiển thị và trạng thái hoạt động.
- `ThoiKhoaBieu`: lịch học cố định hằng tuần của một `KhoaHoc`, gồm thứ trong tuần, `CaHoc` và phòng học. Trong MVP không nhập giờ tự do hoặc nhập rời lớp/môn/giảng viên trực tiếp ở thời khóa biểu.
- `BuoiHoc`: buổi học cụ thể theo ngày, lưu snapshot khóa học, ca thực tế, phòng thực tế, giảng viên chính và giảng viên dạy thay nếu có để xử lý đổi phòng, dời ca, dạy thay hoặc hủy một buổi.
- `DatPhong`: đặt phòng.

Các bảng học vụ thường có `MaDonVi`, cần chú ý phân quyền theo campus scope.

## Nhóm Bảng Danh Mục Đào Tạo

- `NganhDaoTao`: danh mục ngành đào tạo, có `MaCodeNganh` dùng làm mã hiển thị/nghiệp vụ của ngành.
- `ChuyenNganh`: danh mục chuyên ngành thuộc `NganhDaoTao`; không lưu mã code riêng của chuyên ngành, khi cần hiển thị mã ngành thì join qua `MaNganh` sang `NganhDaoTao`.
- `ChuyenNganhTheoCoSo`: chuyên ngành được mở tại từng cơ sở, dùng `MaChuyenNganh` và `MaDonVi` để quản lý phạm vi/campus scope.

## Nhóm Bảng Khóa Học/Bài Học

- `DanhMucMonHoc`: danh mục môn học.
- `MonHocTienQuyet`: quan hệ môn tiên quyết.
- `KhoaHoc`: bản phân công giảng dạy theo môn/giáo viên/cơ sở/học kỳ/lớp hành chính; `MaLopHocPhan` giữ nullable cho roadmap.
- `Chuong`: chương chuẩn thuộc `DanhMucMonHoc`.
- `BaiHoc`: bài học video/pdf/văn bản/trắc nghiệm, có trường `TomTatAi`.
- `TienDoBaiHoc`: tiến độ học sinh theo bài học.
- `BinhLuan`: bình luận trong bài học.

API cho nhóm này hiện là dự kiến, chưa thấy controller.

Luồng dữ liệu cho chi tiết khóa học: lấy `KhoaHoc` theo id để biết môn, giảng viên, lớp hành chính và học kỳ; từ `KhoaHoc.MaMonHoc` truy ra `DanhMucMonHoc`, rồi lấy `Chuong -> BaiHoc` làm nội dung học tập chuẩn của môn. Trong MVP không copy nội dung chuẩn theo từng `KhoaHoc`.

Luồng dữ liệu lịch học MVP: `ThoiKhoaBieu` lấy lớp/môn/giảng viên/học kỳ/cơ sở từ `KhoaHoc`, chọn ca từ `CaHoc` và phòng từ `PhongHoc`. Sau khi xuất bản lịch cố định, hệ thống dự kiến sinh `BuoiHoc` từng ngày; mọi phát sinh một buổi như dạy thay, đổi phòng, dời ca hoặc hủy buổi được cập nhật trên `BuoiHoc`, không sửa sai lịch cố định toàn kỳ.

## Nhóm Bảng Bài Tập/Bài Nộp

- `BaiTap`: bài tập chuẩn thuộc `DanhMucMonHoc` trong MVP, gồm hạn nộp, số lần nộp và trạng thái.
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

- `ThongBao`: nội dung thông báo chung. P0-NT-Core giữ legacy columns `ma_nguoi_nhan`, `da_doc`, `doc_luc`, `ma_nhom_thong_bao` để tương thích dữ liệu cũ, nhưng logic mới không dùng các cột này cho trạng thái đọc.
- `ThongBaoNguoiNhan`: trạng thái từng người nhận của một thông báo (`da_doc`, `doc_luc`, `da_an`, `an_luc`, `nhan_luc`). Có unique index `ma_thong_bao + ma_nguoi_nhan` và index phục vụ inbox `ma_nguoi_nhan + da_doc + da_an + nhan_luc`.
- `ThongBaoHenGio`: thông báo lên lịch.
- `MauThongBao`: mẫu thông báo.
- `NhatKyThongBao`: log gửi thông báo.
- `TuyChonThongBao`: tùy chọn nhận thông báo.
- `PhieuHoTro`: ticket hỗ trợ.
- `TinNhanHoTro`: tin nhắn trong ticket.
- `CauHoiThuongGap`: FAQ.
- `DonTu`, `NhatKyDuyetDon`: đơn từ và quy trình duyệt.

Notification Center backend MVP đã có API user/admin. Migration `AddNotificationRecipientState` backfill bảo thủ dữ liệu P0-8: mỗi row `ThongBao` cũ được giữ nguyên và tạo một row `ThongBaoNguoiNhan` tương ứng, không gom nhóm theo `ma_nhom_thong_bao` nếu không chắc nội dung giống nhau.

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
- `DotKhenThuong`: đợt xét khen thưởng, RD1 hỗ trợ foundation cho loại `TOP_100_HOC_KY`, scope theo học kỳ/cơ sở, trạng thái nháp/xét/chờ duyệt/duyệt/công bố/hủy và cấu hình tiêu chí JSON.
- `UngVienKhenThuong`: danh sách ứng viên Top 100 theo đợt/học kỳ/cơ sở. RD4 bổ sung `ghi_chu_dieu_chinh`, `nguoi_dieu_chinh`, `ngay_dieu_chinh` để lưu vết điều chỉnh thủ công trước duyệt; unique `ma_dot_khen_thuong + ma_hoc_sinh` chống trùng ứng viên trong một đợt.
- `MauBangKhen`: mẫu bằng khen, lưu file nền, kích thước, hướng giấy và cấu hình render JSON. RD5 dùng lại schema có sẵn để quản lý mẫu, validate `CauHinhJson` theo whitelist field render an toàn và vô hiệu hóa bằng `con_hoat_dong = 0`, không hard delete.
- `KhenThuong`, `CauHinhKhenThuong`: khen thưởng. `KhenThuong` giữ cột legacy (`gpa_dat_duoc`, `url_chung_tu`, `da_huy`) và bổ sung foundation RD1 như `ma_don_vi`, liên kết đợt/mẫu bằng khen, trạng thái lifecycle, điểm xét, xếp hạng, snapshot hiển thị và URL PDF. RD6 bổ sung metadata sinh PDF gồm `ngay_sinh_pdf`, `loi_sinh_pdf`, `so_lan_sinh_pdf`; `url_chung_tu` legacy vẫn có thể là chuỗi rỗng, còn file bằng khen dùng `url_pdf_bang_khen`. RD8 thêm migration `AddRewardLifecycleFields` bổ sung `ngay_cap` và `ghi_chu_vong_doi` để quản lý việc cấp phát và ghi log vòng đời (hủy, cấp, tái sinh chứng chỉ).
- `HoSoKyLuat`: hồ sơ kỷ luật. RD1 giữ cột legacy và bổ sung học kỳ, mức độ vi phạm, hình thức xử lý, trạng thái lifecycle, ngày vi phạm/hiệu lực, chứng từ JSON, thông tin duyệt/gỡ hiệu lực và liên kết nghiệp vụ tùy chọn.
- `YeuCauDoiLich`: yêu cầu đổi lịch.

RD1 chỉ là nền tảng dữ liệu và schema/options API. RD2 dùng lại bảng `DotKhenThuong` để CRUD metadata đợt `TOP_100_HOC_KY`; duplicate active được bảo vệ bởi filtered unique index `ma_hoc_ky + ma_don_vi + loai_dot` với `trang_thai <> da_huy`, service cũng validate trước khi ghi. RD3 tạo danh sách `UngVienKhenThuong` nhưng chưa tạo `KhenThuong`. RD4 thêm migration `AddRewardCandidateApprovalFields` cho metadata điều chỉnh ứng viên, sau đó approve tạo `KhenThuong` chính thức từ ứng viên `duoc_de_xuat`/`them_thu_cong`, set ứng viên `da_duyet_kt` và set đợt `da_duyet`. RD5 không đổi schema; chỉ dùng `MauBangKhen` hiện có để CRUD/preview mẫu. RD6 thêm migration `AddRewardCertificatePdfFields` cho metadata PDF và cập nhật `KhenThuong.UrlPdfBangKhen` khi sinh bằng khen; file cũ không bị xóa khi regenerate. RD-DL-REPORT không đổi schema: báo cáo đọc trực tiếp từ `DotKhenThuong`, `UngVienKhenThuong`, `KhenThuong`, `HoSoKyLuat`, `KhieuNaiKyLuat`, `HocKy`, `DonVi`, `NguoiDung` và tận dụng các index hiện có như campaign theo học kỳ/đơn vị/trạng thái, reward theo đơn vị/trạng thái, discipline theo đơn vị/học kỳ/trạng thái và appeal theo đơn vị/trạng thái. Công bố và workflow kỷ luật vẫn tách task sau. Migration `AddRewardDisciplineFoundation` backfill `KhenThuong.ma_don_vi` từ `NguoiDung.ma_don_vi` của học sinh và fail-fast nếu dữ liệu legacy không thể suy ra cơ sở.

Known limitations cho báo cáo RD-DL:

- Chưa có bảng snapshot/report cache; dashboard query aggregate trực tiếp.
- Chưa lưu lượt tải bằng khen của học sinh nên certificate report không tính được downloaded count.
- Chưa group sâu theo khoa/ngành/lớp vì `KhenThuong`/`HoSoKyLuat` hiện lưu trực tiếp theo học sinh/cơ sở/học kỳ; nếu cần drill-down cần join và định nghĩa nghiệp vụ riêng ở task sau.

Các module này có model/database nhưng API hiện là dự kiến.

## Lưu Ý Cho Agent

- Không đổi tên bảng/cột/entity nếu chưa có migration và yêu cầu rõ.
- Không giả định mọi bảng đã có API.
- Với dữ liệu theo cơ sở, kiểm tra `MaDonVi` và role/campus scope.
- JSON column đã có check constraint `ISJSON` ở nhiều bảng; cần serialize/validate đúng khi dùng.
## HoSoKyLuat
Thêm các cột phục vụ DL1:
- 	ieu_de: nvarchar(255)
- can_cu_xu_ly: nvarchar(2000)
- ghi_chu_noi_bo: nvarchar(2000)
- ly_do_huy: nvarchar(1000)
- 
guoi_huy: FK NguoiDung
- 
gay_huy: datetime2
