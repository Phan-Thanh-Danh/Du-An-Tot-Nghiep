# PARENT Handoff Package

## 1. Identity
- **Canonical backend role**: `Parent`
- **Database role code**: `phu_huynh`
- **Frontend aliases**: `Parent`, `PhuHuynh`

## 2. Architecture & Ownership
- **Exact folder ownership**: `frontend/src/views/PhuHuynh/`
- **Actual home route**: `/parent/dashboard`
- **Layout**: `Layout_PhuHuynh.vue` (or shared layout with Parent-specific sidebar vars)
- **Menu source**: `frontend/src/router/index.js`
- **Shared components**: `frontend/src/components/common/`

## 3. Capabilities

### IMPLEMENTED — Backend + Frontend both connected

| CapabilityId | Operation | Backend Route |
|---|---|---|
| CAP-PAR-001 | View dashboard | `GET /api/parent/dashboard` |
| CAP-PAR-002 | View children list | `GET /api/parent/children` |
| CAP-PAR-003 | View child grades | `GET /api/parent/children/{id}/grades` |
| CAP-PAR-004 | View child schedule | `GET /api/parent/children/{id}/schedule` |
| CAP-PAR-005 | View child attendance | `GET /api/parent/children/{id}/attendance` |
| CAP-PAR-006 | View child alerts | `GET /api/parent/children/{id}/alerts` |
| CAP-PAY-001 | View tuition | `GET /api/parent/children/{id}/tuition` |
| CAP-PAR-007 | View notifications | `GET /api/parent/notifications` |
| CAP-PAR-008 | View profile | `GET /api/parent/profile` |
| CAP-PAR-009 | View transactions | `GET /api/parent/children/{id}/transactions` |

### PARTIAL — Backend IMPLEMENTED, workflow incomplete

| CapabilityId | Operation | Backend Route | Gap |
|---|---|---|---|
| CAP-PAY-002 | Initiate payment | `POST /api/parent/payment` | PayOS webhook reconciliation incomplete (BL-001) |

### MISSING — No backend implementation
- None

## 4. UI/UX
- **Folder**: `frontend/src/views/PhuHuynh/`
- **UX priority**: Dashboard → Child selection → Grades → Attendance → Tuition → Alerts
- **Multi-child navigation**: Parent may have multiple children — ensure child switcher persists selection

## 5. Rules
- **Local state**: `selectedChildId` in Pinia store only — never hardcode child data in component
- **Files that must not be modified**: `router/index.js`, `stores/auth.js`
- **Definition of Done**: All data from `parentApi` module, no local business data files, no mock data
