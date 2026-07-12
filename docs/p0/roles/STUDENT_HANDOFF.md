# STUDENT Handoff Package

## 1. Identity
- **Canonical backend role**: `Student`
- **Database role code**: `sinh_vien`
- **Frontend aliases**: `Student`, `SinhVien`

## 2. Architecture & Ownership
- **Exact folder ownership**: `frontend/src/views/Student/` (newer), `frontend/src/views/SinhVien/` (legacy, consolidate gradually)
- **Actual home route**: `/student/dashboard`
- **Layout**: `Layout_SinhVien.vue`
- **Menu source**: `frontend/src/router/index.js`
- **Shared components**: `frontend/src/components/common/`

## 3. Capabilities

### IMPLEMENTED — Backend + Frontend both connected

| CapabilityId | Operation | Backend Route |
|---|---|---|
| CAP-STD-001 | View dashboard | `GET /api/student/dashboard` |
| CAP-STD-002 | View courses | `GET /api/student/courses` |
| CAP-STD-003 | View schedule | `GET /api/student/schedule` |
| CAP-STD-006 | View assignments | `GET /api/student/assignments` |
| CAP-STD-007 | Submit assignment | `POST /api/student/assignments/{id}/submit` |
| CAP-STD-008 | View grades | `GET /api/student/grades` |
| CAP-STD-009 | View tuition | `GET /api/student/tuition` |
| CAP-STD-010 | View support tickets | `GET /api/student/support-tickets` |
| CAP-STD-011 | Create support ticket | `POST /api/student/support-tickets` |
| CAP-STD-012 | View evaluations | `GET /api/student/evaluations` |
| CAP-STD-013 | Submit evaluation | `POST /api/student/evaluations/submit` |
| CAP-STD-014 | View curriculum | `GET /api/student/curriculum` |
| CAP-APP-001 | Submit application | `POST /api/student/applications/{id}/submit` |
| CAP-APP-002 | View applications | `GET /api/student/applications` |

### PARTIAL — Backend IMPLEMENTED, frontend connection incomplete

| CapabilityId | Operation | Backend Route | Gap |
|---|---|---|---|
| CAP-STD-004 | View available registrations | `GET /api/student/registrations/available` | FE registration portal not fully connected |
| CAP-STD-005 | Register for course | `POST /api/student/registrations` | FE submit + conflict validation partial |
| CAP-STD-015 | View discipline records | `GET /api/student/discipline-records` | FE view exists but detail not wired |
| CAP-STD-016 | View rewards | `GET /api/student/rewards` | FE reward detail not connected |

### MISSING — No backend implementation
- None

## 4. UI/UX
- **Folder**: `frontend/src/views/Student/` (prefer over `SinhVien/` for new work)
- **UX priority**: Dashboard → Grades → Schedule → Evaluations → Applications → Support Tickets

## 5. Rules
- **Files that must not be modified**: `router/index.js`, `stores/auth.js`, `SafeHtmlRenderer.vue`
- **Definition of Done**: Endpoint connected, no mock data, skeleton loading, permissions enforced
