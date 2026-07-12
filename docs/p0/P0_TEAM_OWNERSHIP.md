# P0 Team Ownership

## 1. Domain Owners

> **IMPORTANT**: All folder paths below are verified against the actual `frontend/src/views/` filesystem structure. Do not rename folders without team coordination.

| Domain | Scope / Canonical Role | Frontend Paths (Exclusive) | Owner |
|--------|----------------------|----------------------------|-------|
| Student | `Student` | `frontend/src/views/Student/`, `frontend/src/views/SinhVien/` | Student Team |
| Teacher | `Teacher` | `frontend/src/views/GiangVien/` | Teacher Team |
| AcademicStaff | `AcademicStaff` | `frontend/src/views/GiaoVu/` | Academic Staff Team |
| Parent | `Parent` | `frontend/src/views/PhuHuynh/` | Parent Team |
| Principal | `Principal` | `frontend/src/views/BGH/` | BGH Team |
| SuperAdmin | `SuperAdmin`, `Admin` | `frontend/src/views/SuperAdmin/` | Admin Team |
| Content Council | `HoiDongQuanLyNoiDung` | `frontend/src/pages/content-council/`, `frontend/src/components/content-council/` | Content Council Team |
| Foundation | Core, Layouts, UI | `frontend/src/components/common/`, `frontend/src/layouts/` | Core Frontend Team |
| Backend | ASP.NET API | `Backend/` | Backend Team |
| Governance | Documentation | `docs/governance/` | Project Lead |

## 2. Protected Shared Paths

These paths belong to the Core Frontend Team and **MUST NOT** be modified without cross-team review and approval:
- `frontend/src/router/` (Routing configuration)
- `frontend/src/stores/auth.js` (Role catalog and auth state)
- `frontend/src/services/apiClient.js` (API Client config)
- `frontend/src/components/common/` (UI primitives, skeletons, safe renderers)
- `frontend/src/layouts/` (Shared layouts)
- `frontend/src/assets/` (Global styles and CSS variables)
- `docs/p0/P0_BACKEND_CAPABILITY_MATRIX.csv` (Capability Registry)

## 3. Shared Change Approval Process

1. Propose change via PR/MR modifying the protected file.
2. Core Frontend Team reviews for backward compatibility and design system compliance.
3. If it modifies API contracts, Backend Lead must also approve.
4. Merge is allowed only after approvals and passing unit/security tests (e.g., Safe HTML tests).

## 4. Verified Folder Existence

Run `Get-ChildItem frontend/src/views -Directory` to confirm:

```
Auth/       BGH/        GiangVien/  GiaoVu/
Payment/    PhuHuynh/   SinhVien/   Student/
SuperAdmin/
```

> Student views exist in both `Student/` (newer) and `SinhVien/` (legacy) — consolidation is a future task tracked in backlog.
