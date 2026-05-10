# System Architecture

## Tổng Quan Kiến Trúc

Hệ thống là LMS/Academic Management System theo mô hình frontend SPA + backend REST API + SQL Server.

```text
Vue 3 SPA
  -> HTTP/JSON + JWT
ASP.NET Core API
  -> EF Core
SQL Server
```

Backend hiện có nền tảng auth, organizations, middleware bảo vệ request và schema database rộng cho học vụ. Frontend hiện có app shell cho student và các view demo/chưa nối API đầy đủ.

## Frontend Architecture

- Vue 3 app khởi tạo trong `frontend/src/main.js`.
- Router chính trong `frontend/src/router/index.js`.
- Pinia đã được đăng ký, hiện mới có store mẫu `counter`.
- Student layout nằm ở `frontend/src/components/SinhVien/Layout_SinhVien.vue`.
- Views student nằm trong `frontend/src/views/Student` và `frontend/src/views/SinhVien`.
- Auth guard hiện là stub trong router, cần bổ sung auth store và API client.

## Backend Architecture

- `Program.cs` cấu hình controllers, OpenAPI, DbContext, JWT, authorization policy và middleware.
- Controller hiện có: `AuthController`, `OrganizationsController`, `AccountManagementExampleController`.
- Service hiện có: `AuthService`, `OrganizationService`.
- DTO theo module: `DTOs/Auth`, `DTOs/Organizations`.
- Middleware hiện có: exception, JWT context, first login, campus scope.
- EF Core mapping nằm trong `ApplicationDbContext`.

## Database Architecture

Database dùng SQL Server, schema chính `dbo`. `ApplicationDbContext` có nhiều nhóm bảng:

- Người dùng/phân quyền: `NguoiDung`, `VaiTro`, `PhanQuyenNguoiDung`, `TokenLamMoi`.
- Tổ chức/cơ sở: `DonVi`.
- Học tập: `KhoaHoc`, `Chuong`, `BaiHoc`, `TienDoBaiHoc`.
- Bài tập/bài nộp: `BaiTap`, `BaiNop`, `CanhBaoDaoVan`.
- Thi/quiz: `DeKiemTra`, `CauHoi`, `CauHoiDeKiemTra`, `PhienThiHocSinh`.
- Điểm danh/điểm số: `BuoiHoc`, `DiemDanh`, `DiemSo`, `YeuCauSuaDiem`.
- Thông báo/hỗ trợ: `ThongBao`, `ThongBaoHenGio`, `PhieuHoTro`, `TinNhanHoTro`.
- Báo cáo/AI/audit: `BaoCaoRuiRoVang`, `BaoCaoRuiRoRotMon`, `AnhChupPhanTich`, `NhatKyKiemToan`.

## Authentication Flow

1. Client gọi `POST /api/auth/login` với email/password.
2. `AuthService` kiểm tra user trong `NguoiDung`, xác minh password hash.
3. Nếu hợp lệ, backend tạo JWT qua `JwtHelper`.
4. Response trả `accessToken`, `expiresAt`, `requiresPasswordChange` và thông tin user.
5. Client gửi token trong header `Authorization: Bearer <token>` cho request protected.
6. JWT Bearer middleware xác thực token; `JwtMiddleware` đưa context user vào request.

## Authorization Flow

- Role code trong database được map sang role API ở `AuthRoles.FromDatabaseCode`.
- Policy hiện có:
  - `AdminOnly`: `Admin`, `SuperAdmin`.
  - `AcademicOperations`: `Admin`, `SuperAdmin`, `AcademicStaff`, `CampusAdmin`.
  - `Reports`: `Admin`, `SuperAdmin`, `Principal`, `CampusAdmin`.
- Endpoint organizations tạo/sửa/xóa yêu cầu `SuperAdmin`.
- Organization read dùng JWT và lọc scope theo `CampusId`/role trong service.

## Request/Response Flow

```text
Vue View/Component
  -> API client dự kiến
  -> ASP.NET Controller
  -> Service
  -> ApplicationDbContext
  -> SQL Server
  -> DTO response
```

Hiện frontend chưa có API client chuẩn. Khi bổ sung, nên tạo một lớp client thống nhất để gắn base URL, token, xử lý lỗi và loading state.

## AI Feature Flow

AI features hiện có dấu vết trong database/model nhưng chưa có controller:

- `BaiHoc.TomTatAi`: tóm tắt bài học.
- `BaiNop.DiemAiDeXuat`, `CanhBaoDaoVan`: gợi ý điểm/đạo văn.
- `BaoCaoRuiRoVang`, `BaoCaoRuiRoRotMon`, `DanhSachRuiRoRotMon`: cảnh báo rủi ro.
- `DanhGiaGiaoVien.AiCamXuc`, `AiChuDe`: phân tích đánh giá.

Flow dự kiến:

```text
User action hoặc batch job
  -> AI API/service dự kiến
  -> lấy dữ liệu học vụ từ SQL Server
  -> gọi model/provider AI qua cấu hình bảo mật
  -> lưu kết quả vào bảng AI/report
  -> frontend hiển thị cảnh báo/tóm tắt/gợi ý
```

Không hardcode API key AI trong code.

## Deployment Overview

Dự kiến:

- Backend deploy dưới dạng ASP.NET Core API, cấu hình qua environment variables/user secrets.
- SQL Server deploy riêng, migration chạy có kiểm soát.
- Frontend build static bằng Vite và deploy qua web server/CDN.
- CORS, HTTPS, JWT secret, connection string, logging và backup database cần cấu hình theo môi trường.

Trạng thái hiện tại: chưa thấy cấu hình deployment hoàn chỉnh trong repo.
