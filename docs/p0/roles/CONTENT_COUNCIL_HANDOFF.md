# CONTENT COUNCIL Handoff Package

## 1. Identity
- **Canonical backend role**: `HoiDongQuanLyNoiDung`
- **Database role code**: `hoi_dong_quan_ly_noi_dung`
- **Frontend aliases**: `HoiDongQuanLyNoiDung`, `ContentCouncil`

## 2. Architecture & Ownership
- **Exact folder ownership**: `frontend/src/pages/content-council/`, `frontend/src/components/content-council/`
- **Actual home route**: `/content-council/question-bank` (or similar — verify in router)
- **Layout**: Shared layout with content-council sidebar config
- **Menu source**: `frontend/src/router/index.js`
- **Shared components**: `frontend/src/components/common/`

## 3. Capabilities

### IMPLEMENTED — Backend + Frontend both connected

| CapabilityId | Operation | Backend Route |
|---|---|---|
| CAP-QB-001 | Manage question bank (CRUD, activate, import) | `GET/POST/PUT/DELETE /api/question-bank/questions` |

### PARTIAL — Backend IMPLEMENTED, frontend connection incomplete

| CapabilityId | Operation | Backend Route | Gap |
|---|---|---|---|
| CAP-QZ-001 | Manage quiz (CRUD, add questions) | `GET/POST/PUT/DELETE /api/exam/de-kiem-tra` | FE quiz list/detail views partial |
| CAP-QZ-002 | Publish quiz | `POST /api/exam/de-kiem-tra/{id}/publish` | FE publish button not connected |
| CAP-QZ-003 | Unpublish quiz | `POST /api/exam/de-kiem-tra/{id}/unpublish` | FE unpublish action not connected |

### MISSING — No backend implementation
- None

## 4. UI/UX
- **Folders**: `frontend/src/pages/content-council/`, `frontend/src/components/content-council/`
- **UX priority**: Question bank management → Quiz creation → Quiz publish workflow

## 5. Rules
- **Files that must not be modified**: `router/index.js`, `stores/auth.js`, `SafeHtmlRenderer.vue`
- **SafeHtmlRenderer required**: Question content and answer HTML must always go through `SafeHtmlRenderer`
- **Definition of Done**: All question bank and quiz endpoints connected, SafeHtmlRenderer used for rich content, no mock data
