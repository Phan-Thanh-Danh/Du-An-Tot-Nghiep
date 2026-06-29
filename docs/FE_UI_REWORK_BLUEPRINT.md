# FE UI Rework Blueprint

Dieu kien: khong code UI truoc khi doc file nay cung `FE_UI_SYSTEM_AUDIT.md` va `FE_UI_RESEARCH_NOTES.md`.

## Global Implementation Rules

- Moi page nam trong layout role hien co; khong tao `lg-app-bg` trong view con.
- Header page dung PageContainer cua layout; view con chi render content/action neu layout da co title.
- Button: `GlassButton`.
- Badge: `GlassBadge` hoac wrapper status badge dung `GlassBadge`.
- Panel/card: `GlassPanel`.
- Table: `TableShell`.
- Input/search: `GlassInput` hoac `lg-input`/`lg-control` tokenized.
- Loading: `LoadingSkeleton`/`ProgressBar`.
- Empty: `EmptyState`.
- Toast: `usePopupStore`, khong `alert()`/`confirm()`.
- Dates: `dayjs` format `DD/MM/YYYY`, `HH:mm`, relative Vietnamese if needed.
- Status: luon map sang tieng Viet.
- Dark mode: khong `bg-white`, `text-black`, `border-black`, raw `#fff/#ddd`.

## Shared Components/Helpers Proposed

- `frontend/src/components/Schedule/WeekCalendarGrid.vue`
  - Role-neutral week grid/list.
  - Props: `sessions`, `selectedId`, `mode`.
  - Emits: `select-session`.
  - Uses `GlassPanel`, `GlassBadge`.

- `frontend/src/components/Schedule/SessionStatusBadge.vue`
  - Maps `du_kien`, `dang_diem_danh`, `da_gui`, `da_khoa`, `huy`, `doi_phong`, `doi_ca`, `day_thay`.

- `frontend/src/components/applications/RequestForm.vue`
  - Multi-step request wizard.
  - Only if existing `ApplicationFormRenderer` cannot satisfy wizard UX.

- `frontend/src/components/applications/RequestTimeline.vue`
  - Thin wrapper around existing `ApplicationTimeline` if student/admin timeline needs unified style.

- `frontend/src/components/notifications/NotificationCompose.vue`
  - Role-aware wrapper around existing form + preview, replacing browser confirm.

- `frontend/src/components/reward-discipline/CertificateCard.vue`
  - Reusable certificate status/download card if current reward cards lack preview state.

Add only if needed; prefer modifying existing components first.

## Screens

### 1. Student Schedule

- Role: Student.
- Route: `/student/schedule`.
- Muc tieu: xem lich hoc theo tuan/thang, nhan biet thay doi phong/ca/day thay/huy.
- User job: biet hom nay hoc gi, o dau, luc nao, voi ai.
- Layout: summary strip + week navigator + week grid; mobile switches to timeline list.
- Page header: use layout title; actions in content: week nav and today button.
- Primary action: none; student read-only.
- Filter/search/sort: subject filter, status/type filter, search by subject/room/teacher.
- Content area: `WeekCalendarGrid` with day columns and session cards.
- Detail/action panel: right drawer/modal with session details, change reason, teacher, room, ca, time.
- Empty state: no sessions in selected week.
- Loading state: skeleton grid.
- Error state: readable panel with retry.
- Responsive: desktop 7-column grid; tablet 2-column day groups; mobile timeline by date.
- Light mode: `surface-card` cards, soft blue/cyan accents.
- Dark mode: no flat white; use token surfaces only.
- Core components: `GlassPanel`, `GlassBadge`, `GlassButton`, `GlassInput`, `LoadingSkeleton`, `EmptyState`.
- Data source: `studentData.mock.js` plus minimal schedule adapter if fields missing.
- Actions/state: select session, navigate week, filter.
- Accessibility: date buttons have labels; session cards are buttons; no ARIA grid unless keyboard implemented.

### 2. Student Attendance

- Role: Student.
- Route: `/student/attendance`.
- Muc tieu: xem ty le chuyen can, lich su diem danh, mon co nguy co.
- User job: biet buoi nao vang/di muon/co phep va can lam gi.
- Layout: KPI summary + subject quota bars + filters + history table/list + detail drawer.
- Page header: layout title.
- Primary action: "Xem don giai trinh" or link to `/student/requests` when applicable.
- Filter/search/sort: subject, status, date range.
- Content area: `TableShell` on desktop, cards on mobile.
- Detail/action panel: session detail with teacher, room, note, allowed next action.
- Empty/loading/error: standard components.
- Responsive: table -> cards.
- Light/dark: token-only.
- Components: `ProgressBar`, `GlassBadge`, `TableShell`, `GlassButton`.
- Data source: existing student data or schedule adapter.
- Actions/state: filter, view detail, navigate to requests.
- Accessibility: table headers; status badges have text.

### 3. Teacher Attendance Today

- Role: Teacher.
- Route: `/teacher/attendance`.
- Muc tieu: diem danh nhanh cho cac buoi hom nay.
- User job: chon buoi, mo diem danh, mark sinh vien, bulk mark, submit.
- Layout: left session list + main attendance table + sticky summary/lock panel.
- Page header: layout title; content has date/session selector.
- Primary action: `Mo diem danh` or `Gui diem danh`.
- Filter/search/sort: search student, status chips.
- Content area: student rows with segmented status controls: co mat, vang, di muon, co phep.
- Detail/action panel: sticky summary counts, timer, lock state.
- Empty state: no class today.
- Loading state: skeleton list/table.
- Error state: retry panel.
- Responsive: session list above table, sticky summary at top.
- Light/dark: token-only, no raw green/red blocks.
- Components: `GlassPanel`, `GlassButton`, `GlassBadge`, `TableShell`, `GlassInput`.
- Data source: minimal attendance mock adapter if main still lacks shared mock.
- Actions/state: chua_mo -> dang_diem_danh -> da_gui; da_gui/da_khoa disables updates.
- Accessibility: status radio group per student, buttons disabled with clear text.

### 4. Teacher Attendance History

- Role: Teacher.
- Route: `/teacher/attendance-history`.
- Muc tieu: tra cuu buoi da diem danh va tao yeu cau mo khoa.
- User job: tim buoi, xem chi tiet, gui ly do mo khoa.
- Layout: filters + history list/table + unlock request section.
- Page header: layout title.
- Primary action: `Tao yeu cau mo khoa` disabled until session selected.
- Filters: date range, class/course, attendance status.
- Content area: history `TableShell`.
- Detail/action panel: selected session detail + modal request unlock.
- Empty/loading/error: standard.
- Responsive: table -> cards, modal full width mobile.
- Light/dark: token-only.
- Components: `GlassPanel`, `GlassInput`, `GlassBadge`, `GlassButton`, `TableShell`.
- Data source: attendance mock adapter.
- Actions/state: create local unlock request with statuses `cho_duyet`, `da_duyet`, `tu_choi`, `het_han`.
- Accessibility: modal close label, reason textarea label/error.

### 5. Student Requests

- Role: Student.
- Route: `/student/requests`.
- Muc tieu: quan ly don tu, tao don, theo doi SLA/timeline.
- User job: tao don dung loai, nop minh chung, xem tien do.
- Layout: summary cards + filters/search + request list + detail drawer/wizard.
- Header: layout title; primary action in content top right.
- Primary action: `Tao don moi`.
- Filters: status, type, search, date.
- Content area: cards with title, type, created/submitted date, status, SLA, action.
- Detail/action panel: readonly form data, evidence, timeline, allowed actions.
- Create form: 4 steps: chon loai don, dien form, minh chung, xem lai.
- Empty/loading/error: standard.
- Responsive: cards one-column mobile; wizard stepper vertical mobile.
- Light/dark: form panels `GlassPanel variant=readable`.
- Components: existing application components, `RequestForm` if needed.
- Data source: prefer `applicationsApi` when backend available; if UI-only run needed, add minimal fallback adapter, not inline data.
- Actions/state: local validation, draft/submit/cancel/resubmit states.
- Accessibility: GOV.UK style error summary + field errors.

### 6. Admin/Staff Application Queue

- Role: SuperAdmin/Admin/AcademicStaff.
- Route: `/super-admin/approvals/requests`; future staff route only if router has it.
- Muc tieu: nhan, phan cong, xu ly, approve/reject/request supplement/process.
- User job: triage nhanh don can xu ly.
- Layout: filter toolbar + `TableShell` queue + sticky detail/action panel.
- Header: layout title.
- Primary action: none globally; row/detail actions.
- Filters: status, type, assignee, SLA, campus, search.
- Content: queue table with status, SLA, student, type, created/submitted.
- Detail/action panel: timeline, evidence metadata, assignment, decision form.
- Empty/loading/error: standard.
- Responsive: table becomes cards; detail panel modal.
- Light/dark: no raw table borders.
- Components: existing admin application components.
- Data source: `applicationsApi`.
- Actions/state: receive, assign, supplement, approve, reject, process; disabled by status.
- Accessibility: form labels, buttons expose current action.

### 7. Notification Inbox

- Role: Student/Teacher/BGH.
- Routes: `/student/notifications`, `/teacher/notifications`, `/bgh/notifications`.
- Muc tieu: doc nhanh, loc nhanh, mark read.
- User job: xu ly thong bao chua doc/khan cap va mo action lien quan.
- Layout: header stats + filters/search + balanced split pane.
- Header: role-aware title/copy.
- Primary action: `Danh dau tat ca da doc` when unread > 0.
- Filters: Tat ca, Chua doc, Khan cap, Hoc vu, Tai chinh.
- Content: list with unread dot, category, priority, sender, time, excerpt.
- Detail panel: title, sender, time, category, priority, safe body renderer, related action.
- Empty/loading/error: standard.
- Responsive: list first, detail as drawer/mobile route-like panel.
- Light/dark: no empty giant pane; `GlassPanel` readable detail.
- Components: notification components; remove `v-html`.
- Data source: `notificationsApi`; optional minimal offline fallback only if required.
- Actions/state: select, mark read, mark all, hide.
- Accessibility: list items are buttons, active selected state, safe text rendering.

### 8. Admin Notification Compose

- Role: SuperAdmin/Admin/AcademicStaff.
- Route: `/super-admin/notifications/send`; staff notices route should reuse pattern.
- Muc tieu: tao thong bao thu cong/specialized voi preview nguoi nhan.
- User job: soan noi dung, chon pham vi, preview, gui.
- Layout: compose form 2 columns: form + preview/recipient panel.
- Header: layout title.
- Primary action: `Gui thong bao`.
- Filters/inputs: category, priority, scope, schedule if supported, recipient search/custom list.
- Content: form sections; preview card.
- Detail/action panel: recipient preview and validation/errors.
- Empty/loading/error: recipient preview skeleton/error.
- Responsive: preview below form.
- Light/dark: editor/textarea surface token.
- Components: `NotificationComposeForm`, `RecipientPreviewPanel`, `GlassButton`, `GlassPanel`.
- Data source: `notificationsApi`.
- Actions/state: preview loading, submit loading, confirmation modal instead of browser `confirm()`.
- Accessibility: no browser confirm; explicit confirmation panel.

### 9. Student Rewards

- Role: Student.
- Route: view exists `Student/RewardsView.vue`; route/menu decision required.
- Muc tieu: ton vinh thanh tich va tai bang khen.
- User job: xem thanh tich, trang thai PDF, tai/xem bang khen.
- Layout: hero achievement summary + certificate cards + timeline/detail preview.
- Header: student layout title if route added.
- Primary action: download/view certificate when generated.
- Filters: semester/status if list grows.
- Content: `CertificateCard` list.
- Detail panel: certificate preview metadata, timeline.
- Empty/loading/error: standard.
- Responsive: hero collapses, cards stack.
- Light/dark: amber accent restrained; no big glow overload.
- Components: existing reward components + optional `CertificateCard`.
- Data source: `rewardDisciplineMockService`.
- Actions/state: local download loading, unavailable PDF state.
- Accessibility: download button disabled with reason.

### 10. Student Discipline

- Role: Student.
- Route: view exists `Student/DisciplineView.vue`; route/menu decision required.
- Muc tieu: xem ho so ky luat ca nhan kin dao, khi co the khieu nai.
- User job: hieu trang thai/hieu luc, xem timeline, gui khieu nai neu duoc.
- Layout: privacy notice + list cards + detail/appeal form.
- Header: student layout title if route added.
- Primary action: `Gui khieu nai` only in detail and when allowed.
- Filters: active/expired/appeal status.
- Content: private list, no internal evidence/hash/raw notes.
- Detail panel: public summary, effective dates, timeline, appeal form.
- Empty/loading/error: standard.
- Responsive: detail replaces list on mobile.
- Light/dark: serious neutral/danger accents, low decoration.
- Components: existing discipline components.
- Data source: `rewardDisciplineMockService`.
- Actions/state: local appeal submit.
- Accessibility: appeal textarea label/error.

### 11. SuperAdmin Awards

- Role: SuperAdmin.
- Route: `/super-admin/awards`.
- Muc tieu: quan ly khen thuong/certificate/campaign summary without nested shell.
- User job: xem campaign/candidates/certificates, thao tac duyet/generate theo status.
- Layout: KPI + tabs/filters + campaign/candidate table + detail panel.
- Header: SuperAdmin layout title.
- Primary action: create campaign only if route intended for campaign CRUD; otherwise review candidates.
- Filters: semester, campus, status, type.
- Content: use existing `RewardCampaignTable`, `RewardCandidateTable`, `RewardCampaignDetail`.
- Detail panel: selected campaign/candidate.
- Empty/loading/error: standard.
- Responsive: table -> cards.
- Light/dark: no nested `lg-app-bg`, no hardcoded gradients.
- Components: reward-discipline components, `GlassPanel`, `TableShell`.
- Data source: `rewardDisciplineMockService`.
- Actions/state: approve/reject/generate local mock.
- Accessibility: table actions labelled.

### 12. SuperAdmin Discipline

- Role: SuperAdmin.
- Route: `/super-admin/discipline`.
- Muc tieu: quan ly ho so ky luat va appeal noi bo, bao mat thong tin.
- User job: xem ho so, loc theo muc do/trang thai, xu ly appeal.
- Layout: KPI + filters + records table + appeals panel/detail.
- Header: SuperAdmin layout title.
- Primary action: create record only if DL1 UI in scope.
- Filters: status, severity, campus, semester, search.
- Content: `DisciplineRecordTable` and `DisciplineAppealTable`.
- Detail panel: `DisciplineRecordDetail`, sanitized where needed.
- Empty/loading/error: standard.
- Responsive: table -> card/detail modal.
- Light/dark: restrained warning/danger accents.
- Components: existing discipline components.
- Data source: `rewardDisciplineMockService`.
- Actions/state: create/update/resolve mock.
- Accessibility: no browser alert; modal labels.

### 13. Staff Schedule Manager

- Role: AcademicStaff.
- Route: `/staff/schedule`.
- Muc tieu: quan ly TKB basic theo backend model.
- User job: tao/sua/cancel TKB by course/thu/ca/phong, publish/generate sessions.
- Layout: filters + schedule table + create/edit side panel + generate sessions summary.
- Header: staff layout title.
- Primary action: `Tao lich`.
- Filters: hoc ky, khoa hoc, lop, giao vien, phong, ca, trang thai.
- Content: `TableShell` schedule list, optional calendar summary.
- Detail/action panel: form uses fields `maKhoaHoc`, `thuTrongTuan`, `maCaHoc`, `maPhong`, date range, status.
- Empty/loading/error: standard.
- Responsive: table -> cards; form drawer full width mobile.
- Light/dark: no print HTML with hardcoded color.
- Components: `GlassPanel`, `GlassInput`, `GlassButton`, `GlassBadge`, `TableShell`.
- Data source: existing `useSchedule` only temporary; should not show direct teacher/class input in final UI.
- Actions/state: create/update/cancel/publish/generate mock state.
- Accessibility: form labels, inline errors.

### 14. Staff Conflict Checker

- Role: AcademicStaff.
- Route: `/staff/conflicts`.
- Muc tieu: check xung dot teacher/class/room before save.
- User job: nhap KhoaHoc + thu + ca + phong, xem conflicts.
- Layout: check form left, conflict result right/table below.
- Header: staff layout title.
- Primary action: `Kiem tra xung dot`.
- Filters: conflict type, severity.
- Content: conflict cards/table with teacher/class/room type.
- Detail panel: suggested resolution.
- Empty/loading/error: standard.
- Responsive: form/result stack.
- Light/dark: token-only.
- Components: `GlassPanel`, `GlassInput`, `GlassButton`, `GlassBadge`, `TableShell`.
- Data source: temporary mock matching P0-3 response shape.
- Actions/state: local check, clear, apply suggestion mock.
- Accessibility: errors linked to fields.

### 15. BGH Schedule Review

- Role: Principal.
- Routes: `/bgh/schedule/pending`, `/conflicts`, `/published`, `/changes`.
- Muc tieu: giam sat/danh gia TKB, khong nhap lieu chi tiet.
- User job: xem pending, conflict, published, changes.
- Layout: shared tabs/header summary + route-specific table/detail.
- Header: BGH layout title.
- Primary action: approve/return only in pending if business allows.
- Filters: campus, semester, department, status.
- Content: `TableShell` + detail panel.
- Empty/loading/error: standard.
- Responsive: cards mobile.
- Light/dark: token-only.
- Components: `GlassPanel`, `GlassBadge`, `GlassButton`, `TableShell`.
- Data source: existing inline mocks can be consolidated later.
- Actions/state: approve/return local mock, view detail.
- Accessibility: table headers and action labels.

## Rework Order

1. Create/confirm shared status/date helpers and minimal schedule/attendance data source.
2. Rework Student schedule + attendance.
3. Rework Teacher attendance + history.
4. Rework Student requests and admin queue surfaces.
5. Rework Notification inbox/compose/template placeholder.
6. Rework Student reward/discipline routes/views and SuperAdmin awards/discipline.
7. QA light/dark desktop/mobile on required routes.
8. Build, lint, unit tests.

## Out Of Scope For UI Rework

- Backend API changes.
- New backend services.
- New frontend API integration beyond existing services.
- PDF generation.
- Real upload/download storage flow if not already wired.
- Adding third-party UI library unless a later implementation step proves native/Glass components insufficient.
