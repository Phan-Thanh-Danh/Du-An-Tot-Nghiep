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

## Cohorts APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/master-data/cohorts` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Danh sách khóa tuyển sinh có phân trang, tìm kiếm theo mã/tên, lọc năm bắt đầu và trạng thái hoạt động. |
| GET | `/api/master-data/cohorts/{id}` | Admin/SuperAdmin/AcademicStaff/CampusAdmin | Chi tiết khóa tuyển sinh. |
| GET | `/api/master-data/cohorts/{id}/training-program-setup` | Admin/SuperAdmin/Chairman/CampusAdmin/SubCampusAdmin/AcademicStaff | Xem cấu hình setup chương trình đào tạo theo khóa tuyển sinh: chuyên ngành đã/chưa có chương trình và chương trình nguồn đề xuất để clone nếu có. |
| POST | `/api/master-data/cohorts` | SuperAdmin | Tạo khóa tuyển sinh, mã khóa là duy nhất toàn hệ thống và được chuẩn hóa uppercase. |
| PUT | `/api/master-data/cohorts/{id}` | SuperAdmin | Cập nhật khóa tuyển sinh, năm học và trạng thái hoạt động. |
| DELETE | `/api/master-data/cohorts/{id}` | SuperAdmin | Vô hiệu hóa khóa tuyển sinh bằng `ConHoatDong = false`, không xóa vật lý. |
| PATCH | `/api/master-data/cohorts/{id}/activate` | SuperAdmin | Kích hoạt khóa tuyển sinh bằng `ConHoatDong = true`. |
| PATCH | `/api/master-data/cohorts/{id}/deactivate` | SuperAdmin | Vô hiệu hóa khóa tuyển sinh bằng `ConHoatDong = false`. |

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

## Training Programs APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/master-data/training-programs` | SuperAdmin/Chairman/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách chương trình đào tạo chuẩn có phân trang, lọc theo chuyên ngành, ngành, khóa tuyển sinh, trạng thái và trạng thái hoạt động. `MaDonVi` chỉ dùng để lọc chương trình có chuyên ngành đang được mở tại cơ sở qua `ChuyenNganhTheoCoSo`; campus chỉ xem chương trình `active`. |
| GET | `/api/master-data/training-programs/{id}` | SuperAdmin/Chairman/CampusAdmin/SubCampusAdmin/AcademicStaff | Chi tiết chương trình đào tạo chuẩn; campus chỉ xem được chương trình active thuộc chuyên ngành đang được mở trong phạm vi cơ sở của mình. |
| POST | `/api/master-data/training-programs` | SuperAdmin | Tạo chương trình đào tạo chuẩn theo `ChuyenNganh + KhoaTuyenSinh`, mã chương trình được chuẩn hóa uppercase và mặc định ở `draft`. |
| POST | `/api/master-data/training-programs/{id}/clone` | SuperAdmin | Clone chương trình đã `approved` hoặc `active` sang khóa tuyển sinh mới. Bản clone chỉ tạo bản ghi `ChuongTrinhDaoTao`, giữ `TrangThai = draft`, `ConHoatDong = true`, lưu `NguonChuongTrinhId` và vẫn phải đi qua workflow gửi duyệt/duyệt/kích hoạt. |
| PUT | `/api/master-data/training-programs/{id}` | SuperAdmin | Cập nhật chương trình đào tạo chuẩn nếu chương trình đang `draft` hoặc `rejected`. |
| DELETE | `/api/master-data/training-programs/{id}` | SuperAdmin | Vô hiệu hóa mềm chương trình đào tạo bằng `TrangThai = inactive`, `ConHoatDong = false`. |
| PATCH | `/api/master-data/training-programs/{id}/submit` | SuperAdmin | Gửi duyệt chương trình `draft` hoặc `rejected`, chuyển sang `pending_approval`. |
| PATCH | `/api/master-data/training-programs/{id}/approve` | Chairman | Chủ tịch duyệt chương trình đang `pending_approval`, chuyển sang `approved`. Body tùy chọn `{ "ghiChuDuyet": "..." }`. |
| PATCH | `/api/master-data/training-programs/{id}/reject` | Chairman | Chủ tịch từ chối chương trình đang `pending_approval`, chuyển sang `rejected`. Body bắt buộc `{ "lyDoTuChoi": "..." }`. |
| PATCH | `/api/master-data/training-programs/{id}/activate` | Chairman | Kích hoạt chương trình đã duyệt hoặc đang inactive bằng `TrangThai = active`, `ConHoatDong = true`; không cho có chương trình active khác cùng chuyên ngành và khóa tuyển sinh. |
| PATCH | `/api/master-data/training-programs/{id}/deactivate` | Chairman | Vô hiệu hóa chương trình active bằng `TrangThai = inactive`, `ConHoatDong = false`. |
| PATCH | `/api/master-data/training-programs/{id}/archive` | Chairman | Lưu trữ chương trình approved/active/inactive bằng `TrangThai = archived`, `ConHoatDong = false`. |

Ghi chú: `ChuongTrinhDaoTao` là khung chuẩn toàn hệ thống theo `ChuyenNganh + KhoaTuyenSinh`. `ChuyenNganhTheoCoSo` chỉ xác định cơ sở nào được phép mở chuyên ngành và được dùng cho scope/filter khi campus xem dữ liệu; không phải khóa tạo chương trình. Người duyệt cuối cùng của workflow là `Chairman`/Chủ tịch, không dùng `Principal` cho luồng duyệt này. Danh sách môn học trong chương trình nằm ở module `MonHocTrongChuongTrinh` và được clone cùng khung chương trình; nội dung học liệu/deep syllabus không được clone trong luồng này.

## Training Program Subjects APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/master-data/training-program-subjects` | SuperAdmin/Chairman/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách môn học trong chương trình có phân trang, lọc theo chương trình, môn học, học kỳ, loại môn, bắt buộc và trạng thái hoạt động. Campus/SubCampus/AcademicStaff chỉ xem chương trình active trong scope cơ sở. |
| GET | `/api/master-data/training-program-subjects/{id}` | SuperAdmin/Chairman/CampusAdmin/SubCampusAdmin/AcademicStaff | Chi tiết môn học trong chương trình. |
| GET | `/api/master-data/training-program-subjects/by-program/{programId}` | SuperAdmin/Chairman/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách môn học đang hoạt động của một chương trình, sắp theo học kỳ, thứ tự và tên môn. |
| POST | `/api/master-data/training-program-subjects` | SuperAdmin | Thêm môn học vào chương trình đào tạo đang `draft` hoặc `rejected`; không cho trùng `MaChuongTrinh + MaMonHoc`. |
| PUT | `/api/master-data/training-program-subjects/{id}` | SuperAdmin | Cập nhật học kỳ, tín chỉ, loại môn, bắt buộc, thứ tự, ghi chú và trạng thái hoạt động nếu chương trình đang `draft` hoặc `rejected`. |
| DELETE | `/api/master-data/training-program-subjects/{id}` | SuperAdmin | Xóa mềm môn học khỏi chương trình bằng `ConHoatDong = false` nếu chương trình đang `draft` hoặc `rejected`. |

Ghi chú: `MonHocTrongChuongTrinh` chỉ khai báo danh sách môn trong khung chương trình đào tạo. Nội dung chi tiết môn học, video, bài học, quiz và đề cương chi tiết vẫn thuộc các module khác như `CourseSyllabus`; clone chương trình đào tạo chỉ clone danh sách môn trong chương trình, không clone nội dung học liệu.

## Building APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/master-data/buildings` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách tòa nhà có phân trang, tìm kiếm theo mã/tên, lọc đơn vị/trạng thái và scope theo `MaDonVi`. |
| GET | `/api/master-data/buildings/{id}` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff | Chi tiết tòa nhà trong phạm vi đơn vị được xem. |
| POST | `/api/master-data/buildings` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin | Tạo tòa nhà trong đơn vị được quản lý; mã tòa nhà không trùng trong cùng đơn vị. |
| PUT | `/api/master-data/buildings/{id}` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin | Cập nhật tòa nhà trong phạm vi đơn vị được quản lý. |
| DELETE | `/api/master-data/buildings/{id}` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin | Xóa mềm tòa nhà bằng `ConHoatDong = false`; không cho xóa nếu còn tầng hoặc phòng học hoạt động. |

## Floor APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/master-data/floors` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách tầng/lầu có phân trang, tìm kiếm theo tên tầng/tòa, lọc đơn vị, tòa nhà, trạng thái và scope theo `MaDonVi`. |
| GET | `/api/master-data/floors/{id}` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff | Chi tiết tầng/lầu trong phạm vi đơn vị được xem. |
| GET | `/api/master-data/buildings/{buildingId}/floors` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách tầng/lầu của một tòa nhà. |
| POST | `/api/master-data/floors` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin | Tạo tầng/lầu trong tòa nhà được quản lý; `ThuTuTang` không trùng trong cùng tòa nhà. |
| PUT | `/api/master-data/floors/{id}` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin | Cập nhật tầng/lầu trong phạm vi đơn vị được quản lý. |
| DELETE | `/api/master-data/floors/{id}` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin | Xóa mềm tầng/lầu bằng `ConHoatDong = false`; không cho xóa nếu còn phòng học hoạt động. |

## Rooms APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/master-data/rooms` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách phòng học có phân trang, tìm kiếm theo mã/tên phòng/tòa/tầng, lọc đơn vị, tòa nhà, tầng, loại phòng, trạng thái và scope theo `MaDonVi`. |
| GET | `/api/master-data/rooms/{id}` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff | Chi tiết phòng học gồm cơ sở, tòa nhà và tầng/lầu trong phạm vi được xem. |
| GET | `/api/master-data/floors/{floorId}/rooms` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách phòng học thuộc một tầng/lầu. |
| POST | `/api/master-data/rooms` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin | Tạo phòng học trong phạm vi đơn vị được quản lý; bắt buộc `MaDonVi`, `MaToaNha`, `MaTang`, kiểm tra tầng thuộc tòa và tòa thuộc đơn vị. |
| PUT | `/api/master-data/rooms/{id}` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin | Cập nhật phòng học trong phạm vi đơn vị được quản lý; validate tòa/tầng, loại phòng, trạng thái, sức chứa và mã phòng. |
| DELETE | `/api/master-data/rooms/{id}` | SuperAdmin/Admin/CampusAdmin/SubCampusAdmin | Xóa mềm phòng học bằng `TrangThaiPhong = ngung_hoat_dong`, không xóa vật lý. |

Ghi chú: cấu trúc phòng học hiện theo mô hình `DonVi -> ToaNha -> Tang -> PhongHoc`. Migration `AddFacilityBuildingFloorRoomStructure` thêm bảng `ToaNha`, `Tang`, cột `MaToaNha`, `MaTang`, `GhiChu` cho `PhongHoc` và backfill phòng học cũ vào `Tòa nhà mặc định / Tầng 1`.

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

## Finance APIs

### Cấu Hình Học Phí Chương Trình - Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/finance/program-tuition-configs` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Danh sách cấu hình học phí chương trình có phân trang; lọc theo `keyword`, `maDonVi`, `maChuongTrinhDaoTao`, `maHocKy`, `namHocTrongChuongTrinh`, `hocKyTrongNam`, `soThuTuHocKy`, `conHoatDong`. Response có `coDuocSua`, `lyDoKhongDuocSua` theo ngày học kỳ. |
| GET | `/api/finance/program-tuition-configs/{id}` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Chi tiết cấu hình học phí gồm cơ sở, chương trình đào tạo, học kỳ, học phí chính khóa, phí học liệu, tổng dự kiến và trạng thái có được sửa. |
| POST | `/api/finance/program-tuition-configs` | SuperAdmin | Tạo cấu hình học phí cố định theo `MaDonVi + MaChuongTrinhDaoTao + MaHocKy`; MVP chỉ hỗ trợ `loaiCachTinhHocPhi = co_dinh_theo_hoc_ky`. |
| POST | `/api/finance/program-tuition-configs/bulk` | SuperAdmin | Tạo/cập nhật hàng loạt cấu hình cho toàn bộ chương trình, ví dụ 3 năm x 3 học kỳ = 9 dòng. Nếu chương trình đã có cấu hình active thì cần `confirmReplace = true`; backend chỉ thay thế kỳ chưa diễn ra, giữ nguyên kỳ đang diễn ra/đã kết thúc. |
| PUT | `/api/finance/program-tuition-configs/{id}` | SuperAdmin | Cập nhật cấu hình học phí; backend tự tính `tongTienDuKien = soTienHocPhi + tienHocLieu`. Chỉ cho sửa học kỳ chưa diễn ra. |
| PATCH | `/api/finance/program-tuition-configs/{id}/deactivate` | SuperAdmin | Vô hiệu hóa mềm cấu hình bằng `ConHoatDong = false`; không hard delete trong MVP. Chỉ cho deactivate học kỳ chưa diễn ra. |

Ghi chú: Module này chỉ quản lý cấu hình học phí chương trình đào tạo, chưa sinh hóa đơn, chưa tích hợp VNPay/MoMo, chưa xử lý học bổng/miễn giảm phức tạp và không có logic dự bị tiếng Anh. Cấu hình active không được trùng theo `MaDonVi + MaChuongTrinhDaoTao + MaHocKy`. API áp dụng template cấu hình riêng vào chương trình là roadmap, chưa có controller/entity template trong MVP.

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
