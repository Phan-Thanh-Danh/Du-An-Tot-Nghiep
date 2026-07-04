# Phân quyền (Permission Matrix)

Tài liệu mô tả ma trận phân quyền trong hệ thống LMS.

## Các Role Chính
- `SuperAdmin` (super_admin): Quản trị cấp cao nhất, toàn quyền.
- `Chairman` (chairman): Chủ tịch hội đồng, xem toàn bộ dữ liệu.
- `CampusAdmin` (campus_admin): Quản trị viên cơ sở, quản lý cơ sở.
- `SubCampusAdmin` (sub_campus_admin): Quản trị viên phân hiệu.
- `AcademicStaff` (academic_staff): Giáo vụ, quản lý học vụ (khóa học, phòng học, tkb).
- `Teacher` (teacher): Giảng viên, quản lý lớp dạy, điểm danh, chấm bài.
- `Student` (student): Sinh viên, xem khóa học, nộp bài, xem lịch.

## Policies
- `AdminUserManagement`: Quản trị người dùng admin (`SuperAdmin`, `CampusAdmin`).
- `RbacManagement`: Quản lý role/permission (`SuperAdmin`).
- `AcademicOperations`: Các hoạt động học vụ (`SuperAdmin`, `CampusAdmin`, `SubCampusAdmin`, `AcademicStaff`).

## Chặn Truy Cập Ngang Hàng (IDOR / Ownership)
- **Sinh viên (Student)**:
  - Dashboard, Lịch học, Học phí, Điểm số, Kỷ luật: `[Authorize(Roles = "Student")]` và luôn sử dụng `GetCurrentUserId()` để trích xuất `MaHocSinh`.
  - Không cho phép truyền ID sinh viên khác qua URL (nếu gọi bằng ID khác sẽ bị trả về 403 Forbidden hoặc 404 Not Found theo query condition).
- **Giảng viên (Teacher)**:
  - Dashboard, Lớp học, Bài tập, Chấm bài: `[Authorize(Roles = "Teacher")]` và sử dụng `GetCurrentUserId()` để lọc đúng lớp đang phụ trách.
  - Không thể chấm bài hoặc xem bài nộp của lớp mà mình không phụ trách.
- **Giáo vụ (AcademicStaff)**:
  - Chỉ quản lý dữ liệu trong scope của Campus/SubCampus mà họ được phân công (dựa vào `MaDonVi` lưu trong Token/Context).

## Xử Lý Lỗi Frontend (apiClient.js)
Frontend xử lý 4xx, 5xx qua interceptor chuẩn trong `apiClient.js`:
- `401 Unauthorized`: "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." (Sẽ thử refresh token tự động nếu hỗ trợ).
- `403 Forbidden`: "Bạn không có quyền thực hiện hành động này."
- `404 Not Found`: "Dữ liệu không tồn tại hoặc bạn không có quyền truy cập." (Ngăn chặn IDOR dò quét dữ liệu).
- `500 Internal Server Error`: "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."
