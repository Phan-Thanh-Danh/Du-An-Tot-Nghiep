# Ma trận API cho ứng dụng LMS Mobile

Ngày đối chiếu: 04/08/2026.

Phạm vi tài liệu chỉ áp dụng cho `lms_mobile/`. Endpoint được đối chiếu từ controller và DTO backend thật; mobile không tạo backend riêng và không dùng dữ liệu học vụ giả.

## Cấu hình và đăng nhập

- Android Emulator: `http://10.0.2.2:5097/api`.
- Web/Windows/macOS/Linux/iOS Simulator: tự dùng `http://127.0.0.1:5097/api` khi không truyền `API_BASE_URL`.
- Thiết bị thật: dùng IP LAN của máy chạy backend, ví dụ `--dart-define=API_BASE_URL=http://192.168.x.x:5097/api`.
- Xác thực dùng JWT từ `POST /api/auth/login`; token được lưu bằng secure storage và tự gắn vào request.
- Mobile chỉ chấp nhận role `Student` và `Parent`. Đường tắt chọn role không JWT đã bị loại bỏ; route guard chặn URL Student/Parent khi chưa đăng nhập hoặc sai role.
- Thẻ demo lấy `DEMO_STUDENT_USERNAME`, `DEMO_STUDENT_PASSWORD`, `DEMO_PARENT_USERNAME`, `DEMO_PARENT_PASSWORD` từ Dart environment nếu có; nếu không, ứng dụng tự đọc file asset local. Thẻ hiển thị tài khoản/mật khẩu, hỗ trợ sao chép, điền form và đăng nhập một chạm.
- Máy demo local dùng file `assets/demo/demo_credentials.local.json` đã được `.gitignore`; không commit mật khẩu vào source.
- `web_dev_config.yaml` cố định Flutter Web debug tại `127.0.0.1:5173`, vì vậy lệnh `flutter run` không dùng origin ngẫu nhiên và khớp CORS backend.

Ví dụ chạy demo:

```powershell
flutter run
```

Tài khoản chuẩn trong DB local hiện dùng cho demo là `p12test_student011@lms.local` và `p15test_parent01@lms.local`; mật khẩu nằm trong file local bị bỏ qua bởi Git. Phụ huynh được liên kết với sinh viên dữ liệu mẫu `student.cntt01@lms.local`.

## Phụ huynh

| Giao diện/chức năng | Endpoint backend | Trạng thái mobile | Ghi chú |
|---|---|---|---|
| Đăng nhập | `POST /api/auth/login` | Đã kết nối | Điều hướng theo role backend trả về. |
| Dashboard | `GET /api/parent/dashboard` và các API con em | Đã kết nối | Hiện còn nhiều request để làm giàu dữ liệu. |
| Danh sách/chi tiết con em | `GET /api/parent/children`, `GET /api/parent/children/{id}` | Đã kết nối | DTO danh sách còn thiếu một số chỉ số tổng hợp. |
| Bảng điểm | `GET /api/parent/children/{id}/grades` | Đã kết nối | Backend chưa trả đủ tín chỉ/điểm chữ. |
| Thời khóa biểu | `GET /api/parent/children/{id}/schedule` | Đã kết nối | Backend nên bổ sung ngày/giờ cụ thể và ID lịch. |
| Chuyên cần | `GET /api/parent/children/{id}/attendance` | Đã kết nối | Có dữ liệu cốt lõi. |
| Học phí/hóa đơn | `GET /api/parent/children/{id}/invoices` | Đã kết nối | Có tổng quan công nợ và trạng thái hóa đơn. |
| Thanh toán học phí | `POST /api/parent/children/{id}/invoices/{invoiceId}/payments`, `GET /api/parent/children/{id}/payments/{transactionId}` | Đã kết nối | Hiện QR từ payload Backend, lưu PNG, mở PayOS và polling trạng thái. |
| Thông báo | `GET /api/parent/notifications` | Đã kết nối | Có loading/error/empty. |
| Đánh dấu đã đọc | `POST /api/parent/notifications/{id}/read` | Đã kết nối | Có thể tối ưu thao tác tất cả bằng endpoint `read-all`. |
| Hồ sơ | `GET /api/account/me` | Đã kết nối | Dùng API tài khoản dùng chung. Backend chưa trả địa chỉ/avatar. |
| Cập nhật hồ sơ | `PUT /api/account/profile` | Đã kết nối email/số điện thoại | Trường địa chỉ chưa có trong contract. |
| Đổi mật khẩu | `PUT /api/account/change-password` | Đã kết nối | Gửi `currentPassword`, `newPassword`, `confirmPassword`. |
| Lịch thi của con em | Chưa có | Cần bổ sung backend | Dự kiến `GET /api/parent/children/{id}/exams`; mobile đang báo chưa hỗ trợ. |

## Sinh viên

| Giao diện/chức năng | Endpoint backend | Trạng thái mobile | Ghi chú |
|---|---|---|---|
| Đăng nhập | `POST /api/auth/login` | Đã kết nối | JWT tự gắn vào request. |
| Dashboard | `GET /api/student/dashboard` | Đã kết nối | Dữ liệu thật từ DB. |
| Khóa học/chi tiết | `GET /api/student/courses`, `GET /api/student/courses/{courseId}` | Đã kết nối | Không dùng ảnh/banner giả. |
| Bài tập/nộp bài | `GET /api/student/assignments`, `POST /api/student/assignments/{id}/submit` | Đã kết nối | Nộp multipart field `file`; backend kiểm tra dung lượng/định dạng và lưu qua storage. |
| Bảng điểm | `GET /api/student/grades` | Đã kết nối | Backend có thêm API detail; mobile hiện chưa có màn chi tiết môn. |
| Thời khóa biểu | `GET /api/student/schedule` | Đã kết nối | Truyền phạm vi ngày và phân trang. |
| Chuyên cần | `GET /api/student/attendance` | Đã kết nối mới | Đọc `items` từ response phân trang. |
| Lịch thi | `GET /api/exam/student/list` | Đã kết nối mới | Contract chưa có phòng/ghế nên mobile không tự bịa dữ liệu. |
| Học phí | `GET /api/student/tuition/invoices` | Đã kết nối | Có tổng quan công nợ và trạng thái hóa đơn. |
| Thanh toán học phí | `POST /api/student/tuition/invoices/{invoiceId}/payments`, `GET /api/student/tuition/payments/{transactionId}` | Đã kết nối | Gửi provider `payos`, hiện/lưu QR, mở checkout URL và polling trạng thái. |
| Thông báo | `GET /api/notifications` | Đã kết nối mới | Dùng API thông báo dùng chung thay cho bản tóm tắt dashboard. |
| Đánh dấu đã đọc | `PATCH /api/notifications/{id}/read` | Đã kết nối mới | Mobile cập nhật UI sau khi backend trả thành công. |
| Hồ sơ | `GET /api/account/me` | Đã kết nối mới | Có email, số điện thoại, lớp, ngành, cơ sở. |
| Cập nhật hồ sơ | `PUT /api/account/profile` | Đã kết nối mới | Cho phép sửa email/số điện thoại. |
| Đổi mật khẩu | `PUT /api/account/change-password` | Đã kết nối mới | Dùng contract Account hiện tại. |

## Thanh toán và R2

- Mobile dùng đúng endpoint PayOS hiện có của Backend. Người dùng phải xác nhận trước khi POST tạo giao dịch; QR được render từ `qrPayload`, có thể lưu thành PNG, và `checkoutUrl` được mở ngoài ứng dụng.
- Mobile chỉ xem trạng thái Backend trả về và polling định kỳ; không tự ghi nhận thanh toán thành công, không dùng endpoint cũ `POST /api/parent/payment`, không hardcode tài khoản nhận tiền.
- **Cần bổ sung BE cho QR gộp nhiều con:** dự kiến `POST /api/parent/tuition/payments/batch` với body `{ "provider": "payos", "items": [{ "childId": 1, "invoiceId": 10 }] }`. Backend phải xác nhận phụ huynh đang có quyền với toàn bộ `childId`, tính tổng số dư của các hóa đơn, tạo đúng một PayOS order và trả cùng contract QR hiện tại kèm danh sách phân bổ. Khi webhook thành công, BE phải phân bổ tiền và cập nhật tất cả hóa đơn trong một transaction DB. Mobile không thể ghép các QR theo từng `invoiceId` thành một QR an toàn nếu chưa có endpoint này.
- R2 không phải database để mobile truy vấn trực tiếp. Mobile nộp tệp qua API bài tập; backend chịu trách nhiệm ghi metadata vào SQL Server và upload/kiểm tra object qua storage service.
- Không đưa R2 key, PayOS key, connection string hoặc mật khẩu demo vào source mobile.

## Phần backend còn thiếu hoặc nên bổ sung

1. P0: API lịch thi của từng con em cho phụ huynh.
2. P1: bổ sung địa chỉ/avatar vào account profile nếu nghiệp vụ cho phép.
3. P1: làm giàu DTO children/grades/schedule để giảm N+1 request ở mobile.
4. P2: bổ sung phòng/ghế thi vào `StudentExamListItemDto` và chuẩn hóa trạng thái/timestamp.

## Nguyên tắc mobile

- Không dùng mock repository trong luồng chạy thật.
- Không hardcode token, mật khẩu, QR, số tài khoản hoặc dữ liệu học vụ.
- API chưa có phải hiển thị trạng thái không hỗ trợ; không báo thành công giả.
- Mọi request protected dùng JWT từ secure storage.
