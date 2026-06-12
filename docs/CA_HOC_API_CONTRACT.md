# CaHoc API Contract

Tài liệu bổ sung cho P0-1 — Quản lý Ca Học. Endpoint dùng base path `/api` và response theo chuẩn `{ success, message, data }`.

## Đã có trong branch `feature/p0-1-ca-hoc-api-v2`

| Method | Endpoint | Auth | Mục đích |
|---|---|---|---|
| GET | `/api/ca-hoc` | AcademicOperations | Danh sách ca học có phân trang, tìm kiếm theo tên ca/buổi và lọc `conHoatDong`. |
| GET | `/api/ca-hoc/active` | AcademicOperations | Danh sách ca học đang hoạt động, sắp xếp theo `ThuTu`, dùng cho dropdown tạo/sửa thời khóa biểu. |
| GET | `/api/ca-hoc/{id}` | AcademicOperations | Xem chi tiết một ca học. |
| POST | `/api/ca-hoc` | Admin/SuperAdmin/CampusAdmin/AcademicStaff | Tạo ca học mới, validate tên, buổi, giờ và thứ tự. |
| PUT | `/api/ca-hoc/{id}` | Admin/SuperAdmin/CampusAdmin/AcademicStaff | Cập nhật ca học; chặn sửa giờ nếu ca đã được dùng trong `ThoiKhoaBieu` hoặc `BuoiHoc`. |
| PATCH | `/api/ca-hoc/{id}/toggle-active` | Admin/SuperAdmin/CampusAdmin/AcademicStaff | Bật/tắt trạng thái hoạt động của ca học. |

## Validation chính

- `TenCa` bắt buộc, tối đa 50 ký tự, không trùng.
- `Buoi` chỉ nhận `sang`, `chieu`, `toi`.
- `GioBatDau` và `GioKetThuc` nhận định dạng `HH:mm` hoặc `HH:mm:ss`.
- `GioKetThuc` phải lớn hơn `GioBatDau`.
- `ThuTu` phải lớn hơn 0.
- Khi ca học đã được dùng trong `ThoiKhoaBieu` hoặc `BuoiHoc`, backend không cho sửa giờ để tránh làm sai lịch cũ.

## Ghi chú nghiệp vụ

- `GET /api/ca-hoc/active` là API P0 cho frontend dropdown chọn ca học khi tạo thời khóa biểu.
- Tắt hoạt động không xóa dữ liệu cũ; ca đã tắt không nên hiển thị trong form tạo TKB mới.
- Audit log được ghi cho tạo, cập nhật và bật/tắt ca học.
