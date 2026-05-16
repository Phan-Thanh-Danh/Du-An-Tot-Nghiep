# API Contract

Tài liệu này phân biệt rõ endpoint **đã có** và endpoint **dự kiến**. Chỉ đánh dấu **đã có** khi đã thấy controller/action trong `Backend/Controllers`.

Base path hiện tại: `/api`

## Response/Error Chung

- Thành công ở API mới: `{ success, message, data }`.
- Validation lỗi model: `400` với `{ success, message, errors, traceId, statusCode }`.
- Authentication lỗi: `401` với `{ success, message, errors, traceId, statusCode }`.
- Authorization lỗi: `403` với `{ success, message, errors, traceId, statusCode }`.
- Lỗi nghiệp vụ nên dùng `ApiException` và đi qua `ExceptionMiddleware`.

## Auth APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| POST | `/api/auth/login` | Public | Đăng nhập bằng email/password, trả access token, expiresAt, requiresPasswordChange, user. |
| POST | `/api/auth/refresh-token` | Public | Xoay vòng refresh token và cấp access token mới. |
| POST | `/api/auth/logout` | Public | Thu hồi refresh token hiện tại. |
| POST | `/api/auth/revoke-token` | Admin/SuperAdmin/CampusAdmin | Thu hồi refresh token theo yêu cầu quản trị. |
| POST | `/api/auth/change-password` | JWT | Đổi mật khẩu người dùng hiện tại. |
| POST | `/api/auth/forgot-password` | Public | Gửi OTP đặt lại mật khẩu qua email. |
| POST | `/api/auth/verify-otp` | Public | Xác thực OTP đặt lại mật khẩu. |
| POST | `/api/auth/reset-password` | Public | Đặt lại mật khẩu bằng OTP đã xác thực. |

### Dự kiến/cần bổ sung

- `GET /api/auth/me`

## Users APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/admin/accounts` | Admin/SuperAdmin/CampusAdmin | Endpoint mẫu, chưa phải account management đầy đủ. |
| GET | `/api/account/me` | JWT | Xem hồ sơ tài khoản hiện tại. |
| PUT | `/api/account/profile` | JWT | Cập nhật email, họ tên, số điện thoại của tài khoản hiện tại. |
| PUT | `/api/account/change-password` | JWT | Đổi mật khẩu tài khoản hiện tại. |
| GET | `/api/admin/users` | Admin/SuperAdmin/CampusAdmin | Danh sách user có phân trang, tìm kiếm, lọc role/trạng thái/đơn vị và scope theo cơ sở. |
| GET | `/api/admin/users/{id}` | Admin/SuperAdmin/CampusAdmin | Chi tiết user, gồm vai trò, đơn vị và lớp hành chính nếu là học sinh. |
| POST | `/api/admin/users` | Admin/SuperAdmin/CampusAdmin | Tạo user, hash password, gán role và ghi audit. |
| PUT | `/api/admin/users/{id}` | Admin/SuperAdmin/CampusAdmin | Cập nhật thông tin, đơn vị, lớp, vai trò và ghi audit. |
| PATCH | `/api/admin/users/{id}/lock` | Admin/SuperAdmin/CampusAdmin | Khóa tài khoản bằng trạng thái `bi_khoa`, không xóa vật lý. |
| PATCH | `/api/admin/users/{id}/unlock` | Admin/SuperAdmin/CampusAdmin | Mở khóa tài khoản bằng trạng thái `hoat_dong`. |
| PATCH | `/api/admin/users/{id}/reset-password` | Admin/SuperAdmin/CampusAdmin | Admin đặt lại mật khẩu user và ghi audit. |

### Dự kiến/cần bổ sung

- Import CSV tài khoản.

## RBAC APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/admin/rbac/roles` | Admin/SuperAdmin/CampusAdmin | Danh sách vai trò. |
| GET | `/api/admin/rbac/roles/{id}` | Admin/SuperAdmin/CampusAdmin | Chi tiết vai trò. |
| POST | `/api/admin/rbac/roles` | Admin/SuperAdmin | Tạo vai trò hệ thống; chỉ chấp nhận mã vai trò được backend hỗ trợ. |
| PUT | `/api/admin/rbac/roles/{id}` | Admin/SuperAdmin | Cập nhật mã/tên vai trò; không đổi mã nếu vai trò đang được gán. |
| DELETE | `/api/admin/rbac/roles/{id}` | Admin/SuperAdmin | Xóa vai trò nếu chưa được gán cho user. |
| GET | `/api/admin/rbac/users/{userId}/roles` | Admin/SuperAdmin/CampusAdmin | Xem vai trò đã gán cho user theo scope đơn vị. |
| PUT | `/api/admin/rbac/users/{userId}/roles` | Admin/SuperAdmin/CampusAdmin | Gán lại vai trò cho user, cập nhật `PhanQuyenNguoiDung` và `NguoiDung.VaiTroChinh`. |

### Dự kiến/cần bổ sung

- Ma trận permission chi tiết theo từng action nếu mở rộng ngoài role-based authorization hiện tại.

## Administrative Classes APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/admin/classes` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Danh sách lớp hành chính có phân trang, tìm kiếm, lọc đơn vị/trạng thái và scope theo cơ sở. |
| GET | `/api/admin/classes/{id}` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Chi tiết lớp hành chính. |
| POST | `/api/admin/classes` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Tạo lớp hành chính, mã lớp là duy nhất toàn hệ thống. |
| PUT | `/api/admin/classes/{id}` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Cập nhật lớp hành chính, đơn vị, giáo viên chủ nhiệm, năm nhập học và trạng thái. |
| DELETE | `/api/admin/classes/{id}` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Xóa mềm lớp hành chính bằng `ConHoatDong = false`. |

### Dự kiến/cần bổ sung

- Import danh sách lớp hành chính từ CSV/Excel nếu cần cho dữ liệu demo lớn.

## Academic Terms APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/master-data/academic-terms` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Danh sách học kỳ có phân trang, tìm kiếm, lọc đơn vị/trạng thái khóa và scope theo cơ sở. |
| GET | `/api/master-data/academic-terms/{id}` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Chi tiết học kỳ. |
| POST | `/api/master-data/academic-terms` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Tạo học kỳ, mã học kỳ là duy nhất trong từng đơn vị. |
| PUT | `/api/master-data/academic-terms/{id}` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Cập nhật học kỳ nếu học kỳ chưa khóa. |
| DELETE | `/api/master-data/academic-terms/{id}` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Xóa học kỳ đã khóa nếu chưa có dữ liệu liên quan như lớp học phần, đợt đăng ký, điểm, thời khóa biểu hoặc hóa đơn. |
| PATCH | `/api/master-data/academic-terms/{id}/lock` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Khóa học kỳ bằng `DaKhoa = true`. |
| PATCH | `/api/master-data/academic-terms/{id}/unlock` | SuperAdmin/CampusAdmin | Mở khóa học kỳ trong phạm vi đơn vị được quản lý. |

## Subjects APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/master-data/subjects` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Danh sách môn học có phân trang, tìm kiếm theo mã/tên và lọc trạng thái hoạt động. |
| GET | `/api/master-data/subjects/{id}` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Chi tiết môn học. |
| POST | `/api/master-data/subjects` | SuperAdmin | Tạo môn học, mã môn học là duy nhất toàn hệ thống và được chuẩn hóa uppercase. |
| PUT | `/api/master-data/subjects/{id}` | SuperAdmin | Cập nhật môn học và trạng thái hoạt động. |
| DELETE | `/api/master-data/subjects/{id}` | SuperAdmin | Khóa mềm môn học bằng `ConHoatDong = false`, không xóa vật lý. |
| PATCH | `/api/master-data/subjects/{id}/activate` | SuperAdmin | Mở khóa môn học bằng `ConHoatDong = true`. |
| PATCH | `/api/master-data/subjects/{id}/deactivate` | SuperAdmin | Khóa môn học bằng `ConHoatDong = false`. |

## Majors APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/master-data/majors` | SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách ngành đào tạo có phân trang, tìm kiếm theo mã/tên và lọc trạng thái hoạt động. |
| GET | `/api/master-data/majors/{id}` | SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Chi tiết ngành đào tạo. |
| POST | `/api/master-data/majors` | SuperAdmin | Tạo ngành đào tạo, mã ngành là duy nhất toàn hệ thống và được chuẩn hóa uppercase. |
| PUT | `/api/master-data/majors/{id}` | SuperAdmin | Cập nhật ngành đào tạo và trạng thái hoạt động. |
| DELETE | `/api/master-data/majors/{id}` | SuperAdmin | Khóa mềm ngành đào tạo bằng `ConHoatDong = false`, không xóa vật lý. |
| PATCH | `/api/master-data/majors/{id}/activate` | SuperAdmin | Mở khóa ngành đào tạo bằng `ConHoatDong = true`. |
| PATCH | `/api/master-data/majors/{id}/deactivate` | SuperAdmin | Khóa ngành đào tạo bằng `ConHoatDong = false`. |

## Specializations APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/master-data/specializations` | SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách chuyên ngành chuẩn toàn hệ thống có phân trang, tìm kiếm, lọc ngành và trạng thái hoạt động. |
| GET | `/api/master-data/specializations/{id}` | SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Chi tiết chuyên ngành chuẩn. |
| POST | `/api/master-data/specializations` | SuperAdmin | Tạo chuyên ngành chuẩn thuộc một ngành đào tạo đang hoạt động, mã chuyên ngành là duy nhất toàn hệ thống và được chuẩn hóa uppercase. |
| PUT | `/api/master-data/specializations/{id}` | SuperAdmin | Cập nhật chuyên ngành chuẩn, ngành cha và trạng thái hoạt động. |
| DELETE | `/api/master-data/specializations/{id}` | SuperAdmin | Khóa mềm chuyên ngành chuẩn bằng `ConHoatDong = false`, không xóa vật lý. |
| PATCH | `/api/master-data/specializations/{id}/activate` | SuperAdmin | Mở khóa chuyên ngành chuẩn bằng `ConHoatDong = true`. |
| PATCH | `/api/master-data/specializations/{id}/deactivate` | SuperAdmin | Khóa chuyên ngành chuẩn bằng `ConHoatDong = false`. |

## Campus Specializations APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/master-data/campus-specializations` | SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách chuyên ngành được mở theo cơ sở, có phân trang, lọc theo campus scope, ngành, chuyên ngành, đơn vị, trạng thái và trạng thái hoạt động. |
| GET | `/api/master-data/campus-specializations/{id}` | SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Chi tiết chuyên ngành theo cơ sở trong phạm vi được xem. |
| POST | `/api/master-data/campus-specializations` | SuperAdmin/CampusAdmin | SuperAdmin tạo cho mọi cơ sở; CampusAdmin chỉ tạo đề xuất `pending_approval` trong campus/sub-campus thuộc phạm vi. |
| PUT | `/api/master-data/campus-specializations/{id}` | SuperAdmin | Cập nhật chuyên ngành theo cơ sở. |
| DELETE | `/api/master-data/campus-specializations/{id}` | SuperAdmin | Khóa mềm chuyên ngành theo cơ sở, đặt `ConHoatDong = false` và `TrangThai = inactive`. |
| PATCH | `/api/master-data/campus-specializations/{id}/activate` | SuperAdmin | Kích hoạt chuyên ngành tại cơ sở, đặt `TrangThai = active`. |
| PATCH | `/api/master-data/campus-specializations/{id}/deactivate` | SuperAdmin | Vô hiệu hóa chuyên ngành tại cơ sở, đặt `TrangThai = inactive`. |
| PATCH | `/api/master-data/campus-specializations/{id}/approve` | SuperAdmin | Phê duyệt đề xuất mở chuyên ngành tại cơ sở, đặt `TrangThai = approved`. |
| PATCH | `/api/master-data/campus-specializations/{id}/reject` | SuperAdmin | Từ chối đề xuất mở chuyên ngành tại cơ sở, đặt `TrangThai = rejected` và có thể cập nhật `GhiChu`. |

## Course Syllabuses APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/master-data/course-syllabuses` | SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff/Teacher | Danh sách đề cương môn học theo môn gốc và chuyên ngành, có phân trang, lọc theo campus scope, môn, ngành, chuyên ngành, đơn vị, trạng thái. |
| GET | `/api/master-data/course-syllabuses/{id}` | SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff/Teacher | Chi tiết đề cương môn học trong phạm vi được xem. |
| POST | `/api/master-data/course-syllabuses` | SuperAdmin/CampusAdmin | SuperAdmin tạo đề cương chuẩn `MaDonVi = null` hoặc override mọi cơ sở; CampusAdmin chỉ tạo override trong campus/sub-campus thuộc phạm vi. |
| PUT | `/api/master-data/course-syllabuses/{id}` | SuperAdmin | Cập nhật đề cương môn học. |
| DELETE | `/api/master-data/course-syllabuses/{id}` | SuperAdmin | Lưu trữ mềm đề cương môn học bằng `TrangThai = archived`, `ConHoatDong = false`. |
| PATCH | `/api/master-data/course-syllabuses/{id}/activate` | SuperAdmin | Kích hoạt đề cương môn học bằng `TrangThai = active`, `ConHoatDong = true`. |
| PATCH | `/api/master-data/course-syllabuses/{id}/deactivate` | SuperAdmin | Vô hiệu hóa đề cương môn học bằng `TrangThai = inactive`, `ConHoatDong = false`. |
| PATCH | `/api/master-data/course-syllabuses/{id}/approve` | SuperAdmin | Duyệt đề cương môn học bằng `TrangThai = approved`, `ConHoatDong = true`. |
| PATCH | `/api/master-data/course-syllabuses/{id}/archive` | SuperAdmin | Lưu trữ đề cương môn học bằng `TrangThai = archived`, `ConHoatDong = false`. |

Ghi chú: `DanhMucMonHoc` chỉ là môn học gốc. Nội dung chương/bài học chuẩn theo chuyên ngành sẽ gắn với `CourseSyllabus` trong các migration/module tiếp theo. TODO: thêm `MaSyllabus` vào `LopHocPhan` để lớp học phần biết đang dùng phiên bản đề cương nào.

## Organizations APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/organizations` | JWT | Danh sách tổ chức theo phạm vi user. |
| GET | `/api/organizations/tree` | JWT | Cây tổ chức theo phạm vi user. |
| GET | `/api/organizations/{id}` | JWT | Chi tiết tổ chức. |
| POST | `/api/organizations` | SuperAdmin | Tạo tổ chức. |
| PUT | `/api/organizations/{id}` | SuperAdmin | Cập nhật tổ chức. |
| DELETE | `/api/organizations/{id}` | SuperAdmin | Xóa mềm tổ chức. |
| DELETE | `/api/organizations/{id}/hard-delete` | SuperAdmin | Xóa cứng nếu không có dữ liệu liên quan. |
| GET | `/api/organizations/{id}/subtree` | JWT | Lấy cây con của tổ chức. |

### Dự kiến/cần bổ sung

- `PATCH /api/organizations/{id}/restore`
- `GET /api/organizations/{id}/users`
- `GET /api/organizations/{id}/courses`

## Courses APIs

### Đã có

Chưa thấy controller course trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/courses`
- `GET /api/courses/{id}`
- `POST /api/courses`
- `PUT /api/courses/{id}`
- `PATCH /api/courses/{id}/publish`
- `GET /api/courses/{id}/chapters`
- `GET /api/students/me/courses`

## Lessons APIs

### Đã có

Chưa thấy controller lesson trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/courses/{courseId}/lessons`
- `GET /api/lessons/{id}`
- `POST /api/courses/{courseId}/lessons`
- `PUT /api/lessons/{id}`
- `PATCH /api/lessons/{id}/progress`
- `POST /api/lessons/{id}/comments`

## Assignments APIs

### Đã có

Chưa thấy controller assignment trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/assignments`
- `GET /api/assignments/{id}`
- `POST /api/courses/{courseId}/assignments`
- `PUT /api/assignments/{id}`
- `PATCH /api/assignments/{id}/publish`
- `PATCH /api/assignments/{id}/close`

## Submissions APIs

### Đã có

Chưa thấy controller submission trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/assignments/{assignmentId}/submissions`
- `GET /api/submissions/{id}`
- `POST /api/assignments/{assignmentId}/submissions`
- `PUT /api/submissions/{id}/grade`
- `GET /api/students/me/submissions`

## Exams/Quiz APIs

### Đã có

Chưa thấy controller exams/quiz trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/exams`
- `GET /api/exams/{id}`
- `POST /api/exams`
- `PUT /api/exams/{id}`
- `GET /api/exams/{id}/questions`
- `POST /api/exams/{id}/start`
- `POST /api/exam-sessions/{sessionId}/submit`
- `GET /api/exam-sessions/{sessionId}/result`

## Attendance APIs

### Đã có

Chưa thấy controller attendance trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/attendance`
- `GET /api/students/me/attendance`
- `POST /api/sessions/{sessionId}/attendance`
- `PUT /api/attendance/{id}`
- `POST /api/attendance-unlock-requests`
- `PUT /api/attendance-unlock-requests/{id}/approve`

## Grades APIs

### Đã có

Chưa thấy controller grades trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/grades`
- `GET /api/students/me/grades`
- `PUT /api/grades/{id}`
- `POST /api/grade-change-requests`
- `PUT /api/grade-change-requests/{id}/approve`
- `GET /api/courses/{courseId}/gradebook`

## Notifications APIs

### Đã có

Chưa thấy controller notification trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/notifications`
- `GET /api/notifications/{id}`
- `POST /api/notifications`
- `PATCH /api/notifications/{id}/read`
- `GET /api/notification-preferences`
- `PUT /api/notification-preferences`

## Reports APIs

### Đã có

Chưa thấy controller reports trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/reports/dashboard`
- `POST /api/reports/export`
- `GET /api/reports/attendance-risk`
- `GET /api/reports/failure-risk`
- `GET /api/reports/classroom-usage`
- `GET /api/audit-logs`

## AI APIs

### Đã có

Chưa thấy controller AI trong repo hiện tại.

### Dự kiến/cần bổ sung

- `POST /api/ai/lessons/{lessonId}/summarize`
- `POST /api/ai/submissions/{submissionId}/plagiarism-check`
- `POST /api/ai/submissions/{submissionId}/grade-suggestion`
- `GET /api/ai/risk/attendance`
- `GET /api/ai/risk/failure`
- `POST /api/ai/evaluations/sentiment`

## Quy Tắc Cập Nhật Contract

- Thêm controller mới thì cập nhật file này trong cùng PR/task.
- Nếu endpoint chỉ là ý tưởng, để ở mục `Dự kiến/cần bổ sung`.
- Không sửa frontend gọi endpoint dự kiến nếu backend chưa có mock/server contract rõ ràng.
