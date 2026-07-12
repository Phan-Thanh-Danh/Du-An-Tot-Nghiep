# P0 Backend Backlog — Missing & Partial Capabilities

This document tracks capabilities where backend or frontend implementation is incomplete. Every `MISSING` or `PARTIAL` row in `P0_BACKEND_CAPABILITY_MATRIX.csv` must have a corresponding entry here.

**Referential integrity rule**: Every `MISSING` CapabilityId must appear in this file. A validator check enforces this.

---

## PARTIAL Capabilities (backend exists, workflow incomplete)

### CAP-PAY-002 — Parent initiates payment
- **Capability ID**: CAP-PAY-002
- **Role**: `Parent`
- **Current Route**: `POST /api/parent/payment`
- **Controller**: `ParentController::MakePayment`
- **Issue**: Payment gateway integration (PayOS) exists but transaction confirmation callback and idempotency handling are incomplete. The endpoint accepts the request but full redirect flow and webhook reconciliation need hardening.
- **Backlog Task**: BL-001 — Complete PayOS webhook reconciliation in `FinancePaymentWebhooksController` and end-to-end payment state machine.

### CAP-TCH-004 — Teacher views class grades (frontend partial)
- **Capability ID**: CAP-TCH-004
- **Role**: `Teacher`
- **Backend Route**: `GET /api/teacher/classes/{id}/grades`
- **Issue**: Backend IMPLEMENTED. Frontend gradebook view exists but bulk export and filter-by-assessment are not connected.
- **Backlog Task**: BL-002 — Connect Teacher gradebook filter and export UI to backend endpoints.

### CAP-STF-006 — AcademicStaff publishes timetable (frontend partial)
- **Capability ID**: CAP-STF-006
- **Role**: `AcademicStaff`
- **Backend Route**: `POST /api/thoi-khoa-bieu/publish`
- **Issue**: Backend IMPLEMENTED. Frontend publish confirmation dialog not wired.
- **Backlog Task**: BL-003 — Wire timetable publish confirmation in `ScheduleManagerView.vue`.

### CAP-BGH-006 — Principal views AI evaluation analysis (frontend partial)
- **Capability ID**: CAP-BGH-006
- **Role**: `Principal`
- **Backend Route**: `GET /api/bgh/evaluations/ai-analysis`
- **Issue**: Backend IMPLEMENTED. Frontend AI analysis panel renders but streaming/loading state incomplete.
- **Backlog Task**: BL-004 — Fix loading and error state for AI analysis panel in BGH Evaluations view.

### CAP-BGH-008 — Principal views schedule changes (frontend partial)
- **Capability ID**: CAP-BGH-008
- **Role**: `Principal`
- **Backend Route**: `GET /api/bgh/schedule/changes`
- **Issue**: Backend IMPLEMENTED. Frontend schedule-changes view is placeholder.
- **Backlog Task**: BL-005 — Connect schedule changes view in BGH dashboard.

---

## MISSING Capabilities (no backend implementation)

*No capabilities are currently fully MISSING from backend after CAP-QZ-001 and CAP-USR-001 were corrected.*

> **Note on CAP-QZ-001 correction**: Previously marked as MISSING, but `QuizManagementController` (`POST /api/exam/de-kiem-tra/{id}/publish`) provides publish functionality. Reclassified to `CAP-QZ-002 IMPLEMENTED`.

> **Note on CAP-USR-001 correction**: Previously marked as MISSING due to generator bug (searched `AdminAccountsController` instead of `AdminUsersController`). `AdminUsersController` at `api/admin/users` provides full CRUD. Reclassified to IMPLEMENTED.

---

## Legend

| Status | Definition |
|--------|-----------|
| `MISSING` | No backend controller action exists for this workflow |
| `PARTIAL` | Backend endpoint exists but core business logic, side-effects, or frontend connection is incomplete |
| `IMPLEMENTED` | Backend and frontend both fully connected with no known gaps |
