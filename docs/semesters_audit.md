# Báo cáo Kiểm tra Code - Trang Quản lý Học kỳ (SemestersView)

Qua kiểm tra source code của Frontend (`SemestersView.vue`, `academicTermApi.js`) và Backend (`AcademicTermsController.cs`, `AcademicTermService.cs`, `AcademicTermDto.cs`, `HocKy.cs`), dưới đây là danh sách các lỗi và thiếu sót nghiệp vụ được phát hiện:

## 1. Lỗi Giao diện & Hiển thị dữ liệu (Frontend)
- **Thiếu cột "Cơ sở" (TenDonVi):** Dành cho vai trò `SuperAdmin` quản lý tập trung, nhưng cấu hình `columns` trong `SemestersView.vue` lại không có cột Cơ sở. Backend (DTO) đã trả về trường `TenDonVi` nhưng FE không hiển thị, khiến Super Admin không thể phân biệt được học kỳ này thuộc về cơ sở/campus nào.
- **Lỗi hiển thị Trạng thái:** Giao diện đang dùng biểu thức `term.trangThai || 'Đang mở'` (nếu `DaKhoa` = false). Tuy nhiên, Backend hoàn toàn không có trường `TrangThai` (cả trong DB lẫn DTO). Hậu quả là tất cả các học kỳ chưa khóa đều sẽ hiển thị là `"Đang mở"`, kể cả khi ngày hiện tại đã vượt qua `NgayKetThuc` (lẽ ra phải là "Đã kết thúc").

## 2. Lỗi Phân trang & Fetch dữ liệu (Frontend)
- **Hardcode PageSize = 100:** Hàm `loadSemesters` hiện tại đang fix cứng gọi API với `{ pageSize: 100 }` và không hề nhận tham số `pageIndex`, `pageSize` động từ component `SuperAdminApiListView`. Nếu số lượng học kỳ vượt quá 100, người dùng sẽ vĩnh viễn không xem được các học kỳ còn lại.

## 3. Chênh lệch tính năng giữa FE và BE (Thiếu luồng UI)
- **Frontend thiếu toàn bộ tính năng CRUD:** UI hiện tại chỉ là một bảng Read-only (chỉ có hàm `loadSemesters`). Các nút chức năng (Thêm, Sửa, Xóa, Khóa, Mở khóa) đều không được khai báo. Lời ghi chú trong code cho biết: *"Tạo, sửa, khóa học kỳ cần audit action riêng trước khi đưa vào claim full action/API"*.
- **Backend đã code dư/hoàn thiện API:** Trái ngược với FE, Backend đã hoàn thiện 100% các API tạo, sửa, xóa, khóa (`POST`, `PUT`, `DELETE`, `PATCH /lock`, `PATCH /unlock`), kèm theo đầy đủ các quy tắc xác thực (ví dụ: cấm sửa khi đã khóa, bắt buộc phải khóa trước khi xóa, kiểm tra thứ tự học kỳ trùng lặp, v.v.).

## 4. Đánh giá tính toàn vẹn Dữ liệu (BE -> FE)
- Dữ liệu map từ BE sang FE cơ bản là khớp (ID, Mã học kỳ, Tên học kỳ, Ngày bắt đầu, Ngày kết thúc, cờ Đã khóa).
- **Backend Validation tốt:** Backend bắt lỗi logic rất chặt chẽ (VD: không cho phép tạo 2 học kỳ Spring trong cùng 1 năm học của cùng 1 cơ sở, không cho xóa nếu còn dữ liệu lớp học phần/điểm số liên quan).

---

**Kết luận:** Backend đã sẵn sàng và hoàn chỉnh. Frontend hiện đang bị chặn đứng ở mức "View (Read-only)", thiếu cột thông tin cơ sở, hardcode phân trang và có sai sót nhỏ trong logic hiển thị cột "Trạng thái".
