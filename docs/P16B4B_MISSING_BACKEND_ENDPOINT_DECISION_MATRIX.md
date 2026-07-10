# P16B.4B — Missing Backend Endpoint Decision Matrix

> Date: 2026-07-10
> Base commit: `070eed8` P16B.4A triage FE_ONLY_STATIC: connect 9 existing APIs, reclassify 8 wrappers
> Method: Grep docs + scan 85 Backend controllers + component inspection
> Phase type: DOCS ONLY — no code change

---

## Worklist Count

| Status | Count | Source |
| --- | ---: | --- |
| `NEEDS_BE_ENDPOINT` from P16B.2 (SuperAdmin gaps) | 11 | `P16_FULL_SCREEN_ACTION_API_MATRIX.md` lines 257–267 |
| `NEEDS_BE_ENDPOINT` from P16B.4A (FE_ONLY_STATIC) | 7 | `P16B4A_FE_ONLY_STATIC_WORKLIST.md` |
| `REMOVE_FROM_100_PERCENT_CLAIM` from P16B.1 (Finance + Placeholder) | 7 | `P16B1_HIGH_RISK_WORKLIST.md` lines 32–38; `P16_FULL_SCREEN_ACTION_API_MATRIX.md` lines 75–77 |
| **Total rows in scope** | **25** | |

> Note: P16B.1 finance rows (`student-debts`, `payments`, `refunds`) already had `FinanceMonitorView` added as stopgap in P16B.1; the deeper "dedicated finance admin endpoint" remains missing. The Placeholder rows (`schedules/approval`, `finance/tuition-config`, `notifications/history`) are REMOVE_FROM_100_PERCENT_CLAIM from P16B.1 origin.

---

## Backend Capabilities Discovered

> Ran controller scan (`85 files`) before deciding. Key findings:

| Controller | Route prefix | Relevant to NEEDS rows |
| --- | --- | --- |
| `SuperAdminController.cs` | `api/super-admin` | Has `security/alerts`, `system/modules`, `dashboard/stats`, `dashboard/activities` |
| `ExamController.cs` | `api/exam` | Has `ky-thi` (exam period admin), `ca-thi` (exam session), `student/list`, `taking/*`, `grading/*`, `reports/summary` |
| `AttendanceController.cs` | `api` | Has `student/attendance`, `teacher/attendance/today`, `buoi-hoc/{id}/attendance` |
| `LearningProgressController.cs` | `api/learning-progress` | Has class-level student progress |
| `BghAcademicController.cs` | `api/bgh` | Has `academic/overview`, `gpa`, `at-risk`, `reports`, `pass-fail`, `schedule/changes` |
| `StudentSupportTicketsController.cs` | `api/student/support-tickets` | Has GET, GET/:id, POST, messages, close |
| `TeacherExamResultsController.cs` | `api/teacher` | Has `exam-results` (teacher-scoped, not student-scoped) |
| `TeacherClassesController.cs` | `api/teacher` | Has `classes`, `classes/{id}`, `classes/{id}/workspace`, `classes/{id}/progress` |
| `AdminApplicationReportsController.cs` | `api/admin/applications/reports` | Has overview, by-type, pending, overdue, processing-time, by-assignee, trends |
| `AdminWorkflowController.cs` | `api/admin/applications/workflow` | Has workflow GET |
| `ProgramTuitionConfigsController.cs` | `api/finance/program-tuition-configs` | Tuition config schema |
| `StudentGradesController.cs` | `api/student` (TBC) | Student grades — reusable for teacher gradebook view |

---

## Decision Matrix

### Group A — SuperAdmin Backend Gaps (from P16B.2)

| Route | Component | Current Status | Business Module | Existing Backend | Can Repoint? | Decision | Reason | Next Phase |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `/super-admin/training/exam-periods` | `ExamPeriodsView.vue` | `NEEDS_BE_ENDPOINT` | Exam period admin | `ExamController`: `GET/POST api/exam/ky-thi`, `GET api/exam/ky-thi/{id}` — full CRUD | ✅ YES | `REPOINT_EXISTING_API` | Endpoint `api/exam/ky-thi` already implements exam period CRUD with paging, filtering. FE just needs to call it instead of local data. | P16B.4C |
| `/super-admin/operations/attendance-policy` | `AttendancePolicyView.vue` | `NEEDS_BE_ENDPOINT` | Attendance policy config | No `attendance-policy` endpoint. `AttendanceController` only has session/daily attendance, not policy config | ❌ NO | `KEEP_AS_PENDING_ADMIN_ROADMAP` | No entity for `ChinhSachDiemDanh` / policy config in DB. Requires schema design. Out of scope for thesis defense. | — |
| `/super-admin/operations/pass-fail-rules` | `PassFailRulesView.vue` | `NEEDS_BE_ENDPOINT` | Pass/fail rule config | `BghAcademicController` has `academic/pass-fail` (read-only report), not a config endpoint | ❌ NO | `KEEP_AS_PENDING_ADMIN_ROADMAP` | Pass/fail rules require a config table not in current schema. BGH academic is read-only analytics, not rule administration. | — |
| `/super-admin/support/tickets` | `SupportTicketsView.vue` | `NEEDS_BE_ENDPOINT` | Admin ticket management | `StudentSupportTicketsController`: student-scoped CRUD. No admin-scoped ticket dashboard | ⚠️ PARTIAL | `HIDE_FROM_DEMO_AND_CLAIM` | Student tickets exist at `api/student/support-tickets`. Admin view would need a separate admin-scoped endpoint (`api/admin/support-tickets`) that doesn't exist. Demo uses Student role for support tickets. Hiding admin-side is safe. | P16B.4D |
| `/super-admin/support/faq` | `FAQManagementView.vue` | `NEEDS_BE_ENDPOINT` | FAQ management | No FAQ entity, no FAQ controller in 85 files | ❌ NO | `REMOVE_ROUTE` | No FAQ business model in DB. No controller, no entity, no migration. Route has no backend backing and no data to show. Can be removed without impacting any demo claim. | P16B.4D |
| `/super-admin/approvals/history` | `ApprovalsHistoryView.vue` | `NEEDS_BE_ENDPOINT` | Approval history | `AdminApplicationsController`: `GET api/admin/applications` with status filter; `AdminApplicationReportsController`: full reports with by-type, trends, overdue | ✅ YES | `REPOINT_EXISTING_API` | Approval history is a filtered view of `GET api/admin/applications?status=approved,rejected,cancelled`. `AdminApplicationReports` provides trends and analysis. FE can reuse existing endpoints. | P16B.4C |
| `/super-admin/evaluations/config` | `EvaluationsConfigView.vue` | `NEEDS_BE_ENDPOINT` | Evaluation form config | `BghEvaluationController` has evaluation management; `StudentEvaluationsController` for student side. No "config" endpoint for evaluation form templates | ⚠️ PARTIAL | `KEEP_AS_PENDING_ADMIN_ROADMAP` | `BghEvaluationController` manages evaluations but not form configuration. Config would require EvaluationForm/Template management beyond current scope. | — |
| `/super-admin/evaluations/results` | `EvaluationsResultsView.vue` | `NEEDS_BE_ENDPOINT` | Evaluation results | `BghEvaluationController`: `GET api/bgh/evaluations`, `GET api/bgh/evaluations/ranking`, `GET api/bgh/evaluations/overview`, `GET api/bgh/evaluations/ai-analysis` | ✅ YES | `REPOINT_EXISTING_API` | BGH evaluation endpoints are exactly what this view needs. SuperAdmin can be authorized to the same endpoints (BGH endpoints accessible with extended role policy) or the view can redirect to `/bgh/evaluations` which is already connected. | P16B.4C |
| `/super-admin/reports/education-overview` | `EducationOverviewView.vue` | `NEEDS_BE_ENDPOINT` | Education analytics | `BghAcademicController`: `academic/overview`, `academic/gpa`, `academic/at-risk`, `academic/pass-fail`. `SuperAdminController`: `dashboard/stats` | ✅ YES | `REPOINT_EXISTING_API` | BGH academic overview is an existing real API backed by DB queries. SuperAdmin view should repoint to `api/bgh/academic/overview` + related endpoints (or composite). | P16B.4C |
| `/super-admin/security/alerts` | `SecurityAlertsView.vue` | `NEEDS_BE_ENDPOINT` | Security monitoring | `SuperAdminController`: `GET api/super-admin/security/alerts` — **exists!** | ✅ YES | `REPOINT_EXISTING_API` | The endpoint `api/super-admin/security/alerts` is in `SuperAdminController.cs`. FE just needs to call it. Single-line fix to connect the view to this API. | P16B.4C |
| `/super-admin/system/modules` | `SystemModulesView.vue` | `NEEDS_BE_ENDPOINT` | System health | `SuperAdminController`: `GET api/super-admin/system/modules` — **exists!** | ✅ YES | `REPOINT_EXISTING_API` | The endpoint `api/super-admin/system/modules` is in `SuperAdminController.cs`. FE just needs to call it. | P16B.4C |

---

### Group B — FE_ONLY_STATIC Remaining (from P16B.4A)

| Route | Component | Current Status | Business Module | Existing Backend | Can Repoint? | Decision | Reason | Next Phase |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `/super-admin/reports/learning` | `LearningReportView.vue` | `NEEDS_BE_ENDPOINT` | Learning analytics | `LearningProgressController`: `GET api/learning-progress/classes/{classId}/students`. No aggregated learning analytics endpoint. | ⚠️ PARTIAL | `HIDE_FROM_DEMO_AND_CLAIM` | Detailed per-class progress exists but no aggregated learning report (cross-class, cross-term) endpoint. Too expensive to design now. Thesis demo uses BGH academic overview instead. | P16B.4D |
| `/super-admin/reports/attendance` | `AttendanceReportView.vue` | `NEEDS_BE_ENDPOINT` | Attendance analytics | `AttendanceController`: session-level attendance only, no aggregated report | ❌ NO | `HIDE_FROM_DEMO_AND_CLAIM` | No attendance analytics/reporting aggregate endpoint. Demo uses BGH academic overview for overall stats. Route is outside demo scope. | P16B.4D |
| `/super-admin/reports/campus-comparison` | `CampusComparisonView.vue` | `NEEDS_BE_ENDPOINT` | Multi-campus analytics | No multi-campus endpoint. Organizations hierarchy exists but no cross-campus comparison analytics | ❌ NO | `KEEP_AS_PENDING_ADMIN_ROADMAP` | Would require significant analytics design (cross-campus aggregation, index). Future roadmap only. | — |
| `/super-admin/reports/export` | `DataExportView.vue` | `NEEDS_BE_ENDPOINT` | Data export | No export endpoint in 85 controllers | ❌ NO | `KEEP_AS_PENDING_ADMIN_ROADMAP` | Export requires format negotiation, background jobs, storage. Out of thesis scope. Roadmap feature. | — |
| `/teacher/class-grades` | `ClassGradebookView.vue` | `NEEDS_BE_ENDPOINT` | Teacher gradebook | `TeacherClassesController`: `GET api/teacher/classes/{id}` (basic info only). `StudentGradesController` has student-side grades. `TeacherSubmissionsController` has assignment submission data. | ⚠️ PARTIAL | `IMPLEMENT_NOW` | Teacher gradebook is a core academic function expected in thesis defense. `TeacherClassesController` already has `classes/{id}/progress`. `StudentGradesController` (`api/student/grades`) has real grade data. A lightweight teacher-scoped grade summary endpoint can be added to existing `TeacherClassesController` as `GET api/teacher/classes/{id}/grades` — single endpoint, entity already mapped. | P16B.4C |
| `/teacher/grading-input` | `ClassGradesView.vue` | `NEEDS_BE_ENDPOINT` | Teacher grade entry | Same as above + `ExamController` has `grading/publish`, `grading/auto`, `grading/essay` for exam grades | ⚠️ PARTIAL | `IMPLEMENT_NOW` | Teacher manual grade entry is expected demo functionality. `ExamController` already has `grading/*` endpoints. Teacher needs a component-grade (điểm thành phần) entry endpoint at `PUT api/teacher/classes/{id}/grades/{studentId}`. Scope is small: 1 endpoint, entity `BangDiemLopHoc` or similar already exists. | P16B.4C |
| `/student/exams/2` | `ExamResultView.vue` | `NEEDS_BE_ENDPOINT` | Student exam result | `ExamController`: `GET api/exam/taking/session/{id}`, `student/list`. `TeacherExamResultsController`: `GET api/teacher/exam-results` (teacher-scoped). No `GET api/student/exam-results/{id}` | ⚠️ PARTIAL | `IMPLEMENT_NOW` | Student viewing their own exam result is a core student-facing feature expected in demo. `ExamController` already has session management. A `GET api/exam/student/result/{sessionId}` endpoint can read from existing `PhienThi`/`BaiLam` entities. Very contained scope. | P16B.4C |

---

### Group C — REMOVE_FROM_100_PERCENT_CLAIM (from P16B.1)

| Route | Component | Current Status | Business Module | Existing Backend | Can Repoint? | Decision | Reason | Next Phase |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `/super-admin/finance/student-debts` | `Finance/FinanceMonitorView.vue` | `REMOVE_FROM_100_PERCENT_CLAIM` | Finance admin | `StudentTuitionController`: `api/student/tuition/invoices` (student-scoped). `ProgramTuitionConfigsController`. No admin debt summary endpoint | ⚠️ PARTIAL | `HIDE_FROM_DEMO_AND_CLAIM` | Student-facing tuition exists. Admin-level debt monitor would need `GET api/admin/finance/student-debts` aggregating across students. `FinanceMonitorView` currently renders but fetches no real data. Demo focus is on Parent finance view. Route should be documented as roadmap. | P16B.4D |
| `/super-admin/finance/payments` | `Finance/FinanceMonitorView.vue` | `REMOVE_FROM_100_PERCENT_CLAIM` | Finance admin | `FinancePaymentWebhooksController`: webhook only, not an admin payments listing | ❌ NO | `HIDE_FROM_DEMO_AND_CLAIM` | No admin payment list endpoint. Parent payment flow exists (`api/parent/payment`). Admin payments view is roadmap. | P16B.4D |
| `/super-admin/finance/refunds` | `Finance/FinanceMonitorView.vue` | `REMOVE_FROM_100_PERCENT_CLAIM` | Finance admin | No refund endpoint | ❌ NO | `HIDE_FROM_DEMO_AND_CLAIM` | No refund business model endpoint. Roadmap. | P16B.4D |
| `/super-admin/operations/schedules` | `PlaceholderView.vue` | `REMOVE_FROM_100_PERCENT_CLAIM` | Schedule approval | `ThoiKhoaBieuController` handles drafts, publish, conflict check — but for Staff/GiaoVu, not a SuperAdmin approval screen | ⚠️ PARTIAL | `REMOVE_ROUTE` | Route points to `PlaceholderView`. Schedule admin is handled via Staff role (`/staff/conflicts`, `/staff/schedule/pending`). SuperAdmin duplicate is confusing and has no backing. Remove and redirect demo to Staff schedule screens. | P16B.4D |
| `/super-admin/operations/schedules/approval` | `PlaceholderView.vue` | `REMOVE_FROM_100_PERCENT_CLAIM` | Schedule approval | Same as above | ❌ NO | `REMOVE_ROUTE` | Same as above — placeholder with no business backing. | P16B.4D |
| `/super-admin/finance/tuition-config` | `PlaceholderView.vue` | `REMOVE_FROM_100_PERCENT_CLAIM` | Tuition config | `ProgramTuitionConfigsController`: `api/finance/program-tuition-configs` — exists but is finance-team scoped, not SuperAdmin | ⚠️ PARTIAL | `HIDE_FROM_DEMO_AND_CLAIM` | Real endpoint exists but requires `FinanceConstants.FinanceAuthorizationRoles.SchemaReaders` role — SuperAdmin may not be in this role group. Demo can reference tuition config via the dedicated finance module. Placeholder route adds no value. | P16B.4D |
| `/super-admin/notifications/history` | `PlaceholderView.vue` | `REMOVE_FROM_100_PERCENT_CLAIM` | Notification history | `AdminNotificationsController`: `GET api/admin/notifications` — full notification history **exists!** | ✅ YES | `REPOINT_EXISTING_API` | `GET api/admin/notifications` already returns the full notification list (history). Route just needs to point to a real view instead of PlaceholderView. Simple component reuse from `NoticeHistoryView` already done in P16B.4A. | P16B.4C |

---

## Decision Summary

| Decision | Count | Routes |
| --- | ---: | --- |
| `REPOINT_EXISTING_API` | **7** | exam-periods, approvals/history, evaluations/results, education-overview, security/alerts, system/modules, notifications/history |
| `IMPLEMENT_NOW` | **3** | teacher/class-grades, teacher/grading-input, student/exams/result |
| `HIDE_FROM_DEMO_AND_CLAIM` | **7** | support/tickets, reports/learning, reports/attendance, finance/student-debts, finance/payments, finance/refunds, finance/tuition-config |
| `REMOVE_ROUTE` | **3** | support/faq, operations/schedules, operations/schedules/approval |
| `KEEP_AS_PENDING_ADMIN_ROADMAP` | **5** | operations/attendance-policy, operations/pass-fail-rules, evaluations/config, reports/campus-comparison, reports/export |
| **Total** | **25** | |

---

## Proposed Next Phases

### P16B.4C — Connect/Implement NOW (7 repoints + 3 implements = 10 routes)

Priority order:

1. **`REPOINT_EXISTING_API` (no new backend needed, only FE wiring):**
   - `/super-admin/security/alerts` → call `GET api/super-admin/security/alerts` (1-line FE change)
   - `/super-admin/system/modules` → call `GET api/super-admin/system/modules` (1-line FE change)
   - `/super-admin/approvals/history` → reuse `applicationsApi.getApplications({status:'approved,rejected,cancelled'})` (already exists)
   - `/super-admin/evaluations/results` → redirect to `/bgh/evaluations` or call `GET api/bgh/evaluations` with SuperAdmin role
   - `/super-admin/reports/education-overview` → call `GET api/bgh/academic/overview` with SuperAdmin authorization
   - `/super-admin/training/exam-periods` → call `GET api/exam/ky-thi` (existing ExamController)
   - `/super-admin/notifications/history` → reuse `NoticeHistoryView` pattern already used in Staff (P16B.4A)

2. **`IMPLEMENT_NOW` (requires small new backend endpoints):**
   - `GET api/teacher/classes/{id}/grades` — teacher sees student grade list per class
   - `PUT api/teacher/classes/{id}/grades/{studentId}` — teacher enters component grades
   - `GET api/exam/student/result/{sessionId}` — student views their own exam result

### P16B.4D — Hide/Remove/Route cleanup (10 routes)

   - Remove `PlaceholderView` routes: `operations/schedules`, `operations/schedules/approval`, `support/faq`
   - Add `HIDE_FROM_DEMO_AND_CLAIM` note in router for 7 routes
   - Update docs matrix status for all 25 rows

### P16B.5 — Runtime Action Audit

   - For all `PASS_LOAD_ONLY_ACTIONS_PENDING` routes: verify submit/save/delete actions actually call backend
   - POST/PUT/DELETE smoke with real session token

---

## IMPLEMENT_NOW Detail — Endpoint Scope

### 1. `GET api/teacher/classes/{classId}/grades`
- **Controller**: Add to `TeacherClassesController.cs` (existing, route `api/teacher`)
- **Entity**: `BangDiemLopHoc` / `DiemSo` (check schema — likely already mapped)
- **Returns**: List of students in class with their grade components
- **Complexity**: LOW — entities exist, teacher→class auth already in controller

### 2. `PUT api/teacher/classes/{classId}/grades/{studentId}`
- **Controller**: Same as above
- **Body**: Component grade values (điểm giữa kỳ, cuối kỳ, etc.)
- **Complexity**: LOW-MEDIUM — need validation, grade lock check

### 3. `GET api/exam/student/result/{sessionId}`
- **Controller**: Add to `ExamController.cs` (existing, route `api/exam`)
- **Entity**: `PhienThi` (PhienThi.MaPhienThi) + `BaiLam` (answers, score)
- **Auth**: Student must own the session
- **Complexity**: LOW — session+result data already exists for grading endpoints

---

## REPOINT Detail — Authorization Note

> `api/bgh/*` endpoints are authorized with `[Authorize(Roles = "BGH")]`. Before repointing SuperAdmin views to these endpoints, must either:
> 1. Add `SuperAdmin` to allowed roles in `BghAcademicController` / `BghEvaluationController` (preferred, minimal), OR
> 2. Create a thin SuperAdmin-scoped wrapper calling the same service methods

> `api/exam/ky-thi` uses `[Authorize(Policy = "AcademicOperations")]` — check `AuthConstants` to confirm SuperAdmin is in this policy.

---

## Verification

```text
git status --short   → no modified files (docs only in this phase)
git diff --check     → clean (LF warnings in docs only)
conflict marker grep → 0 hits
Build                → not required (docs only)
```

---

## Files Changed in P16B.4B

- `docs/P16B4B_MISSING_BACKEND_ENDPOINT_DECISION_MATRIX.md` [NEW]
- `docs/P16_FULL_SCREEN_ACTION_API_MATRIX.md` [UPDATE — append P16B.4B section]
- `docs/P16B1_HIGH_RISK_WORKLIST.md` [UPDATE — append P16B.4B decision]
