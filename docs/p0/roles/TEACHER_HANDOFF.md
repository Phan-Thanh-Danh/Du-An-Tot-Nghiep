# TEACHER Handoff Package

## 1. Identity
- **Canonical backend role**: `Teacher`
- **Database role code**: `giao_vien`
- **Existing frontend aliases**: GiangVien, Teacher

## 2. Architecture & Ownership
- **Exact folder ownership**: `frontend/src/views/GiangVien/`
- **Actual home route**: `/teacher/dashboard`
- **Layout**: `Layout_GiangVien.vue`
- **Menu source**: `frontend/src/router/index.js` and API dynamic menus
- **Shared components**: `frontend/src/components/common/`

## 3. Capabilities

### Supported operations
- Teacher opens attendance ("EP-BD53AAA7")
- Teacher bulk-updates attendance ("EP-E86CF615")
- Teacher submits attendance ("EP-B3CFFB29")

### PARTIAL operations
- None

### MISSING operations
- None

## 4. UI/UX
- **Wrong-context views**: Ensure no other role's logic leaks into `frontend/src/views/GiangVien/`
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
