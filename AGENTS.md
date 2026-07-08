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
- Interface và lớp truy vấn/data access theo module cũng đặt trong `Backend/Services/<Module>` nếu team tách riêng khỏi service chính; không tạo thư mục `Repositories` riêng trừ khi có yêu cầu thống nhất mới.
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

## Design Token & Color System

Tất cả màu sắc phải dùng **semantic tokens** (CSS variables) thay vì hardcode Tailwind color classes.

### Token Layers (trong `liquid-glass.css`)

1. **Core/Primitive tokens** (`--lg-*`): màu gốc (primary, secondary, accent, cyan, indigo)
2. **Surface tokens** (`--surface-*`): nền cho page, card, sidebar, topbar, input, dropdown, modal
3. **Text tokens** (`--text-*`): heading, body, label, placeholder, link, inverse
4. **Border tokens** (`--border-*`): default, card, input, input-focus, focus-ring
5. **Semantic bg/text tokens** (`--color-*-bg`, `--color-*-text`): success, warning, danger, info

### Utility Classes

- `text-heading` / `text-body` / `text-label` / `text-link` / `text-placeholder` — semantic text colors
- `surface-card` / `surface-sidebar` / `surface-input` — semantic surface backgrounds
- `border-default` / `border-card` — semantic border colors

### Role-Specific Sidebar Variables (set via `:style` inline)

Mỗi role (Student/Teacher, GiaoVu, BGH) custom sidebar qua CSS variables:

| Variable | Student/Teacher | GiaoVu | BGH |
|---|---|---|---|
| `--sidebar-accent` | blue-600 (#2563eb) | teal-600 (#0d9488) | blue-800 (#1e40af) |
| `--sidebar-accent-dark` | blue-400 (#60a5fa) | teal-300 (#5eead4) | blue-400 (#60a5fa) |
| `--sidebar-indicator` | blue-600 | teal-500 (#14b8a6) | blue-800 (#1e40af) |
| `--active-start` | blue-700 (#1d4ed8) | teal-700 (#0f766e) | blue-900 (#1e3a8a) |
| `--active-mid` | blue-600 (#2563eb) | teal-600 (#0d9488) | blue-800 (#1e40af) |
| `--active-end` | cyan-600 (#0891b2) | teal-500 (#14b8a6) | blue-600 (#2563eb) |

### Glassmorphism Pattern

Glassmorphism chỉ dùng ở **khu vực có hierarchy** (sidebar, topbar, card nổi, modal). Không dùng glass cho bảng dữ liệu, form dài, hay vùng text-heavy.

- `lg-glass` / `lg-glass-strong` / `lg-glass-soft` — các biến thể glass surface (đều có dark mode)
- `lg-sidebar` — sidebar glass với blur + gradient (custom glow per role qua `--sidebar-glow-*`)
- `lg-topbar` — topbar glass effect
- Các panel content như Teacher Dashboard dùng `lg-glass-soft` thay vì `bg-white border-slate-100`

Khi thêm component mới: **KHÔNG dùng hardcode** `bg-white`, `text-slate-*`, `border-slate-100`. Dùng semantic tokens hoặc `lg-*` class.

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
  - `GET /api/student/dashboard` (P15B — real DB queries, no mock)
  - `GET /api/student/grades` (P15B)
  - `GET /api/student/support-tickets` (P15B)
  - `GET /api/student/support-tickets/{id}` (P15B)
  - `POST /api/student/support-tickets` (P15B)
  - `POST /api/student/support-tickets/{id}/messages` (P15B)
  - `POST /api/student/support-tickets/{id}/close` (P15B)
  - `GET /api/student/evaluations` (P15B)
  - `POST /api/student/evaluations/submit` (P15B)
  - `GET /api/parent/dashboard` (P15A)
  - `GET /api/parent/children` (P15A)
  - `GET /api/parent/children/{id}` (P15A)
  - `GET /api/parent/children/{id}/grades` (P15A)
  - `GET /api/parent/children/{id}/schedule` (P15A)
  - `GET /api/parent/children/{id}/attendance` (P15A)
  - `GET /api/parent/children/{id}/alerts` (P15A)
  - `GET /api/parent/children/{id}/tuition` (P15A)
  - `GET /api/parent/children/{id}/transactions` (P15A)
  - `GET /api/parent/children/{id}/invoices` (P15A)
  - `POST /api/parent/payment` (P15A)
  - `GET /api/parent/notifications` (P15A)
  - `GET /api/parent/notifications/history` (P15A)
  - `GET /api/parent/profile` (P15A)
  - `GET /api/parent/access-rights` (P15A)

  - `POST /api/admin/discipline-records/{id}/remove-effect` (DL3)
  - `POST /api/admin/discipline-records/{id}/void-approved` (DL3)
  - `GET /api/admin/discipline-appeals` (DL3)
  - `GET /api/admin/discipline-appeals/{id}` (DL3)
  - `POST /api/admin/discipline-appeals/{id}/resolve` (DL3)
  - `GET /api/student/discipline-records` (DL3)
  - `GET /api/student/discipline-records/{id}` (DL3)
  - `POST /api/student/discipline-records/{id}/appeals` (DL3)
  - `GET /api/student/discipline-records/appeals/{appealId}` (DL3)
  - `POST /api/thoi-khoa-bieu/xep-lich-thong-minh` (P12)
  - `GET /api/thoi-khoa-bieu/drafts/{draftId}` (P12)
  - `GET /api/thoi-khoa-bieu/drafts` (P12)
  - `POST /api/thoi-khoa-bieu/xep-lich-thong-minh/publish` (P12)
  - `POST /api/thoi-khoa-bieu/xep-lich-thong-minh/check-xung-dot-batch` (P12)
  - `DELETE /api/thoi-khoa-bieu/drafts/{draftId}` (P12)
  - `POST /api/teacher/exams` (P15C.1 — teacher-scoped with ownership validation)
  - `GET /api/bgh/dashboard` (P15D)
  - `GET /api/bgh/evaluations` (P15D)
  - `GET /api/bgh/evaluations/ranking` (P15D)
  - `GET /api/bgh/evaluations/{id}` (P15D)
  - `GET /api/bgh/evaluations/overview` (P15D)
  - `GET /api/bgh/evaluations/ai-analysis` (P15D)
  - `GET /api/bgh/academic/overview` (P15D)
  - `GET /api/bgh/academic/gpa` (P15D)
  - `GET /api/bgh/academic/at-risk` (P15D)
  - `GET /api/bgh/academic/reports` (P15D)
  - `GET /api/bgh/academic/pass-fail` (P15D)
  - `GET /api/bgh/schedule/changes` (P15D)
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

## Ghi Chú P15F.1 Browser Smoke

- Khi chạy E2E/smoke bằng browser, ưu tiên backend `http://localhost:5097` và frontend `https://localhost:5173` nếu không có yêu cầu khác.
- Connection string dev hiện dùng SQL Server `DELL\SQLEXPRESS02`, database `LMS`.
- Chrome smoke artifact chuẩn đặt trong `docs/artifacts/<phase-or-task>`.
- Skeleton loading dùng bộ component chung trong `frontend/src/components/common/skeleton`; không tạo skeleton rời rạc theo từng màn nếu có thể tái sử dụng.
- Không thêm mock data mới. Nếu cần dữ liệu để test, seed/kiểm tra từ SQL Server thật và ghi rõ trạng thái dữ liệu trong report.

## Ghi Chú P15F.2 DB Reset / Zero-Mock

- Khi reset DB local cho smoke lớn, chạy backend với `SeedProfile=LargeDemo` để tái tạo dữ liệu lớn.
- Sau clean reset ngày 2026-07-08, dữ liệu kỳ vọng tối thiểu: khoảng `10000+` học sinh và `100+` giáo viên; kết quả đã kiểm tra là `10005` học sinh, `110` giáo viên.
- Base seed phải chạy trước `LargeDemo` để giữ các tài khoản test P12/P15: Staff, Teacher, Student, BGH, Parent, ContentCouncil.
- Tài khoản Parent chuẩn cho smoke: `p15test_parent01@lms.local / Test@123`.
- Không dùng lại `ENABLE_MOCK_API`, `withFallback`, thư mục `frontend/src/mocks`, hay service mock độc lập; dữ liệu test phải đến từ SQL Server thật hoặc seed thật.

## Ghi Chú P15F.3 Release Hardening

- Không commit machine-specific connection string hoặc secret thật trong `Backend/appsettings.json`; file này chỉ dùng default/generic placeholder. Connection string local như `DELL\SQLEXPRESS02` đặt trong `Backend/appsettings.Development.json` hoặc biến môi trường.
- Không lưu SMTP/R2/PayOS secret thật trong config mặc định; dùng secret manager, environment variables, hoặc local dev config không đưa vào release.
- Application evidence storage mặc định dùng Local temp storage ngoài Production để backend vẫn khởi động khi không có R2 secret trong config mặc định; Production phải cấu hình storage thật qua biến môi trường/secret manager.
- Module Phụ huynh không dùng local business data file cho tên học sinh, điểm, học phí, chuyên cần, cảnh báo, thông báo. Các màn Parent phải lấy dữ liệu qua `parentApi`; local state chỉ được dùng cho UI state như selected child id.
- Browser smoke có thể ghi `API connection matrix: 165/165 connected`, nhưng chỉ ghi full 165-route browser PASS khi đã thật sự click/kiểm tra đủ 165 role-route assignments.
