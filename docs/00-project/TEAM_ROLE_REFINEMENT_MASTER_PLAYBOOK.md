# TEAM ROLE REFINEMENT MASTER PLAYBOOK

> **DocumentVersion:** 1.0.0  
> **LastValidatedCommit:** `a0d3116`  
> **BaselineTag:** `v1.0.0-role-refinement-baseline`  
> **Owner:** Project Lead  
> **ChangeLog:**  
> - 1.0.0 — Initial baseline after document reorganization (2026-07-12)

---

## 1. Mục đích và Phạm vi

### 1.1 Mục tiêu

Giai đoạn **Role Refinement** hoàn thiện toàn bộ 7 role sau cho LMS:

| # | Role | Handoff File |
|---|---|---|
| 1 | Teacher | `docs/p0/roles/TEACHER_HANDOFF.md` |
| 2 | Student | `docs/p0/roles/STUDENT_HANDOFF.md` |
| 3 | Parent | `docs/p0/roles/PARENT_HANDOFF.md` |
| 4 | AcademicStaff | `docs/p0/roles/ACADEMIC_STAFF_HANDOFF.md` |
| 5 | Principal | `docs/p0/roles/PRINCIPAL_HANDOFF.md` |
| 6 | SuperAdmin | `docs/p0/roles/SUPER_ADMIN_HANDOFF.md` |
| 7 | Content Council | `docs/p0/roles/CONTENT_COUNCIL_HANDOFF.md` |

### 1.2 Đã khóa từ P0 (không được sửa)

- Backend authentication & JWT foundation
- EF Core `ApplicationDbContext` mappings
- Auth middleware & `CurrentUser` injection
- Backend controller routing conventions (`/api/...`)
- Frontend router base structure (`frontend/src/router/index.js`)
- Pinia store patterns (`frontend/src/stores/auth.js`)
- CSS design token system (`liquid-glass.css`)
- Glassmorphism UI components (`lg-glass`, `lg-sidebar`, `lg-topbar`)
- **161 tài liệu đã được di chuyển** theo Move Plan Batch 1–5

### 1.3 Được phép sửa

- Code trong `frontend/src/views/<Role>/` và `frontend/src/components/<Role>/`
- API service modules cho role tương ứng
- Backend controllers thuộc role (nếu có sẵn)
- Backend services/DTOs cho role tương ứng
- File cấu hình UX/UI của role

### 1.4 Nghiêm cấm tự ý sửa

- Auth foundation (JWT, middleware, login flow)
- `ApplicationDbContext.cs` entity mappings
- Core layout components (`Layout_SinhVien.vue`, sidebar, topbar)
- CSS design tokens
- `frontend/src/stores/auth.js` trừ khi được phê duyệt riêng
- Backend `Program.cs`, middleware pipeline
- Database migrations có sẵn

### 1.5 Điều kiện dừng và Escalation

Dừng ngay và báo Project Lead nếu:

- Phát hiện endpoint mapping sai trong Capability Matrix
- Cần thay đổi database schema
- Cần thêm dependency mới
- Cần refactor shared component ảnh hưởng nhiều role
- Build pipeline hoặc CI không xanh

---

## 2. Thứ tự đọc tài liệu bắt buộc

Mỗi agent hoặc thành viên **phải đọc theo đúng thứ tự**:

1. **TEAM_ROLE_REFINEMENT_MASTER_PLAYBOOK.md** — file này
2. **P0_TEAM_OWNERSHIP.md** (`docs/p0/P0_TEAM_OWNERSHIP.md`)
3. **Handoff của role được giao** (`docs/p0/roles/<ROLE>_HANDOFF.md`)
4. **P0_BACKEND_CAPABILITY_MATRIX.csv** (`docs/p0/P0_BACKEND_CAPABILITY_MATRIX.csv`)
5. **P0_BACKEND_ENDPOINT_INVENTORY.csv** (`docs/p0/P0_BACKEND_ENDPOINT_INVENTORY.csv`)
6. **P0_FRONTEND_INTEGRATION_BACKLOG.md** (`docs/p0/P0_FRONTEND_INTEGRATION_BACKLOG.md`)
7. **P0_MISSING_BACKEND_BACKLOG.md** (`docs/p0/P0_MISSING_BACKEND_BACKLOG.md`) — nếu có backend gap
8. **API_CONTRACT.md** (`docs/API_CONTRACT.md`) nếu cần dùng API
9. Các UX/design document liên quan
10. Source code hiện tại của role

> **Quy tắc:** Không được đọc riêng một file handoff rồi bắt đầu code ngay.

---

## 3. Quy tắc Source of Truth

Khi tài liệu mâu thuẫn, thứ tự ưu tiên:

```
Backend source code
→ Endpoint Inventory
→ Capability Matrix
→ Role Handoff
→ Frontend Integration Backlog
→ Các báo cáo/audit cũ
```

> Các audit historical không được dùng để phủ định source code hiện tại.

---

## 4. Phạm vi sở hữu của từng Role

### Teacher

| Khoản mục | Giá trị |
|---|---|
| Owned folders | `frontend/src/views/GiangVien/`, teacher-specific components |
| Read-only shared | `stores/auth.js`, `constants/roleCatalog.js`, `router/`, `components/common/` |
| Capability prefix | CAP-TCH, CAP-ATT |
| Handoff | `docs/p0/roles/TEACHER_HANDOFF.md` |

### Student

| Khoản mục | Giá trị |
|---|---|
| Owned folders | `frontend/src/views/Student/`, `frontend/src/views/SinhVien/` |
| Read-only shared | `stores/auth.js`, `constants/roleCatalog.js`, `router/`, `components/common/` |
| Capability prefix | CAP-STD |
| Handoff | `docs/p0/roles/STUDENT_HANDOFF.md` |

### Parent

| Khoản mục | Giá trị |
|---|---|
| Owned folders | `frontend/src/views/PhuHuynh/`, parent-specific components |
| Read-only shared | `stores/auth.js`, `constants/roleCatalog.js`, `router/`, `components/common/` |
| Capability prefix | CAP-PAR |
| API prefix | `/api/parent/` |
| Handoff | `docs/p0/roles/PARENT_HANDOFF.md` |

### AcademicStaff

| Khoản mục | Giá trị |
|---|---|
| Owned folders | `frontend/src/views/GiaoVu/`, staff-specific components |
| Read-only shared | `stores/auth.js`, `constants/roleCatalog.js`, `router/`, `components/common/` |
| Capability prefix | CAP-STF |
| Handoff | `docs/p0/roles/ACADEMIC_STAFF_HANDOFF.md` |

### Principal (BGH)

| Khoản mục | Giá trị |
|---|---|
| Owned folders | `frontend/src/views/BGH/`, principal-specific components |
| Read-only shared | `stores/auth.js`, `constants/roleCatalog.js`, `router/`, `components/common/` |
| Capability prefix | CAP-BGH |
| API prefix | `/api/bgh/` |
| Handoff | `docs/p0/roles/PRINCIPAL_HANDOFF.md` |

### SuperAdmin

| Khoản mục | Giá trị |
|---|---|
| Owned folders | `frontend/src/views/SuperAdmin/`, admin-specific components |
| Read-only shared | `stores/auth.js`, `constants/roleCatalog.js`, `router/`, `components/common/` |
| Capability prefix | CAP-SAD |
| Handoff | `docs/p0/roles/SUPER_ADMIN_HANDOFF.md` |

### Content Council

| Khoản mục | Giá trị |
|---|---|
| Owned folders | Content council-specific views/components |
| Read-only shared | `stores/auth.js`, `constants/roleCatalog.js`, `router/`, `components/common/` |
| Capability prefix | CAP-CNT |
| Handoff | `docs/p0/roles/CONTENT_COUNCIL_HANDOFF.md` |

---

## 5. Task Card Template

Mỗi capability phải được triển khai dưới dạng task card hoàn chỉnh:

```markdown
### Capability ID: [CAP-XXX-XXX]
- **Business operation:** [mô tả]
- **Current backend status:** IMPLEMENTED / PARTIAL / MISSING
- **Current frontend status:** IMPLEMENTED / PARTIAL / STATIC_UI_ONLY / MISSING
- **Backend endpoint IDs:** [EP-XXXXXXXX]
- **Current problems:** [mô tả]
- **Files likely affected:** [danh sách file]
- **Expected UX:** [mô tả]
- **Loading state:** [skeleton / spinner / none]
- **Empty state:** [hướng dẫn / none]
- **Error state:** [retry / message / none]
- **Forbidden state:** [403 redirect / message]
- **Success state:** [toast / redirect / message]
- **Validation rules:** [các rule]
- **Responsive requirements:** [desktop / tablet / mobile]
- **Accessibility requirements:** [keyboard / aria / focus]
- **Acceptance criteria:** [danh sách]
- **Required tests:** [danh sách]
- **Evidence required:** [screenshot / video / log]
- **Out of scope:** [các việc không thuộc task này]
```

---

## 6. Những gì phải kiểm tra và sửa

### 6.1 Functional

- [ ] Route truy cập được
- [ ] Quyền role chính xác (role khác không vào được)
- [ ] API thật được gọi (không mock)
- [ ] Request payload đúng DTO
- [ ] Response được map đúng UI
- [ ] Không dùng mock/static fallback trái yêu cầu
- [ ] Create/update/delete có confirm và feedback
- [ ] Dữ liệu reload đúng sau mutation
- [ ] Không tạo API mới khi endpoint đã tồn tại

### 6.2 UI

- [ ] Không vỡ layout
- [ ] Không tràn chữ
- [ ] Không dùng màu ngẫu nhiên (phải dùng design token)
- [ ] Component đồng nhất với design system hiện có
- [ ] Table, form, card, dialog và toast nhất quán
- [ ] Responsive desktop, tablet và mobile
- [ ] Không để raw JSON hoặc thông báo lỗi kỹ thuật cho người dùng

### 6.3 UX

- [ ] Loading hoặc skeleton
- [ ] Empty state có hướng dẫn hành động
- [ ] Error state có retry
- [ ] Forbidden state đúng role
- [ ] Success feedback (toast / message)
- [ ] Disabled state khi đang submit
- [ ] Unsaved changes warning nếu cần
- [ ] Confirm dialog cho hành động phá hủy
- [ ] Search/filter/pagination hợp lý
- [ ] Keyboard focus rõ ràng
- [ ] Label form và error message dễ hiểu

### 6.4 Security

- [ ] Không render HTML trực tiếp ngoài `SafeHtmlRenderer`
- [ ] Không hardcode quyền chỉ ở frontend
- [ ] Không gửi role/user ID không đáng tin từ UI nếu backend có thể lấy từ token
- [ ] Không log token hoặc dữ liệu nhạy cảm
- [ ] Không tự sửa auth foundation

---

## 7. Quy tắc "Được phép báo hoàn thành"

Thành viên hoặc agent chỉ được ghi `COMPLETED` khi **tất cả** điều kiện dưới đây đều đạt:

- [ ] CapabilityId đã được thực hiện đúng phạm vi
- [ ] EndpointId sử dụng đúng theo Matrix
- [ ] Không còn mock/static fallback ngoài phạm vi cho phép
- [ ] Happy path hoạt động trên browser thật
- [ ] Loading state đã kiểm tra
- [ ] Empty state đã kiểm tra
- [ ] Error state đã kiểm tra
- [ ] Forbidden/role access đã kiểm tra
- [ ] Create/update/delete đã kiểm tra nếu có
- [ ] Responsive đã kiểm tra
- [ ] Keyboard/accessibility cơ bản đã kiểm tra
- [ ] Unit test liên quan pass
- [ ] Frontend lint pass
- [ ] Frontend build pass
- [ ] Backend build pass nếu có sửa backend
- [ ] Không sửa file ngoài ownership hoặc đã có phê duyệt
- [ ] Có evidence cụ thể (screenshot / video / log)
- [ ] PR ghi CapabilityId và EndpointId

> **Không chấp nhận báo hoàn thành bằng:** "Code đã viết xong", "Build pass", "UI nhìn ổn", "API đã kết nối", "Validator xanh".

---

## 8. Kiểm tra thực tế bắt buộc

Mỗi role phải có test matrix với các loại test sau:

| Loại test | Nội dung |
|---|---|
| Authentication | Đăng nhập đúng role |
| Route access | Truy cập route hợp lệ |
| Unauthorized | Role khác bị chặn |
| API loading | Request thật được gửi |
| Happy path | Dữ liệu hiển thị đúng |
| Mutation | Create/update/delete đúng |
| Error | API 400/401/403/404/500 |
| Empty | Không có dữ liệu |
| Responsive | Desktop/tablet/mobile |
| Persistence | Refresh vẫn đúng |
| Regression | Màn hình cũ không bị hỏng |

Mỗi test phải ghi nhận:

```
Test ID: [TEST-XXX]
Capability ID: [CAP-XXX-XXX]
Precondition: ...
Steps: ...
Expected result: ...
Actual result: ...
PASS/FAIL: ...
Evidence: [screenshot/log]
Tester: ...
Commit SHA: ...
```

---

## 9. Evidence bắt buộc cho mỗi PR

Mỗi PR phải đính kèm:

- [ ] Screenshot trạng thái bình thường
- [ ] Screenshot empty/error nếu có
- [ ] Route đã test
- [ ] API method và route
- [ ] CapabilityId
- [ ] EndpointId
- [ ] Command build/test đã chạy
- [ ] Kết quả thực tế
- [ ] Danh sách file thay đổi
- [ ] Known limitations còn lại

> Đối với mutation quan trọng nên có video ngắn hoặc chuỗi screenshot trước–sau.

---

## 10. Quy tắc Git và PR

- **Không commit trực tiếp main**
- Một branch cho một role hoặc một capability group
- **Không trộn nhiều role trong một PR**
- Không refactor shared foundation trong PR role
- PR phải nhỏ và review được

### Tên branch

```
feature/teacher/<capability>
feature/student/<capability>
feature/parent/<capability>
feature/academic-staff/<capability>
feature/principal/<capability>
feature/super-admin/<capability>
feature/content-council/<capability>
```

### PR title

```
[CAP-TCH-004] Complete teacher class grades workflow
```

---

## 11. Trạng thái công việc chuẩn

Chỉ sử dụng các trạng thái:

| Trạng thái | Ý nghĩa |
|---|---|
| `NOT_STARTED` | Chưa bắt đầu |
| `IN_ANALYSIS` | Đang phân tích requirement |
| `IN_PROGRESS` | Đang code |
| `BLOCKED_BACKEND` | Chờ backend hoàn thiện |
| `BLOCKED_DEPENDENCY` | Chờ dependency khác |
| `READY_FOR_TEST` | Code xong, chờ test |
| `TEST_FAILED` | Test không đạt |
| `READY_FOR_REVIEW` | Test đạt, chờ review |
| `COMPLETED` | Hoàn thành (sau test thực tế và review evidence) |

> `COMPLETED` chỉ được đặt sau test thực tế và review evidence.

---

## 12. Quy tắc cho Agent

- Không tin hoàn toàn tài liệu historical
- Luôn đọc source trước khi tạo task
- Không suy đoán EndpointId
- Không tạo mock để làm UI "trông hoàn thành"
- Không thay đổi status sang `IMPLEMENTED` chỉ vì file hoặc route tồn tại
- Không báo pass nếu chưa chạy command
- Không tự ý sửa shared foundation
- Khi phát hiện mâu thuẫn phải báo rõ file, dòng và source ưu tiên
- Sau khi sửa phải regenerate các artifact liên quan
- Không được chỉ cập nhật tài liệu để che lỗi source

---

## 13. Final Role Acceptance Gate

Mỗi role chỉ được đóng khi **tất cả** điều kiện sau đạt:

- [ ] 100% capability trong phạm vi role đã được phân loại lại
- [ ] Không có capability `PARTIAL` mà không có backlog
- [ ] Không có endpoint mapping sai
- [ ] Không có route chết
- [ ] Không có screen chỉ là static skeleton nhưng ghi `COMPLETED`
- [ ] Không có high-severity UX issue
- [ ] Không có build/lint/test failure
- [ ] Không có lỗi permission nghiêm trọng
- [ ] Role owner và reviewer đã sign-off

---

## Phụ lục: Cách file master không bị stale

- File này **không copy** toàn bộ 549 endpoint hoặc toàn bộ Matrix vào Markdown
- Nó **link** tới `P0_BACKEND_CAPABILITY_MATRIX.csv` và `P0_BACKEND_ENDPOINT_INVENTORY.csv`
- Chỉ ghi capability theo role và workflow cần làm
- Dùng `CapabilityId`/`EndpointId` làm khóa
- Có `LastValidatedCommit` và `DocumentVersion` ở header
- Có script validator `tools/docs/validate-p0.mjs` kiểm tra mọi CapabilityId
- Không chỉnh sửa thủ công các bảng dữ liệu dài — dùng script sinh tự động

---

> **Baseline:** `v1.0.0-role-refinement-baseline` (`a0d3116`)  
> **Trạng thái:** P0 Foundation — FINAL APPROVED.  
> **Document moves — COMPLETED** (161 docs moved, 21 HELD_DUE_TO_REFS).  
> **Role Refinement — READY TO START.**
