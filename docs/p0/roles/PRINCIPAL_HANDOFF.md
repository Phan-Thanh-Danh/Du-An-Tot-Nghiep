# PRINCIPAL (BGH) Handoff Package

## 1. Identity
- **Canonical backend role**: `Principal`
- **Database role code**: `hieu_truong`
- **Frontend aliases**: `BGH`, `Principal`

## 2. Architecture & Ownership
- **Exact folder ownership**: `frontend/src/views/BGH/`
- **Actual home route**: `/bgh/dashboard`
- **Layout**: `Layout_BGH.vue`
- **Menu source**: `frontend/src/router/index.js`
- **Shared components**: `frontend/src/components/common/`

## 3. Capabilities

### IMPLEMENTED — Backend + Frontend both connected

| CapabilityId | Operation | Backend Route |
|---|---|---|
| CAP-BGH-007 | View dashboard | `GET /api/bgh/dashboard` |
| CAP-BGH-001 | View academic overview | `GET /api/bgh/academic/overview` |
| CAP-BGH-002 | View GPA report | `GET /api/bgh/academic/gpa` |
| CAP-BGH-003 | View at-risk students | `GET /api/bgh/academic/at-risk` |
| CAP-BGH-005 | View evaluation overview | `GET /api/bgh/evaluations/overview` |

### PARTIAL — Backend IMPLEMENTED, frontend connection incomplete

| CapabilityId | Operation | Backend Route | Gap |
|---|---|---|---|
| CAP-BGH-004 | View academic reports | `GET /api/bgh/academic/reports` | FE report table partial |
| CAP-BGH-006 | View AI evaluation analysis | `GET /api/bgh/evaluations/ai-analysis` | FE streaming/loading state incomplete (BL-004) |
| CAP-BGH-008 | View schedule changes | `GET /api/bgh/schedule/changes` | FE view is placeholder (BL-005) |

### MISSING — No backend implementation
- None

## 4. Additional Read-Only Access (via BghFacadeController)
The BGH facade provides read-only views of system data:
- `GET /api/bgh/master-data/training-programs`
- `GET /api/bgh/master-data/academic-terms`
- `GET /api/bgh/users` (list of users)
- `GET /api/bgh/schedules`
- `GET /api/bgh/audit-logs`

## 5. UI/UX
- **Folder**: `frontend/src/views/BGH/`
- **UX priority**: Dashboard → Academic Overview → At-Risk → Evaluation → GPA Report
- **Data**: All views must use real API data — no mock/static content

## 6. Rules
- **Files that must not be modified**: `router/index.js`, `stores/auth.js`
- **Definition of Done**: All endpoints connected, skeleton loading implemented, permissions enforced
