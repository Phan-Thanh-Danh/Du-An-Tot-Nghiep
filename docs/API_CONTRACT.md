# API Contract

Tài liệu này phân biệt rõ endpoint **đã có** và endpoint **dự kiến**. Chỉ đánh dấu **đã có** khi đã thấy controller/action trong `Backend/Controllers`.

Base path hiện tại: `/api`

Ghi chú thuật ngữ: `DonVi` là tên bảng/entity kỹ thuật trong backend. Trong nghiệp vụ và dữ liệu demo, `DonVi` tương ứng **Cơ sở đào tạo**; không rename bảng để tránh phá migration/schema hiện có.

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
| POST | `/api/master-data/specializations` | SuperAdmin | Tạo chuyên ngành chuẩn thuộc một ngành đào tạo đang hoạt động; tên chuyên ngành là duy nhất trong ngành đào tạo. |
| PUT | `/api/master-data/specializations/{id}` | SuperAdmin | Cập nhật chuyên ngành chuẩn, ngành cha và trạng thái hoạt động; không dùng mã code riêng cho chuyên ngành. |
| DELETE | `/api/master-data/specializations/{id}` | SuperAdmin | Khóa mềm chuyên ngành chuẩn bằng `ConHoatDong = false`, không xóa vật lý. |
| PATCH | `/api/master-data/specializations/{id}/activate` | SuperAdmin | Mở khóa chuyên ngành chuẩn bằng `ConHoatDong = true`. |
| PATCH | `/api/master-data/specializations/{id}/deactivate` | SuperAdmin | Khóa chuyên ngành chuẩn bằng `ConHoatDong = false`. |

Ghi chú: `ChuyenNganh` không có mã code riêng; khi response cần mã ngành để hiển thị, backend lấy `MaCodeNganh` bằng join sang `NganhDaoTao`.

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

Ghi chú: `MonHocTrongChuongTrinh` chỉ khai báo danh sách môn trong khung chương trình đào tạo. Trong MVP, nội dung học liệu chuẩn đi theo `DanhMucMonHoc -> Chuong -> BaiHoc` và bài tập chuẩn đi theo `DanhMucMonHoc -> BaiTap`; clone chương trình đào tạo chỉ clone danh sách môn trong chương trình, không clone nội dung học liệu.

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

## CaHoc APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/ca-hoc` | Admin/SuperAdmin/AcademicStaff/CampusAdmin/Chairman/HoiDongQuanLyNoiDung | Danh sách ca học có phân trang, tìm kiếm theo tên ca/buổi và lọc trạng thái hoạt động. |
| GET | `/api/ca-hoc/active` | Admin/SuperAdmin/AcademicStaff/CampusAdmin/Chairman/HoiDongQuanLyNoiDung | Danh sách ca học đang hoạt động để frontend dùng dropdown khi tạo/sửa thời khóa biểu. |
| GET | `/api/ca-hoc/{id}` | Admin/SuperAdmin/AcademicStaff/CampusAdmin/Chairman/HoiDongQuanLyNoiDung | Chi tiết ca học. |
| POST | `/api/ca-hoc` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Tạo ca học; validate tên ca không trùng, buổi chỉ nhận `sang`, `chieu`, `toi`, giờ hợp lệ và thứ tự lớn hơn 0. |
| PUT | `/api/ca-hoc/{id}` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Cập nhật ca học; không cho sửa giờ bắt đầu/kết thúc nếu ca đã được dùng trong `ThoiKhoaBieu` hoặc `BuoiHoc`. |
| PATCH | `/api/ca-hoc/{id}/toggle-active` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Đảo trạng thái `ConHoatDong`, không hard delete ca học. |

Ghi chú: `CaHoc` là danh mục ca học cố định dùng bởi `ThoiKhoaBieu` và `BuoiHoc`. Task P0-1 chưa triển khai CRUD thời khóa biểu, sinh buổi học hoặc điểm danh.

## ThoiKhoaBieu APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/thoi-khoa-bieu` | Admin/SuperAdmin/CampusAdmin/AcademicStaff | Danh sách thời khóa biểu có phân trang, lọc theo khóa học, học kỳ, lớp, giáo viên, phòng, ca học, thứ, trạng thái và khoảng ngày. Scope dữ liệu theo `KhoaHoc.MaDonVi`. |
| GET | `/api/thoi-khoa-bieu/{id}` | Admin/SuperAdmin/CampusAdmin/AcademicStaff | Chi tiết thời khóa biểu gồm thông tin khóa học, học kỳ, lớp, môn, giáo viên, ca học và phòng học. |
| POST | `/api/thoi-khoa-bieu/check-xung-dot` | Admin/SuperAdmin/CampusAdmin/AcademicStaff | Kiểm tra xung đột theo `MaHocKy + ThuTrongTuan + MaCaHoc`, gồm trùng giáo viên, lớp hành chính và phòng học. Request hợp lệ luôn trả `200`, kể cả khi có xung đột. |
| POST | `/api/thoi-khoa-bieu` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Tạo thời khóa biểu từ `MaKhoaHoc + ThuTrongTuan + MaCaHoc + MaPhong`, mặc định `TrangThai = nhap` nếu bỏ trống. Không nhập trực tiếp lớp, môn hoặc giáo viên. |
| PUT | `/api/thoi-khoa-bieu/{id}` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Cập nhật thời khóa biểu chưa bị hủy; không cho duplicate `MaKhoaHoc + ThuTrongTuan + MaCaHoc` với bản ghi chưa `da_huy`. |
| PATCH | `/api/thoi-khoa-bieu/{id}/cancel` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Hủy thời khóa biểu bằng `TrangThai = da_huy`, không xóa vật lý. |
| POST | `/api/thoi-khoa-bieu/{id}/generate-sessions` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Sinh `BuoiHoc` từ thời khóa biểu đã `da_xuat_ban`; chỉ tạo các ngày trùng `ThuTrongTuan`, bỏ qua buổi đã tồn tại theo `MaTkb + NgayHoc`. |

Ghi chú: P0-3 kiểm tra xung đột lịch ở mức `MaHocKy + ThuTrongTuan + MaCaHoc`, bỏ qua bản ghi `da_huy` và bỏ qua chính bản ghi hiện tại khi update bằng `excludeMaTkb`. P0-4 sinh `BuoiHoc` từ TKB đã xuất bản nhưng chưa làm điểm danh, đổi lịch, dạy thay, đổi phòng, đổi ca hoặc frontend. Unique index `UQ_ThoiKhoaBieu_KhoaHoc_Thu_Ca` chỉ áp dụng cho bản ghi có `TrangThai <> N'da_huy'`, nên có thể tạo lại lịch cùng khóa học/thứ/ca sau khi bản ghi cũ đã hủy.

## BuoiHoc APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/buoi-hoc` | Admin/SuperAdmin/CampusAdmin/AcademicStaff | Danh sách buổi học có phân trang, lọc theo thời khóa biểu, khóa học, giáo viên, phòng, ca học, trạng thái buổi và khoảng ngày. Scope dữ liệu theo `KhoaHoc.MaDonVi`. |
| GET | `/api/buoi-hoc/{id}` | Admin/SuperAdmin/CampusAdmin/AcademicStaff | Chi tiết buổi học gồm thông tin khóa học, học kỳ, lớp, môn, ngày học, ca học, phòng, giáo viên, giáo viên dạy thay và trạng thái điểm danh. |
| PUT | `/api/buoi-hoc/{id}/change-teacher` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Đổi giáo viên dạy thay cho buổi học chưa hủy, chưa diễn ra và chưa khóa điểm danh. Request gồm `maGiaoVienDayThay`, `lyDoThayDoi`. |
| PUT | `/api/buoi-hoc/{id}/change-room` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Đổi phòng học cho buổi học chưa hủy, chưa diễn ra và chưa khóa điểm danh. Request gồm `maPhong`, `lyDoThayDoi`. |
| PUT | `/api/buoi-hoc/{id}/change-shift` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Đổi ca học cho buổi học chưa hủy, chưa diễn ra và chưa khóa điểm danh. Request gồm `maCaHoc`, `lyDoThayDoi`. |
| PATCH | `/api/buoi-hoc/{id}/cancel` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Hủy buổi học bằng `TrangThaiBuoi = da_huy`, không xóa vật lý. Request gồm `lyDoThayDoi`. |

Ghi chú: `BuoiHoc` là buổi học thật theo ngày cụ thể, sinh từ `ThoiKhoaBieu + NgayHoc`. P0-5 hỗ trợ điều chỉnh phát sinh từng buổi học nhưng không sửa `ThoiKhoaBieu`, không generate lại lịch, không làm điểm danh và không xóa vật lý. Khi đổi giáo viên/phòng/ca, hệ thống kiểm tra xung đột theo `NgayHoc + MaCaHoc`: trùng giáo viên hiệu lực (`MaGiaoVienDayThay ?? MaGiaoVien`), trùng lớp hành chính hoặc trùng phòng tùy nghiệp vụ. Nếu có xung đột, API trả `409 Conflict` kèm `data.conflicts`.

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

Ghi chú: `DanhMucMonHoc` là môn học gốc. Trong MVP, chương/bài học/bài tập chuẩn đang gắn trực tiếp với `DanhMucMonHoc`; `CourseSyllabus` quản lý đề cương/phiên bản theo môn, chuyên ngành và cơ sở. TODO: thêm `MaSyllabus` vào `LopHocPhan` nếu cần mỗi lớp học phần dùng một phiên bản đề cương cụ thể.

## Finance APIs

### Finance Schema/Options - Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/finance/schema/options` | SuperAdmin/FinanceAdmin/CampusChiefAccountant/CampusAccountant | Trả danh sách loại/trạng thái hóa đơn, loại/trạng thái giao dịch, provider thanh toán MVP (`vietqr`, `payos`), trạng thái duyệt tài khoản nhận tiền, loại/trạng thái hoàn phí và công thức tiền backend đang dùng. |
| GET | `/api/finance/schema/statuses` | SuperAdmin/FinanceAdmin/CampusChiefAccountant/CampusAccountant | Trả riêng các nhóm trạng thái tài chính để FE hiển thị/lọc dữ liệu, không cho FE tự cập nhật trạng thái hóa đơn. |

Ghi chú: Endpoint schema chỉ expose constants/công thức nghiệp vụ, không trả secret provider và không tạo giao dịch thanh toán.

### Cấu Hình Học Phí Chương Trình - Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/finance/program-tuition-configs` | SuperAdmin/FinanceAdmin/CampusChiefAccountant/CampusAccountant | Danh sách cấu hình học phí chương trình có phân trang; lọc theo `keyword`, `maDonVi`, `maChuongTrinhDaoTao`, `maHocKy`, `namHocTrongChuongTrinh`, `hocKyTrongNam`, `soThuTuHocKy`, `loaiCachTinhHocPhi`, `conHoatDong`. FinanceAdmin/SuperAdmin xem toàn hệ thống; kế toán cơ sở xem trong phạm vi campus. Response có `coDuocSua`, `lyDoKhongDuocSua` theo ngày học kỳ. |
| GET | `/api/finance/program-tuition-configs/options` | SuperAdmin/FinanceAdmin/CampusChiefAccountant/CampusAccountant | Trả danh sách cơ sở trong scope, chương trình đào tạo active, học kỳ trong scope và loại cách tính để FE render bộ lọc/form. |
| GET | `/api/finance/program-tuition-configs/{id}` | SuperAdmin/FinanceAdmin/CampusChiefAccountant/CampusAccountant | Chi tiết cấu hình học phí gồm cơ sở, chương trình đào tạo, học kỳ, học phí chính khóa, phí học liệu, tổng dự kiến và trạng thái có được sửa. |
| POST | `/api/finance/program-tuition-configs` | SuperAdmin/FinanceAdmin | Tạo cấu hình học phí theo `MaDonVi + MaChuongTrinhDaoTao + MaHocKy`; backend chỉ nhận `loaiCachTinhHocPhi` thuộc `co_dinh_theo_hoc_ky`, `theo_tin_chi`, `theo_mon_hoc` và tự tính `tongTienDuKien`. |
| POST | `/api/finance/program-tuition-configs/bulk` | SuperAdmin/FinanceAdmin | Tạo hàng loạt bằng `items: CreateProgramTuitionConfigRequest[]`; vẫn giữ kiểu request template cũ (`configs`, `confirmReplace`) để tương thích. Nếu thay thế cấu hình đã có hóa đơn học phí thì dòng đó bị bỏ qua. |
| PUT | `/api/finance/program-tuition-configs/{id}` | SuperAdmin/FinanceAdmin | Cập nhật cấu hình học phí; backend tự tính `tongTienDuKien = soTienHocPhi + tienHocLieu`. Nếu cấu hình đã dùng để phát sinh hóa đơn học phí thì không cho sửa cơ sở, chương trình, học kỳ, vị trí kỳ, cách tính hoặc số tiền; chỉ nên cập nhật ghi chú/trạng thái theo rule. |
| PATCH | `/api/finance/program-tuition-configs/{id}/deactivate` | SuperAdmin/FinanceAdmin | Vô hiệu hóa mềm cấu hình bằng `ConHoatDong = false`; không hard delete. |

Ghi chú: Module này chỉ quản lý cấu hình học phí chương trình đào tạo, chưa sinh hóa đơn, chưa xử lý học bổng/miễn giảm phức tạp và không có logic dự bị tiếng Anh. Cấu hình active không được trùng theo `MaDonVi + MaChuongTrinhDaoTao + MaHocKy`; các trường tiền không được âm và `TongTienDuKien` luôn do backend tính bằng `SoTienHocPhi + TienHocLieu`.

### Hóa Đơn Và Thanh Toán Học Phí Sinh Viên - Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/student/tuition/invoices` | Student | Sinh viên xem danh sách hóa đơn học phí của chính mình. Response có `soTienPhaiDong = MAX(0, soTien - giamTru)` và `conPhaiDong = MAX(0, soTienPhaiDong - daThanhToan)`. |
| GET | `/api/student/tuition/transactions` | Student | Sinh viên xem lịch sử giao dịch học phí theo các hóa đơn của chính mình. |
| POST | `/api/student/tuition/invoices/{invoiceId}/payments` | Student | Tạo giao dịch thanh toán cho hóa đơn của sinh viên hiện tại. Luồng FE học sinh dùng body `{ "provider": "payos" }`; backend gọi PayOS, lưu `checkoutUrl`, `qrPayload`, `paymentLinkId`/public reference và chờ webhook hoặc polling PayOS xác nhận. Frontend không tự cập nhật trạng thái hóa đơn. |
| GET | `/api/student/tuition/payments/{transactionId}` | Student | Sinh viên kiểm tra trạng thái giao dịch thanh toán của chính mình. Nếu giao dịch PayOS còn đang chờ, backend gọi PayOS `GET /v2/payment-requests/{id}` để đồng bộ `GiaoDich/HoaDon` trước khi trả response. |
| POST | `/api/finance/payments/webhooks/payos` | Public webhook | Webhook PayOS, verify chữ ký HMAC-SHA256 bằng `PayOS:ChecksumKey`, cập nhật `GiaoDich` và cộng `HoaDon.DaThanhToan` khi giao dịch hợp lệ. Endpoint idempotent với giao dịch đã `thanh_cong`. |

Ghi chú: PayOS là luồng thanh toán chính cho học sinh: PayOS tạo QR/link, nhận kết quả ngân hàng và gọi webhook để backend cập nhật `HoaDon`/`GiaoDich`. FE học sinh phải hiển thị QR từ `qrPayload`/payment information PayOS trả về hoặc mở `checkoutUrl`; không dùng QR VietQR thủ công cho provider `payos` vì PayOS có thể không match giao dịch và không gửi webhook. Polling là cơ chế dự phòng khi webhook chưa gọi được về môi trường local. VietQR thủ công chỉ dùng khi cần tạo QR chuyển khoản không có tự động xác nhận, giao dịch giữ trạng thái `cho_thanh_toan` để kế toán đối soát.

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

## Audit Logs APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/audit-logs` | SuperAdmin/Admin/CampusAdmin | Danh sách nhật ký kiểm toán có phân trang. SuperAdmin/Admin xem toàn bộ; CampusAdmin chỉ xem log thuộc `MaDonVi` trong phạm vi cơ sở/cơ sở con. |
| GET | `/api/audit-logs/{id}` | SuperAdmin/Admin/CampusAdmin | Chi tiết nhật ký kiểm toán, gồm `oldValue`, `newValue`, `userAgent`, `traceId` nếu có. CampusAdmin chỉ xem chi tiết log trong phạm vi được phép. |

Query parameters `GET /api/audit-logs`:

- `pageNumber`, `pageSize` (tối đa 100)
- `entityType`, `entityId`, `action`
- `changedBy`, `maDonVi`
- `fromDate`, `toDate` (`fromDate` không được lớn hơn `toDate`)
- `keyword`

Response list item:

```json
{
  "id": 1,
  "maDonVi": 2,
  "tenDonVi": "Cơ sở A",
  "entityType": "ProgramTuitionConfig",
  "entityId": "10",
  "action": "UPDATE",
  "changedBy": 1,
  "changedByName": "Super Admin",
  "changedAt": "2026-05-30T08:30:00Z",
  "description": "Cập nhật cấu hình học phí chương trình đào tạo.",
  "ipAddress": "127.0.0.1"
}
```

Response detail thêm:

```json
{
  "oldValue": "{...}",
  "newValue": "{...}",
  "userAgent": "Mozilla/5.0 ...",
  "traceId": "..."
}
```

Ghi chú: audit log được ghi tự động bởi backend khi thao tác Auth/User, RBAC, Organizations và Program Tuition Configs. Ngoài log nghiệp vụ chi tiết, backend còn ghi log request tự động cho các request `/api/*` đã đăng nhập với `entityType = "HttpRequest"` và `action = "HTTP_GET"`, `HTTP_POST`, `HTTP_PUT`, `HTTP_PATCH` hoặc `HTTP_DELETE`; endpoint `/api/audit-logs` được bỏ qua để tránh tự ghi log khi xem log. Không có public `POST`/`PUT`/`DELETE` API để tạo hoặc xóa audit log. Backend mask các field nhạy cảm như password, password hash, token, OTP và secret trước khi lưu JSON `oldValue`/`newValue`.

## Courses APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/courses` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff/Teacher | Danh sách khóa học có phân trang; filter `maDonVi`, `maMonHoc`, `maGiaoVien`, `maHocKy`, `maLop`, `trangThai`, `keyword`. Teacher chỉ xem khóa học do mình phụ trách. |
| GET | `/api/courses/{id}` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff/Teacher | Chi tiết khóa học gồm cơ sở, môn học chuẩn, giảng viên, học kỳ, lớp hành chính và danh sách `Chuong -> BaiHoc` lấy theo `KhoaHoc.MaMonHoc`. |
| POST | `/api/courses` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Tạo khóa học MVP bắt buộc `MaLop`; validate cơ sở, môn học, giảng viên role `Teacher/giao_vien`, học kỳ nếu truyền, lớp thuộc đúng cơ sở và chống trùng `MaDonVi + MaMonHoc + MaHocKy + MaLop`. |
| PUT | `/api/courses/{id}` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Cập nhật giảng viên, học kỳ, lớp hành chính, tiêu đề, mô tả, trạng thái và ảnh bìa; ghi audit khi đổi giảng viên/lớp/trạng thái. |
| DELETE | `/api/courses/{id}` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Lưu trữ mềm khóa học bằng `TrangThai = luu_tru`, không xóa vật lý. |

### Dự kiến/cần bổ sung

- `PATCH /api/courses/{id}/publish`
- `GET /api/courses/{id}/chapters`
- `GET /api/students/me/courses`

Ghi chú dữ liệu: Trong phạm vi MVP, `KhoaHoc` đại diện cho một môn học được mở cho một lớp hành chính trong một học kỳ và do một giảng viên phụ trách. Một giảng viên dạy cùng một môn cho nhiều lớp sẽ tạo nhiều `KhoaHoc` khác nhau. Nhiều giảng viên cùng dạy một môn cho các lớp khác nhau cũng tạo nhiều `KhoaHoc` khác nhau. `LopHocPhan` tạm thời chưa dùng trong MVP và được giữ nullable để mở rộng sau này khi cần hỗ trợ đăng ký học phần, học lại, ghép lớp hoặc chia nhóm thực hành. Nội dung học tập chuẩn vẫn lấy theo `DanhMucMonHoc -> Chuong -> BaiHoc`, không copy theo từng `KhoaHoc`.

Roadmap: Sau MVP cần hỗ trợ cấu hình quiz/bài tập theo `KhoaHoc` để mỗi lớp có lịch mở/đóng khác nhau. `ThoiKhoaBieu` hiện đã gắn `MaKhoaHoc` và `MaCaHoc` ở tầng database; `TienDoBaiHoc` và `DiemSo` vẫn cần cân nhắc thêm `MaKhoaHoc` nullable/required theo mức độ triển khai để phân biệt cùng môn ở lớp/giảng viên/học kỳ khác nhau.

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

Quản lý đề/quiz dành cho `HoiDongQuanLyNoiDung`:

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/exam/de-kiem-tra/search` | HoiDongQuanLyNoiDung | Danh sách đề/quiz có phân trang và lọc theo môn, học kỳ, trạng thái, loại đề, hình thức thi. Backend đồng bộ trạng thái mở/đóng theo cấu hình trước khi trả dữ liệu. |
| GET | `/api/exam/de-kiem-tra/{id}` | HoiDongQuanLyNoiDung | Chi tiết đề/quiz gồm cấu hình điểm, lượt làm, thời gian mở/đóng và danh sách câu hỏi. |
| POST | `/api/exam/de-kiem-tra` | HoiDongQuanLyNoiDung | Tạo đề/quiz ở trạng thái `nhap`; cấu hình quiz nằm trong `cauHinh`. |
| PUT | `/api/exam/de-kiem-tra/{id}` | HoiDongQuanLyNoiDung | Cập nhật đề/quiz khi còn `nhap`. |
| DELETE | `/api/exam/de-kiem-tra/{id}` | HoiDongQuanLyNoiDung | Xóa đề/quiz nháp chưa được sử dụng. |
| GET | `/api/exam/de-kiem-tra/{id}/cau-hoi` | HoiDongQuanLyNoiDung | Danh sách câu hỏi trong đề/quiz. |
| POST | `/api/exam/de-kiem-tra/{id}/cau-hoi` | HoiDongQuanLyNoiDung | Gán thêm câu hỏi cho đề/quiz nháp. |
| PUT | `/api/exam/de-kiem-tra/{id}/cau-hoi` | HoiDongQuanLyNoiDung | Thay thế toàn bộ câu hỏi của đề/quiz nháp. |
| PUT | `/api/exam/de-kiem-tra/{id}/cau-hoi/{maCauHoi}` | HoiDongQuanLyNoiDung | Cập nhật điểm/thứ tự câu hỏi khi đề/quiz chưa được sử dụng. |
| DELETE | `/api/exam/de-kiem-tra/{id}/cau-hoi/{maCauHoi}` | HoiDongQuanLyNoiDung | Gỡ câu hỏi khỏi đề/quiz chưa được sử dụng. |
| PUT | `/api/exam/de-kiem-tra/{id}/cau-hoi/reorder` | HoiDongQuanLyNoiDung | Sắp xếp lại câu hỏi, thứ tự phải liên tục từ 1 đến N. |
| POST | `/api/exam/de-kiem-tra/{id}/publish` | HoiDongQuanLyNoiDung | Validate cấu hình, tổng điểm, câu hỏi rồi chuyển sang `da_len_lich` hoặc `dang_mo`. |
| POST | `/api/exam/de-kiem-tra/{id}/unpublish` | HoiDongQuanLyNoiDung | Chuyển về `nhap` nếu chưa được sử dụng. |
| POST | `/api/exam/de-kiem-tra/{id}/open` | HoiDongQuanLyNoiDung | Mở quiz thủ công, set `trang_thai = dang_mo`. |
| POST | `/api/exam/de-kiem-tra/{id}/close` | HoiDongQuanLyNoiDung | Đóng quiz thủ công, set `trang_thai = da_dong`. |

Luồng làm quiz bài học dành cho `Student`:

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/quiz-attempts/{quizId}/availability` | Student | Kiểm tra quiz có thể làm không, số lượt đã làm, giới hạn lượt, giờ mở/đóng và kết quả hiện tại. Chỉ áp dụng quiz được gắn trong `BaiHocNoiDung` với `LoaiNoiDung = quiz`. |
| POST | `/api/quiz-attempts/{quizId}/start` | Student | Tạo hoặc trả lại lượt làm đang hoạt động; enforce `SoLanLamToiDa`, `KhongGioiHanSoLan`, `MoLuc`, `DongLuc`, deadline theo thời lượng quiz. |
| PUT | `/api/quiz-attempts/sessions/{attemptId}/autosave` | Student | Lưu tạm câu trả lời JSON typed; không chấm điểm. |
| POST | `/api/quiz-attempts/sessions/{attemptId}/submit` | Student | Nộp bài và chấm trắc nghiệm thật theo `DapAnDung`; kết quả/đáp án đúng chỉ trả nếu cấu hình cho phép. |
| GET | `/api/quiz-attempts/{quizId}/history` | Student | Lịch sử các lượt làm và kết quả cuối theo cấu hình `CachTinhDiemCuoi`. |

Luồng thi chính thức vẫn dùng `CaThi`:

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| POST | `/api/exam/taking/start` | Student | Bắt đầu phiên thi theo `MaCaThi`; không dùng giới hạn lượt quiz bài học. |
| POST | `/api/exam/taking/autosave` | Student | Lưu tạm câu trả lời phiên thi chính thức. |
| POST | `/api/exam/taking/submit` | Student | Nộp bài thi chính thức. |
| POST | `/api/exam/grading/auto/{maCaThi}` | Teacher/CampusAdmin/AcademicStaff/Admin/SuperAdmin | Chấm tự động phần trắc nghiệm theo đáp án thật, không dùng điểm ngẫu nhiên. |
| POST | `/api/exam/grading/essay` | Teacher/CampusAdmin/AcademicStaff/Admin/SuperAdmin | Nhập điểm cuối cùng cho bài có tự luận. |

### Dự kiến/cần bổ sung

- API cấu hình quiz theo từng `KhoaHoc` nếu cần lịch mở/đóng khác nhau giữa các lớp.
- API chấm tự luận chi tiết theo từng câu nếu cần rubric thay vì nhập `DiemCuoiCung` trực tiếp.

## Attendance APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/teacher/attendance/today` | Teacher | Danh sách buổi học hôm nay của giáo viên hiện tại, tính theo `MaGiaoVien` hoặc `MaGiaoVienDayThay`. Nếu không có buổi hôm nay thì trả danh sách rỗng. |
| POST | `/api/buoi-hoc/{id}/attendance/start` | Teacher phụ trách buổi học | Mở điểm danh cho buổi học chưa hủy; tạo các dòng `DiemDanh` còn thiếu cho sinh viên active trong `KhoaHoc.MaLop`, mặc định `TrangThai = vang`, `HeSoVang = 1`; set `TrangThaiDiemDanh = dang_diem_danh`, hạn gửi = thời điểm mở + 15 phút. |
| GET | `/api/buoi-hoc/{id}/attendance` | Teacher phụ trách hoặc Admin/SuperAdmin/CampusAdmin/AcademicStaff trong campus scope | Chi tiết điểm danh của một buổi học, gồm thông tin buổi học/khóa/lớp/môn/ca/phòng/giáo viên và danh sách sinh viên. |
| PATCH | `/api/buoi-hoc/{id}/attendance/{maSinhVien}` | Teacher phụ trách buổi học | Cập nhật điểm danh một sinh viên khi `TrangThaiDiemDanh = dang_diem_danh` và chưa quá hạn 15 phút. Body gồm `trangThai` nhận `co_mat`, `vang`, `di_muon`, `co_phep`. |
| PUT | `/api/buoi-hoc/{id}/attendance/bulk` | Teacher phụ trách buổi học | Cập nhật điểm danh nhiều sinh viên trong một request; validate toàn bộ trước khi lưu, không update nửa vời. |
| POST | `/api/buoi-hoc/{id}/attendance/submit` | Teacher phụ trách buổi học | Gửi điểm danh khi đang mở và chưa quá hạn; set `TrangThaiDiemDanh = da_gui`, `DiemDanhDaGuiLuc = now`, `DiemDanhHanChinhSuaLuc = now + 10 phút`. MVP không cho sửa sau submit. |
| GET | `/api/student/attendance` | Student | Sinh viên xem điểm danh của chính mình, có filter `ngayTu`, `ngayDen`, `maKhoaHoc`, `trangThai`, `pageIndex`, `pageSize`. |
| POST | `/api/buoi-hoc/{id}/attendance/unlock-requests` | Teacher phụ trách buổi học | Giáo viên tạo yêu cầu mở khóa điểm danh cho buổi đã gửi/đã khóa. Không cho tạo nếu buổi đã hủy hoặc đã có yêu cầu `cho_duyet`. |
| GET | `/api/teacher/attendance/unlock-requests` | Teacher | Giáo viên xem yêu cầu do mình tạo hoặc thuộc buổi mình phụ trách. Filter `maBuoiHoc`, `maKhoaHoc`, `trangThai`, `pageIndex`, `pageSize`. |
| GET | `/api/admin/attendance/unlock-requests` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Danh sách yêu cầu mở khóa điểm danh theo campus scope `KhoaHoc.MaDonVi`. Filter `maBuoiHoc`, `maKhoaHoc`, `trangThai`, `pageIndex`, `pageSize`. |
| GET | `/api/admin/attendance/unlock-requests/{id}` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Chi tiết yêu cầu mở khóa điểm danh trong campus scope. |
| POST | `/api/admin/attendance/unlock-requests/{id}/approve` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Duyệt yêu cầu `cho_duyet`, set `TrangThai = da_duyet`, mở lại buổi học bằng `TrangThaiDiemDanh = dang_diem_danh`, hạn chỉnh sửa/gửi = now + 10 phút. |
| POST | `/api/admin/attendance/unlock-requests/{id}/reject` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Từ chối yêu cầu `cho_duyet`, lưu lý do từ chối và không mở lại buổi học. |
| POST | `/api/admin/attendance-automation/run-once` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Chạy thủ công job tự động xử lý điểm danh quá hạn. Auto submit các buổi `dang_diem_danh` có `DiemDanhHanGuiLuc <= now`; auto lock các buổi `da_gui` có `DiemDanhHanChinhSuaLuc <= now`; bỏ qua buổi `da_huy`, `chua_mo`, `da_khoa`. |

Ghi chú P0-6/P0-7/P0-9: Nguồn sinh viên MVP lấy từ `BuoiHoc -> KhoaHoc.MaLop -> NguoiDung.MaLop`, không dùng `DangKyHocPhan`. Không làm QR/GPS/FaceID, export Excel, realtime SignalR, email hoặc mobile push. Rule 15 phút được enforce khi update/bulk/submit dựa trên `DiemDanhHanGuiLuc`; P0-9 bổ sung hosted service chạy nền mặc định mỗi 60 giây và endpoint run-once để auto submit/auto lock theo deadline này. P0-7 dùng `YeuCauMoKhoaDiemDanh` để giáo viên xin mở lại điểm danh; khi admin/học vụ duyệt, buổi học được mở lại trong 10 phút, sau đó P0-9 tiếp tục auto submit theo `DiemDanhHanGuiLuc`. Auto lock set `BuoiHoc.TrangThaiDiemDanh = da_khoa`, `BuoiHoc.DiemDanhKhoaLuc = now` và `DiemDanh.KhoaLuc = now`, không set `BuoiHoc.KhoaLuc`.

### Dự kiến/cần bổ sung

- Báo cáo/tổng hợp điểm danh nâng cao.

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

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/notifications` | JWT | User xem danh sách thông báo của chính mình, có phân trang và filter `daDoc`, `loaiThongBao`, `mucDo`, `ngayTu`, `ngayDen`. |
| GET | `/api/notifications/{id}` | JWT | User xem chi tiết một thông báo của chính mình. Nội dung Editor.js được trả qua `noiDungJson`; backend không render HTML. |
| GET | `/api/notifications/unread-count` | JWT | Lấy số thông báo chưa đọc của user hiện tại. |
| PATCH | `/api/notifications/{id}/read` | JWT | Đánh dấu một thông báo của user hiện tại là đã đọc, set `daDoc = true`, `docLuc = now`. |
| PATCH | `/api/notifications/read-all` | JWT | Đánh dấu tất cả thông báo chưa đọc của user hiện tại là đã đọc. |
| POST | `/api/admin/notifications` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Tạo thông báo thủ công. `targetType` nhận `users`, `class`, `course`, `campus`; campus scope được kiểm tra theo `MaDonVi`. |
| GET | `/api/admin/notifications` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Danh sách thông báo quản trị, group theo `maNhomThongBao`, có `recipientCount`, `readCount`, `unreadCount`. |
| GET | `/api/admin/notifications/{id}` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Chi tiết group thông báo dựa trên `maNhomThongBao` của row `{id}` trong phạm vi campus scope. |

Request tạo manual notification:

```json
{
  "tieuDe": "Thông báo nghỉ học",
  "tomTat": "Lớp SD1904 nghỉ học ngày mai.",
  "noiDungJson": "{\"time\":1710000000000,\"blocks\":[]}",
  "noiDungText": "Lớp SD1904 nghỉ học ngày mai.",
  "mucDo": "important",
  "targetType": "class",
  "targetIds": [1]
}
```

Ghi chú P0-8: Bảng `ThongBao` hiện được dùng theo mô hình mỗi dòng là một người nhận; các dòng cùng một lần gửi dùng chung `maNhomThongBao`. `loaiThongBao` ở API map vào cột DB `loai_su_kien`. Editor.js output được lưu nguyên vào `noi_dung_json` và được validate JSON; `noi_dung_text`/`tom_tat` dùng cho preview/search. P0-8 không làm SignalR realtime, email, mobile push hoặc scheduler. Auto notification đã gắn vào các phát sinh buổi học P0-5 và duyệt/từ chối mở khóa điểm danh P0-7.

### Dự kiến/cần bổ sung

- `GET /api/notification-preferences`
- `PUT /api/notification-preferences`

## Applications / Đơn Từ APIs

### Đã có trong P0-DT1/P0-DT2/P0-DT3

P0-DT1 làm foundation schema, constants, state machine, template validator và API read-only để FE đọc loại đơn/mẫu đơn. P0-DT2 bổ sung vòng đời sinh viên: tạo nháp, xem đơn của chính mình, cập nhật nháp/yêu cầu bổ sung, nộp, nộp lại và hủy đơn. P0-DT3 bổ sung upload/download/delete minh chứng an toàn cho sinh viên. Chưa làm phân công, duyệt/từ chối, notification hoặc xử lý nghiệp vụ sau duyệt.

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/applications/schema/types` | JWT | Trả 11 loại đơn hợp lệ: `nghi_phep`, `thi_lai`, `chuyen_truong`, `cap_chung_chi`, `khac`, `phuc_tra_diem`, `bao_luu`, `chuyen_nganh`, `chuyen_co_so`, `xac_nhan`, `rut_hoc`. |
| GET | `/api/applications/schema/statuses` | JWT | Trả trạng thái đơn, trạng thái terminal và danh sách trạng thái tiếp theo theo state machine. |
| GET | `/api/applications/templates` | JWT | Trả danh sách mẫu đơn active version hiện tại. Không trả entity trực tiếp, không trả audit metadata. |
| GET | `/api/applications/templates/{loaiDon}` | JWT | Trả mẫu đơn active theo loại. `loaiDon` không hợp lệ trả `400`; loại hợp lệ nhưng không có mẫu active trả `404`. |
| POST | `/api/student/applications` | Student | Tạo đơn nháp từ template active của `loaiDon`. Backend tự gán `MaHocSinh`, `MaDonVi`, `MaMauDon`, `TrangThai = nhap`; không tin field hệ thống từ client. |
| GET | `/api/student/applications` | Student | Sinh viên xem danh sách đơn của chính mình. Filter `loaiDon`, `trangThai`, `ngayTu`, `ngayDen`, `search`, `pageIndex`, `pageSize`. |
| GET | `/api/student/applications/{id}` | Student | Sinh viên xem chi tiết đơn của chính mình, gồm dữ liệu form, template snapshot an toàn, minh chứng metadata an toàn và timeline public. Không trả `StorageKey`, `UrlBangChung`, ghi chú nội bộ hoặc snapshot nội bộ. |
| PUT | `/api/student/applications/{id}` | Student | Cập nhật đơn ở trạng thái `nhap` hoặc `yeu_cau_bo_sung`. Body cần `rowVersion` base64 để chống lost update. |
| POST | `/api/student/applications/{id}/submit` | Student | Nộp đơn nháp, validate required fields, related entity, rule theo loại đơn và metadata minh chứng; chuyển `nhap -> da_nop`. |
| POST | `/api/student/applications/{id}/resubmit` | Student | Nộp lại đơn đang `yeu_cau_bo_sung`, giữ `NgayNop` ban đầu, clear nội dung yêu cầu bổ sung và chuyển về `dang_xem_xet`. |
| POST | `/api/student/applications/{id}/cancel` | Student | Hủy đơn của chính mình khi trạng thái cho phép (`nhap`, `da_nop`, `yeu_cau_bo_sung`); không hard delete. |
| POST | `/api/student/applications/{id}/attachments` | Student | Upload một hoặc nhiều file minh chứng dạng multipart field `files` và `rowVersion`. Chỉ cho đơn của chính sinh viên ở trạng thái `nhap` hoặc `yeu_cau_bo_sung`. Thành công trả `201`, danh sách metadata an toàn, `rowVersion` mới, số file active và tổng dung lượng. |
| GET | `/api/student/applications/{id}/attachments/{attachmentId}/download` | Student | Download binary file minh chứng của chính sinh viên nếu metadata chưa soft delete và object còn tồn tại. Trả `Content-Disposition: attachment`, content type canonical trong DB, `X-Content-Type-Options: nosniff`, `Cache-Control: private, no-store`. |
| DELETE | `/api/student/applications/{id}/attachments/{attachmentId}` | Student | Soft delete metadata minh chứng của chính sinh viên bằng body `{ "rowVersion": "..." }`. Chỉ cho đơn `nhap` hoặc `yeu_cau_bo_sung`; physical delete best-effort sau khi DB commit. |

Response mẫu:

```json
{
  "success": true,
  "message": "Lấy mẫu đơn thành công.",
  "data": {
    "maMauDon": 1,
    "loaiDon": "nghi_phep",
    "tenLoaiDon": "Đơn nghỉ phép",
    "tenMau": "Mẫu đơn nghỉ phép",
    "phienBan": 1,
    "cauHinhJson": "{\"fields\":[]}",
    "batBuocMinhChung": false,
    "soTepToiDa": 5,
    "dungLuongTepToiDaByte": 10485760,
    "tongDungLuongToiDaByte": 26214400,
    "slaGio": 48
  }
}
```

Ghi chú P0-DT1:

- `DonTu` đã có `MaDonVi`, `MaMauDon`, `TieuDe`, `TrangThaiXuLyNghiepVu`, `NguoiXuLyCuoi`, `NoiDungYeuCauBoSung`, `KetQuaXuLyJson`, `NgayCapNhat`, `NgayNop`, `NgayDuyet`, `HanXuLyLuc`, `RowVersion`.
- `MauDonTu` quản lý template versioned; unique `LoaiDon + PhienBan`; chỉ một template active mỗi loại.
- `TepDinhKemDonTu` chỉ là schema metadata cho minh chứng, lưu `StorageKey`, soft delete; P0-DT1 chưa có upload endpoint.
- `NhatKyDuyetDon` mở rộng để ghi trạng thái cũ/mới, public/internal note, system actor và snapshot JSON.
- Service workflow sau này không được tin FE truyền `MaHocSinh`, `MaDonVi`, `TrangThai`, `NguoiDuyetHienTai`.
- `CampusScopeMiddleware` không đủ cho endpoint đơn từ không truyền `maDonVi`; service DT2/DT3 phải tự filter bằng `CurrentUserContext`.

Ghi chú P0-DT2:

- Tất cả endpoint `/api/student/applications...` dùng policy `ApplicationStudent` và service re-query `NguoiDung` hiện tại, chỉ chấp nhận role DB `hoc_sinh` đang `hoat_dong`.
- Ownership bắt buộc theo `DonTu.MaHocSinh = currentUser.UserId`; truy cập đơn người khác trả `404`.
- Form data được validate theo `MauDonTu.CauHinhJson`: field unknown bị chặn; draft cho phép thiếu required; submit/resubmit bắt buộc required, kiểu dữ liệu, options và related entity hợp lệ.
- Related entity không query động từ client. Backend chỉ kiểm tra các entity đã whitelist như học kỳ, môn học, điểm số, cơ sở, ngành, chuyên ngành, khóa học và buổi học theo quyền của sinh viên.
- Một số loại đơn có rule riêng: nghỉ phép kiểm tra khoảng ngày, phúc tra điểm kiểm tra điểm số/cột điểm, bảo lưu/chuyển trường/rút học/chuyển ngành/chuyển cơ sở chống trùng đơn đang active, xác nhận giới hạn số bản.
- Minh chứng P0-DT2 validate metadata đã có trong `TepDinhKemDonTu` hoặc legacy `UrlBangChung`. Nếu template hoặc field yêu cầu minh chứng mà chưa có file active hoặc legacy `UrlBangChung`, submit trả `400`.
- `rowVersion` là base64 concurrency token. Sai format trả `400`; stale/concurrent update trả `409`.
- `NhatKyDuyetDon` ghi timeline công khai cho tạo/nộp/nộp lại/hủy và log nội bộ khi cập nhật; response student chỉ trả timeline public.

Ghi chú P0-DT3:

- Upload/delete luôn dùng DB-authoritative ownership `DonTu.MaHocSinh = CurrentUser.UserId`; đơn không tồn tại, không thuộc sinh viên hoặc attachment đã bị xóa đều trả `404`.
- Upload/delete dùng `rowVersion`; stale/concurrent mutation trả `409`. Mỗi mutation lock theo `sp_getapplock` resource `ApplicationEvidence:{applicationId}` trong transaction `Serializable`.
- Upload chỉ nhận PDF/JPEG/PNG/WEBP, kiểm tra extension, declared `Content-Type`, magic bytes, kích thước, SHA-256 hash. Không nhận DOCX/SVG/ZIP/HTML/EXE.
- Hard cap toàn hệ thống: tối đa 5 file, 10MB/file, 25MB/tổng. Effective cap là giá trị nhỏ hơn giữa hard cap và `MauDonTu` active/assigned.
- Duplicate hash trong cùng request hoặc với file active hiện có trả `409`.
- Response student chỉ trả metadata an toàn (`maTep`, `tenFileGoc`, `contentType`, `kichThuocByte`, `ngayTao`). Không trả `StorageKey`, `TenFileLuu`, hash hoặc URL public.
- Local object store chỉ dùng Development/Testing với storage root isolated. Production dùng private R2 object store, không public URL.
- Upload object trước DB transaction; nếu DB mutation lỗi thì cleanup object đã upload. Delete revoke quyền truy cập bằng soft delete DB trước, physical delete best-effort sau commit.

### Dự kiến/cần bổ sung

- `GET /api/admin/applications`
- `POST /api/admin/applications/{id}/assign`
- `POST /api/admin/applications/{id}/approve`
- `POST /api/admin/applications/{id}/reject`

## Reports APIs

### Đã có

Chưa thấy controller reports trong repo hiện tại.

### Dự kiến/cần bổ sung

- `GET /api/reports/dashboard`
- `POST /api/reports/export`
- `GET /api/reports/attendance-risk`
- `GET /api/reports/failure-risk`
- `GET /api/reports/classroom-usage`

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
