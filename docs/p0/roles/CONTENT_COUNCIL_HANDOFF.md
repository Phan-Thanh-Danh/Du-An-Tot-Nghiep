# CONTENT_COUNCIL Handoff Package

## 1. Identity
- **Canonical backend role**: `HoiDongQuanLyNoiDung`
- **Database role code**: `hoidong_quanly_noidung`
- **Existing frontend aliases**: ContentCouncil

## 2. Architecture & Ownership
- **Exact folder ownership**: `frontend/src/pages/content-council/, frontend/src/components/content-council/`
- **Actual home route**: `/content-council/subjects`
- **Layout**: `Layout_ContentCouncil.vue`
- **Menu source**: `frontend/src/router/index.js` and API dynamic menus
- **Shared components**: `frontend/src/components/common/`

## 3. Capabilities

### Supported operations
- Content Council manages question bank ("EP-FC063A4E|EP-4490FAE2|EP-45EFC03E|EP-9D82B659|EP-4B47C45F|EP-9DEC4A60|EP-B922FB8B|EP-1C8F7923|EP-0D6B7B01")

### PARTIAL operations
- None

### MISSING operations
- Content Council publishes a quiz

## 4. UI/UX
- **Wrong-context views**: Ensure no other role's logic leaks into `frontend/src/pages/content-council/, frontend/src/components/content-council/`
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
