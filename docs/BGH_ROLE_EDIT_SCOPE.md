# Phạm vi chỉnh sửa role BGH dành cho AI

## 1. Mục đích và mức ưu tiên

Đây là tài liệu giới hạn phạm vi bắt buộc khi AI thực hiện mọi yêu cầu liên quan đến **Ban Giám Hiệu (BGH)**, bao gồm frontend, UI/UX, design, API, business logic backend, database mapping, test và API contract.

AI phải đọc `README.md`, `AGENTS.md`, `CLAUDE.md` và file này trước khi sửa role BGH. Nếu hướng dẫn chung và file này khác nhau về phạm vi file, áp dụng quy tắc chặt hơn. Yêu cầu trực tiếp mới nhất của người dùng luôn có quyền mở rộng phạm vi.

## 2. Quy tắc khóa phạm vi

1. Chỉ được sửa hoặc tạo file nằm trong danh sách cho phép tại mục 4 và mục 5.
2. Trước khi sửa, phải nêu danh sách file dự kiến thay đổi và lý do của từng file.
3. Không được sửa file dùng chung chỉ vì thuận tiện. Ưu tiên giải quyết trong file chuyên biệt BGH.
4. File tại mục 5 chỉ được sửa khi thay đổi trong file BGH không thể đáp ứng yêu cầu và phải ghi rõ ảnh hưởng tới các role khác.
5. Nếu cần một file không có trong tài liệu này, AI phải dừng và xin người dùng cho phép bổ sung file đó vào phạm vi. Không được tự suy diễn quyền sửa.
6. Không xóa, di chuyển, đổi tên file; không format toàn repo; không thêm dependency; không sửa lockfile hoặc config môi trường nếu người dùng chưa cho phép rõ ràng.
7. Không sửa migration, schema hoặc model dùng chung chỉ để làm cho dữ liệu demo hiển thị. Dữ liệu BGH phải đến từ API và SQL Server thật.
8. Khi hoàn tất, phải chạy `git diff --name-only` và xác nhận mọi file thay đổi đều thuộc whitelist này hoặc đã được người dùng cho phép rõ ràng.

## 3. Bản đồ module BGH hiện tại

```text
Principal/BGH
  -> /bgh/* (Vue Router)
  -> Layout_BGH + AppSidebar + views/BGH
  -> bghApi
  -> /api/bgh/*
  -> BghDashboardController
     BghAcademicController
     BghEvaluationController
     BghFacadeController
  -> ApplicationDbContext
  -> SQL Server
```

- Role backend chính: `Principal`.
- Route frontend chính: `/bgh`.
- API base path: `/api/bgh`.
- Campus scope lấy từ `HttpContext.Items["CurrentUser"]`; không được bỏ qua scope này.
- BGH dùng Vue 3, semantic design tokens và các component UI hiện có; không hardcode token, role hoặc dữ liệu nghiệp vụ.

## 4. Whitelist chính — được phép sửa trực tiếp

### 4.1. Frontend BGH

Được phép sửa mọi file đang có hoặc tạo file mới chỉ trong hai vùng chuyên biệt sau:

```text
frontend/src/views/BGH/**
frontend/src/components/BGH/**
```

Các file hiện có đã được kiểm tra:

```text
frontend/src/components/BGH/AppSidebar.vue
frontend/src/components/BGH/Layout_BGH.vue
frontend/src/components/BGH/data/menuData.js

frontend/src/views/BGH/Academic/AcademicOverviewView.vue
frontend/src/views/BGH/Academic/AcademicReportsView.vue
frontend/src/views/BGH/Academic/AtRiskStudentsView.vue
frontend/src/views/BGH/Academic/GPAReportsView.vue
frontend/src/views/BGH/Academic/PassFailRatesView.vue
frontend/src/views/BGH/Academic/StudentHistoryView.vue
frontend/src/views/BGH/AcademicTermsView.vue
frontend/src/views/BGH/AuditLogsView.vue
frontend/src/views/BGH/CurriculumView.vue
frontend/src/views/BGH/Dashboard.vue
frontend/src/views/BGH/Evaluations/AIFeedbackAnalysisView.vue
frontend/src/views/BGH/Evaluations/EvalOverviewView.vue
frontend/src/views/BGH/Evaluations/TeacherEvalDetailsView.vue
frontend/src/views/BGH/Evaluations/TeacherRankingView.vue
frontend/src/views/BGH/EvaluationsView.vue
frontend/src/views/BGH/FacilitiesView.vue
frontend/src/views/BGH/OrganizationsView.vue
frontend/src/views/BGH/PlaceholderView.vue
frontend/src/views/BGH/Profile/ProfileView.vue
frontend/src/views/BGH/ProfileView.vue
frontend/src/views/BGH/ProgramsView.vue
frontend/src/views/BGH/RolesView.vue
frontend/src/views/BGH/Schedule/ConflictListView.vue
frontend/src/views/BGH/Schedule/PendingSchedulesView.vue
frontend/src/views/BGH/Schedule/PublishedSchedulesView.vue
frontend/src/views/BGH/Schedule/ScheduleChangesView.vue
frontend/src/views/BGH/SchedulePendingView.vue
frontend/src/views/BGH/UsersView.vue
```

API module chuyên biệt được phép sửa:

```text
frontend/src/services/bghApi.js
```

### 4.2. Backend BGH

Các controller chuyên biệt được phép sửa:

```text
Backend/Controllers/BghAcademicController.cs
Backend/Controllers/BghDashboardController.cs
Backend/Controllers/BghEvaluationController.cs
Backend/Controllers/BghFacadeController.cs
```

Nếu cần tách code mới theo kiến trúc hiện tại, chỉ được tạo trong namespace/thư mục chuyên biệt BGH:

```text
Backend/DTOs/Bgh/**
Backend/Services/Bgh/**
```

Không tạo thư mục `Repositories` mới. Không tự đổi contract request/response đang có.

### 4.3. Test và tài liệu BGH

```text
Backend.ApiTests/Bgh*Tests.cs
docs/BGH_ROLE_EDIT_SCOPE.md
```

File test BGH hiện có:

```text
Backend.ApiTests/BghPassFailControllerTests.cs
```

## 5. Whitelist có điều kiện — file dùng chung

Các file dưới đây có liên hệ với BGH nhưng ảnh hưởng nhiều role. Chỉ được sửa phần nhỏ nhất liên quan trực tiếp đến BGH và phải nêu rõ lý do trước khi sửa.

### 5.1. Điều hướng, auth và API contract

```text
frontend/src/router/index.js                  # Chỉ block route /bgh
frontend/src/constants/roleCatalog.js         # Chỉ mapping Principal/BGH
frontend/src/data/authPortals.js              # Chỉ portal bgh
Backend/Constants/AuthConstants.cs            # Chỉ constant/mapping Principal
Backend/Program.cs                            # Chỉ policy/DI bắt buộc cho endpoint BGH
docs/API_CONTRACT.md                          # Chỉ endpoint /api/bgh/*
```

### 5.2. Design system và component dùng chung

Chỉ sửa khi yêu cầu là thay đổi design system dùng chung hoặc không thể hoàn thành trong component/view BGH:

```text
frontend/src/assets/liquid-glass.css
frontend/src/components/LmsSelect.vue
frontend/src/components/SinhVien/AppTopbar.vue
frontend/src/components/SinhVien/PageContainer.vue
frontend/src/components/SinhVien/SidebarMenuGroup.vue
frontend/src/components/common/skeleton/SkeletonDashboard.vue
frontend/src/components/common/skeleton/SkeletonTable.vue
frontend/src/components/ui/AiAssistant.vue
frontend/src/components/ui/AnnouncementBanner.vue
frontend/src/components/ui/ConfirmActionDialog.vue
frontend/src/components/ui/GlassBadge.vue
frontend/src/components/ui/GlassButton.vue
frontend/src/components/ui/SidebarRecentFavorites.vue
frontend/src/services/apiClient.js
frontend/src/services/exportService.js
frontend/src/stores/auth.js
frontend/src/stores/popup.js
```

Khi sửa file design/component dùng chung, AI phải kiểm tra ít nhất một màn BGH liên quan và xác nhận không làm thay đổi ngoài ý muốn ở role khác. Ưu tiên truyền class, prop hoặc CSS variable từ BGH thay vì thay hành vi mặc định của component dùng chung.

### 5.3. Data access và entity dùng chung

Chỉ sửa các file này khi yêu cầu BGH bắt buộc thay đổi mapping/entity và người dùng đã đồng ý với ảnh hưởng database:

```text
Backend/Data/ApplicationDbContext.cs
Backend/Models/BuoiHoc.cs
Backend/Models/ChuongTrinhDaoTao.cs
Backend/Models/DanhGiaGiaoVien.cs
Backend/Models/DanhMucMonHoc.cs
Backend/Models/DiemSo.cs
Backend/Models/DonTu.cs
Backend/Models/DonVi.cs
Backend/Models/HocKy.cs
Backend/Models/KhoaHoc.cs
Backend/Models/LopHanhChinh.cs
Backend/Models/NguoiDung.cs
Backend/Models/NhatKyKiemToan.cs
Backend/Models/NhatKyThayDoiDiem.cs
Backend/Models/PhongHoc.cs
Backend/Models/Tang.cs
Backend/Models/ThoiKhoaBieu.cs
Backend/Models/ToaNha.cs
Backend/Models/VaiTro.cs
Backend/Models/YeuCauSuaDiem.cs
```

`Backend/Migrations/**` không thuộc whitelist mặc định. Muốn tạo hoặc sửa migration phải có yêu cầu rõ ràng riêng của người dùng.

## 6. Quy tắc khi sửa design BGH

1. Dùng semantic tokens/CSS variables; không thêm `bg-white`, `text-slate-*`, `border-slate-*` hoặc màu hex hardcode trong view/component mới.
2. Giữ bộ biến sidebar BGH theo `AGENTS.md`: `--sidebar-accent`, `--sidebar-indicator`, `--active-start`, `--active-mid`, `--active-end`.
3. Glassmorphism chỉ dùng cho sidebar, topbar, card nổi, modal hoặc khu vực có hierarchy; không phủ glass lên bảng/form dài.
4. Dữ liệu async phải có loading, error và empty state.
5. Combo box, modal, button và skeleton ưu tiên component chung hiện có. Nếu cần style riêng, đặt class/style trong file BGH trước.
6. Không hardcode dữ liệu API, token hoặc role trong component.

## 7. Quy tắc khi sửa backend BGH

1. Endpoint protected phải có `[Authorize]` và dùng `AuthRoles`.
2. Luôn giữ role scope và campus scope qua `MaDonVi`/`CurrentUser`.
3. Không dùng mock hoặc fallback dữ liệu giả.
4. Truy vấn chỉ đọc ưu tiên `AsNoTracking()`; tránh N+1 và giới hạn tập dữ liệu báo cáo lớn.
5. Lỗi nghiệp vụ dùng `ApiException` hoặc response pattern hiện có; lỗi chung đi qua middleware.
6. Khi thêm hoặc đổi `/api/bgh/*`, cập nhật đúng phần BGH trong `docs/API_CONTRACT.md`.
7. Thêm test BGH tương ứng cho filter, role/campus scope và phép tính báo cáo quan trọng.

## 8. Checklist bắt buộc cho AI

Trước khi sửa:

- [ ] Đã đọc `README.md`, `AGENTS.md`, `CLAUDE.md` và file này.
- [ ] Đã dùng `rg` kiểm tra file/endpoint/component thật.
- [ ] Đã liệt kê chính xác file dự kiến sửa.
- [ ] Mọi file đều thuộc mục 4, hoặc đáp ứng điều kiện tại mục 5.

Sau khi sửa:

- [ ] `git diff --name-only` không có file ngoài phạm vi.
- [ ] Backend liên quan đã chạy build/test phù hợp.
- [ ] Frontend liên quan đã chạy build/lint/test phù hợp.
- [ ] UI BGH có loading/error/empty và dùng semantic tokens.
- [ ] API BGH không mất role/campus scope và không dùng mock.
- [ ] Đã báo rõ file thay đổi, kết quả verify và phần chưa kiểm tra được.

## 9. Lệnh kiểm tra phạm vi nhanh

```powershell
git diff --name-only
rg --files frontend/src/views/BGH frontend/src/components/BGH
rg -n "api/bgh|/bgh|Principal" Backend frontend/src docs/API_CONTRACT.md
```

Nếu `git diff --name-only` có file ngoài whitelist do thay đổi đã tồn tại trước task, AI phải bảo toàn thay đổi đó, không chỉnh sửa và ghi rõ trong báo cáo.
