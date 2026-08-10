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
  - `GET /api/admin/rbac/roles` — danh sách vai trò (trả về `type`, `memberCount`)
  - `GET /api/admin/rbac/roles/{id}` — chi tiết vai trò
  - `GET /api/admin/rbac/roles/{id}/members` — danh sách thành viên của vai trò
  - `POST /api/admin/rbac/roles` — tạo vai trò
  - `PUT /api/admin/rbac/roles/{id}` — cập nhật vai trò
  - `DELETE /api/admin/rbac/roles/{id}` — xóa vai trò
  - `GET /api/admin/accounts` là endpoint mẫu.
  - `GET /api/admin/users` (P16B) — danh sách user phân trang + lọc role/trạng thái/đơn vị; `UserListItemDto` có `maVaiTro`, `maCodeVaiTro`, `maDonVi`, `maLopHanhChinh`, `tenLopHanhChinh`.
  - `GET /api/admin/users/roles` (P16B) — vai trò được phép gán trong quản trị tài khoản.
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
  - `GET /api/staff/dashboard` (Bước 7 — GiaoVu dashboard, real DB queries, role AcademicStaff)
  - `GET /api/master-data/buildings`, `GET /api/master-data/buildings/{id}` (đọc: Staff; CRUD: chỉ SuperAdmin/Admin/CampusAdmin/SubCampusAdmin)
  - `GET /api/master-data/floors`, `GET /api/master-data/buildings/{buildingId}/floors`, `GET /api/master-data/floors/{id}` (roles như buildings)
  - `GET /api/master-data/rooms`, `GET /api/master-data/rooms/{id}`, `GET /api/master-data/floors/{floorId}/rooms` (roles như buildings)
  - `GET /api/ca-hoc`, `GET /api/ca-hoc/active`, `GET /api/ca-hoc/{id}`, `POST/PUT /api/ca-hoc`, `PATCH /api/ca-hoc/{id}/toggle-active` (policy AcademicOperations; **không có DELETE** — xóa ca học qua toggle-active)
  - `GET /api/blocks?maHocKy={id}`, `PUT /api/blocks/{id}` (policy AcademicScheduleConfig; block phải nằm trong khoảng ngày học kỳ)
  - `GET /api/quy-doi-tin-chi`, `POST/PUT/DELETE /api/quy-doi-tin-chi` (policy AcademicScheduleConfig; soTinChi phải duy nhất 1-20)
  - `GET /api/attendance-policy`, `GET /api/attendance-policy/history`, `PUT /api/attendance-policy` (policy AcademicOperations; chính sách điểm danh theo đơn vị — SuperAdmin xem toàn hệ thống, mỗi lần PUT tạo phiên bản mới + ghi audit UPDATE_POLICY; hạn gửi/chỉnh sửa điểm danh và hệ số vắng được AttendanceService đọc từ policy này)
  - `GET /api/pass-fail-rules?maHocKy=&maNganh=&maChuyenNganh=&search=&pageIndex=&pageSize=`, `GET /api/pass-fail-rules/{id}`, `POST /api/pass-fail-rules`, `PUT /api/pass-fail-rules/{id}` (policy AcademicOperations; cấu hình trọng số/ngưỡng đạt/chuyên cần tối thiểu theo môn & học kỳ trên `CauHinhDiemMonHoc` — tổng trọng số phải = 100, ngưỡng 0-10, chuyên cần 0-100; lọc theo ngành/chuyên ngành; ghi audit CREATE/UPDATE_PASS_FAIL_RULE; `TiLeChuyenCanToiThieu > 0` được GradeAggregationService dùng để đánh `rot` khi chuyên cần thấp; không có DELETE)
  - `GET /api/applications/templates?includeInactive=true` — danh sách mẫu đơn từ (`Maudontu`); `includeInactive=true` trả cả mẫu tạm ẩn kèm `dangHoatDong/ngayTao/ngayCapNhat` (dùng cho màn Quản lý mẫu đơn từ)
  - `POST /api/applications/templates` (policy AdminOnly) — tạo mẫu đơn mới (loại đơn chưa có mẫu; validate `cauHinhJson` qua `ApplicationTemplateValidator`; `phienBan=1`; không cho tạo trùng loại)
  - `PUT /api/applications/templates/{loaiDon}` (policy AdminOnly) — cập nhật mẫu đơn (tên, cấu hình JSON, minh chứng, SLA, `dangHoatDong`); đổi `cauHinhJson` → tự tăng `phienBan`; ghi audit `CREATE_APPLICATION_TEMPLATE`/`UPDATE_APPLICATION_TEMPLATE`
  - `DELETE /api/applications/templates/{loaiDon}` (policy AdminOnly) — xóa mẫu đơn (chỉ xóa khi chưa có đơn từ `DonTu` nào tham chiếu `MaMauDon`; nếu có → 400 yêu cầu tạm ẩn thay vì xóa); ghi audit `DELETE_APPLICATION_TEMPLATE`
  - `GET /api/applications/schema/types` — danh sách loại đơn chuẩn (dùng cho màn tạo mẫu; lưu ý `/api/applications/schema/options` không tồn tại ở BE — 404)
  - `GET /api/student/retake/available-subjects` — danh sách **khóa học** (lớp học phần `KhoaHoc`) có thể thi lại: môn rớt xác định từ `DiemSo` join `CauHinhDiemMonHoc` theo `(MaMonHoc, MaHocKy)` khi `GpaMonHoc < NguongDat`; trả `{id=MaKhoaHoc, name=TieuDe, code=MaCodeMonHoc}`; chỉ giữ khóa học của môn có ca thi `'nhap'/'dang_mo'` với `NgayThi >= hôm nay`
  - `GET /api/student/retake/courses/{courseId}/exam-sessions` — ca thi mở của khóa học (lấy ca thi theo `LichThiTong.MaMonHoc` của khóa học; 404 nếu khóa học không tồn tại); trả `{id=MaCaThi, name=...}`
  - Mẫu đơn `thi_lai` dùng field `course_id` (autoFill `availableRetakeSubjects`, relatedEntity `khoa_hoc`) + `exam_session_id` (dependsOn `course_id`); `RetakeExamApplicationSubmissionRule` validate khóa học tồn tại, sinh viên rớt theo ngưỡng cấu hình, ca thi thuộc khóa học & đang mở
  - Lưu ý route: `/super-admin/approvals/requests` là màn **Quản lý mẫu đơn từ** (SuperAdmin); hàng đợi xử lý đơn của sinh viên nằm ở GiaoVu `/staff/requests`

  - `POST /api/admin/discipline-records/{id}/remove-effect` (DL3)
  - `POST /api/admin/discipline-records/{id}/void-approved` (DL3)
  - `GET /api/admin/discipline-appeals` (DL3)
  - `GET /api/bgh/evaluations/overview` (P15D)
  - `GET /api/bgh/evaluations/ai-analysis` (P15D)
  - `GET /api/admin/evaluations/config` — lấy biểu mẫu đánh giá GV hiện hành (bảng `MauDanhGia`, trả `null` nếu chưa có)
  - `PUT /api/admin/evaluations/config` (policy AdminOnly) — upsert biểu mẫu đánh giá GV (tên, `cauHinhJson` — validate qua `ApplicationTemplateValidator`, `dangHoatDong`); ghi audit `UPSERT_EVALUATION_CONFIG`
  - `GET /api/admin/evaluations/summary` — tổng quan cấu hình đánh giá GV (số câu hỏi, lượt đánh giá, GV được đánh giá, học kỳ có đánh giá)
  - `GET /api/admin/evaluations/questions` — danh sách câu hỏi khảo sát kèm `luotSuDung`
  - `POST /api/admin/evaluations/questions` (policy AdminOnly) — tạo câu hỏi (nội dung 1-500 ký tự, mặc định hoạt động)
  - `PUT /api/admin/evaluations/questions/{id}` (policy AdminOnly) — sửa nội dung câu hỏi
  - `POST /api/admin/evaluations/questions/{id}/toggle-active` (policy AdminOnly) — bật/tắt câu hỏi
  - `DELETE /api/admin/evaluations/questions/{id}` (policy AdminOnly) — xóa câu hỏi (chỉ khi chưa có lượt đánh giá `DanhGiaGiaoVien` dùng; nếu có → 400 yêu cầu tạm ẩn)
  - `GET /api/bgh/academic/overview` (P15D)
  - `GET /api/bgh/academic/gpa` (P15D)
  - `GET /api/bgh/academic/at-risk` (P15D)
  - `GET /api/bgh/academic/reports` (P15D)
  - `GET /api/bgh/academic/pass-fail` (P15D)
  - `GET /api/bgh/schedule/changes` (P15D)
  - `POST /api/staff/requests/process-all` `dự kiến` — FE `staffApi.processAllRequests` đánh dấu `× MISSING_BACKEND`, BE chưa có endpoint này (Dashboard giáo vụ không gọi).
  - `POST /api/student/parent-links/invite`, `PUT /api/student/parent-links/{linkId}/permissions`, `DELETE /api/student/parent-links/{linkId}` `dự kiến` — FE `studentApi` đánh dấu `× MISSING_BACKEND`; BE có entity `LienKetPhuHuynh` + seed nhưng chưa có controller.
  - `POST /api/bgh/grade-unlock-requests/{requestId}/approve` (Phase 3)
  - `POST /api/bgh/grade-unlock-requests/{requestId}/reject` (Phase 3)
  - `GET /api/admin/certificate-templates`, `GET /api/admin/certificate-templates/{id}`, `POST/PUT/DELETE /api/admin/certificate-templates[/{id}]`, `POST /api/admin/certificate-templates/{id}/preview` (RD5, SuperAdmin) — quản lý mẫu bằng khen (`MauBangKhen`); `cauHinhJson` hỗ trợ 2 mode: `{mode:"html", html, css}` (HTML/CSS render tại FE) và legacy `{fields:[...]}`; `POST preview` trả `Mode/Html/Css` khi mode html, không ghi DB.
  - `GET /api/admin/reward-campaigns`, `GET /api/admin/reward-campaigns/{id}`, `POST /api/admin/reward-campaigns/top100`, `PUT/PATCH /api/admin/reward-campaigns/{id}[/cancel]`, `POST /api/admin/reward-campaigns/{id}/evaluate`, `GET /api/admin/reward-campaigns/{id}/candidates`, `POST /api/admin/reward-campaigns/{id}/approve`, `POST /api/admin/reward-campaigns/{id}/certificates/generate|regenerate`, `GET /api/admin/reward-campaigns/{id}/certificates` (RD2-RD6) — vòng đời đợt khen thưởng Top 100; SuperAdmin CRUD, Admin/CampusAdmin đọc theo scope.
  - `POST /api/admin/reward-campaigns/{id}/certificates/upload` (SuperAdmin) — upload PDF bằng khen render tại FE (html2pdf.js) cho template mode html: payload `{MaKhenThuong, MaMauBangKhen, FileBase64, GhiChu}`; validate PDF (`%PDF-` magic, ≤20MB), set `TrangThai=PdfGenerated` + audit `UPLOAD_REWARD_CERTIFICATE`; khi template mode html thì BE không tự sinh PDF raw (vốn làm hỏng tiếng Việt), FE tự render + upload.
  - Màn SuperAdmin: `/super-admin/awards` = Khen thưởng (`AwardsView.vue` — cấp phát bằng khen; nếu đợt có mẫu mode html thì FE render HTML→PDF tuần tự + upload từng sinh viên, ngược lại gọi BE generate), `/super-admin/awards/certificate-templates` = Cấu hình giấy khen (`CertificateTemplatesView.vue` — editor HTML/CSS + live preview iframe; token `{{hoTen}}`, `{{mssv}}`, `{{tenHocKy}}`, `{{danhHieu}}`, `{{xepHang}}`, `{{diemXet}}`, `{{ngayCap}}`).
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

## Ghi Chú P15G Full Browser Smoke

- Runner P15G đặt tại `docs/artifacts/p15g-full-smoke/p15g-browser-smoke.mjs`, dùng Chrome CDP `http://127.0.0.1:9222` và ghi kết quả vào `docs/artifacts/p15g-full-smoke/smoke-results-p15g.json`.
- Runner phải resolve ID thật từ list API trước khi vào route detail; nếu list API không có dữ liệu thì ghi `SKIPPED_NO_DATA`, không dùng ID giả.
- Kết quả P15G.3 ngày 2026-07-09: 166 route entries, 166 pass, 0 fail, 0 `SKIPPED_NO_DATA`, console/runtime/network 401/403/404/500 đều bằng 0.
- Dữ liệu detail P15G.3 phải đến từ seed/backend thật: BGH at-risk, BGH teacher evaluation, Teacher class detail/workspace. Không dùng ID giả hoặc fallback local.
