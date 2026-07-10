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
| POST | `/api/auth/login` | Public | Đăng nhập bằng `usernameOrEmail`/password, trả `success`, `message`, access token, expiresAt, requiresPasswordChange, user. |
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
| GET | `/api/bgh/users` | Principal/Admin/SuperAdmin | Danh sách user read-only cho BGH, scope theo cơ sở của người dùng hiện tại; không cho mutation qua endpoint BGH. |

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
| POST | `/api/thoi-khoa-bieu/xep-lich-thong-minh` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | (P12) Sinh lịch thông minh batch: xếp tự động các khóa học trong `MaHocKy + MaDonVi` vào các slot trống. Trả về bản nháp (`ScheduleGenerationJob`) với danh sách `ScheduleDraftItem`. Hỗ trợ lọc theo `MaKhoaHocFilter` và tham số genetic (`TongTheHe`, `TyLeCheo`). |
| GET | `/api/thoi-khoa-bieu/drafts/{draftId}` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | (P12) Lấy chi tiết bản nháp xếp lịch thông minh theo `DraftId`. |
| GET | `/api/thoi-khoa-bieu/drafts` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | (P12) Danh sách bản nháp xếp lịch thông minh theo `MaDonVi + MaHocKy`. |
| POST | `/api/thoi-khoa-bieu/xep-lich-thong-minh/publish` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | (P12) Xuất bản bản nháp: tạo `ThoiKhoaBieu` và `BuoiHoc` trong transaction; rollback nếu có xung đột. |
| POST | `/api/thoi-khoa-bieu/xep-lich-thong-minh/check-xung-dot-batch` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | (P12) Kiểm tra xung đột batch cho danh sách đề xuất `MaKhoaHoc + ThuTrongTuan + MaCaHoc + MaPhong` trong `MaHocKy + MaDonVi`. |
| DELETE | `/api/thoi-khoa-bieu/drafts/{draftId}` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | (P12) Xóa bản nháp xếp lịch thông minh. |

Ghi chú: P0-3 kiểm tra xung đột lịch ở mức `MaHocKy + ThuTrongTuan + MaCaHoc`, bỏ qua bản ghi `da_huy` và bỏ qua chính bản ghi hiện tại khi update bằng `excludeMaTkb`. P0-4 sinh `BuoiHoc` từ TKB đã xuất bản nhưng chưa làm điểm danh, đổi lịch, dạy thay, đổi phòng, đổi ca hoặc frontend. Unique index `UQ_ThoiKhoaBieu_KhoaHoc_Thu_Ca` chỉ áp dụng cho bản ghi có `TrangThai <> N'da_huy'`, nên có thể tạo lại lịch cùng khóa học/thứ/ca sau khi bản ghi cũ đã hủy. P12 (Smart Timetable Engine) thêm OccupationMap, draft persistence, atomic publish với re-validation.

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

## Student Curriculum APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/student/curriculum` | Student | Sinh viên xem khung chương trình đào tạo thật của chính mình. Backend lấy chương trình qua `NguoiDung.MaLop -> LopHanhChinh.MaChuongTrinh`, trả metadata lớp/chương trình và toàn bộ học kỳ theo `ChuongTrinhDaoTao.SoHocKy`; mỗi kỳ chứa danh sách môn đang hoạt động từ `MonHocTrongChuongTrinh`, có trạng thái học dựa trên `DiemSo` nếu đã có dữ liệu. |

### Hóa Đơn Và Thanh Toán Học Phí Sinh Viên - Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/student/tuition/invoices` | Student | Sinh viên xem danh sách hóa đơn học phí của chính mình. Response có `soTienPhaiDong = MAX(0, soTien - giamTru)` và `conPhaiDong = MAX(0, soTienPhaiDong - daThanhToan)`. |
| GET | `/api/student/tuition/transactions` | Student | Sinh viên xem lịch sử giao dịch học phí theo các hóa đơn của chính mình. |
| POST | `/api/student/tuition/invoices/{invoiceId}/payments` | Student | Tạo giao dịch thanh toán cho hóa đơn của sinh viên hiện tại. Luồng FE học sinh dùng body `{ "provider": "payos" }`; backend gọi PayOS, lưu `checkoutUrl`, `qrPayload`, `paymentLinkId`/public reference và chờ webhook hoặc polling PayOS xác nhận. Frontend không tự cập nhật trạng thái hóa đơn. |
| GET | `/api/student/tuition/payments/{transactionId}` | Student | Sinh viên kiểm tra trạng thái giao dịch thanh toán của chính mình. Nếu giao dịch PayOS còn đang chờ, backend gọi PayOS `GET /v2/payment-requests/{id}` để đồng bộ `GiaoDich/HoaDon` trước khi trả response. |
| POST | `/api/finance/payments/webhooks/payos` | Public webhook | Webhook PayOS, verify chữ ký HMAC-SHA256 bằng `PayOS:ChecksumKey`, cập nhật `GiaoDich` và cộng `HoaDon.DaThanhToan` khi giao dịch hợp lệ. Endpoint idempotent với giao dịch đã `thanh_cong`. |

Ghi chú: PayOS là luồng thanh toán chính cho học sinh: PayOS tạo QR/link, nhận kết quả ngân hàng và gọi webhook để backend cập nhật `HoaDon`/`GiaoDich`. FE học sinh phải hiển thị QR từ `qrPayload`/payment information PayOS trả về hoặc mở `checkoutUrl`; không dùng QR VietQR thủ công cho provider `payos` vì PayOS có thể không match giao dịch và không gửi webhook. Polling là cơ chế dự phòng khi webhook chưa gọi được về môi trường local. VietQR thủ công chỉ dùng khi cần tạo QR chuyển khoản không có tự động xác nhận, giao dịch giữ trạng thái `cho_thanh_toan` để kế toán đối soát.

## Reward / Discipline APIs

### Schema/Options - Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/reward-discipline/schema/options` | JWT | Trả danh mục trạng thái/loại dùng cho nền tảng khen thưởng - kỷ luật RD1: đợt khen thưởng Top 100 học kỳ, mẫu bằng khen, trạng thái khen thưởng, mức độ/hình thức/trạng thái kỷ luật. Endpoint chỉ đọc, chưa tạo workflow xét duyệt hoặc xử lý nghiệp vụ. |

### Đợt Khen Thưởng Top 100 - Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/admin/reward-campaigns` | SuperAdmin/Admin/CampusAdmin | Danh sách đợt khen thưởng có phân trang, tìm kiếm và lọc theo `maHocKy`, `maDonVi`, `loaiDot`, `trangThai`, `keyword`, `pageIndex`, `pageSize`. SuperAdmin xem toàn bộ gồm đợt global `maDonVi = null`; Admin/CampusAdmin chỉ xem đợt thuộc cơ sở trong scope, không xem đợt global. |
| POST | `/api/admin/reward-campaigns/top100` | SuperAdmin | Tạo đợt Top 100 học kỳ. Backend cố định `loaiDot = TOP_100_HOC_KY`, trạng thái tạo mới là `nhap`, mặc định `soLuongToiDa = 100` nếu bỏ trống. Validate học kỳ, cơ sở active, mẫu bằng khen active đúng loại, `tieuChiXetJson` là JSON object và chống trùng active theo `maHocKy + maDonVi + loaiDot`. |
| GET | `/api/admin/reward-campaigns/{id}` | SuperAdmin/Admin/CampusAdmin | Xem chi tiết đợt khen thưởng trong scope, gồm học kỳ, đơn vị, mẫu bằng khen, tiêu chí JSON, người tạo/duyệt và mốc thời gian. |
| PUT | `/api/admin/reward-campaigns/{id}` | SuperAdmin | Cập nhật đợt khen thưởng khi trạng thái còn `nhap`; cho sửa học kỳ/cơ sở/tên đợt/số lượng/tiêu chí JSON/mẫu bằng khen/ghi chú nếu không vi phạm duplicate active. |
| PATCH | `/api/admin/reward-campaigns/{id}/cancel` | SuperAdmin | Hủy mềm đợt khen thưởng bằng `trangThai = da_huy`, bắt buộc lý do. Không cho hủy đợt đã `da_cong_bo`; không hard delete. |
| GET | `/api/admin/reward-campaigns/{id}/approval-summary` | SuperAdmin/Admin/CampusAdmin | Tổng quan duyệt Top 100: tổng ứng viên, số được chọn, bị loại, dự phòng, đã tạo khen thưởng, cảnh báo vượt số lượng hoặc đã tạo quyết định. SuperAdmin xem toàn bộ; Admin/CampusAdmin theo scope như danh sách đợt. |
| PATCH | `/api/admin/reward-campaigns/{id}/candidates/{candidateId}` | SuperAdmin | Điều chỉnh ứng viên trước khi duyệt: `trangThai`, `xepHang`, `diemXet`, `ghiChuDieuChinh`, `lyDoDieuChinh`. Chỉ cho đợt `dang_xet`/`cho_duyet`; không cho sau `da_duyet`, `da_cong_bo`, `da_huy`. |
| POST | `/api/admin/reward-campaigns/{id}/candidates/manual-add` | SuperAdmin | Thêm thủ công học sinh/sinh viên đang hoạt động vào danh sách với trạng thái `them_thu_cong`; validate cùng cơ sở, chưa có ứng viên/quyết định khen thưởng trong đợt và không có kỷ luật đang hiệu lực. |
| POST | `/api/admin/reward-campaigns/{id}/candidates/reorder` | SuperAdmin | Sắp xếp lại thứ hạng các ứng viên được chọn (`duoc_de_xuat`, `them_thu_cong`), không cho rank trùng hoặc rank <= 0. |
| POST | `/api/admin/reward-campaigns/{id}/submit-for-approval` | SuperAdmin | Chuyển đợt `dang_xet -> cho_duyet` sau khi validate có ít nhất một ứng viên được chọn, không vượt `soLuongToiDa`, không trùng học sinh/sinh viên. Không tạo `KhenThuong`. |
| POST | `/api/admin/reward-campaigns/{id}/approve` | SuperAdmin | Duyệt danh sách Top 100 từ `dang_xet` hoặc `cho_duyet`, tạo record `KhenThuong` chính thức cho ứng viên `duoc_de_xuat`/`them_thu_cong`, đánh dấu ứng viên `da_duyet_kt`, set đợt `da_duyet`. Idempotency bảo vệ bằng kiểm tra đã có `KhenThuong` trong đợt; gọi lại trả `409`. RD4 chưa sinh PDF nên `urlPdfBangKhen = null`; legacy `UrlChungTu` được lưu chuỗi rỗng để tương thích constraint NOT NULL cũ. |
| POST | `/api/admin/reward-campaigns/{id}/certificates/generate` | SuperAdmin | RD6 sinh PDF bằng khen cho các `KhenThuong` thuộc đợt Top 100 đã `da_duyet`. Nếu không bật `forceRegenerate`, reward đã có PDF ở trạng thái `da_sinh_pdf` được skip. `onlyFailed = true` chỉ sinh reward thiếu PDF hoặc `loi_sinh_pdf`. Batch tiếp tục nếu một reward lỗi và trả summary `successCount/skippedCount/failedCount/items`. |
| POST | `/api/admin/reward-campaigns/{id}/certificates/regenerate` | SuperAdmin | Sinh lại PDF bằng khen, bắt buộc `reason`. Có thể truyền `rewardIds` để giới hạn subset và `maMauBangKhen` để override mẫu active Top 100. Không xóa file PDF cũ; cập nhật `urlPdfBangKhen`, `ngaySinhPdf`, `soLanSinhPdf`, `loiSinhPdf`. |
| GET | `/api/admin/reward-campaigns/{id}/certificates` | SuperAdmin/Admin/CampusAdmin | Danh sách PDF bằng khen của một đợt, có phân trang và lọc `trangThaiPdf`, `keyword`, `pageIndex`, `pageSize`. Admin/CampusAdmin theo scope đợt như các API campaign; không xem đợt global nếu không phải SuperAdmin. |
| GET | `/api/admin/rewards/{rewardId}/certificate/download` | SuperAdmin/Admin/CampusAdmin | Tải PDF bằng khen đã sinh. Response là `application/pdf`, attachment, `X-Content-Type-Options: nosniff`, `Cache-Control: private, no-store`. Trả `404` nếu reward chưa có PDF hoặc file không tồn tại. |

Ví dụ tạo đợt Top 100:

```json
{
  "maHocKy": 1,
  "maDonVi": 2,
  "tenDot": "Top 100 học kỳ Spring 2026",
  "soLuongToiDa": 100,
  "tieuChiXetJson": {
    "minGpa": 8.5,
    "rankBy": "gpa"
  },
  "maMauBangKhen": 1,
  "ghiChu": "Đợt xét thử nghiệm"
}
```

RD6 dùng storage cấu hình `CertificateStorage__Provider=Local`, `CertificateStorage__LocalRoot`, `CertificateStorage__PublicBasePath` để lưu file PDF ngoài `wwwroot`. Renderer RD6 là PDF text-only an toàn từ whitelist field RD5, không nhận HTML/CSS/script và không xóa file cũ khi regenerate. Công bố, notification và workflow kỷ luật vẫn tách task sau.

### Mẫu Bằng Khen - Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/admin/certificate-templates` | SuperAdmin | Danh sách mẫu bằng khen có phân trang, lọc `loaiMau`, `conHoatDong`, `keyword`. |
| POST | `/api/admin/certificate-templates` | SuperAdmin | Tạo mẫu bằng khen Top 100. Validate `loaiMau`, `huongGiay`, `fileNenUrl`, kích thước và `cauHinhJson`. |
| GET | `/api/admin/certificate-templates/{id}` | SuperAdmin | Xem chi tiết mẫu bằng khen, gồm cấu hình JSON render an toàn. |
| PUT | `/api/admin/certificate-templates/{id}` | SuperAdmin | Cập nhật mẫu đang hoạt động. Mẫu đã vô hiệu hóa trả `409`. |
| DELETE | `/api/admin/certificate-templates/{id}` | SuperAdmin | Vô hiệu hóa mẫu bằng `conHoatDong = false`, không hard delete kể cả khi đã được gắn với đợt/khen thưởng. |
| POST | `/api/admin/certificate-templates/{id}/preview` | SuperAdmin | Trả payload preview an toàn từ dữ liệu mẫu hoặc `maKhenThuong`; không ghi DB, không sinh PDF. |

`cauHinhJson` RD5/RD6 phải là object có `fields` array 1..50. Field chỉ nhận các key whitelist: `hoTen`, `mssv`, `tenHocKy`, `danhHieu`, `xepHang`, `diemXet`, `ngayCap`; mỗi field bắt buộc `key`, `x`, `y`, `fontSize`, `align`, `color`, `bold`. Backend không nhận HTML/CSS/script tùy ý và chặn `FileNenUrl` dạng base64. RD5 chưa upload file nền qua object storage riêng; `fileNenUrl` là URL/path an toàn đã có sẵn. RD6 sinh PDF batch từ cấu hình này và cập nhật `UrlPdfBangKhen`.

Ví dụ tạo mẫu bằng khen:

```json
{
  "tenMau": "Mẫu Top 100 học kỳ",
  "loaiMau": "TOP_100_HOC_KY",
  "fileNenUrl": "https://cdn.example.test/templates/top100.png",
  "chieuRong": 3508,
  "chieuCao": 2480,
  "huongGiay": "A4_NGANG",
  "cauHinhJson": {
    "fields": [
      { "key": "hoTen", "x": 100, "y": 240, "fontSize": 42, "align": "center", "color": "#111111", "bold": true },
      { "key": "mssv", "x": 100, "y": 310, "fontSize": 20, "align": "center", "color": "#333333", "bold": false },
      { "key": "ngayCap", "x": 100, "y": 620, "fontSize": 18, "align": "right", "color": "#333333", "bold": false }
    ]
  }
}
```

### Student Rewards - Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/student/rewards` | Student | Danh sách khen thưởng của chính học sinh đang đăng nhập, có phân trang và lọc (`maHocKy`, `loaiKhenThuong`, `trangThai`, `hasCertificate`, `keyword`). Chỉ hiển thị khen thưởng đã duyệt/cấp/có PDF. Các field nội bộ như `loiSinhPdf` bị ẩn. |
| GET | `/api/student/rewards/{rewardId}` | Student | Chi tiết một khen thưởng của chính học sinh. Trả `404` nếu khen thưởng không tồn tại hoặc thuộc về học sinh khác (tránh data leakage). |
| GET | `/api/student/rewards/{rewardId}/certificate/download` | Student | Tải PDF bằng khen của chính học sinh. Tương tự như admin, response trả về `application/pdf`, attachment, kèm `nosniff` và `no-store`. Không trả về `403` mà trả `404` nếu không tìm thấy file hoặc sai quyền sở hữu. Trả `409` nếu khen thưởng đã bị hủy. |

### Dự kiến/cần bổ sung

- Công bố bằng khen, notification.
- CRUD hồ sơ kỷ luật, phê duyệt/gỡ hiệu lực.

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
| POST | `/api/courses/bulk-assign` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Tạo khóa học cho tối đa 5 lớp từ `MaMonHoc + MaGiaoVien + MaHocKy? + MaLopIds`; backend xác định scope theo giảng viên/lớp/học kỳ, không tin `MaDonVi` từ FE; lớp trùng `MaDonVi + MaMonHoc + MaHocKy + MaLop` được đưa vào `skipped` thay vì fail toàn batch. |
| POST | `/api/courses/{id}/clone` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Nhân bản khóa học sang học kỳ/lớp/giảng viên tùy chọn, trạng thái mới luôn `nhap`, tiêu đề mặc định thêm `(Bản sao)`; vẫn chặn trùng unique `MaDonVi + MaMonHoc + MaHocKy + MaLop`. |
| PUT | `/api/courses/{id}` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Cập nhật giảng viên, học kỳ, lớp hành chính, tiêu đề, mô tả, trạng thái và ảnh bìa; ghi audit khi đổi giảng viên/lớp/trạng thái. |
| DELETE | `/api/courses/{id}` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Lưu trữ mềm khóa học bằng `TrangThai = luu_tru`, không xóa vật lý. |
| POST | `/api/courses/batch-archive` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Lưu trữ mềm nhiều khóa học bằng `TrangThai = luu_tru`; trả `successIds`, `failed`, `count` và không fail toàn batch khi một khóa học không hợp lệ. |
| POST | `/api/courses/batch-publish` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Xuất bản nhiều khóa học bằng `TrangThai = da_xuat_ban`; khóa học đã `luu_tru` bị đưa vào `failed` để không hồi sinh dữ liệu lưu trữ. |

### Dự kiến/cần bổ sung

- `PATCH /api/courses/{id}/publish`
- `GET /api/courses/{id}/chapters`
- `GET /api/students/me/courses`

Ghi chú dữ liệu: Trong phạm vi MVP, `KhoaHoc` đại diện cho một môn học được mở cho một lớp hành chính trong một học kỳ và do một giảng viên phụ trách. Một giảng viên dạy cùng một môn cho nhiều lớp sẽ tạo nhiều `KhoaHoc` khác nhau. Nhiều giảng viên cùng dạy một môn cho các lớp khác nhau cũng tạo nhiều `KhoaHoc` khác nhau. Luồng học tập của sinh viên lấy danh sách khóa học từ `NguoiDung.MaLop -> KhoaHoc.MaLop`, không phụ thuộc `LopHocPhan`/`DangKyHocPhan`; `KhoaHoc.MaLopHocPhan` chỉ là nullable legacy/roadmap. Nội dung học tập chuẩn vẫn lấy theo `DanhMucMonHoc -> Chuong -> BaiHoc`, không copy theo từng `KhoaHoc`.

Roadmap: Sau MVP cần hỗ trợ cấu hình quiz/bài tập theo `KhoaHoc` để mỗi lớp có lịch mở/đóng khác nhau. `ThoiKhoaBieu` hiện đã gắn `MaKhoaHoc` và `MaCaHoc` ở tầng database; `TienDoBaiHoc` và `DiemSo` vẫn cần cân nhắc thêm `MaKhoaHoc` nullable/required theo mức độ triển khai để phân biệt cùng môn ở lớp/giảng viên/học kỳ khác nhau.

## Registration / Enrollment APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/admin/registration-periods` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách đợt đăng ký trong scope cơ sở; tên đợt được derive từ học kỳ vì schema `GiaiDoanDangKy` chưa có cột tên riêng. |
| GET | `/api/admin/registration-periods/{id}` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Chi tiết đợt đăng ký kèm số lớp và số sinh viên đã đăng ký/waitlist. |
| POST | `/api/admin/registration-periods` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Tạo đợt nháp từ `maHocKy`, `openDate`, `closeDate`, `maxCredits`; validate học kỳ thuộc cơ sở và `closeDate > openDate`. |
| PUT | `/api/admin/registration-periods/{id}` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Sửa đợt nháp/đang mở; không cho sửa đợt đã đóng. |
| POST | `/api/admin/registration-periods/{id}/open` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Chuyển `GiaiDoanDangKy.TrangThai` sang `dang_mo`; có audit log. |
| POST | `/api/admin/registration-periods/{id}/close` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Chuyển đợt sang `da_dong`; sinh viên không thể đăng ký/hủy khi không có đợt đang mở. |
| DELETE | `/api/admin/registration-periods/{id}` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Chỉ xóa được đợt ở trạng thái `nhap`. |
| GET | `/api/admin/registration-periods/{id}/registrations` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách lớp học phần/capacity thuộc học kỳ của đợt. |
| GET | `/api/admin/registrations` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách đăng ký sinh viên gần nhất trong scope cơ sở, giới hạn 500 dòng. |
| GET | `/api/admin/course-sections/capacity` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Danh sách `LopHocPhan` kèm capacity, số đã đăng ký, waitlist, trạng thái và thông tin môn/giảng viên/TKB khi có `KhoaHoc.MaLopHocPhan`. |
| PUT | `/api/admin/course-sections/{id}/capacity` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Cập nhật `SucChua`; không được nhỏ hơn số đăng ký chính thức; tự promote waitlist theo FIFO nếu tăng sức chứa. |
| POST | `/api/admin/course-sections/{id}/cancel` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Hủy lớp học phần bằng `TrangThai = da_huy`, chuyển đăng ký active/waitlist sang `lop_bi_huy`, có audit log. |
| POST | `/api/admin/course-sections/{id}/reopen` | Admin/SuperAdmin/CampusAdmin/SubCampusAdmin/AcademicStaff | Mở lại lớp bằng `TrangThai = mo`, đồng bộ lại `SoDaDangKy`. |
| GET | `/api/student/registrations/available` | Student | Trả lớp học phần trong cơ sở/học kỳ của đợt `dang_mo` hiện tại; kèm trạng thái đăng ký của sinh viên nếu có. |
| GET | `/api/student/registrations` | Student | Danh sách đăng ký của sinh viên hiện tại, trừ đăng ký đã rút. |
| POST | `/api/student/registrations` | Student | Đăng ký lớp học phần từ `maLopHocPhan`; validate đợt đang mở, scope cơ sở, trùng môn/học kỳ, trùng lịch nếu có TKB gắn `KhoaHoc.MaLopHocPhan`, capacity, waitlist và giới hạn tín chỉ. |
| POST | `/api/student/registrations/{id}/withdraw` | Student | Hủy đăng ký active/waitlist khi đợt tương ứng còn đang mở; tự promote waitlist nếu sinh viên rút khỏi danh sách chính thức. |

### Schema limitations / follow-up

- `GiaiDoanDangKy` chưa có tên đợt, mô tả, cờ chốt danh sách, deadline hủy riêng hoặc cấu hình ngành/chương trình được phép đăng ký.
- `NguoiDung` entity hiện chưa có mã sinh viên/MSSV riêng trong contract này; API admin registrations tạm dùng `MaNguoiDung` làm `studentCode`.
- Kiểm tra cơ sở/học kỳ đã có; kiểm tra chuyên ngành hiện mới ở mức hạn chế vì `LopHocPhan` chưa gắn chương trình/ngành.
- Kiểm tra trùng lịch chỉ chạy khi lớp học phần có `KhoaHoc.MaLopHocPhan` và `ThoiKhoaBieu`; lớp chưa map course/TKB sẽ bỏ qua conflict check thay vì đoán lịch.

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

P6 hoàn thiện workflow bài tập theo role Student/Teacher trên schema hiện có `BaiTap`/`BaiNop`.

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/teacher/assignments` | Teacher | Danh sách bài tập thuộc các môn mà teacher đang phụ trách qua `KhoaHoc.MaGiaoVien`. Có phân trang `pageIndex`, `pageSize`. |
| POST | `/api/teacher/assignments` | Teacher | Tạo bài tập cho `courseId` teacher phụ trách. Lưu `BaiTap.MaMonHoc` theo khóa học; trạng thái `nhap`, `da_xuat_ban`, `da_dong`. |
| GET | `/api/teacher/assignments/{id}` | Teacher | Chi tiết bài tập trong phạm vi môn teacher phụ trách. |
| PUT | `/api/teacher/assignments/{id}` | Teacher | Cập nhật tiêu đề, mô tả, hạn nộp, số lần nộp, định dạng, hướng dẫn chấm, trạng thái. |
| DELETE | `/api/teacher/assignments/{id}` | Teacher | Xóa bài tập chưa có bài nộp; nếu đã có bài nộp thì đóng bài tập (`da_dong`). |
| GET | `/api/student/assignments` | Student | Danh sách bài tập đã publish/closed thuộc môn sinh viên đã đăng ký học phần (`da_dang_ky`). |
| GET | `/api/student/assignments/{assignmentId}` | Student | Chi tiết bài tập, rules nộp bài, lịch sử nộp và điểm/feedback đã công bố. |

### Dự kiến/cần bổ sung

- `GET /api/assignments`
- `GET /api/assignments/{id}`
- `POST /api/courses/{courseId}/assignments`
- `PUT /api/assignments/{id}`
- `PATCH /api/assignments/{id}/publish`
- `PATCH /api/assignments/{id}/close`

Ghi chú P6: `BaiTap` hiện chưa có cột `ma_khoa_hoc`, nên assignment được lưu theo `MaMonHoc`; `courseId` trong API teacher dùng để kiểm quyền và suy ra `MaMonHoc`. `BaiTap` cũng chưa có cột `diem_toi_da`; workflow chấm điểm dùng giới hạn hiện có của `BaiNop.diem_so` là 0-10.

## Submissions APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| POST | `/api/student/assignments/{assignmentId}/submit` | Student | Nộp bài bằng file lên storage hiện có. Chặn bài nháp, bài đã đóng, quá hạn, hết lượt nộp, hoặc student không thuộc môn. |
| GET | `/api/student/submissions` | Student | Lịch sử bài nộp của student hiện tại, chỉ trả điểm/feedback khi đã công bố. |
| GET | `/api/student/submissions/{id}` | Student | Chi tiết một bài nộp của student hiện tại. |
| GET | `/api/teacher/assignments/{id}/submissions` | Teacher | Danh sách bài nộp của một bài tập teacher có quyền. |
| GET | `/api/teacher/submissions` | Teacher | Danh sách bài nộp thuộc các môn teacher phụ trách. |
| GET | `/api/teacher/submissions/{id}` | Teacher | Chi tiết bài nộp trong phạm vi teacher phụ trách. |
| PUT | `/api/teacher/submissions/{id}/grade` | Teacher | Chấm điểm 0-10, lưu feedback, tùy chọn công bố cho student. |

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
| GET | `/api/exam/student/list` | Student | Danh sách bài thi/kiểm tra của học sinh hiện tại. Backend tự suy ra lớp, chương trình đào tạo và chuyên ngành từ JWT user, chỉ trả đề có môn thuộc `MonHocTrongChuongTrinh` của chương trình đó; frontend không truyền filter khoa/ngành. |
| POST | `/api/exam/taking/start` | Student | Bắt đầu phiên thi theo `MaCaThi`; không dùng giới hạn lượt quiz bài học. |
| POST | `/api/exam/taking/autosave` | Student | Lưu tạm câu trả lời phiên thi chính thức. |
| POST | `/api/exam/taking/submit` | Student | Nộp bài thi chính thức. |
| POST | `/api/exam/grading/auto/{maCaThi}` | Teacher/CampusAdmin/AcademicStaff/Admin/SuperAdmin | Chấm tự động phần trắc nghiệm theo đáp án thật, không dùng điểm ngẫu nhiên. |
| POST | `/api/exam/grading/essay` | Teacher/CampusAdmin/AcademicStaff/Admin/SuperAdmin | Nhập điểm cuối cùng cho bài có tự luận. |

### Dự kiến/cần bổ sung

- API cấu hình quiz theo từng `KhoaHoc` nếu cần lịch mở/đóng khác nhau giữa các lớp.
- API chấm tự luận chi tiết theo từng câu nếu cần rubric thay vì nhập `DiemCuoiCung` trực tiếp.

## Teacher Exam APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| POST | `/api/teacher/exams` | Teacher | Tạo đề thi cho teacher phụ trách. Validate quyền sở hữu khóa học qua `MaGiaoVien` + `TenMonHoc` + `MaCodeLop`. Tạo `DeKiemTra` và liên kết câu hỏi có sẵn (numeric MaCauHoi) qua `CauHoiDeKiemTra`. Trả `400` nếu thiếu tên/thời gian không hợp lệ; `404` nếu không tìm thấy lớp/môn phù hợp. |

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

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/teacher/classes/{classId}/grades` | Teacher | Lấy bảng điểm của lớp học. |
| PUT | `/api/teacher/classes/{classId}/grades/{studentId}` | Teacher | Cập nhật điểm thành phần (assignment, exam) của học sinh. |

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
| GET | `/api/notifications` | JWT | Alias backward-compatible của `/api/notifications/me`. |
| GET | `/api/notifications/me` | JWT | User xem danh sách thông báo của chính mình. Query từ `ThongBaoNguoiNhan` join `ThongBao`, chỉ trả `MaNguoiNhan = current user` và `daAn = false`. Filter `daDoc`/`isRead`, `loaiThongBao`, `mucDo`, `keyword`, `ngayTu`/`fromDate`, `ngayDen`/`toDate`. |
| GET | `/api/notifications/{id}` | JWT | User xem chi tiết một thông báo của chính mình. Nội dung Editor.js được trả qua `noiDungJson`; backend không render HTML. |
| GET | `/api/notifications/unread-count` | JWT | Alias backward-compatible của `/api/notifications/me/unread-count`. |
| GET | `/api/notifications/me/unread-count` | JWT | Lấy số thông báo chưa đọc của user hiện tại từ `ThongBaoNguoiNhan`. |
| PATCH | `/api/notifications/{id}/read` | JWT | Đánh dấu một thông báo của user hiện tại là đã đọc, set `ThongBaoNguoiNhan.daDoc = true`, `docLuc = now`. Idempotent. |
| PATCH | `/api/notifications/read-all` | JWT | Đánh dấu tất cả thông báo chưa đọc và chưa ẩn của user hiện tại là đã đọc. Không đổi `docLuc` của bản đã đọc trước đó. |
| DELETE | `/api/notifications/{id}` | JWT | Ẩn thông báo của user hiện tại bằng `ThongBaoNguoiNhan.daAn = true`, không hard delete. |
| POST | `/api/admin/notifications/preview-recipients` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Backend resolve danh sách người nhận cuối cùng theo `phamViGui`/`targetType`, trả count và tối đa 100 người đầu tiên để preview. |
| POST | `/api/admin/notifications` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Tạo và gửi thông báo thủ công trong transaction. Tạo 1 row `ThongBao` nội dung chung và nhiều row `ThongBaoNguoiNhan`. Không tạo thông báo nếu resolve ra 0 người nhận. |
| GET | `/api/admin/notifications` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Lịch sử gửi thông báo trong campus scope. Filter `maDonVi`/`campusId`, `loaiThongBao`, `mucDo`, `trangThai`, `nguoiTao`, `keyword`, `ngayTu`/`fromDate`, `ngayDen`/`toDate`. Trả `recipientCount`, `readCount`, `unreadCount`, `hiddenCount`. |
| GET | `/api/admin/notifications/{id}` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Chi tiết thông báo chung và thống kê trong scope hiện tại. |
| GET | `/api/admin/notifications/{id}/recipients` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Danh sách người nhận có phân trang; filter `daDoc`, `daAn`, `keyword`. |
| GET | `/api/admin/notifications/{id}/statistics` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Thống kê `tongNguoiNhan`, `tongDaDoc`, `tongChuaDoc`, `tongDaAn`. |
| PATCH | `/api/admin/notifications/{id}/cancel` | SuperAdmin/Admin/CampusAdmin/AcademicStaff | Hủy thông báo khi chưa gửi (`nhap`/scheduled future). Thông báo đã `da_gui` không được hủy trong P0-NT-Core. |

Request tạo manual notification:

```json
{
  "tieuDe": "Thông báo nghỉ học",
  "tomTatNoiDung": "Lớp SD1904 nghỉ học ngày mai.",
  "noiDungJson": "{\"time\":1710000000000,\"blocks\":[]}",
  "noiDungText": "Lớp SD1904 nghỉ học ngày mai.",
  "mucDo": "important",
  "loaiThongBao": "manual",
  "phamViGui": "lop_hanh_chinh",
  "targetType": "class",
  "targetIds": [1]
}
```

Recipient scopes P0-NT-Core:

- `toan_he_thong`: chỉ SuperAdmin; resolve toàn bộ user active.
- `don_vi`/legacy `campus`: resolve user active trong đơn vị được phép.
- `lop_hanh_chinh`/legacy `class`: lớp phải trong scope; resolve học sinh active của lớp.
- `khoa_hoc`/legacy `course`: khóa học trong scope; resolve học sinh lớp của khóa học và giáo viên phụ trách.
- `vai_tro`: resolve user active theo role trong scope.
- `nguoi_dung`/legacy `users`: validate từng user active trong scope.

Ghi chú P0-NT-Core: `ThongBao` là nội dung chung; `ThongBaoNguoiNhan` giữ trạng thái từng người nhận (`daDoc`, `docLuc`, `daAn`, `anLuc`). Migration `AddNotificationRecipientState` giữ các cột legacy `ma_nguoi_nhan`, `da_doc`, `doc_luc`, `ma_nhom_thong_bao` trên `ThongBao` để tương thích dữ liệu/API P0-8, nhưng logic mới đọc/ghi trạng thái qua `ThongBaoNguoiNhan`. Migration backfill bảo thủ: mỗi row `ThongBao` cũ được giữ nguyên và tạo một row recipient tương ứng, không tự gom nhóm theo `ma_nhom_thong_bao` để tránh merge sai nội dung. Editor.js JSON phải là object hợp lệ, `blocks` nếu có phải là array, reject `data:image`/base64, và backend extract `noiDungText`/summary nếu client không gửi.

Known limitations P0-NT-Core: chưa làm notification templates Task 10, học phí con nợ nâng cao Task 11, scheduled/background notification, email/push/SMS thật, tích hợp Khen thưởng/Kỷ luật, và frontend.

### Dự kiến/cần bổ sung

- `GET /api/notification-preferences`
- `PUT /api/notification-preferences`

## Applications / Đơn Từ APIs

### Đã có trong P0-DT1/P0-DT2/P0-DT3/P0-DT4/P0-DT5/P0-DT6

P0-DT1 làm foundation schema, constants, state machine, template validator và API read-only để FE đọc loại đơn/mẫu đơn. P0-DT2 bổ sung vòng đời sinh viên: tạo nháp, xem đơn của chính mình, cập nhật nháp/yêu cầu bổ sung, nộp, nộp lại và hủy đơn. P0-DT3 bổ sung upload/download/delete minh chứng an toàn cho sinh viên. P0-DT4 bổ sung hàng đợi admin, SLA, tiếp nhận, phân công/phân công lại và admin download minh chứng. P0-DT5 bổ sung yêu cầu bổ sung, duyệt và từ chối đơn. P0-DT6 bổ sung nền xử lý nghiệp vụ sau duyệt, chỉ auto ghi nhận đơn `xac_nhan`; các loại khác chuyển `can_xu_ly_thu_cong` hoặc ghi nhận kết quả thủ công. P0-DT7 bổ sung báo cáo tổng quan quản trị và đóng audit/metadata cho upload/xóa minh chứng. Chưa làm notification hoặc side-effect nghiệp vụ thật.

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/applications/schema/types` | JWT | Trả 11 loại đơn hợp lệ: `nghi_phep`, `thi_lai`, `chuyen_truong`, `cap_chung_chi`, `khac`, `phuc_tra_diem`, `bao_luu`, `chuyen_nganh`, `chuyen_co_so`, `xac_nhan`, `rut_hoc`. |
| GET | `/api/applications/schema/statuses` | JWT | Trả trạng thái đơn, trạng thái terminal và danh sách trạng thái tiếp theo theo state machine. |
| GET | `/api/applications/templates` | JWT | Trả danh sách mẫu đơn active version hiện tại. Không trả entity trực tiếp, không trả audit metadata. |
| GET | `/api/applications/templates/{loaiDon}` | JWT | Trả mẫu đơn active theo loại. `loaiDon` không hợp lệ trả `400`; loại hợp lệ nhưng không có mẫu active trả `404`. |
| POST | `/api/student/applications` | Student | Tạo đơn nháp từ template active của `loaiDon`. Backend tự gán `MaHocSinh`, `MaDonVi`, `MaMauDon`, `TrangThai = nhap`; không tin field hệ thống từ client. |
| GET | `/api/student/applications` | Student | Sinh viên xem danh sách đơn của chính mình. Filter `loaiDon`, `trangThai`, `ngayTu`, `ngayDen`, `search`, `pageIndex`, `pageSize`. Response có `trangThaiXuLyNghiepVu`, `tenTrangThaiXuLyNghiepVu`; không trả JSON xử lý nội bộ. |
| GET | `/api/student/applications/{id}` | Student | Sinh viên xem chi tiết đơn của chính mình, gồm dữ liệu form, template snapshot an toàn, minh chứng metadata an toàn, `lyDoTuChoi`, `noiDungYeuCauBoSung`, trạng thái xử lý nghiệp vụ và timeline public. Không trả `StorageKey`, `UrlBangChung`, ghi chú nội bộ, `KetQuaXuLyJson`, `NhatKyTuDong` hoặc snapshot nội bộ. |
| PUT | `/api/student/applications/{id}` | Student | Cập nhật đơn ở trạng thái `nhap` hoặc `yeu_cau_bo_sung`. Body cần `rowVersion` base64 để chống lost update. |
| POST | `/api/student/applications/{id}/submit` | Student | Nộp đơn nháp, validate required fields, related entity, rule theo loại đơn và metadata minh chứng; chuyển `nhap -> da_nop`. |
| POST | `/api/student/applications/{id}/resubmit` | Student | Nộp lại đơn đang `yeu_cau_bo_sung`, giữ `NgayNop` ban đầu, clear nội dung yêu cầu bổ sung và chuyển về `dang_xem_xet`. |
| POST | `/api/student/applications/{id}/cancel` | Student | Hủy đơn của chính mình khi trạng thái cho phép (`nhap`, `da_nop`, `yeu_cau_bo_sung`); không hard delete. |
| POST | `/api/student/applications/{id}/attachments` | Student | Upload một hoặc nhiều file minh chứng dạng multipart field `files` và `rowVersion`. Chỉ cho đơn của chính sinh viên ở trạng thái `nhap` hoặc `yeu_cau_bo_sung`. Thành công trả `201`, danh sách metadata an toàn, `rowVersion` mới, số file active và tổng dung lượng. |
| GET | `/api/student/applications/{id}/attachments/{attachmentId}/download` | Student | Download binary file minh chứng của chính sinh viên nếu metadata chưa soft delete và object còn tồn tại. Trả `Content-Disposition: attachment`, content type canonical trong DB, `X-Content-Type-Options: nosniff`, `Cache-Control: private, no-store`. |
| DELETE | `/api/student/applications/{id}/attachments/{attachmentId}` | Student | Soft delete metadata minh chứng của chính sinh viên bằng body `{ "rowVersion": "..." }`. Chỉ cho đơn `nhap` hoặc `yeu_cau_bo_sung`; physical delete best-effort sau khi DB commit. |
| GET | `/api/admin/applications` | ApplicationQueueRead | Hàng đợi admin có filter `maDonVi`, `maHocSinh`, `nguoiDuyetHienTai`, `loaiDon`, `trangThai`, `trangThaiXuLyNghiepVu`/`processingStatus`, `assignmentState`, `slaStatus`, `tuNgayNop`, `denNgayNop`, `search`, `pageIndex`, `pageSize`. Nếu không truyền `trangThai`, chỉ trả `da_nop`, `dang_xem_xet`. |
| GET | `/api/admin/applications/queue-summary` | ApplicationQueueRead | Tổng quan queue trong scope hiện tại: active, submitted, in review, need supplement, unassigned, overdue, due soon. `active` là alias của `totalActive`; `waitingForSupplement` là alias của `needSupplement`. Backend tính summary bằng một conditional aggregate SQL query trong cùng scope/filter. |
| GET | `/api/admin/applications/reports/overview` | ApplicationQueueRead | Báo cáo tổng quan đơn từ trong campus scope hiện tại. Filter: `maDonVi`/`campusId`, `loaiDon`/`type`, `trangThai`/`status`, `trangThaiXuLyNghiepVu`/`processingStatus`, `nguoiDuyetHienTai`/`assigneeId`, `nguoiXuLyCuoi`/`processorId`, `tuNgayNop`/`submittedFrom`, `denNgayNop`/`submittedTo`. Alias cùng giá trị được chấp nhận, khác giá trị trả `400`; campus ngoài scope trả `403`. |
| GET | `/api/admin/applications/{applicationId}` | ApplicationQueueRead | Admin xem chi tiết đơn trong campus scope, gồm form data safe, attachment metadata safe, timeline admin và allowed actions. Timeline admin không trả raw `SnapshotJson`; chỉ trả `metadata` đã sanitize theo allowlist. Ngoài scope trả `404`. |
| POST | `/api/admin/applications/{applicationId}/receive` | ApplicationReceive | Tiếp nhận đơn `da_nop` chưa có `NguoiDuyetHienTai`; body `{ "rowVersion": "..." }`; chuyển sang `dang_xem_xet`, gán current user, ghi log public `tiep_nhan`. |
| POST | `/api/admin/applications/{applicationId}/assign` | ApplicationAssignmentManage | Phân công hoặc phân công lại; body `{ "assigneeId": 1, "rowVersion": "...", "lyDo": "..." }`. Reassign bắt buộc `lyDo` 10-1000 ký tự và ghi log internal. |
| POST | `/api/admin/applications/{applicationId}/request-supplement` | ApplicationReviewOperate | Yêu cầu bổ sung hồ sơ. Body `{ "request": "...", "internalNote": "...", "rowVersion": "..." }`; `request` required/trim 10-2000 ký tự, `internalNote` optional tối đa 2000. Chỉ `dang_xem_xet -> yeu_cau_bo_sung`, giữ assignee/deadline, ghi timeline public và audit cùng transaction. |
| POST | `/api/admin/applications/{applicationId}/approve` | ApplicationSensitiveDecision | Duyệt đơn. Body `{ "publicNote": "...", "internalNote": "...", "rowVersion": "..." }`; `publicNote` optional tối đa 1000, default `Đơn đã được phê duyệt.`. Chỉ `dang_xem_xet -> da_duyet`, revalidate template/form/reference/rule/evidence trước khi duyệt, clear assignee, set `TrangThaiXuLyNghiepVu = cho_xu_ly`, ghi timeline/audit atomically. |
| POST | `/api/admin/applications/{applicationId}/reject` | ApplicationSensitiveDecision | Từ chối đơn. Body `{ "reason": "...", "internalNote": "...", "rowVersion": "..." }`; `reason` required/trim 10-2000 ký tự, `internalNote` optional tối đa 2000. Chỉ `dang_xem_xet -> tu_choi`, lưu `LyDoTuChoi`, clear assignee/supplement, không set `NgayDuyet`, ghi timeline/audit atomically. |
| POST | `/api/admin/applications/{applicationId}/process` | ApplicationProcessingOperate | Chạy xử lý nghiệp vụ sau duyệt tự động. Body `{ "rowVersion": "..." }`. Chỉ áp dụng đơn `da_duyet` có `TrangThaiXuLyNghiepVu = cho_xu_ly` hoặc `xu_ly_that_bai`; `chua_xu_ly` trả `409`, đơn chưa duyệt trả `400`. `xac_nhan` được auto `da_ghi_nhan`; loại chưa có handler chuyển `can_xu_ly_thu_cong`. Trạng thái terminal hoặc manual-required là no-op idempotent, không ghi log/audit mới. |
| POST | `/api/admin/applications/{applicationId}/record-processing-result` | ApplicationProcessingOperate | Ghi nhận kết quả xử lý thủ công. Body `{ "outcome": "da_ghi_nhan|xu_ly_thanh_cong|xu_ly_that_bai", "publicNote": "...", "internalNote": "...", "result": {}, "rowVersion": "..." }`. Chỉ cho trạng thái hiện tại `cho_xu_ly`, `can_xu_ly_thu_cong`, `xu_ly_that_bai`; terminal `da_ghi_nhan`/`xu_ly_thanh_cong` trả `409`. `result` phải là object/null an toàn, tối đa 16KB, depth 5, 50 properties, array tối đa 100 item, cấm key nhạy cảm. |
| GET | `/api/admin/applications/assignees` | ApplicationAssignmentManage | Danh sách người có thể xử lý đơn trong scope hiện tại, filter `maDonVi`, `search`, `pageIndex`, `pageSize`. |
| GET | `/api/admin/applications/{applicationId}/attachments/{attachmentId}/download` | ApplicationQueueRead | Admin download minh chứng trong scope hiện tại. Metadata đã soft delete, ngoài scope hoặc object không tồn tại trả `404`; storage lỗi trả `503`. |

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

Ghi chú P0-DT4:

- Policy mới: `ApplicationQueueRead` cho SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff/Principal; `ApplicationReceive` cho SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff; `ApplicationAssignmentManage` cho SuperAdmin/Admin/CampusAdmin/SubCampusAdmin.
- Service admin re-query current user từ DB qua `CurrentUserContext`, chỉ chấp nhận user `hoat_dong`; không tin role/campus claim đơn thuần.
- Scope: SuperAdmin/Admin global; CampusAdmin gồm cơ sở và cơ sở con; SubCampusAdmin/AcademicStaff/Principal exact campus. Filter ngoài scope trả `403`; detail/download ngoài scope trả `404`.
- SLA status: `paused` khi `yeu_cau_bo_sung`; `overdue`, `due_soon`, `on_track` cho trạng thái active dựa trên `HanXuLyLuc`; terminal hoặc thiếu deadline là `none`. `ApplicationQueue:SlaWarningBeforeHours` mặc định 24, hợp lệ 1-168.
- Receive/assign/reassign dùng transaction `Serializable`, `sp_getapplock` resource `ApplicationWorkflow:{applicationId}`, rowVersion base64 8 byte. Stale/race/deadlock/timeout trả `409`.
- Receive chỉ cho `da_nop` và chưa có assignee. Assign cho `da_nop`, `dang_xem_xet`, `yeu_cau_bo_sung`; không cho `nhap`, `da_duyet`, `tu_choi`, `da_huy`.
- Same-assignee assign là no-op nhưng vẫn validate rowVersion; stale trả `409`, fresh trả detail hiện tại và không ghi log.
- Admin detail/download không trả `StorageKey`, `TenFileLuu`, `FileHash`, password/hash người dùng hoặc URL public.
- Admin timeline không trả `SnapshotJson` nội bộ. `metadata` chỉ gồm các field allowlist: `operation`, `fromAssigneeId`, `toAssigneeId`, `reasonProvided`, `templateAssigned`, `changedFields`, `attachmentIds`, `attachmentId`, `fileCount`. Snapshot rỗng, malformed hoặc root không phải object trả `metadata = null`; key lạ hoặc key nhạy cảm như storage key, hash, token, secret, connection string bị bỏ qua.
- Queue summary giữ alias backward-compatible nhưng không chạy count riêng cho alias. SLA `overdue`/`dueSoon` chỉ tính cho `da_nop` và `dang_xem_xet`; `yeu_cau_bo_sung` là paused nên không góp vào hai bucket đó.
- Verification P0-DT4.1 dùng TRX làm nguồn kết quả test chính. GitHub Actions hiện có workflow backend build-only: restore và build Release, không chạy integration test vì các API test cần SQL Server isolated và local object storage.

Ghi chú P0-DT5:

- Policy mới: `ApplicationReviewOperate` cho SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff. `ApplicationSensitiveDecision` giữ cho SuperAdmin/Admin/CampusAdmin/Principal.
- Role matrix: SuperAdmin/Admin/CampusAdmin được yêu cầu bổ sung, duyệt, từ chối; SubCampusAdmin chỉ yêu cầu bổ sung; AcademicStaff chỉ yêu cầu bổ sung đơn đang giao cho chính mình; Principal chỉ duyệt/từ chối trong exact campus; Teacher/Student/Parent không có quyền.
- Mọi decision yêu cầu đơn đã có `NguoiDuyetHienTai`; chưa phân công trả `409`. Campus ngoài scope trả `404` ở service. Role/campus/current status được re-query từ DB, không tin JWT claim đơn thuần.
- Decision dùng transaction `Serializable`, `sp_getapplock` resource `ApplicationWorkflow:{applicationId}`, RowVersion base64 8 byte. Missing/invalid RowVersion trả `400`; stale/race/deadlock/lock timeout/SQL timeout trả `409`.
- Request supplement không reset SLA; `yeu_cau_bo_sung` hiển thị SLA `paused`. Student resubmit hiện reset deadline theo template `SlaGio`.
- Approve revalidate exact template đã gắn trong đơn; legacy `MaMauDon = null` chỉ resolve active template cùng loại nếu còn an toàn. Validate lại template JSON, form JSON, related references, rule theo loại đơn và minh chứng trước khi đổi trạng thái.
- Timeline decision hiển thị cho student nhưng chỉ trả public note. Admin detail vẫn trả internal note trong timeline admin; raw `SnapshotJson` không expose. Metadata decision chỉ gồm `decision`, `previousAssigneeId`, `processorId`.
- Audit decision được insert trực tiếp vào `NhatKyKiemToan` trong cùng transaction với update `DonTu` và insert timeline; old/new value chỉ chứa status, assignee, last processor và business processing status.
- Known limitations: chưa có escalation, notification, workflow nhiều cấp, bulk decision hoặc frontend.

Ghi chú P0-DT6:

- Policy mới `ApplicationProcessingOperate` cho SuperAdmin/Admin/CampusAdmin/SubCampusAdmin/AcademicStaff. Principal, Teacher, Student, Parent không được xử lý nghiệp vụ sau duyệt.
- Service dùng transaction `Serializable`, `sp_getapplock` resource `ApplicationWorkflow:{applicationId}`, `RowVersion` base64 8 byte và map stale/race/deadlock/timeout về `409`.
- Auto handler hiện chỉ có `xac_nhan` và chỉ ghi nhận metadata xử lý trong `KetQuaXuLyJson`, `NhatKyTuDong`; không cấp chứng chỉ, không đổi điểm, không đổi điểm danh, không chuyển cơ sở/ngành/lớp và không tạo side-effect nghiệp vụ thật.
- Loại đơn chưa có handler tự động được chuyển `can_xu_ly_thu_cong` để admin/học vụ ghi nhận kết quả thủ công qua endpoint riêng.
- Mỗi mutation thực sự thêm một timeline `xu_ly_nghiep_vu` public cho student và một audit `xu_ly_nghiep_vu`. No-op idempotent không tạo timeline/audit mới.
- Admin timeline metadata DT6 chỉ expose allowlist `operation`, `handler`, `processingStatusFrom`, `processingStatusTo`, `outcome`, `processorId`; không expose raw result JSON, form values hoặc dữ liệu nhạy cảm.

Ghi chú P0-DT7:

- Report overview dùng cùng policy `ApplicationQueueRead` và `ApplicationCampusScopeService`; service re-query actor từ DB, user bị khóa/inactive/đổi role sẽ bị từ chối. SuperAdmin/Admin xem toàn hệ thống; CampusAdmin xem campus hiện tại và descendants; SubCampusAdmin/AcademicStaff/Principal exact campus.
- `summary.totalApplications` là tổng `DonTu` sau scope/filter. `pendingReview` chỉ gồm `da_nop`, `dang_xem_xet`; `waitingForSupplement` là `yeu_cau_bo_sung`; `approved`, `rejected`, `cancelled` theo trạng thái đơn; processing buckets theo `TrangThaiXuLyNghiepVu`.
- `overdue` và `dueSoon` chỉ tính `da_nop`, `dang_xem_xet` có `HanXuLyLuc`; không tính `yeu_cau_bo_sung`, terminal hoặc draft. `dueSoon` dùng `ApplicationQueue:SlaWarningBeforeHours`.
- `approvalRate` và `rejectionRate` dùng denominator `approved + rejected`; nếu không có quyết định thì cả hai bằng 0. `averageReviewHours` tính từ `NgayNop` tới timeline quyết định mới nhất (`phe_duyet`/`tu_choi`), fallback `NgayDuyet`; thiếu timestamp hợp lệ trả `null`.
- Status breakdown luôn trả đủ `nhap`, `da_nop`, `dang_xem_xet`, `yeu_cau_bo_sung`, `da_duyet`, `tu_choi`, `da_huy`. Processing breakdown luôn trả đủ `chua_xu_ly`, `cho_xu_ly`, `da_ghi_nhan`, `xu_ly_thanh_cong`, `xu_ly_that_bai`, `can_xu_ly_thu_cong`. Type breakdown luôn trả đủ 11 loại đơn active trong hệ thống, cộng bucket legacy nếu DB có.
- Query report dùng SQL aggregate/grouping với tags `P0-DT7 ReportSummary`, `P0-DT7 ReportByType`, `P0-DT7 ReportByCampus`, `P0-DT7 ReportReviewDuration`; không parse JSON, không load toàn bộ đơn về RAM và không N+1 campus.
- Future timeline snapshot cho upload minh chứng: `{ "operation": "upload_evidence", "fileCount": n }`; delete: `{ "operation": "delete_evidence", "attachmentId": id }`. Legacy snapshot `{ "attachmentAction": "upload", "count": n }` và `{ "attachmentAction": "delete", "maTep": id }` vẫn được sanitizer map sang metadata typed, không expose raw/sensitive keys.
- Upload/delete minh chứng ghi thêm `NhatKyKiemToan` tối giản trong cùng DB transaction với metadata/timeline. Audit old/new chỉ gồm `activeFileCount` và `totalSizeBytes`, `HanhDong = cap_nhat`, `LoaiDoiTuong = DonTu`, không chứa filename, storage key, file hash, path, MIME bytes hoặc full attachment.
- Known limitations DT7: chưa export, charts, trend analytics, business-hour SLA, post-approval duration analytics, advanced audit search hoặc frontend.

Ghi chú P0-DT8:

- Không có API Đơn từ mới. Các endpoint hiện có tạo notification như side effect sau khi mutation chính đã lưu thành công.
- Event gửi cho học sinh: `submit`, `receive/assign`, `request-supplement`, `approve`, `reject`, `cancel`, `process` recorded/succeeded/failed/manual-required.
- Notification dùng `LoaiThongBao = system`, `LoaiDoiTuongLienKet = DonTu`, `MaDoiTuongLienKet = applicationId`, `DuongDan = /student/applications/{id}`. Học sinh đọc qua `/api/notifications/me` và detail/read/hide APIs hiện có.
- Lỗi gửi notification không rollback quyết định đơn, timeline hoặc audit nghiệp vụ chính; backend log warning và tiếp tục trả kết quả mutation thành công.
- Payload notification chỉ chứa public status/message. Không đưa `internalNote`, `GhiChuNoiBo`, `SnapshotJson`, `KetQuaXuLyJson`, `NhatKyTuDong`, raw form data, storage key hoặc file hash vào thông báo học sinh.
- Dedup nhẹ theo student + linked `DonTu` + title event để tránh gửi trùng cùng transition.
- Known limitations DT8: chưa email/push/SMS, chưa gửi phụ huynh, chưa notification reminder/SLA, chưa notification template riêng và chưa frontend integration.

## Reports APIs

### Đã có

| Method | Endpoint | Quyền | Mô tả |
|---|---|---|---|
| GET | `/api/admin/applications/reports/overview` | ApplicationQueueRead | Báo cáo tổng quan module đơn từ theo campus scope/filter, trả summary, status/processing/type/campus breakdown, approval/rejection rates và average review hours. |

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
## DL1 - Discipline Record Creation

### GET /api/admin/discipline-records
Get list of discipline records with filters.

**Request Query**
- maDonVi (int?)
- maHocKy (int?)
- maHocSinh (int?)
- mucDoKyLuat (string?)
- hinhThucXuLy (string?)
- 	rangThai (string?)
- keyword (string?)
- 	uNgay (date?)
- denNgay (date?)
- pageIndex (int, default 1)
- pageSize (int, default 20)

**Response**
- Items: Array of list item DTOs.
- TotalCount
- PageIndex
- PageSize

### GET /api/admin/discipline-records/{id}
Get detail of discipline record.

### POST /api/admin/discipline-records
Create new discipline record.

**Request Body**
- maHocSinh (int, required)
- maHocKy (int?)
- 	ieuDe (string, required)
- moTaViPham (string, required)
- 
gayViPham (date, required)
- mucDoKyLuat (string, required)
- hinhThucXuLy (string, required)
- canCuXuLy (string?)
- ghiChuNoiBo (string?)
- evidenceJson (object?)

### PUT /api/admin/discipline-records/{id}
Update discipline record (only allowed if in Draft or PendingApproval state).

### POST /api/admin/discipline-records/{id}/submit
Submit discipline record to change state from Draft to PendingApproval.

### POST /api/admin/discipline-records/{id}/cancel
Cancel a discipline record.

**Request Body**
- 
eason (string, required)


### POST /api/admin/discipline-records/{id}/remove-effect
Gỡ hiệu lực của một hồ sơ kỷ luật đang có hiệu lực.
- Request Body: `{ "reason": "string", "removalNote": "string" }`
- Response: `DisciplineRecordResultDto`
- Role: SuperAdmin, CampusAdmin, SubCampusAdmin

### POST /api/admin/discipline-records/{id}/void-approved
Hủy một hồ sơ kỷ luật đã duyệt nhưng chưa có hiệu lực (hoặc đã duyệt nhưng không kích hoạt).
- Request Body: `{ "reason": "string", "internalNote": "string" }`
- Response: `DisciplineRecordResultDto`
- Role: SuperAdmin, CampusAdmin, SubCampusAdmin

### GET /api/admin/discipline-appeals
Lấy danh sách các khiếu nại kỷ luật (có phân trang).
- Query: `PageIndex`, `PageSize`, `MaDonVi`, `TrangThai`, `Keyword`
- Response: `PagedResultDto<DisciplineAppealListItemDto>`
- Role: SuperAdmin, GiaoVu

### GET /api/admin/discipline-appeals/{id}
Xem chi tiết một khiếu nại kỷ luật.
- Response: `DisciplineAppealDetailDto`
- Role: SuperAdmin, GiaoVu

### POST /api/admin/discipline-appeals/{id}/resolve
Xử lý (chấp nhận/từ chối) một khiếu nại.
- Request Body: `{ "decision": "chap_nhan/tu_choi", "reason": "string", "resolutionNote": "string", "removeEffect": bool }`
- Response: `DisciplineAppealDetailDto`
- Role: SuperAdmin, GiaoVu

### GET /api/student/discipline-records
Lấy danh sách hồ sơ kỷ luật của chính học sinh (có phân trang).
- Query: `PageIndex`, `PageSize`, `MaHocKy`, `TrangThai`
- Response: `PagedResultDto<StudentDisciplineRecordListItemDto>`
- Role: Student

### GET /api/student/discipline-records/{id}
Xem chi tiết một hồ sơ kỷ luật của chính học sinh.
- Response: `StudentDisciplineRecordDetailDto`
- Role: Student

### POST /api/student/discipline-records/{id}/appeals
Tạo một khiếu nại đối với một hồ sơ kỷ luật.
- Request Body: `{ "reason": "string", "evidenceJson": any }`
- Response: `DisciplineAppealListItemDto`
- Role: Student

### GET /api/student/discipline-records/appeals/{appealId}
Xem chi tiết một khiếu nại của chính học sinh.
- Response: `DisciplineAppealDetailDto`
- Role: Student


## Reward and Discipline Notifications

### Resend Reward Notification
`POST /api/admin/rewards/{id}/notifications/resend`
- **Role**: `SuperAdmin`
- **Description**: Manually resends the notification for a reward.
- **Request Body**:
```json
{
  "reason": "String (min 10 chars)"
}
```
- **Response**: `200 OK`

### Resend Discipline Record Notification
`POST /api/admin/discipline-records/{id}/notifications/resend`
- **Role**: `SuperAdmin`
- **Description**: Manually resends the notification for a discipline record.
- **Request Body**:
```json
{
  "reason": "String (min 10 chars)"
}
```
- **Response**: `200 OK`

### Resend Discipline Appeal Notification
`POST /api/admin/discipline-appeals/{id}/notifications/resend`
- **Role**: `SuperAdmin`
- **Description**: Manually resends the notification for a discipline appeal.
- **Request Body**:
```json
{
  "reason": "String (min 10 chars)"
}
```
- **Response**: `200 OK`

## Reward and Discipline Reports

### Đã có

Base route: `/api/admin/reward-discipline/reports`

Quyền:

- `SuperAdmin`, `Admin`: xem toàn hệ thống.
- `CampusAdmin`: chỉ xem dữ liệu thuộc cơ sở hiện tại và cơ sở con theo campus scope.
- Role khác, gồm `Student`, không được gọi các endpoint báo cáo admin.

Common query:

- `maDonVi` optional; nếu ngoài scope trả `403`.
- `maHocKy` optional.
- `fromDate`, `toDate` optional; `fromDate > toDate` trả `400`.
- `groupBy` optional tùy endpoint: `day`, `month`, `semester`, `campus`.
- `pageIndex`, `pageSize` nhận vào cho contract dashboard; service normalize `pageSize` tối đa 100 khi dùng.

Sensitive data policy:

- Aggregate report không trả `MoTa`, `ChungTuJson`, `GhiChuNoiBo`, lý do khiếu nại chi tiết, storage key, file hash hoặc stack trace.
- Recent failed certificate chỉ trả lỗi đã cắt ngắn/an toàn từ metadata PDF, không trả raw exception/stack trace.
- `top-students` chỉ trả chỉ báo tổng hợp; `balanced score` là chỉ báo nội bộ, không dùng làm quyết định tự động.

| Method | Endpoint | Query chính | Mô tả |
|---|---|---|---|
| GET | `/api/admin/reward-discipline/reports/overview` | common query | Tổng quan khen thưởng/kỷ luật: campaign, rewards, PDF, discipline records, appeals, breakdown theo trạng thái và latest events an toàn. |
| GET | `/api/admin/reward-discipline/reports/rewards` | common query + `loaiDot`, `loaiKhenThuong`, `trangThai` | Báo cáo khen thưởng: campaign status, ứng viên, approved/issued/cancelled/restored rewards, breakdown theo loại/học kỳ/cơ sở/trạng thái và top rewarded students. |
| GET | `/api/admin/reward-discipline/reports/discipline` | common query + `mucDoKyLuat`, `hinhThucXuLy`, `trangThai` | Báo cáo kỷ luật: active/approved/rejected/expired/removed/cancelled, breakdown theo mức độ/hình thức/trạng thái/học kỳ/cơ sở và repeat discipline students. |
| GET | `/api/admin/reward-discipline/reports/certificates` | common query + `maDotKhenThuong`, `maMauBangKhen`, `trangThai` | Báo cáo sinh bằng khen/PDF: eligible, generated, failed, failure rate, breakdown theo template/campaign/status và recent failures an toàn. |
| GET | `/api/admin/reward-discipline/reports/appeals` | common query + `trangThai` | Báo cáo khiếu nại kỷ luật: pending/accepted/rejected, SLA mặc định 72 giờ, average resolution time và breakdown theo trạng thái/mức độ/học kỳ/cơ sở. |
| GET | `/api/admin/reward-discipline/reports/trends` | `metric`, `groupBy`, common query | Xu hướng theo `day`, `month`, `semester`. Metric: `rewards`, `issued_rewards`, `certificates_generated`, `discipline_records`, `active_discipline`, `discipline_appeals`. Với `groupBy=day`, date range tối đa 370 ngày nếu truyền đủ hai mốc. |
| GET | `/api/admin/reward-discipline/reports/top-students` | `mode=reward/discipline/balanced`, `limit`, `maDonVi`, `maHocKy` | Sinh viên nổi bật/cần lưu ý trong scope; `limit` mặc định 10, tối đa 50. |

Known limitations:

- Chưa có export Excel/PDF báo cáo.
- Chưa có frontend dashboard/chart.
- `totalDownloadedByStudents` trong certificate report hiện trả `null` vì schema chưa lưu lượt tải bằng khen.
- Chưa group theo khoa/ngành/lớp vì dữ liệu reward/discipline hiện lưu trực tiếp theo học sinh/cơ sở/học kỳ; cần thiết kế riêng nếu muốn drill-down sâu hơn.

### NT-SPECIAL - Specialized Notifications (DL/RD)
- `GET /api/admin/specialized-notifications/categories`
- `POST /api/admin/specialized-notifications/preview-recipients`
- `POST /api/admin/specialized-notifications/tuition`
- `POST /api/admin/specialized-notifications/academic`
- `POST /api/admin/specialized-notifications/urgent`
- `POST /api/admin/specialized-notifications/maintenance`

### 8.8 Application Advanced Reports (DT-REPORT-PLUS)
- `GET /api/admin/applications/reports/overview` (SuperAdmin, Admin, CampusAdmin, GiaoVu) - Lấy tổng quan đơn từ
- `GET /api/admin/applications/reports/by-type` (SuperAdmin, Admin, CampusAdmin, GiaoVu) - Lấy báo cáo theo loại đơn
- `GET /api/admin/applications/reports/pending` (SuperAdmin, Admin, CampusAdmin, GiaoVu) - Lấy báo cáo đơn đang chờ xử lý, có phân trang
- `GET /api/admin/applications/reports/overdue` (SuperAdmin, Admin, CampusAdmin, GiaoVu) - Lấy báo cáo đơn quá hạn, có phân trang
- `GET /api/admin/applications/reports/processing-time` (SuperAdmin, Admin, CampusAdmin, GiaoVu) - Lấy báo cáo thời gian xử lý
- `GET /api/admin/applications/reports/by-assignee` (SuperAdmin, Admin, CampusAdmin, GiaoVu) - Lấy báo cáo theo người xử lý
- `GET /api/admin/applications/reports/trends` (SuperAdmin, Admin, CampusAdmin, GiaoVu) - Lấy báo cáo xu hướng (theo ngày, tháng, học kỳ)

## Notification Templates
### Admin
- `GET /api/admin/notification-templates` - Get list of templates
- `GET /api/admin/notification-templates/{id}` - Get template details
- `POST /api/admin/notification-templates` - Create new template
- `PUT /api/admin/notification-templates/{id}` - Update template
- `DELETE /api/admin/notification-templates/{id}` - Delete template
- `POST /api/admin/notification-templates/{id}/activate` - Activate template
- `POST /api/admin/notification-templates/{id}/deactivate` - Deactivate template
- `POST /api/admin/notification-templates/preview` - Preview template content

## Schedule & Attendance (TKB & Điểm Danh)

### Ca Học
- `GET /api/ca-hoc` - Get list of shifts
- `GET /api/ca-hoc/active` - Get active shifts
- `GET /api/ca-hoc/{id}` - Get shift details
- `POST /api/ca-hoc` - Create shift
- `PUT /api/ca-hoc/{id}` - Update shift
- `PATCH /api/ca-hoc/{id}/toggle-active` - Toggle active state

### Thời Khóa Biểu
- `GET /api/thoi-khoa-bieu` - Get schedules
- `GET /api/thoi-khoa-bieu/{id}` - Get schedule details
- `POST /api/thoi-khoa-bieu` - Create schedule
- `PUT /api/thoi-khoa-bieu/{id}` - Update schedule
- `PATCH /api/thoi-khoa-bieu/{id}/cancel` - Cancel schedule
- `POST /api/thoi-khoa-bieu/check-xung-dot` - Check conflict
- `POST /api/thoi-khoa-bieu/{id}/generate-sessions` - Generate BuoiHoc sessions
- `POST /api/thoi-khoa-bieu/xep-lich-thong-minh` - Smart timetable generate draft
- `GET /api/thoi-khoa-bieu/drafts/{draftId}` - Get draft detail
- `GET /api/thoi-khoa-bieu/drafts` - List drafts
- `POST /api/thoi-khoa-bieu/xep-lich-thong-minh/publish` - Publish draft
- `POST /api/thoi-khoa-bieu/xep-lich-thong-minh/check-xung-dot-batch` - Batch conflict check
- `DELETE /api/thoi-khoa-bieu/drafts/{draftId}` - Delete draft

### Buổi Học
- `GET /api/buoi-hoc` - Get sessions
- `GET /api/buoi-hoc/{id}` - Get session details
- `PUT /api/buoi-hoc/{id}/change-teacher` - Change substitute teacher
- `PUT /api/buoi-hoc/{id}/change-room` - Change room
- `PUT /api/buoi-hoc/{id}/change-shift` - Change shift
- `PATCH /api/buoi-hoc/{id}/cancel` - Cancel session

### Attendance (Điểm Danh)
- `GET /api/teacher/attendance/today` - Get today's attendance for teacher
- `POST /api/buoi-hoc/{id}/attendance/start` - Start attendance
- `GET /api/buoi-hoc/{id}/attendance` - Get attendance for a session
- `PATCH /api/buoi-hoc/{id}/attendance/students/{hsId}` - Mark student attendance
- `PUT /api/buoi-hoc/{id}/attendance/bulk-update` - Bulk update attendance
- `POST /api/buoi-hoc/{id}/attendance/submit` - Submit/lock attendance
- `POST /api/buoi-hoc/{id}/attendance/unlock-requests` - Request attendance unlock
- `GET /api/teacher/attendance/unlock-requests` - Get teacher's unlock requests
- `GET /api/admin/attendance/unlock-requests` - Get admin unlock requests
- `GET /api/admin/attendance/unlock-requests/{id}` - Get unlock request details
- `POST /api/admin/attendance/unlock-requests/{id}/approve` - Approve unlock request
- `POST /api/admin/attendance/unlock-requests/{id}/reject` - Reject unlock request

## BGH APIs

### Đã có

| Method | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| GET | `/api/bgh/dashboard` | BGH/Admin/SuperAdmin | Lấy số liệu thống kê tổng quan (sinh viên, giáo viên, tỷ lệ đi học, lớp học). |
| GET | `/api/bgh/evaluations` | BGH/Admin/SuperAdmin | Danh sách tất cả đánh giá giáo viên. |
| GET | `/api/bgh/evaluations/ranking` | BGH/Admin/SuperAdmin | Xếp hạng giáo viên theo điểm đánh giá. |
| GET | `/api/bgh/evaluations/{id}` | BGH/Admin/SuperAdmin | Chi tiết một phiếu đánh giá. |
| GET | `/api/bgh/evaluations/overview` | BGH/Admin/SuperAdmin | Tổng quan thống kê đánh giá toàn trường. |
| GET | `/api/bgh/evaluations/ai-analysis` | BGH/Admin/SuperAdmin | Phân tích đánh giá bằng AI. |
| GET | `/api/bgh/academic/overview` | BGH/Admin/SuperAdmin | Tổng quan học vụ (GPA, Pass rate, tín chỉ, chứng chỉ). |
| GET | `/api/bgh/academic/gpa` | BGH/Admin/SuperAdmin | Báo cáo phổ điểm GPA, trung bình các khóa. |
| GET | `/api/bgh/academic/at-risk` | BGH/Admin/SuperAdmin | Danh sách sinh viên nguy cơ học thuật. |
| GET | `/api/bgh/academic/reports` | BGH/Admin/SuperAdmin | Các báo cáo học vụ chi tiết (môn học rớt nhiều, nợ học phí, bảo lưu). |
| GET | `/api/bgh/academic/pass-fail` | BGH/Admin/SuperAdmin | Tỷ lệ Đậu/Rớt môn học. |
| GET | `/api/bgh/schedule/changes` | BGH/Admin/SuperAdmin | Các thay đổi lịch trình (nghỉ, bù, đổi phòng). |
