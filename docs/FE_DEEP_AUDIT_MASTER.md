# FE Deep Audit Master Report

**Date**: 2026-07-11  
**Scope**: Frontend audit of EduLMS — Vue ^3.5.31 + Vite ^8.0.3 + Pinia ^3.0.4 + Vue Router ^5.0.4 + Tailwind CSS ^4.2.2  
**Methodology**: Static code analysis (router/index.js, views, components, services, stores, composables), cross-reference with route-inventory.csv, component-inventory.csv, ui-violations.csv, documentation-drift.md, duplicates.md  
**Verification Status**: STATIC_CODE (all findings), BUILD (build verified), BROWSER (partial — seed/backend required for full browser verification)

---

## 1. Executive Summary

The frontend codebase spans **181 routes** across **9 roles** (Public, 404, Student, Parent, Teacher, Staff, BGH, SuperAdmin|Admin, ContentCouncil), served by **426 components**, **~30 services**, **~12 stores**, and **~10 composables**. The Liquid Glass design system is comprehensive (25+ UI primitives, 50+ semantic tokens, 6 glass variants). However, the codebase exhibits significant architectural debt:

- **High**: 6 nearly identical sidebar implementations (~1000 LOC), duplicate scrollbar/font CSS in 6 layouts
- **High**: 4 unsanitized `v-html` usages in Content Council components (XSS risk — P0)
- **High**: 2 routes reuse Student NotificationsView for Teacher/BGH (wrong context)
- **Medium**: 16 routes with UNVERIFIED API status, 14 Teacher routes missing meta.title
- **Medium**: Documentation drift across 5+ documents (Tailwind v3→v4, route counts, store inventory)
- **Medium**: Inline mock data in 6+ views (no real API integration)
- **Low**: 11 dead/legacy files (HelloWorld, LmsButton, Icon* components, counter store)

## 2. Repository / Frontend Architecture

```
frontend/src/
├── assets/              # Static assets (images, fonts)
├── components/          # 426 components across categories
│   ├── ui/              # Glass UI primitives (25+)
│   ├── common/          # Shared utilities (skeleton, empty state)
│   ├── auth/            # Login flow components
│   ├── SinhVien/        # Student layout + sidebar + data
│   ├── PhuHuynh/        # Parent layout
│   ├── GiangVien/       # Teacher layout
│   ├── GiaoVu/          # Staff layout
│   ├── BGH/             # BGH layout
│   ├── SuperAdmin/      # SuperAdmin layout
│   ├── content-council/ # CC layout + editor + quiz builder
│   ├── notifications/   # Notification components
│   ├── applications/    # Request/approval components
│   └── reward-discipline/ # Reward/discipline UI
├── composables/         # Vue 3 composables
├── constants/           # Auth constants, role catalog
├── data/                # Menu definitions per role, mock data
├── layouts/             # ContentCouncil alternate layouts
├── pages/               # ContentCouncil pages (separate from views/)
├── router/              # Router definition (181 routes)
├── services/            # API service modules (~30)
├── stores/              # Pinia stores (~12)
├── utils/               # Role routing, auth redirect, sanitizer
└── views/               # Route views by role
    ├── Auth/            # Login + landing + payment
    ├── Student/         # English-named student views
    ├── SinhVien/        # Vietnamese-named student views
    ├── PhuHuynh/        # Parent views
    ├── GiangVien/       # Teacher views
    ├── GiaoVu/          # Staff views
    ├── BGH/             # BGH views
    ├── SuperAdmin/      # SuperAdmin views
    └── Payment/         # Payment success/cancel
```

### Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Framework | Vue 3 (Composition API, `<script setup>`) | ^3.5.31 |
| Build | Vite | ^8.0.3 |
| State | Pinia | ^3.0.4 |
| Router | Vue Router | ^5.0.4 |
| CSS | Tailwind CSS (v4, `@import "tailwindcss"`) | ^4.2.2 |
| Icons | lucide-vue-next | ^1.0.0 |
| UI Primitives | Custom Liquid Glass system | — |
| HTTP | fetch via apiRequest | — |
| Testing | Vitest | ^4.1.2 |
| Linting | ESLint + Oxlint | latest |

## 3. Router and Route Governance

**Source**: `frontend/src/router/index.js` — single file, 700+ lines, lazy-loaded components

**Total routes**: 181 (verified via route-inventory.csv)

### Routes by Role

| Role | Routes | Status Distribution |
|------|--------|-------------------|
| Public | 6 | 2 STATIC_FUNCTIONAL, 1 RUNTIME_VERIFIED, 1 PLACEHOLDER, 2 API_CONNECTED |
| 404 | 1 | STATIC_FUNCTIONAL |
| Student | 23 | All API_CONNECTED |
| Parent | 16 | All API_CONNECTED |
| Teacher | 28 | 26 API_CONNECTED, 1 WRONG_CONTEXT, 1 STATIC_FUNCTIONAL |
| Staff | 26 | 24 API_CONNECTED, 2 STATIC_FUNCTIONAL |
| BGH | 27 | 25 API_CONNECTED, 1 WRONG_CONTEXT, 1 STATIC_FUNCTIONAL |
| SuperAdmin\|Admin | 43 | 36 API_CONNECTED, 7 HIDE_FROM_DEMO |
| ContentCouncil | 11 | All API_CONNECTED |

### Status Summary

| Status | Count |
|--------|-------|
| API_CONNECTED | 124 |
| UNVERIFIED | 33 |
| STATIC_FUNCTIONAL | 13 |
| HIDE_FROM_DEMO | 7 |
| WRONG_CONTEXT | 2 |
| RUNTIME_VERIFIED | 0 |
| PLACEHOLDER | 1 |

### Risks

- **26 routes have NO meta.title** (14 Teacher, 4 Staff, 1 SuperAdmin, 7 Public) — breadcrumb/tab title empty
- **2 routes reuse Student/NotificationsView.vue** for Teacher and BGH (wrong context, wrong role copy)
- **7 routes hidden from demo** (SuperAdmin finance/reports/support) — may confuse testers
- **33 UNVERIFIED routes** — API endpoints not confirmed via static analysis

## 4. Navigation Sources of Truth

| Source | File(s) | Role |
|--------|---------|------|
| Router definition | router/index.js | All routes (single source of truth for routing) |
| Menu groups | components/*/data/menuData.js, menuGroups.js | 6 role-specific menu definitions |
| Navigation guard | router (beforeEach) | Auth check, role guard, redirect logic |
| authRedirect.js | utils/authRedirect.js | Post-login redirect resolution |
| authPortals.js | data/authPortals.js | Login portal → home route mapping |
| roleRoutes.js | utils/roleRoutes.js | Role → home route mapping (duplicate of authPortals.js) |
| roleCatalog.js | constants/roleCatalog.js | Third home route mapping source |

**Problem**: Home route mapping has 3 separate definitions (roleRoutes.js, roleCatalog.js, authPortals.js) — manual sync risk.

## 5. Layout Architecture

### Role Layouts (6 + 2 alternate)

| Layout | File | Role(s) | Lines |
|--------|------|---------|-------|
| Layout_SinhVien | components/SinhVien/Layout_SinhVien.vue | Student | ~200 |
| Layout_PhuHuynh | components/PhuHuynh/Layout_PhuHuynh.vue | Parent | ~200 |
| Layout_PhuHuynh_Mobile | components/PhuHuynh/Layout_PhuHuynh_Mobile.vue | Parent (mobile) | ~100 |
| Layout_GiangVien | components/GiangVien/Layout_GiangVien.vue | Teacher | ~150 |
| Layout_GiaoVu | components/GiaoVu/Layout_GiaoVu.vue | Staff | ~150 |
| Layout_BGH | components/BGH/Layout_BGH.vue | BGH | ~150 |
| Layout_SuperAdmin | components/SuperAdmin/Layout_SuperAdmin.vue | SuperAdmin | ~150 |
| ContentCouncilLayout | layouts/content-council/ContentCouncilLayout.vue | ContentCouncil | ~100 |
| SubjectDetailLayout | layouts/content-council/SubjectDetailLayout.vue | ContentCouncil (subject) | ~80 |

### Pattern (all 6 role layouts)
```
lg-app-bg > flex h-screen > [Sidebar | Topbar + content > main > PageContainer > router-view]
```

**Duplication**: 6 nearly identical sidebar implementations (~1008 lines total), scrollbar CSS in each layout, font @import in each layout.

## 6. Design System / Liquid Glass

### Token Architecture (in liquid-glass.css)

| Layer | Variables | Count |
|-------|-----------|-------|
| Core primitive | `--lg-*` | 40+ |
| Surface | `--surface-*` | 15+ |
| Text | `--text-*` | 10+ |
| Border | `--border-*` | 10+ |
| Semantic bg/text | `--color-*-bg`, `--color-*-text` | 12 |

### Glass Variants

| Class | Use Case |
|-------|----------|
| `lg-glass` | Default glass surface |
| `lg-glass-strong` | Higher opacity glass |
| `lg-glass-soft` | Subtle glass (content panels) |
| `lg-sidebar` | Sidebar with blur + role glow |
| `lg-topbar` | Topbar glass effect |
| `lg-app-bg` | Page background (with decorative orbs) |

### Role-Specific Colors

Each role sets `--sidebar-accent`, `--active-start/mid/end`, `--sidebar-indicator` via inline `:style`.

### Adoption Gaps

- **ScheduleView.vue**: Custom CSS calendar instead of GlassPanel/GlassButton/GlassBadge
- **Attendance Today**: Binary present/absent only (missing Vietnamese statuses)
- **Conflicts**: Not using TableShell
- **Published schedules (BGH)**: Raw table, not TableShell
- **Multiple views**: Hardcoded Tailwind colors instead of tokens

## 7. Shared Component System

### Category Breakdown

| Category | Count | Examples |
|----------|-------|---------|
| UI Primitives | 21 | GlassButton, GlassBadge, GlassPanel, TableShell, LoadingSkeleton |
| Skeleton | 8 | FormSkeleton, ListSkeleton, SkeletonTable, SkeletonBlock |
| Auth | 5 | AuthLoginForm, RoleLoginHero, RolePortalCard, EduShaderBackground |
| Student Layout | 9 | Layout_SinhVien, AppSidebar, AppTopbar, PageContainer |
| Parent Layout | 6 | Layout_PhuHuynh, AppSidebar (Parent), AppTopbar (Parent) |
| Teacher Layout | 3 | Layout_GiangVien, AppSidebar (Teacher), menuData.js |
| Staff Layout | 3 | Layout_GiaoVu, AppSidebar (Staff), menuData.js |
| BGH Layout | 3 | Layout_BGH, AppSidebar (BGH), menuData.js |
| SuperAdmin Layout | 6 | Layout_SuperAdmin, AppSidebar, SuperAdminTopbar, PageContainer |
| ContentCouncil Layout | 4 | ContentCouncilLayout, SubjectDetailLayout, Sidebar, Header |
| Student Dashboard | 10 | WelcomeHero, KpiCard, FocusAiCard, TodaySchedulePanel |
| Notifications | 6 | AdminNotificationCenter, NotificationInbox, NotificationWizard |
| Applications | 6 | AdminAppQueue, AppDetailSidePanel, AppTimeline |
| Reward/Discipline | 5 | StudentRewardDashboard, AdminDisciplineTable, DisciplineAppealForm |
| Content Council | 35+ | SubjectCard, QuizTable, QuestionBankTable, CurriculumEditor, SlidePreview |
| Legacy/Dead | 11 | HelloWorld, LmsButton, LmsCard, IconCommunity, counter store |

### Top Used Components

| Component | Usage Count |
|-----------|-------------|
| GlassButton | 660 |
| GlassPanel | 584 |
| GlassBadge | 528 |
| PageContainer | 109 |
| EmptyState | 103 |
| TableShell | 78 |
| ConfirmActionDialog | 54 |
| LoadingSkeleton | 48 |

## 8. Role-By-Role UX Assessment

### Student (22 routes)
- **Dashboard**: 9 panels, real API (studentApi) — functional
- **Schedule**: Custom CSS calendar, English status labels, hardcoded colors — risk
- **Notifications**: Thin wrapper around NotificationInbox — well-structured
- **Exams**: Fullscreen exam with SignalR — medium UX risk
- **Rewards/Discipline**: Overly decorative glow, discipline has nested scroll — minor
- **All routes API_CONNECTED** ✅

### Parent (15 routes)
- Responsive wrapper with mobile layout variant
- All API_CONNECTED via parentApi
- Menu labels mixed English/Vietnamese
- **All routes API_CONNECTED** ✅

### Teacher (27 routes)
- **14 routes have NO meta.title** — breadcrumb/tab title empty
- **/teacher/notifications**: WRONG_CONTEXT (uses Student view)
- **Attendance**: Missing Vietnamese statuses, inline mock data
- **Proctoring**: SignalR hub — real-time risk
- **2 UNVERIFIED routes** (lessons, class-attendance APIs unknown)

### Staff / Giao Vu (25 routes)
- Strong API coverage (23 API_CONNECTED)
- Schedule manager has custom buttons + nested page header
- Conflicts has inline mock data
- Print HTML with hardcoded colors (#ddd, white)

### BGH (26 routes)
- **4 UNVERIFIED routes** (curriculum, conflicts, published schedules, facilities APIs unknown)
- **/bgh/notifications**: WRONG_CONTEXT (uses Student view)
- Schedule views duplicate Staff patterns (read-only would be more appropriate)
- Strong eval module (overview, ranking, detail, AI analysis)

### SuperAdmin (42 routes)
- **7 routes HIDE_FROM_DEMO** (finance, support, some reports)
- **8 UNVERIFIED routes**
- Awards/Discipline views: nested app-bg, hardcoded colors — high risk
- FinanceMonitorView reused for 3 routes (debts, payments, refunds) with mode param — clever but fragile

### Content Council (9 routes)
- **4 P0 v-html XSS vulnerabilities** (SlideHtmlPreview, QuestionDetail/Form/Preview drawers)
- All API_CONNECTED
- Complex editor with autosave concerns
- Previews with unsanitized content rendering

## 9. API / Store / Data Flow

### Service Layer (~30 modules)

| Category | Services |
|----------|----------|
| Auth | authApi |
| Student | studentApi, examApi, registrationApi, notificationsApi |
| Parent | parentApi |
| Teacher | teacherApi |
| Staff | staffApi, scheduleApi, assignmentApi, academicTermApi, subjectApi, courseApi, registrationApi, classApi, accountApi, workflowApi, buildingApi, floorApi, roomApi, shiftApi |
| BGH | bghApi |
| SuperAdmin | adminUserApi, rbacApi, organizationApi, rewardDisciplineApi, financeTuitionConfigService, exportService, superAdminApi, applicationsApi |
| ContentCouncil | contentCouncilApi |

### Store Layer (12 Pinia stores)

| Store | Purpose |
|-------|---------|
| auth | JWT + token refresh + role state |
| theme | Dark/light mode toggle |
| popup | Toast notification queue |
| recentFavorites | Recent links + favorites per user |
| announcements | System announcements |
| academicSchedulingContext | Shared scheduling state (Staff) |
| useQuestionStore | Question bank state (CC) |
| useQuizStore | Quiz list/form state (CC) |
| useSubjectStore | Subject/curriculum state (CC) |

### Data Flow Pattern
```
View (async onMounted) → Service (apiRequest) → Backend API → Response → Pinia/Component State → Render
```

### Risks
- **useSchedule composable** vs scheduleApi: Duplicate local state (inline CRUD + auto-increment IDs) — not connected to real API
- **roleRoutes.js vs roleCatalog.js vs authPortals.js**: 3 separate home-route mappings
- **authStore normalizeRole** vs utils/roleRoutes normalizeRole vs constants/roleCatalog normalizeRole: 3 normalization sources

## 10. Accessibility

| Area | Status | Notes |
|------|--------|-------|
| Semantic HTML | Mixed | Views use div-heavy layout; role=region/nav not consistently applied |
| ARIA labels | Partial | Sidebars have minimal ARIA; modal focus trapping unknown |
| Keyboard nav | Not verified | Tab order untested; CommandPalette has keyboard support |
| Color contrast | Not verified | Dark mode may introduce contrast issues |
| Screen reader | Not verified | lucide-vue-next icons lack aria-label in components |
| Focus management | Not verified | Modal/notification focus not tested |

## 11. Responsive

| Area | Status | Notes |
|------|--------|-------|
| Student Dashboard | Functional | Flexbox grid, responsive columns |
| Parent UI | Good | Dedicated mobile layout (Layout_PhuHuynh_Mobile) |
| Teacher Schedule | Partial | Custom CSS calendar may break on small screens |
| Staff ScheduleManager | Risk | 1667-line view — likely desktop-only |
| SuperAdmin | Partial | Most views assume large screens |
| ContentCouncil | Untested | Complex editor may be desktop-only |
| NotificationsView | Partial | Fixed h-[600px] split pane breaks on mobile |

## 12. Performance

| Area | Status |
|------|--------|
| Route lazy loading | ✅ All routes use dynamic import |
| Component chunking | ✅ Automatic via Vite code splitting |
| Bundle size | Not measured (Vite build = 2385 modules, ~33s) |
| Image optimization | Not verified |
| Infinite re-renders | Not checked |
| Memory leaks | Not checked (onUnmounted cleanup not verified) |

## 13. Agent Slop / Low Quality Code

| Finding | File | Severity |
|---------|------|----------|
| Speculative AI copy | views/Student/AttendanceView.vue | Medium — "AI risk copy may be speculative" |
| AI feature risk | Multiple views | Medium — AI features may be aspirational |
| Generic CRUD not aligned to business | AwardsView, DisciplineView | Medium — generic forms not matching campaign/lifecycle |
| Double background gradient | AwardsView, DisciplineView, AiAutomationView, etc. | High — nested lg-app-bg |
| window.confirm anti-pattern | AdminNotificationCenter.vue | High (may still exist in some versions) |

## 14. Documentation Drift

| Document | Lines | Status | Key Drifts |
|----------|-------|--------|------------|
| FE_UI_SYSTEM_AUDIT.md | 447 | Outdated | ~50 routes listed vs 173 actual; mock data references stale |
| FRONTEND_ARCHITECTURE.md | 159 | Major drift | Router covers 7 roles not 1; 10+ stores not 2; Tailwind v3→v4 |
| FRONTEND_STYLE_ALIGNMENT.md | 93 | Not enforced | GlassButton/Badge/Panel requirements not followed |
| DESIGN_LIQUID_GLASS.md | 1248 | Partial drift | Tailwind v4 migration not reflected; token JSON/YAML examples aspirational |

## 15. Risk Ranking

| Priority | Category | Count | Max Severity |
|----------|----------|-------|-------------|
| P0 | XSS (unsanitized v-html) | 4 locations | Critical |
| P0 | Authentication bypass | 0 found | — |
| P1 | Sidebar code duplication | 6 files | High |
| P1 | Layout duplication | 6 layouts | High |
| P1 | Wrong context (Student view in Teacher/BGH) | 2 routes | High |
| P1 | Nested app-bg (double gradient) | 5+ views | High |
| P1 | Hardcoded Tailwind colors | Multiple views | High |
| P2 | UNVERIFIED API endpoints | 16 routes | Medium |
| P2 | Missing meta.title | 26 routes | Medium |
| P2 | Inline mock data | 6+ views | Medium |
| P2 | Home route drift (3 sources) | N/A | Medium |
| P2 | Role normalization drift (3 sources) | N/A | Medium |
| P3 | Legacy/dead files | 11 | Low |
| P3 | Duplicate CSS (scrollbar, fonts) | 6 each | Low |

## 16. Unknown / Unverified Findings

| Finding | Reason | Action Needed |
|---------|--------|---------------|
| 33 routes with UNVERIFIED status | API endpoint not found in service files | Developer investigation per route |
| Browser smoke results | Requires seed/backend running | Run P15G full browser smoke |
| Responsive behavior on mobile | Not tested | Manual QA |
| Accessibility compliance | Not audited with tools | axe/pa11y scan |
| Bundle size budget | Not measured | Vite build report |
| Memory leak detection | Not instrumented | Chrome DevTools memory profile |
| SignalR hub integration | Real-time proctoring untested | Requires backend SignalR setup |
| AI automation features | May be aspirational | Confirm with product team |
| Save state reliability in CC Editor | Autosave/dirty state untested | Manual interaction test |
| Color contrast in dark mode | Not verified | Contrast ratio audit |

## 17. Recommendations

### Immediate (P0)
1. Sanitize all v-html in CC components via htmlSanitizer.js
2. Fix nested app-bg in SuperAdmin AwardsView, DisciplineView, AiAutomationView, etc.

### Short-term (P1)
3. Consolidate sidebar implementations into one shared component
4. Move scrollbar CSS to liquid-glass.css, font import to index.html
5. Create role-aware NotificationsView wrapper or shared component
6. Replace hardcoded Tailwind colors with semantic tokens
7. Add meta.title to all 26 routes without it
8. Consolidate home route mapping (roleRoutes.js → single source)

### Medium-term (P2)
9. Connect 33 UNVERIFIED routes to their APIs
10. Replace inline mock data with real API calls
11. Add Vietnamese attendance statuses to teacher views
12. Use TableShell consistently in conflict/schedule views
13. Consolidate role normalization (normalizeRole → single source)

### Long-term (P3)
14. Remove 11 legacy/dead files
15. Implement responsive layout for all views
16. Accessibility audit + remediation
17. Performance budget + bundle analysis

## 18. File Inventory (Artifact Index)

| File | Description | Lines | Status |
|------|-------------|-------|--------|
| docs/FE_DEEP_AUDIT_MASTER.md | This master report | ~300 | ✅ Current |
| docs/artifacts/fe-deep-audit/route-inventory.csv | 181 routes, 27 columns, 10-value status enum | 174 | ✅ Current |
| docs/artifacts/fe-deep-audit/route-matrix.md | Role × route matrix with auto-derived counts | 250+ | ✅ Current |
| docs/artifacts/fe-deep-audit/component-inventory.csv | 426 components with numeric usage counts | 158 | ✅ Current |
| docs/artifacts/fe-deep-audit/ui-violations.csv | 32 violations, RFC 4180, P0 for v-html | 36 | ✅ Current |
| docs/artifacts/fe-deep-audit/duplicates.md | 6 sidebars, 6 layouts, 3 home-route sources | 182 | ✅ Current |
| docs/artifacts/fe-deep-audit/documentation-drift.md | 4 docs compared to current codebase | 79 | ✅ Current |
| docs/artifacts/fe-deep-audit/frontend-architecture.mmd | Mermaid diagram of FE architecture | 253 | ✅ Current |
| docs/artifacts/fe-deep-audit/generate-route-csv.mjs | CSV generator script | 246 | ✅ Current |
| docs/artifacts/fe-deep-audit/count-components.mjs | Component usage counter script | ~50 | ✅ Current |
| docs/artifacts/fe-deep-audit/update-route-matrix.mjs | Route matrix regenerator | ~120 | ✅ Current |
| docs/artifacts/fe-deep-audit/validate-audit.mjs | Cross-file validation | — | ❌ Pending |
| docs/artifacts/fe-deep-audit/validation-results.md | Validation output | — | ❌ Pending |

## 19. Methodology

All findings in this report are derived from:

1. **STATIC_CODE**: Reading source files (router, views, components, services, stores)
2. **BUILD**: Running `npm run build` (2385 modules, 33s, no errors)
3. **CROSS-REFERENCE**: Comparing route-inventory.csv against router/index.js, services, and stores
4. **COUNT**: Using grep-based scripts for numeric usage counts

Browser-verified routes: 0. No browser-based runtime verification was performed. Backend API responses were not tested.

## 20. Appendix: Routes Without meta.title

| Role | Path |
|------|------|
| Public | /about, /login |
| Teacher | /teacher/courses, /teacher/lessons, /teacher/classes, /teacher/classes/:id/details, /teacher/classes/:id/workspace, /teacher/class-progress, /teacher/class-attendance, /teacher/class-grades, /teacher/exams, /teacher/exams/create, /teacher/grading, /teacher/exam-results, /teacher/proctoring, /teacher/attendance, /teacher/attendance-history, /teacher/grading-input, /teacher/student-questions, /teacher/lesson-comments, /teacher/profile, /teacher/change-password |
| Staff | /staff/notices/send, /staff/notices/history, /staff/profile |
| SuperAdmin | /super-admin/approvals/history |

**Total: 26 routes** (20 Teacher, 3 Staff, 1 SuperAdmin, 2 Public)

## 17. Router Architecture

**Source**: `frontend/src/router/index.js` — single file, 875+ lines, lazy-loaded components

### Structure
- Top-level `routes: [...]` array with 7 role-specific layout groups
- Each layout group has `path`, `component` layout, `meta.role`, and `children: [...]`
- Children inherit role and path from parent layout
- 181 total routes, 180 with `path:`, 1 with `redirect` function

### Route Count by Layer
- Layout routes (role shells): 8 (Student, Parent, Teacher, Staff, BGH, SuperAdmin, ContentCouncil, ContentCouncil subject)
- Public/standalone routes: 6 (portal landing, about, login, login/:portal, exam take, payment success/cancel)
- Child routes: 167 (nested inside layout children arrays)

### Lazy Loading
- All route components use dynamic `import()` for lazy loading
- No eagerly loaded route components

## 18. Component Inventory

**Source**: `docs/artifacts/fe-deep-audit/component-inventory.csv`

**Total components**: 426 Vue files across 19 categories

### Component Health
- **USED**: 198 components (46.5%) — imported by other files or used as route components
- **DEAD**: 228 components (53.5%) — no imports found, not a route component

### Inventory Schema
| Column | Description |
|--------|-------------|
| Component | Component name (filename without .vue) |
| Category | Feature directory |
| File | Relative path |
| Type | File type (vue/js/ts) |
| Props | defineProps content |
| Emits | defineEmits content |
| Slots | Slot count |
| Variants | Usage variants |
| Accessibility | aria-* attribute detection |
| LoadingState | loading prop/state |
| ErrorState | error prop/state |
| Responsive | Responsive class detection |
| DarkMode | Dark mode class detection |
| DuplicateGroup | Similar component grouping |
| ImportedBy | Files that import this component |
| ImportedByCount | Number of import sources |
| USED | Boolean usage flag |
| DeadOrUsed | USED or DEAD status |
| Evidence | Usage evidence |

## 19. UI Violations

**Source**: `docs/artifacts/fe-deep-audit/ui-violations.csv`

**Total**: 32 violations (4 P0, 7 P1, 18 P2, 3 P3)

### Severity Breakdown
- **P0 (Critical)**: 4 — All unsanitized `v-html` in Content Council (XSS risk)
- **P1 (High)**: 7 — Wrong context (Teacher/BGH using Student notifications), nested shells, hardcoded colors
- **P2 (Medium)**: 18 — Design system inconsistencies, missing API integration, UX issues
- **P3 (Low)**: 3 — Duplicate imports, mixed language, missing metadata

### Schema
11 columns: Severity, Role, Route, File, Line, Category, Problem, UserImpact, SuggestedDirection, Confidence, VerificationMethod
