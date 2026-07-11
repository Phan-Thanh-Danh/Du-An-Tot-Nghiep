# PRINCIPAL Handoff Package

## 1. Identity
- **Canonical backend role**: `Principal`
- **Database role code**: `hieu_truong`
- **Existing frontend aliases**: BGH, Principal

## 2. Architecture & Ownership
- **Exact folder ownership**: `frontend/src/views/BGH/`
- **Actual home route**: `/bgh/dashboard`
- **Layout**: `Layout_BGH.vue`
- **Menu source**: `frontend/src/router/index.js` and API dynamic menus
- **Shared components**: `frontend/src/components/common/`

## 3. Capabilities

### Supported operations
- None fully implemented

### PARTIAL operations
- None

### MISSING operations
- None

## 4. UI/UX
- **Wrong-context views**: Ensure no other role's logic leaks into `frontend/src/views/BGH/`
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
