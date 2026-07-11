# PARENT Handoff Package

## 1. Identity
- **Canonical backend role**: `Parent`
- **Database role code**: `phu_huynh`
- **Existing frontend aliases**: PhuHuynh, Parent

## 2. Architecture & Ownership
- **Exact folder ownership**: `frontend/src/views/PhuHuynh/`
- **Actual home route**: `/parent/dashboard`
- **Layout**: `Layout_PhuHuynh.vue`
- **Menu source**: `frontend/src/router/index.js` and API dynamic menus
- **Shared components**: `frontend/src/components/common/`

## 3. Capabilities

### Supported operations
- Parent views tuition ("EP-6C1D0760")

### PARTIAL operations
- Parent initiates payment ("EP-FFA37155")

### MISSING operations
- None

## 4. UI/UX
- **Wrong-context views**: Ensure no other role's logic leaks into `frontend/src/views/PhuHuynh/`
- **Static/mock screens**: Must be connected to real APIs
- **UX direction**: Follow the feature UX contracts. (Priority: High)

## 5. Rules
- **Files that must not be modified**: `router/index.js`, `stores/auth.js`, `SafeHtmlRenderer.vue` (Require Core Team review)
- **Prioritized implementation tasks**: Complete all MISSING capabilities first.
- **Definition of Done**:
  - Endpoint fully connected.
  - No mock data used.
  - SafeHtmlRenderer used for any rich text.
  - Skeleton loading implemented.
  - Permissions strictly enforced on FE and BE.
