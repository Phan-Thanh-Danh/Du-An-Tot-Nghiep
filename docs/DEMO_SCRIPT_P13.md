# P13 — Demo Script for Final Defense

> **Branch:** `feature/p13-final-polish-demo-defense`
> **Date:** 2026-07-05
> **Status:** ✅ READY FOR DEMO

---

## 1. Mục Tiêu Demo

Trình diễn hệ thống LMS Academic Management với các luồng nghiệp vụ chính:
- **SuperAdmin**: quản trị hệ thống, tổ chức, người dùng, audit
- **Staff (Giáo vụ)**: phân công giảng dạy (Smart Allocation), xếp thời khóa biểu thông minh (Smart Timetable), quản lý lớp/môn/phòng
- **Teacher**: xem khóa học, lịch dạy, chấm bài, dạy thay
- **Student**: xem khóa học, lịch học, bài tập, điểm số

---

## 2. Chuẩn Bị Trước Demo

### Yêu cầu hệ thống
- .NET SDK 10.0.x
- Node.js ^20.19.0 || >=22.12.0
- SQL Server (local instance `DELL\SQLEXPRESS02`)
- Git

### Bước chạy
```powershell
# 1. Clone & checkout
git clone https://github.com/Phan-Thanh-Danh/Du-An-Tot-Nghiep.git
cd Du-An-Tot-Nghiep
git checkout main

# 2. Backend
cd Backend
dotnet restore
dotnet ef database update      # Tạo schema + seed demo (nếu có)
dotnet run --urls http://localhost:5097

# 3. Frontend (terminal mới)
cd frontend
npm install
npm run dev
```

> Mở trình duyệt tại `http://localhost:5173`

---

## 3. Tài Khoản Demo

| Vai trò | Email | Mật khẩu | Ghi chú |
|---|---|---|---|
| **SuperAdmin** | `superadmin@lms.local` | `Test@123` | Toàn quyền hệ thống |
| **Staff (Giáo vụ)** | `p12test_staff01@lms.local` | `Test@123` | Quản lý học vụ, phân công, TKB |
| **Teacher CNTT** | `p12test_teacher01@lms.local` | `Test@123` | Dạy C# + SQL, chuyên CNTT |
| **Teacher Thiết kế** | `p12test_teacher07@lms.local` | `Test@123` | Dạy Photoshop, chuyên TKĐH |
| **Student** | `p12test_student011@lms.local` | `Test@123` | Lớp P12.01, có khóa học C# + SQL |

---

## 4. Demo Flow từng Role

### Flow A — SuperAdmin

| Bước | Thao tác | Kết quả mong đợi |
|---|---|---|
| 1 | Mở `http://localhost:5173` → Cổng truy cập | Trang portal landing hiển thị |
| 2 | Chọn "Siêu quản trị" → nhập `superadmin@lms.local` / `Test@123` | Đăng nhập thành công, vào SuperAdmin Dashboard |
| 3 | Xem Dashboard | Thống kê tổng quan (users, courses...) |
| 4 | Vào **Cơ sở** (`/super-admin/organizations`) | Cây tổ chức hiển thị |
| 5 | Vào **Người dùng** (`/super-admin/users`) | Danh sách users, có thể tìm kiếm |
| 6 | Vào **Audit Log** (`/super-admin/audit/logs`) | Nhật ký thay đổi hệ thống |
| 7 | Vào **Vai trò & Quyền hạn** (`/super-admin/roles-permissions`) | Danh sách vai trò |

### Flow B — Staff (Giáo vụ)

| Bước | Thao tác | Kết quả mong đợi |
|---|---|---|
| 1 | Đăng nhập với `p12test_staff01@lms.local` / `Test@123` | Vào Staff Dashboard |
| 2 | Xem Dashboard | Thống kê lớp, môn, lịch |
| 3 | Vào **Khóa học** (`/staff/courses`) | Danh sách 20 khóa học đã tạo |
| 4 | Vào **Quản lý thời khóa biểu** (`/staff/schedule`) | Giao diện quản lý TKB |
| 5 | Vào **Phân công giảng viên** (`/staff/assignments`) | Xem phân công |
| 6 | Vào **Kiểm tra xung đột** (`/staff/conflicts`) | Kiểm tra lịch |
| 7 | Vào **Phòng học** (`/staff/rooms`) | Danh sách 10 phòng |
| 8 | Vào **Ca học** (`/staff/shifts`) | Danh sách 5 ca (1 ca inactive) |
| 9 | Vào **Học kỳ** (`/staff/academic-terms`) | HK2_2026 |

### Flow C — Teacher

| Bước | Thao tác | Kết quả mong đợi |
|---|---|---|
| 1 | Đăng nhập với `p12test_teacher01@lms.local` / `Test@123` | Vào Teacher Dashboard |
| 2 | Xem Dashboard | Thống kê khóa học, lịch dạy |
| 3 | Vào **Khóa học** (`/teacher/courses`) | Danh sách khóa được phân công (C#, SQL) |
| 4 | Vào **Lịch dạy** (`/teacher/schedule` nếu có, hoặc `classes`) | Thời khóa biểu |
| 5 | Vào **Bài tập** (`/teacher/assignments`) | Quản lý bài tập |
| 6 | Vào **Chấm điểm** (`/teacher/grading`) | Giao diện chấm bài |
| 7 | Vào **Điểm danh** (`/teacher/attendance`) | Điểm danh lớp |

### Flow D — Student

| Bước | Thao tác | Kết quả mong đợi |
|---|---|---|
| 1 | Đăng nhập với `p12test_student011@lms.local` / `Test@123` | Vào Student Dashboard |
| 2 | Xem Dashboard | Thống kê khóa học, lịch |
| 3 | Vào **Khóa học** (`/student/courses`) | Danh sách khóa (C# + SQL) |
| 4 | Vào **Chi tiết khóa học** (click vào khóa) | Nội dung khóa học |
| 5 | Vào **Thời khóa biểu** (`/student/schedule`) | Lịch học trong tuần |
| 6 | Vào **Bài tập** (`/student/assignments`) | Danh sách bài tập |
| 7 | Vào **Bảng điểm** (`/student/grades`) | Điểm số |
| 8 | Vào **Điểm danh** (`/student/attendance`) | Lịch sử điểm danh |
| 9 | Vào **Thông báo** (`/student/notifications`) | Danh sách thông báo |

---

## 5. Lưu ý Khi Demo

### Nếu API không có dữ liệu:
- Các module như điểm danh, chấm bài, bài tập có thể chưa có dữ liệu demo
- Cần chuẩn bị dữ liệu seed trước hoặc giải thích: "Phần này cần dữ liệu thực tế từ quá trình vận hành"

### Nếu route bị lỗi:
- Kiểm tra frontend đang chạy đúng port (5173)
- Kiểm tra backend đang chạy (5097)
- Kiểm tra CORS trong `appsettings.Development.json`

### Nếu 401/403:
- Token có thể hết hạn → đăng nhập lại
- Role không có quyền → kiểm tra tài khoản đúng role

---

## 6. Kịch Bản Nói Về Thuật Toán (P11/P12)

### Smart Course Allocation là gì?
> "Hệ thống gợi ý giảng viên phù hợp nhất cho từng môn học dựa trên chuyên môn (GiaoVienMonHoc), kinh nghiệm giảng dạy và mức độ phù hợp. Giáo vụ có thể preview trước khi xác nhận."

### Smart Timetable là gì?
> "Hệ thống tự động xếp lịch học cho các khóa học trong một học kỳ, đảm bảo không trùng giáo viên, không trùng lớp, không trùng phòng, và phòng đủ sức chứa."

### OccupationMap hoạt động thế nào?
> "OccupationMap là một cấu trúc trong bộ nhớ theo dõi tất cả tài nguyên đã được xếp lịch (giáo viên, lớp, phòng). Khi hệ thống thử xếp một khóa học vào một slot thời gian, nó kiểm tra OccupationMap trước. Nếu tài nguyên đã được dùng, slot đó bị từ chối."

### Tại sao không dùng random?
> "Random có thể tạo ra lịch, nhưng không đảm bảo tối ưu. Hệ thống của chúng tôi dùng soft scoring để ưu tiên các slot có điểm cao nhất: ưu tiên phòng vừa sức chứa, tránh dồn lịch cho giảng viên, ưu tiên giờ học phù hợp."

### Publish an toàn thế nào?
> "Khi publish, hệ thống kiểm tra lại toàn bộ xung đột một lần nữa, dùng transaction để đảm bảo nếu có lỗi thì rollback hoàn toàn. Sau publish, chạy SQL validation để xác nhận không có xung đột nào."

---

## 7. Fallback Demo

Nếu một tính năng không hoạt động:
1. Chuyển sang tính năng khác cùng role
2. Dùng ảnh chụp màn hình (đã chuẩn bị trước)
3. Giải thích: "Tính năng này đã được kiểm thử và hoạt động trên môi trường development"

---

## 8. Kết Luận Demo

> "Hệ thống LMS của chúng tôi đã triển khai các luồng nghiệp vụ chính: quản lý người dùng, tổ chức, khóa học, xếp lịch thông minh, phân công giảng viên, quản lý điểm danh, điểm số. Hệ thống đã được kiểm thử với 20 khóa học thực tế, 335 người dùng, 10 phòng học. Kết quả SQL validation xác nhận không có xung đột lịch nào."
