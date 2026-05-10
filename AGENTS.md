# AGENTS.md

Tài liệu context bắt buộc cho AI agent khi làm việc trong repo LMS Academic Management System.

## Mô Tả Project

Đây là dự án tốt nghiệp LMS/Academic Management System. Backend dùng ASP.NET Core, EF Core, SQL Server và JWT. Frontend dùng Vue 3, Vite, Pinia, Vue Router và Tailwind. Database/model đã bao phủ nhiều nghiệp vụ học vụ, trong khi API hiện thực mới có Auth, Organizations và một controller mẫu admin accounts.

Mục tiêu khi agent tham gia: bám sát kiến trúc hiện tại, không tự bịa API, không thay đổi business logic ngoài yêu cầu, cập nhật tài liệu khi thêm API/module mới.

## Stack Hiện Tại

Backend:
- ASP.NET Core `net10.0`
- EF Core `10.0.x`
- SQL Server
- JWT Bearer Authentication
- Controllers, Services, DTOs, Middlewares

Frontend:
- Vue 3
- Vite
- Vue Router
- Pinia
- Tailwind CSS
- Vitest, ESLint, Oxlint, Prettier
- lucide-vue-next

## Quy Tắc Cho Agent

- Đọc tài liệu markdown trước khi sửa code: `README.md`, `AGENTS.md`, `CLAUDE.md`, `docs/*`.
- Kiểm tra file thật bằng `rg` trước khi kết luận API/model/store đã có.
- Chỉ sửa phạm vi được yêu cầu.
- Nếu thiếu thông tin, dùng nhãn `dự kiến` hoặc `cần bổ sung`.
- Không đổi stack, không thêm dependency mới nếu không có yêu cầu rõ.
- Không format toàn bộ repo.
- Không xóa code, migration hoặc entity hiện có.

## Quy Ước Backend

- Controller đặt trong `Backend/Controllers`.
- Service interface/implementation đặt trong `Backend/Services/<Module>`.
- DTO đặt trong `Backend/DTOs/<Module>`.
- Entity đặt trong `Backend/Models`.
- Mapping database tập trung trong `Backend/Data/ApplicationDbContext.cs`.
- Constants auth đặt trong `Backend/Constants/AuthConstants.cs`.
- Lỗi nghiệp vụ dùng `ApiException`; lỗi chung đi qua `ExceptionMiddleware`.
- Auth context hiện được middleware đưa vào `HttpContext.Items["CurrentUser"]`.
- Khi thêm endpoint protected, dùng `[Authorize]`; khi cần role, dùng `AuthRoles`.

## Quy Ước Frontend

- Route đặt trong `frontend/src/router/index.js`.
- Layout student hiện dùng `frontend/src/components/SinhVien/Layout_SinhVien.vue`.
- View student hiện nằm lẫn trong `frontend/src/views/Student` và `frontend/src/views/SinhVien`; khi thêm mới cần nhất quán theo khu vực đang sửa.
- Store Pinia đặt trong `frontend/src/stores`.
- Đã có API client cơ bản tại `frontend/src/services/apiClient.js`; khi bổ sung module mới, tạo API module rõ ràng thay vì gọi API rải rác.
- Đã có auth store tại `frontend/src/stores/auth.js`; dùng store này cho login/logout/role state.
- Không hardcode token/user role trong component.
- UI cần có trạng thái loading, error, empty cho dữ liệu async.

## Quy Ước API

- Base path backend hiện dùng `/api/...`.
- Endpoint đã có:
  - `POST /api/auth/login`
  - `POST /api/auth/change-password`
  - `GET /api/organizations`
  - `GET /api/organizations/tree`
  - `GET /api/organizations/{id}`
  - `POST /api/organizations`
  - `PUT /api/organizations/{id}`
  - `DELETE /api/organizations/{id}`
  - `DELETE /api/organizations/{id}/hard-delete`
  - `GET /api/organizations/{id}/subtree`
  - `GET /api/admin/accounts` là endpoint mẫu.
- Endpoint chưa có controller phải ghi `dự kiến`.
- Không tự đổi request/response DTO mà không cập nhật contract.

## Quy Ước Database

- SQL Server là nguồn dữ liệu chính.
- EF Core migrations đã tồn tại trong `Backend/Migrations`.
- `ApplicationDbContext` ánh xạ schema `dbo` với nhiều bảng tiếng Việt không dấu/có dấu theo model.
- Không đổi tên cột/bảng/entity nếu không tạo migration có chủ đích.
- Không hard delete dữ liệu học vụ nếu nghiệp vụ chưa cho phép; ưu tiên trạng thái/xóa mềm nếu model hỗ trợ.
- Cần chú ý tenant/campus scope qua `MaDonVi`.

## Cách Verify Sau Khi Sửa Code

Backend:

```powershell
cd Backend
dotnet restore
dotnet build
```

Frontend:

```powershell
cd frontend
npm install
npm run build
npm run test:unit
npm run lint
```

Khi chỉ sửa tài liệu:

```powershell
Get-ChildItem README.md,AGENTS.md,CLAUDE.md,docs,.cursor/rules -Recurse
```

## Những Việc Không Được Làm

- Không sửa business logic trong task tài liệu.
- Không đổi stack backend/frontend.
- Không thêm dependency mới khi chưa được duyệt.
- Không bịa endpoint đã tồn tại.
- Không hardcode secret/token/password.
- Không copy nguyên source từ LMS open-source khác.
- Không xóa code, migration, model hoặc route hiện có.
- Không rewrite unrelated files.
