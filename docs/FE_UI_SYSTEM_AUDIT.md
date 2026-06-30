# FE UI System Audit

Branch audit: `feature/fe-ui-system-rework-audit-first`

Luu y quan trong: audit nay duoc thuc hien tren `main` moi nhat. Dirty FE work o branch cu `feature/fe-mock-modules-polish-batch` da duoc stash voi message `preserve fe mock polish before ui system rework`; khong duoc xem la source hop le cho branch rework neu chua restore co chu dich.

## 1. Current UI system

### Stack

- Vue 3 + Vite.
- Vue Router, Pinia.
- Tailwind CSS v4.
- `lucide-vue-next`.
- `dayjs`.
- Editor.js packages da co san cho notification/application rich content.
- Khong co Bootstrap/Element Plus/Vuetify.

### Style tokens

Nguon chinh:

- `frontend/src/assets/main.css`
- `frontend/src/assets/liquid-glass.css`
- `frontend/src/assets/base.css`

Token/lop bat buoc dung lai:

- Surface: `surface-card`, `surface-input`, `surface-modal`, `surface-table`, `surface-table-header`, `surface-solid`.
- Text: `text-heading`, `text-body`, `text-label`, `text-muted`, `text-placeholder`, `text-link`.
- Border: `border-default`, `border-card`, `border-input`, `border-table`.
- Glass: `lg-glass`, `lg-glass-soft`, `lg-glass-strong`, `lg-readable`, `lg-solid-soft`, `lg-card`, `lg-card-hover`.
- Controls: `lg-input`, `lg-control`, `lg-button-primary`, `lg-button-secondary`, `lg-button-ghost`, `lg-button-subtle`, `lg-button-danger`, `lg-icon-button`, `lg-badge`.
- Typography/icon utilities: `ui-page-title`, `ui-section-title`, `ui-card-title`, `ui-body`, `ui-caption`, `ui-label`, `ui-icon-*`.

Light/dark:

- Default la light mode.
- Dark mode toggle qua `stores/theme.js`, class `dark` tren `document.documentElement`.
- Token dark da co cho surfaces, text, borders, semantic colors.
- Rui ro hien co: mot so view dung `bg-white`, `text-slate-*`, `border-slate-*`, `bg-black/40`, inline `#ddd`, `#fff`, `bg-gradient-to-br` co the doc duoc nhung khong dong bo token.

### UI core components

Bat buoc dung lai:

- `GlassButton.vue`: variants `primary`, `secondary`, `ghost`, `subtle`, `danger`, `success`; co loading.
- `GlassBadge.vue`: variants `neutral`, `primary`, `success`, `warning`, `danger`, `info`, `violet`.
- `GlassPanel.vue`: variants `default`, `strong`, `soft`, `flat`, `surface`, `solid`, `readable`.
- `GlassInput.vue`: label, error, aria-invalid.
- `TableShell.vue`: wrapper table, density, sticky header.
- `LoadingSkeleton.vue`, `ProgressBar.vue`, `EmptyState.vue`, `PopupNotification.vue`.
- `ThemeToggle.vue` va `stores/theme.js` cho light/dark.

Khong nen tao moi:

- `AppButton`, `AppCard`, `AppBadge`, component button/card rieng.
- CSS global moi cho module neu `liquid-glass.css` da co token/lop tuong duong.

### Layout by role

Student:

- Layout: `frontend/src/components/SinhVien/Layout_SinhVien.vue`.
- Uses: `AppSidebar`, `AppTopbar`, `PageContainer`, `AnnouncementBanner`, `AiAssistant`.
- Menu: `frontend/src/components/SinhVien/data/menuData.js`.
- Accent: blue/cyan.
- Issue: menu chua co `/student/rewards` va `/student/discipline` tren `main` du route can duoc can nhac them sau blueprint/code phase neu route ton tai.

Teacher:

- Layout: `frontend/src/components/GiangVien/Layout_GiangVien.vue`.
- Reuses student `AppTopbar` and `PageContainer`.
- Menu: `frontend/src/components/GiangVien/data/menuData.js`.
- Routes: `/teacher/attendance`, `/teacher/attendance-history`, `/teacher/requests`, `/teacher/notifications`.
- Issue: `/teacher/notifications` dang reuse `views/Student/NotificationsView.vue`, context role chua dung.

Staff/Giao vu:

- Layout: `frontend/src/components/GiaoVu/Layout_GiaoVu.vue`.
- Menu: `frontend/src/components/GiaoVu/data/menuData.js`.
- Routes: `/staff/schedule`, `/staff/conflicts`, `/staff/rooms`, `/staff/notices/send`, `/staff/notices/history`.
- Issue: staff requests/approvals route chua co trong router main, nhung co folder `views/GiaoVu/Requests/*`.

BGH:

- Layout: `frontend/src/components/BGH/Layout_BGH.vue`.
- Menu: `frontend/src/components/BGH/data/menuData.js`.
- Routes lien quan: `/bgh/schedule/pending`, `/bgh/schedule/conflicts`, `/bgh/schedule/published`, `/bgh/schedule/changes`, `/bgh/notifications`.
- Issue: `/bgh/notifications` reuse Student notification view.

SuperAdmin:

- Layout: `frontend/src/components/SuperAdmin/Layout_SuperAdmin.vue`.
- Menu: `frontend/src/components/SuperAdmin/data/menuData.js`.
- Routes lien quan: `/super-admin/approvals/requests`, `/super-admin/approvals/history`, `/super-admin/approvals/reports`, `/super-admin/awards`, `/super-admin/discipline`, `/super-admin/rewards-discipline`, `/super-admin/rewards/campaigns`, `/super-admin/discipline/records`, `/super-admin/discipline/appeals`, `/super-admin/notifications/send`, `/super-admin/notifications/templates`.
- Issue: mot so route dung placeholder hoac view cu style lech; `Layout_SuperAdmin` con hardcode scrollbar `#cbd5e1/#94a3b8`.

### Router inventory

Student route co that:

- `/student/schedule` -> `views/Student/ScheduleView.vue`
- `/student/attendance` -> `views/Student/AttendanceView.vue`
- `/student/requests` -> `views/Student/RequestsView.vue`
- `/student/notifications` -> `views/Student/NotificationsView.vue`
- Chua co route `/student/rewards` va `/student/discipline` tren `main`, du co views `Student/RewardsView.vue`, `Student/DisciplineView.vue`.

Teacher route co that:

- `/teacher/attendance` -> `views/GiangVien/AttendanceTodayView.vue`
- `/teacher/attendance-history` -> `views/GiangVien/AttendanceHistoryView.vue`
- `/teacher/requests` -> `views/GiangVien/PendingRequestsView.vue`
- `/teacher/requests-history` -> `views/GiangVien/RequestsHistoryView.vue`
- `/teacher/notifications` -> `views/Student/NotificationsView.vue`

Staff route co that:

- `/staff/schedule` -> `views/GiaoVu/Schedule/ScheduleManagerView.vue`
- `/staff/conflicts` -> `views/GiaoVu/Schedule/ConflictCheckView.vue`
- `/staff/rooms` -> `views/GiaoVu/Schedule/RoomManagementView.vue`
- `/staff/notices/send` -> `views/GiaoVu/Notices/SendNoticeView.vue`
- `/staff/notices/history` -> `views/GiaoVu/Notices/NoticeHistoryView.vue`

BGH route co that:

- `/bgh/schedule/pending` -> `views/BGH/SchedulePendingView.vue`
- `/bgh/schedule/conflicts` -> `views/BGH/Schedule/ConflictListView.vue`
- `/bgh/schedule/published` -> `views/BGH/Schedule/PublishedSchedulesView.vue`
- `/bgh/schedule/changes` -> `views/BGH/Schedule/ScheduleChangesView.vue`
- `/bgh/notifications` -> `views/Student/NotificationsView.vue`

SuperAdmin route co that:

- `/super-admin/approvals/requests` -> `views/SuperAdmin/ApprovalsRequestsView.vue`
- `/super-admin/approvals/history` -> `views/SuperAdmin/ApprovalsHistoryView.vue`
- `/super-admin/approvals/reports` -> `views/SuperAdmin/ApplicationReportsView.vue`
- `/super-admin/awards` -> `views/SuperAdmin/AwardsView.vue`
- `/super-admin/discipline` -> `views/SuperAdmin/DisciplineView.vue`
- `/super-admin/rewards-discipline` -> `views/SuperAdmin/RewardDisciplineView.vue`
- `/super-admin/rewards/campaigns` -> `views/SuperAdmin/RewardCampaignsView.vue`
- `/super-admin/discipline/records` -> `views/SuperAdmin/DisciplineRecordsView.vue`
- `/super-admin/discipline/appeals` -> `views/SuperAdmin/DisciplineAppealsView.vue`
- `/super-admin/notifications/send` -> `views/SuperAdmin/SendNotificationView.vue`
- `/super-admin/notifications/templates` -> `views/SuperAdmin/NotificationTemplatesView.vue`

### Data/mock/services inventory on main

Existing:

- `frontend/src/data/studentData.mock.js`: large student profile/courses/grades/exams/dashboard dataset.
- `frontend/src/mocks/rewardDisciplineMockData.js`
- `frontend/src/mocks/rewardDisciplineMockService.js`
- `frontend/src/services/applicationsApi.js`: real API client for applications.
- `frontend/src/services/notificationsApi.js`: real API client for notification center.
- `frontend/src/composables/useSchedule.js`: inline module-level schedule mock events and CRUD helpers.
- `frontend/src/stores/auth.js`: includes dev mock login for student/teacher/staff/bgh/admin/parent and syncs `studentData.mock.js`.
- `frontend/src/stores/popup.js`: popup/toast store.

Missing on main:

- `frontend/src/mocks/scheduleAttendanceMockData.js`
- `frontend/src/mocks/scheduleAttendanceMockService.js`
- `frontend/src/mocks/applicationsMockData.js`
- `frontend/src/mocks/applicationsMockService.js`
- `frontend/src/mocks/notificationsMockData.js`
- `frontend/src/mocks/notificationsMockService.js`

These existed only in dirty/stashed branch context, not in `main`.

## 2. Current bad UI diagnosis

### `/student/schedule`

- Visual: custom CSS calendar is better than raw table but not using `GlassPanel`, `GlassButton`, `GlassBadge`; header duplicates layout `PageContainer` title.
- UX: subject-only filter; no clear today summary or status explanation for room change/teacher substitute/shift change.
- Light/dark: mostly tokenized, but modal/button classes are custom and need dark QA.
- Route/layout: correct Student layout.
- Role/context: student only, OK.
- Data: generated from `studentDashboardMock.courses`; lacks actual `BuoiHoc` fields like `LoaiThayDoi`, `LyDoThayDoi`, `TrangThaiBuoi`.
- Component/style: raw select/button classes; badge labels `Published`, `Cancelled`, `Makeup`, `Online` are raw English.

### `/student/attendance`

- Visual: uses `TableShell`, `GlassBadge`, `GlassButton`, token surfaces; acceptable base.
- UX: has AI risk copy that may look speculative; explanation workflow in student attendance should be application/requests flow, not free-floating modal if backend not connected.
- Light/dark: mostly tokenized; inline style acceptable but should consolidate.
- Route/layout: correct Student layout.
- Role/context: student-only summary/history fits.
- Data: inline computed mock based on current view; should move to stable data source or derive from schedule/attendance data.
- Component/style: good table shell, but select controls should use `GlassInput` or consistent control wrapper.

### `/student/requests`

- Visual: delegates to `StudentApplicationsHome`, but that component calls real API immediately.
- UX: missing offline/mock fallback; no guaranteed multi-step create flow visible if API unavailable.
- Light/dark: components are mostly tokenized, but need verify.
- Route/layout: correct Student layout.
- Role/context: correct.
- Data: uses `applicationsApi` real endpoints, not mock-only. This is risky if FE phase is supposed to be UI-first/offline.
- Component/style: application components exist and should be reused, but status keys like `YEU_CAU_BO_SUNG` need label mapping everywhere.

### `/student/notifications`

- Visual: split pane is useful but current wrapper has `bg-[var(--surface-page)]`, fixed `h-[600px]`; detail pane can feel empty/boxed.
- UX: filters only all/unread/urgent/search; requested categories include hoc vu/tai chinh.
- Light/dark: uses tokens but detail panel uses `v-html`; prose styling may drift.
- Route/layout: correct Student layout; also reused by Teacher/BGH, which is wrong context.
- Role/context: student copy/header only; teacher/BGH need role-aware labels.
- Data: calls real `notificationsApi`; no mock fallback on main.
- Component/style: `v-html` on `notification.noiDung` is prohibited for user input unless sanitized; must render safe text/blocks.

### `/student/rewards`

- Visual: component exists but route/menu missing on main.
- UX: `StudentRewardDashboard` uses reward mock; hero/list acceptable start but needs certificate preview/download state.
- Light/dark: some components use `text-[var(...)]`, OK; needs remove overly decorative glow if it feels celebratory template.
- Route/layout: view exists but router lacks route.
- Role/context: student reward context correct.
- Data: uses `rewardDisciplineMockService`.
- Component/style: status badge components exist; should map all statuses to Vietnamese.

### `/student/discipline`

- Visual: view adds `p-6 h-full bg-[var(--surface-page)] overflow-y-auto`, creating nested page shell inside Student layout.
- UX: list/detail/appeal flow exists, but needs more private/serious presentation and avoid exposing sensitive/internal data.
- Light/dark: surface token used, but outer bg can create flat nested panel.
- Route/layout: view exists but router lacks route.
- Role/context: correct if routed.
- Data: uses `rewardDisciplineMockService`.
- Component/style: should remove extra shell and rely on `PageContainer`.

### `/teacher/attendance`

- Visual: basic panels/table look cleaner than old raw table, but data is inline and only binary present/absent.
- UX: missing required statuses `co_mat`, `vang`, `di_muon`, `co_phep`; no session states `chua_mo`, `dang_diem_danh`, `da_gui`, `da_khoa`; no sticky timer/lock/request unlock.
- Light/dark: tokenized CSS; OK base.
- Route/layout: correct Teacher layout.
- Role/context: teacher attendance only.
- Data: inline `todayClasses` and `students`.
- Component/style: uses core components; should keep pattern but rework logic.

### `/teacher/attendance-history`

- Visual: detailed modal/table exists but large inline mock makes view hard to maintain.
- UX: has history/detail/edit mock, but unlock request flow should be clearer and aligned with P0-7.
- Light/dark: mostly tokens.
- Route/layout: correct.
- Data: inline history and students per session.
- Component/style: should extract data/adapters only if necessary.

### `/teacher/requests`

- Visual: decent split-pane pattern.
- UX: uses raw `Pending` badge in visible UI; teacher request scope unclear versus admin application queue.
- Light/dark: tokenized base.
- Route/layout: correct.
- Data: inline request array.
- Component/style: uses `GlassPanel`, `GlassBadge`, `GlassButton`; good base.

### `/teacher/notifications`

- Visual/UX: reuses Student notification view; wrong page context.
- Light/dark: same risks as student notifications.
- Route/layout: Teacher layout but student content component.
- Data: real `notificationsApi`.
- Component/style: needs role-aware wrapper or shared component props.

### `/staff/schedule`

- Visual: uses `PageContainer` inside `Layout_GiaoVu`, likely nested page header; custom buttons instead of `GlassButton`.
- UX: creates schedule by direct subject/teacher/room/time, not aligned with backend model `ThoiKhoaBieu = KhoaHoc + ThuTrongTuan + CaHoc + PhongHoc`.
- Light/dark: has raw print HTML with `#ddd`, `white`; PDF export opens printed doc outside token system.
- Route/layout: correct.
- Data: `useSchedule` inline mock with hardcoded hex colors.
- Component/style: `MonthView` and custom controls; needs alignment and no raw print UI in this phase.

### `/staff/conflicts`

- Visual: table and KPI use tokens but not `TableShell` component consistently.
- UX: likely demo conflicts rather than P0-3 conflict request shape.
- Light/dark: mostly tokens but should check.
- Route/layout: correct.
- Data: inline.
- Component/style: raw table classes.

### `/staff/notices/send`

- Visual/UX: older notice send view likely separate from Notification Center components; should unify with `AdminNotificationCenter`/compose components.
- Light/dark: must inspect during code phase.
- Route/layout: correct.
- Data: likely service/mock mixed.
- Component/style: should reuse `NotificationComposeForm` or new role-aware wrapper.

### `/bgh/schedule/pending`

- Visual: separate view plus BGH schedule folder creates duplicate patterns.
- UX: approval workflow exists visually but should align with pending/published/changes routes.
- Light/dark: check.
- Route/layout: correct.
- Data: inline and `useSchedule`.
- Component/style: mix custom cards/table.

### `/bgh/schedule/conflicts`

- Visual: table with many custom classes; acceptable direction but needs `TableShell`.
- UX: BGH should review conflicts, not mutate staff actions casually.
- Light/dark: token mostly.
- Data: inline.
- Component/style: raw actions.

### `/bgh/schedule/published`

- Visual: read-only table but raw status `online`, Join Link English.
- UX: lacks summary and change explanation.
- Light/dark: tokens mostly.
- Data: inline.
- Component/style: raw table instead of `TableShell`.

### `/bgh/schedule/changes`

- Visual: dense table, semantic warning block OK.
- UX: should show change types: doi_phong, doi_ca, day_thay, huy.
- Light/dark: token mostly.
- Data: inline.
- Component/style: raw table.

### `/super-admin/awards`

- Visual: creates its own `lg-app-bg` inside SuperAdmin layout, causing nested shell/orbs.
- UX: generic award CRUD, not Top 100 campaign/candidate/certificate flow.
- Light/dark: many hardcoded Tailwind colors (`bg-sky-500/10`, `text-sky-500`, gradient amber/orange).
- Route/layout: correct route, wrong nested shell.
- Data: large inline mock.
- Component/style: custom toast, raw buttons (`lg-btn-*`) instead of `GlassButton`.

### `/super-admin/discipline`

- Visual: nested `lg-app-bg`, own toast, heavy red/rose styling.
- UX: generic discipline decision form, not aligned with DL1-DL3 lifecycle/appeal privacy.
- Light/dark: many hardcoded colors.
- Route/layout: correct route, wrong nested shell.
- Data: inline mock.
- Component/style: custom modal/toast; should use existing reward-discipline components.

### `/super-admin/notifications/send`

- Visual: delegates to `AdminNotificationCenter`; better than custom view.
- UX: uses `confirm()` in `AdminNotificationCenter`, prohibited.
- Light/dark: wrapper creates extra `bg-[var(--surface-page)]`; mostly OK.
- Route/layout: correct.
- Data: real `notificationsApi`.
- Component/style: should replace `confirm()` with Liquid Glass confirm modal/popup.

### `/super-admin/notifications/templates`

- Visual: placeholder panel.
- UX: no template management despite backend service/client.
- Light/dark: tokenized but sparse.
- Route/layout: correct.
- Data: no real or mock data.
- Component/style: empty placeholder needs real list/table/empty/error states or explicitly out of phase.

## 3. Business coverage matrix

| Module | Role | Route hiện có | View hiện có | Chức năng cần có | Component cần có | Data source hiện có | Gap cần sửa |
|---|---|---|---|---|---|---|---|
| Schedule + Attendance | Student | `/student/schedule` | `Student/ScheduleView.vue` | Week grid/list, today summary, change badges | `GlassPanel`, `GlassBadge`, `GlassButton`, week grid component | `studentData.mock.js`, inline derived sessions | Move data out of view, translate statuses, show room/teacher/shift changes |
| Schedule + Attendance | Student | `/student/attendance` | `Student/AttendanceView.vue` | Attendance summary/history/filter/explanation | `TableShell`, `GlassBadge`, `ProgressBar`, `EmptyState` | inline/current computed mock | Align status keys and remove speculative AI copy if not backed |
| Schedule + Attendance | Teacher | `/teacher/attendance` | `GiangVien/AttendanceTodayView.vue` | Open attendance, quick mark, bulk, submit, lock state | `GlassPanel`, `TableShell`, status segmented buttons | inline mock | Add all statuses and session state machine |
| Schedule + Attendance | Teacher | `/teacher/attendance-history` | `GiangVien/AttendanceHistoryView.vue` | History filters, unlock request list/modal | `GlassPanel`, `GlassInput`, `GlassBadge`, modal | inline mock | Simplify and align with unlock statuses |
| Schedule + Attendance | Staff | `/staff/schedule`, `/staff/conflicts`, `/staff/rooms` | GiaoVu schedule views | CRUD TKB, conflict check, shifts, generate sessions, adjustments | `TableShell`, action panel, conflict card | `useSchedule.js` inline mock | Backend model mismatch; raw tables; print/export out of scope |
| Schedule + Attendance | BGH | `/bgh/schedule/*` | BGH schedule views | Pending/published/conflicts/changes overview | `TableShell`, detail panel, read-only audit timeline | inline mock | Duplicate patterns, raw status, not enough change semantics |
| Applications | Student | `/student/requests` | `Student/RequestsView.vue` | List, filter, create wizard, detail, evidence, timeline | Existing application components + `GlassPanel` | `applicationsApi` real | No mock fallback on main; wizard visibility uncertain |
| Applications | Teacher | `/teacher/requests` | `GiangVien/PendingRequestsView.vue` | Teacher related requests inbox | `GlassPanel`, split pane | inline mock | Raw `Pending`, unclear business mapping |
| Applications | Staff/SuperAdmin | `/super-admin/approvals/requests`, `/approvals/history`, `/approvals/reports` | SuperAdmin application views/components | Queue, receive, assign, review, process, reports | Admin application components | `applicationsApi` real | Need role-aware empty/loading/error and no raw status |
| Notifications | Student | `/student/notifications` | `Student/NotificationsView.vue` + components | Inbox, unread, filters, detail, mark read | Notification components | `notificationsApi` real | No mock fallback; `v-html`; fixed height/sparse pane |
| Notifications | Teacher/BGH | `/teacher/notifications`, `/bgh/notifications` | Reuses student view | Role-aware inbox | Shared inbox with role props | `notificationsApi` real | Wrong context/copy |
| Notifications | SuperAdmin | `/super-admin/notifications/send`, `/templates` | AdminNotificationCenter, template placeholder | Compose, recipient preview, templates | Notification compose/list components | `notificationsApi` real | `confirm()`, sparse template route |
| Rewards | Student | View exists, route missing | `Student/RewardsView.vue` | Hero, cert list, preview/download | reward components | `rewardDisciplineMockService` | Need route/menu decision; preview/download state |
| Discipline | Student | View exists, route missing | `Student/DisciplineView.vue` | Private list, detail, appeal timeline | reward-discipline components | `rewardDisciplineMockService` | Nested shell, privacy copy, route/menu gap |
| Rewards/Discipline | SuperAdmin | `/super-admin/awards`, `/discipline`, `/rewards/*`, `/discipline/*` | Multiple views/components | Campaigns, candidates, certificates, records, appeals, reports | Existing reward-discipline components + `TableShell` | `rewardDisciplineMockService` plus inline mock | Awards/Discipline views duplicate shell and inline data |

## 4. Mock data policy

Use existing first:

- Student academic/profile/course context: `frontend/src/data/studentData.mock.js`.
- Reward/discipline: `frontend/src/mocks/rewardDisciplineMockData.js` and `rewardDisciplineMockService.js`.
- Schedule staff calendar base: `frontend/src/composables/useSchedule.js` can be kept short-term but should be refactored to token-safe data without hardcoded hex.
- Auth personas: `stores/auth.js` dev mock users.

Do not add large mocks unless:

- A route has no viable source on main and needs meaningful UI rendering.
- The field is required by visible UI and cannot be derived from existing data.
- The data is placed in one module-scoped mock file/service, not duplicated in every component.

Likely minimal additions during code phase:

- One schedule/attendance mock adapter may be needed because main lacks `scheduleAttendanceMockData.js`.
- One notification mock adapter may be needed only if UI must run offline without backend; otherwise use `notificationsApi` and provide loading/error states.
- Application mock fallback may be needed only if product decision is mock-first; otherwise keep `applicationsApi`.

Remove/avoid:

- Large inline arrays in views when there is a module component/service.
- Separate fake service per single screen.
- Raw English status keys in UI.

## 5. Rework plan

Keep:

- Role layouts and router shell.
- `components/ui/*` as design system.
- `components/applications/*` as base for request UI.
- `components/notifications/*` as base for inbox/compose UI.
- `components/reward-discipline/*` as base for reward/discipline UI.
- `stores/popup.js`, `stores/theme.js`, `stores/auth.js`.

Modify:

- `Student/ScheduleView.vue`, `Student/AttendanceView.vue`, `Student/RequestsView.vue`, `Student/NotificationsView.vue`, `Student/RewardsView.vue`, `Student/DisciplineView.vue`.
- `GiangVien/AttendanceTodayView.vue`, `GiangVien/AttendanceHistoryView.vue`, `GiangVien/PendingRequestsView.vue`; add/repoint teacher notification view only if route context requires.
- `GiaoVu/Schedule/*`, `GiaoVu/Notices/*` where in scope.
- `BGH/Schedule*`.
- `SuperAdmin/AwardsView.vue`, `SuperAdmin/DisciplineView.vue`, `SuperAdmin/SendNotificationView.vue`, `SuperAdmin/NotificationTemplatesView.vue`, application approval views if in scope.
- Module components to remove `v-html`, `confirm()`, raw status/date, custom toasts.

Possible add:

- A shared `StatusLabel`/status-map helper per module, or simple local status maps in component folders.
- A single `frontend/src/mocks/scheduleAttendanceMockData.js` only if needed for schedule/attendance rework.
- A role-aware notification wrapper component if `NotificationInbox` needs context props.

Route decisions:

- Keep all existing paths.
- Do not duplicate route names.
- Add `/student/rewards` and `/student/discipline` only if menu/product scope confirms these student views should be reachable in this rework.
- Do not add demo-only routes.

Library decision:

- No new library required for audit/blueprint.
- Code phase should first use native controls + existing Glass components.
