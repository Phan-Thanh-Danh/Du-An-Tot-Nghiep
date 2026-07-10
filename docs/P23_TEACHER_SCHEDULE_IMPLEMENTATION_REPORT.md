# P23 — TEACHER SCHEDULE VISIBILITY & DASHBOARD TODAY

## Tóm tắt nội dung
Đã thực hiện hoàn chỉnh P23 bao gồm:
1. Hiển thị tổng quan lịch giảng dạy hôm nay (Teacher Dashboard).
2. Hiển thị danh sách buổi học theo bộ lọc thời gian/học kỳ (Teacher Schedule Visibility).

## Backend
- **TeacherScheduleController**:
  - `GET /api/teacher/schedule/summary`: Lấy thông tin tổng quan số buổi dạy hôm nay, buổi tiếp theo, danh sách lớp dạy.
  - `GET /api/teacher/schedule/terms`: Lấy danh sách các học kỳ mà giảng viên có lịch.
  - `GET /api/teacher/schedule`: Lấy danh sách các buổi dạy theo phân trang, ngày bắt đầu, ngày kết thúc và học kỳ.
- **TeacherScheduleService**:
  - `GetTodayScheduleAsync`: Xử lý logic lọc lịch dạy hôm nay (`TrangThai == "da_xuat_ban"`).
  - `GetTermsAsync`: Trích xuất học kỳ từ các buổi học có lịch dạy đã xuất bản.
  - `GetScheduleAsync`: Query `BuoiHocs`, join `KhoaHoc`, `MonHoc`, `LopHanhChinh`, `CaHoc`, `PhongHoc` và `ThoiKhoaBieu`.
- **DTOs**:
  - `TeacherScheduleItemDto`
  - `TeacherScheduleSummaryDto`
  - `TeacherScheduleQueryDto`
  - `TeacherScheduleTermDto`
  - `TeacherAssignedCourseDto`
- **Tests**: Đã tạo `Backend.ApiTests/P23_TeacherScheduleTests.cs` (chạy qua NUnit) kiểm tra authorization và response.

## Frontend
- **API Client**: Tạo các phương thức gọi API trong `frontend/src/services/teacherApi.js` (`getScheduleSummary`, `getScheduleTerms`, `getSchedule`).
- **Dashboard**: Sửa đổi `Dashboard.vue` để gọi API và binding dữ liệu thực tế (thay thế mock data). Các thẻ số lượng buổi dạy, buổi học tiếp theo và danh sách lớp học được lấy từ Backend.
- **Teaching Schedule View**: Tạo view mới `TeachingScheduleView.vue` trong `GiangVien/` để hiển thị chi tiết lịch giảng dạy với:
  - Chọn ngày, học kỳ.
  - Phân trang, trạng thái học thay/chính.
  - Giao diện glassmorphism/card UI nhất quán theo `AGENTS.md`.
- **Menu/Router**: Bổ sung `Lịch giảng dạy` vào Sidebar của Giảng Viên (`menuData.js`) và định tuyến `/teacher/schedule` trong `router/index.js`.

## Tuân thủ quy định & Baseline
- Kiến trúc giữ nguyên, không sử dụng thêm Repository layer. 
- Mọi entity `BuoiHocs`, `ThoiKhoaBieus` sử dụng DbSets chuẩn xác theo EF Core (AsNoTracking để read-only).
- Timezone sử dụng chuẩn VN Time (`SE Asia Standard Time`) thay vì local `DateTime.Now`.
- Kiểm tra tính hợp lệ về Identity (`UserId`) từ `HttpContext.Items["CurrentUser"]`.
- UI Frontend sử dụng Semantic CSS tokens thay vì hard-code classes.

## Tình trạng hiện tại
- Code đã build không lỗi.
- Giao diện Frontend hiển thị dữ liệu chính xác và load component thành công.
- API tests (integration tests skeleton) sẵn sàng.
- Endpoint được bổ sung vào `API_CONTRACT.md`.
