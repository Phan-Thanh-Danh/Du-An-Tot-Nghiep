# Backend Architecture

## Stack Backend

- ASP.NET Core `net10.0`
- EF Core `10.0.x`
- SQL Server
- JWT Bearer Authentication
- OpenAPI trong môi trường Development
- CORS policy `FrontendDev` cho Vite dev server, có thể cấu hình bằng `Cors:AllowedOrigins`
- Controller-Service-DTO pattern

## Cấu Trúc Thư Mục Hiện Tại

```text
Backend/
├── Constants/
├── Controllers/
├── Data/
├── DTOs/
├── Exceptions/
├── Helpers/
├── Middlewares/
├── Migrations/
├── Models/
├── Services/
├── Backend.csproj
└── Program.cs
```

## Controllers

Đã có:

- `AuthController`: `POST /api/auth/login`, `POST /api/auth/change-password`.
- `OrganizationsController`: CRUD tổ chức/cây tổ chức.
- `AccountManagementExampleController`: endpoint mẫu `/api/admin/accounts`.

Quy ước:
- Controller chỉ nhận request, gọi service, trả DTO/result.
- Không viết business logic dài trong controller.
- Endpoint protected dùng `[Authorize]`.

## Services

Đã có:

- `IAuthService`/`AuthService`: login, đổi mật khẩu, audit login/password.
- `IOrganizationService`/`OrganizationService`: cây tổ chức, validate parent, scope theo campus, xóa mềm/xóa cứng.

Quy ước:
- Service xử lý nghiệp vụ và gọi `ApplicationDbContext`.
- Lỗi nghiệp vụ throw `ApiException`.
- Mapping entity -> DTO có thể đặt trong service nếu module nhỏ; module lớn nên cân nhắc mapper/helper nội bộ.

## DTOs

Đã có:

- `DTOs/Auth`: login request/response, current user context, change password.
- `DTOs/Organizations`: create/update/response/tree DTO.

Quy ước:
- Không expose entity trực tiếp ra API.
- Request DTO không chứa field hệ thống như password hash, audit fields nếu không cần.
- Response DTO dùng tên API dễ hiểu, không ép frontend biết tên cột DB.

## EF Core DbContext

`ApplicationDbContext`:

- Khai báo `DbSet` cho toàn bộ entity nghiệp vụ.
- Cấu hình table, key, column, index, check constraint và relationship.
- Dùng SQL Server qua `UseSqlServer`.

`Program.cs` hiện gọi `context.Database.MigrateAsync()` khi app khởi động. Khi deploy thật, cần cân nhắc quy trình migration có kiểm soát.

## Middleware

Đã có:

- `ExceptionMiddleware`: chuẩn hóa lỗi.
- `JwtMiddleware`: đọc JWT/current user context.
- `FirstLoginMiddleware`: xử lý trạng thái đăng nhập lần đầu.
- `CampusScopeMiddleware`: hỗ trợ scope theo cơ sở.

Thứ tự hiện tại trong `Program.cs`: exception -> HTTPS -> migration/seed -> CORS -> authentication -> JWT -> first-login -> campus-scope -> authorization -> controllers.

## JWT Authentication

- Cấu hình trong `Program.cs` bằng `JwtBearerDefaults.AuthenticationScheme`.
- Token sinh bởi `JwtHelper`.
- Claim types tùy biến nằm trong `CustomClaimTypes`.
- Login map role/status từ DB sang API role/status.

Không hardcode JWT secret trong code. Dùng configuration/user secrets/environment variables.

## Authorization Policies

Policy hiện có:

- `AdminOnly`: `Admin`, `SuperAdmin`.
- `AcademicOperations`: `Admin`, `SuperAdmin`, `AcademicStaff`, `CampusAdmin`.
- `Reports`: `Admin`, `SuperAdmin`, `Principal`, `CampusAdmin`.

Endpoint có role riêng nên dùng `[Authorize(Roles = ...)]` với `AuthRoles`.

## Error Handling

- Model validation trả `400` với `statusCode`, `message`, `traceId`.
- JWT challenge trả `401`.
- Forbidden trả `403`.
- Business error nên dùng `ApiException`.
- Không trả stack trace cho client.

## Validation

Hiện validation chủ yếu nằm trong service và data annotations/ApiBehaviorOptions. Khi thêm API:

- Validate input trong DTO/service.
- Kiểm tra quyền truy cập theo role và `MaDonVi`.
- Kiểm tra foreign key tồn tại trước khi tạo/cập nhật.
- Không tin dữ liệu từ frontend.

## Logging/Audit

Đã có bảng/model audit:

- `NhatKyKiemToan`
- `NhatKyThayDoiDiem`
- `NhatKyThongBao`
- `NhatKyDuyetDon`

`AuthService` đã ghi audit cho login/password. Các module nhạy cảm như điểm, tài chính, quyền, tổ chức nên ghi audit khi triển khai.

## Testing Recommendation

Ưu tiên:

1. Unit test service cho Auth và Organizations.
2. Integration test controller với database test hoặc test container SQL Server nếu phù hợp.
3. Test authorization cho role/campus scope.
4. Test migration/schema khi thay đổi database.
5. Test lỗi validation và response shape.

Lệnh verify cơ bản:

```powershell
cd Backend
dotnet restore
dotnet build
```
