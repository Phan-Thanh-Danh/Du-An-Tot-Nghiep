# ACADEMIC STAFF (GiaoVu) Handoff Package

## 1. Identity
- **Canonical backend role**: `AcademicStaff`
- **Database role code**: `giao_vu`
- **Frontend aliases**: `AcademicStaff`, `GiaoVu`

## 2. Architecture & Ownership
- **Exact folder ownership**: `frontend/src/views/GiaoVu/`
- **Actual home route**: `/staff/dashboard`
- **Layout**: `Layout_GiaoVu.vue`
- **Menu source**: `frontend/src/router/index.js`
- **Shared components**: `frontend/src/components/common/`

## 3. Capabilities

### IMPLEMENTED — Backend + Frontend both connected

| CapabilityId | Operation | Backend Route |
|---|---|---|
| CAP-STF-002 | View timetable list | `GET /api/thoi-khoa-bieu` |
| CAP-STF-003 | Create timetable | `POST /api/thoi-khoa-bieu` |
| CAP-STF-004 | Generate sessions | `POST /api/thoi-khoa-bieu/{id}/generate-sessions` |
| CAP-STF-005 | AI-generate timetable | `POST /api/thoi-khoa-bieu/generate` |
| CAP-STF-009 | View teaching preferences summary | `GET /api/staff/teaching-preferences/summary` |

### PARTIAL — Backend IMPLEMENTED, frontend connection incomplete

| CapabilityId | Operation | Backend Route | Gap |
|---|---|---|---|
| CAP-STF-001 | View dashboard | `GET /api/staff/dashboard` | FE dashboard cards partial |
| CAP-STF-006 | Publish timetable | `POST /api/thoi-khoa-bieu/publish` | Confirmation dialog not wired (BL-003) |
| CAP-STF-007 | Book room | `POST /api/staff/rooms/book` | FE booking form partial |
| CAP-STF-008 | View room bookings | `GET /api/staff/rooms/bookings` | FE list not fully connected |
| CAP-STF-010 | Manage academic terms | `GET/POST/PUT /api/academic-terms` | FE admin term form partial |

### MISSING — No backend implementation
- None

## 4. UI/UX
- **Folder**: `frontend/src/views/GiaoVu/`
- **UX priority**: Timetable management → Room bookings → Teaching preferences → Academic terms → Dashboard

## 5. Rules
- **Files that must not be modified**: `router/index.js`, `stores/auth.js`
- **Policy**: `AcademicOperations` policy enforced on timetable generation endpoints
- **Definition of Done**: Endpoints connected, no mock data, skeleton loading, permissions enforced
