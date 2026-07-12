# TEACHER Handoff Package

## 1. Identity
- **Canonical backend role**: `Teacher`
- **Database role code**: `giao_vien`
- **Frontend aliases**: `GiangVien`, `Teacher`

## 2. Architecture & Ownership
- **Exact folder ownership**: `frontend/src/views/GiangVien/`
- **Actual home route**: `/teacher/dashboard`
- **Layout**: `Layout_GiangVien.vue`
- **Menu source**: `frontend/src/router/index.js`
- **Shared components**: `frontend/src/components/common/`

## 3. Capabilities

### IMPLEMENTED — Backend + Frontend both connected

| CapabilityId | Operation | Backend Route |
|---|---|---|
| CAP-ATT-001 | Open attendance session | `POST /api/buoi-hoc/{id}/attendance/start` |
| CAP-ATT-002 | Bulk-update attendance | `PUT /api/buoi-hoc/{id}/attendance/bulk` |
| CAP-ATT-003 | Submit attendance | `POST /api/buoi-hoc/{id}/attendance/submit` |
| CAP-TCH-001 | View dashboard | `GET /api/teacher/dashboard` |
| CAP-TCH-002 | View class list | `GET /api/teacher/classes` |
| CAP-TCH-003 | View class workspace | `GET /api/teacher/classes/{id}/workspace` |
| CAP-TCH-006 | View schedule | `GET /api/teacher/schedule` |
| CAP-TCH-007 | View today schedule | `GET /api/teacher/schedule/today` |

### PARTIAL — Backend IMPLEMENTED, frontend connection incomplete

| CapabilityId | Operation | Backend Route | Gap |
|---|---|---|---|
| CAP-ATT-004 | View attendance history | `GET /api/teacher/attendance/history` | FE not connected |
| CAP-TCH-004 | View class grades | `GET /api/teacher/classes/{id}/grades` | Filter/export not wired (BL-002) |
| CAP-TCH-005 | Update student grade | `PUT /api/teacher/classes/{id}/grades/{studentId}` | FE inline edit not wired |
| CAP-TCH-008 | Submit leave request | `POST /api/teacher/requests` | FE form not complete |
| CAP-TCH-009 | View request history | `GET /api/teacher/requests/history` | FE list not connected |
| CAP-TCH-010 | Create exam | `POST /api/teacher/exams` | FE creation flow partial |
| CAP-TCH-011 | View teaching preferences | `GET /api/teacher/teaching-preferences/{maHocKy}` | FE view partial |
| CAP-TCH-012 | Submit teaching preferences | `POST /api/teacher/teaching-preferences/{maHocKy}/submit` | FE submit not wired |
| CAP-TCH-013 | View student questions | `GET /api/teacher/student-questions` | FE not connected |
| CAP-TCH-014 | Reply to student question | `POST /api/teacher/student-questions/{id}/reply` | FE not connected |
| CAP-TCH-015 | Manage assignments | `GET/POST /api/teacher/assignments` | FE partial |
| CAP-TCH-016 | Grade submissions | `PUT /api/teacher/submissions/{id}/grade` | FE grading UI partial |
| CAP-TCH-017 | View exam results | `GET /api/teacher/exam-results` | FE not connected |

### MISSING — No backend implementation
- None

## 4. UI/UX
- **Folder to own exclusively**: `frontend/src/views/GiangVien/`
- **Static/mock screens**: Connect all PARTIAL views to real APIs before adding new features
- **UX priority**: Attendance flow → Class workspace → Gradebook → Schedule → Teaching preferences

## 5. Rules
- **Files that must not be modified without Core Team review**: `router/index.js`, `stores/auth.js`, `SafeHtmlRenderer.vue`
- **Definition of Done**:
  - Endpoint fully connected, no mock data
  - SafeHtmlRenderer used for any rich text (e.g. lesson comments)
  - Skeleton loading implemented for all async data
  - Permissions strictly enforced on FE and BE
