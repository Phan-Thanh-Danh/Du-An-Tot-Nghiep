# CONTENT_COUNCIL Handoff Package

## 1. Identity
- **Canonical backend role**: `HoiDongQuanLyNoiDung`
- **Database role code**: `hoidong_quanly_noidung`
- **Existing frontend aliases**: ContentCouncil

## 2. Architecture & Ownership
- **Exact folder ownership**: `frontend/src/views/ContentCouncil/`
- **Actual home route**: `/content-council/dashboard`
- **Layout**: `Layout_ContentCouncil.vue`
- **Menu source**: `frontend/src/router/index.js` and API dynamic menus
- **Shared components**: `frontend/src/components/common/`

## 3. Capabilities
- **Supported operations**: [See P0_BACKEND_CAPABILITY_MATRIX.csv]
- **PARTIAL operations**: [See P0_BACKEND_CAPABILITY_MATRIX.csv]
- **MISSING operations**: [See P0_MISSING_BACKEND_BACKLOG.md]

## 4. API Endpoints
- **Exact API endpoints**: Check `P0_BACKEND_ENDPOINT_INVENTORY.csv` matching role `HoiDongQuanLyNoiDung`
- **Proposed API operations**: Check `P0_MISSING_BACKEND_BACKLOG.md`

## 5. UI/UX
- **Wrong-context views**: Ensure no other role's logic leaks into `frontend/src/views/ContentCouncil/`
- **Static/mock screens**: Must be connected to real APIs
- **UX direction**: Follow the feature UX contracts.

## 6. Rules
- **Files that must not be modified**: `router/index.js`, `stores/auth.js`, `SafeHtmlRenderer.vue` (Require Core Team review)
- **Prioritized implementation tasks**: Complete all MISSING capabilities first.
- **Definition of Done**:
  - Endpoint fully connected.
  - No mock data used.
  - SafeHtmlRenderer used for any rich text.
  - Skeleton loading implemented.
  - Permissions strictly enforced on FE and BE.
