# LMS Mobile

Ứng dụng Flutter dành cho Sinh viên và Phụ huynh, dùng chung API ASP.NET Core của hệ thống LMS.

## Chạy bằng `flutter run`

Backend thật phải chạy ở cổng `5097`. Do cấu hình Development hiện đang trỏ tới LocalDB không có trên máy demo, mở terminal trong `Backend` và truyền connection string local lúc chạy:

```powershell
$env:ConnectionStrings__DefaultConnection='Server=.\SQLEXPRESS;Database=LMS_MobileDemo;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;'
dotnet run
```

Lưu ý: backend hiện tự chạy `MigrateAsync()` và seed khi khởi động. Hãy sao lưu/xác nhận cập nhật DB trước lần chạy đầu tiên với SQL Server Express.

Để bổ sung lại bộ dữ liệu trình diễn dành riêng cho mobile (script chạy lại không nhân đôi dữ liệu):

```powershell
sqlcmd -S '.\SQLEXPRESS' -d LMS_MobileDemo -E -C -b -f 65001 -i '.\lms_mobile\scripts\seed_mobile_demo.sql'
```

Sau khi backend báo `Now listening on: http://0.0.0.0:5097`, mở terminal thứ hai trong `lms_mobile`:

```powershell
flutter run
```

Nếu Windows báo plugin Flutter cần symlink, bật **Developer Mode** một lần trong Windows Settings rồi chạy lại. Mobile dùng `qr_flutter` để tạo QR từ payload Backend, `file_saver` để lưu ảnh QR và `url_launcher` để mở trang PayOS bên ngoài ứng dụng.

`web_dev_config.yaml` làm cho Chrome tự dùng `127.0.0.1:5173`, đúng CORS backend. Web/desktop tự gọi `http://127.0.0.1:5097/api`; Android Emulator tự gọi `http://10.0.2.2:5097/api`. Thiết bị thật cần truyền IP LAN bằng `--dart-define=API_BASE_URL=http://<IP-LAN>:5097/api`.

File `assets/demo/demo_credentials.local.json` chỉ tồn tại trên máy demo và đã được Git bỏ qua. `flutter run` tự nạp file này để màn login hiện sẵn hai tài khoản, mật khẩu, nút sao chép, điền form và đăng nhập nhanh.

Luồng thanh toán luôn yêu cầu xác nhận trước khi tạo giao dịch. Trạng thái thanh toán được polling từ Backend; mobile không tự đánh dấu hóa đơn đã thanh toán và không chứa tài khoản ngân hàng hay PayOS secret.

Đối chiếu endpoint và trạng thái tích hợp tại [docs/api_needed_for_mobile.md](docs/api_needed_for_mobile.md).
