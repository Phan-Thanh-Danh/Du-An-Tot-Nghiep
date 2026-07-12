# SUPER ADMIN Handoff Package

## 1. Identity
- **Canonical backend role**: `SuperAdmin`
- **Database role code**: `super_admin`
- **Frontend aliases**: `SuperAdmin`, `Admin`
- **Note**: `Admin` is a sub-role with reduced permissions. Both share this handoff.

## 2. Architecture & Ownership
- **Exact folder ownership**: `frontend/src/views/SuperAdmin/`
- **Actual home route**: `/super-admin/dashboard`
- **Layout**: Shared admin layout
- **Menu source**: `frontend/src/router/index.js`
- **Shared components**: `frontend/src/components/common/`

## 3. Capabilities

### IMPLEMENTED — Backend + Frontend both connected

| CapabilityId | Operation | Backend Route |
|---|---|---|
| CAP-SA-001 | List users | `GET /api/admin/users` |
| CAP-SA-002 | Create user | `POST /api/admin/users` |
| CAP-SA-008 | Manage organizations | `GET/POST/PUT/DELETE /api/organizations` |
| CAP-USR-001 | Admin user management (legacy alias) | `GET/POST /api/admin/users` |

### PARTIAL — Backend IMPLEMENTED, frontend connection incomplete

| CapabilityId | Operation | Backend Route | Gap |
|---|---|---|---|
| CAP-SA-003 | Update user | `PUT /api/admin/users/{id}` | FE edit form partial |
| CAP-SA-004 | Lock user | `PATCH /api/admin/users/{id}/lock` | FE action button not wired |
| CAP-SA-005 | Unlock user | `PATCH /api/admin/users/{id}/unlock` | FE action button not wired |
| CAP-SA-006 | Reset password | `PATCH /api/admin/users/{id}/reset-password` | FE form not wired |
| CAP-SA-007 | View system dashboard | `GET /api/super-admin/dashboard/stats` | FE stats cards partial |
| CAP-SA-009 | View audit logs | `GET /api/audit-logs` | FE list filter not fully connected |

### MISSING — No backend implementation
- None

## 4. Additional Admin Capabilities
The Admin role also covers:
- `GET /api/admin/rbac` — RBAC management (`RbacController`)
- `GET/POST /api/admin/discipline-appeals` — Discipline appeals
- `GET/POST /api/admin/registrations` — Manage student registrations
- `GET/POST/PUT /api/master-data/*` — Training programs, subjects, terms (SuperAdmin only)

## 5. UI/UX
- **Folder**: `frontend/src/views/SuperAdmin/`
- **UX priority**: User management → Organizations → System dashboard → Audit logs

## 6. Rules
- **Files that must not be modified**: `router/index.js`, `stores/auth.js`
- **Policy**: `AdminUserManagement` policy is enforced on all user management endpoints — do not bypass on FE
- **Definition of Done**: All endpoints connected, skeleton loading, permissions enforced
