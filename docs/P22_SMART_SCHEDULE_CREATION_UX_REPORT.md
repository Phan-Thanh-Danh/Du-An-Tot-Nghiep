# P22 Smart Schedule Creation UX & Term-Safe Date Logic Report

> Phase: `P22`
> Date: 2026-07-10
> Scope: Staff schedule creation UX and published schedule loading
> Decision: `P22_PASS`

## Summary

P22 improves the Staff schedule creation flow without changing backend contracts. The schedule manager now derives course, term, campus, room, shift, and existing schedule context from real APIs before building create or smart-scheduling payloads.

No backend files were modified for this phase.

## Files Changed

- `frontend/src/views/GiaoVu/Schedule/ScheduleManagerView.vue`
- `frontend/src/views/GiaoVu/Schedule/StaffPublishedSchedulesView.vue`

## What Changed

- Replaced hardcoded shift catalog with real shift options loaded through Staff schedule APIs.
- Removed unsafe create defaults such as implicit term/campus IDs; schedule create payloads now use selected course and API-derived context.
- Added course selection that auto-fills read-only course metadata: term, campus, class, subject, teacher, and class size.
- Added academic-term lookup so course term dates are derived from real `/api/master-data/academic-terms` data when `/api/courses` only returns `maHocKy`.
- Added term-safe date modes:
  - whole term
  - from today within term
  - custom date range clamped to the course term
- Added local smart slot suggestions using real courses, rooms, shifts, and existing schedules.
- Added hard conflict checks for teacher, class, room, campus mismatch, inactive room, and capacity.
- Added soft ranking signals for room fit, evening slots, Saturday slots, and teacher/class day load.
- Added bulk draft review for multiple unscheduled courses.
- Added whole-term smart draft generation through the existing smart schedule API contract.
- Updated published schedule screen to load via `scheduleApi.list()` with published status and to show real loading/error/empty states.

## API Behavior

| Flow | API |
| --- | --- |
| Course options | `courseApi.getCourses()` |
| Academic term dates | `academicTermApi.list()` |
| Room options | `staffApi.getRooms()` |
| Shift options | `staffApi.getCaHoc()` |
| Existing schedules | `scheduleApi.list()` |
| Create draft/published schedule | `scheduleApi.create()` |
| Conflict check | `scheduleApi.checkConflicts()` |
| Whole-term smart draft | `scheduleApi.generateDraft()` |
| Published schedules | `scheduleApi.list({ TrangThai: 'da_xuat_ban' })` |

## Safety Checks

| Check | Result |
| --- | --- |
| Frontend build | PASS |
| `git diff --check` for P22 files | PASS |
| P22 mock/fake/dummy/hardcoded ID grep | PASS, 0 hits |
| Backend changes | None |

The focused grep checked the P22 schedule files and `scheduleApi.js` for:

```text
mock|fake|dummy|DEMO_|setTimeout|maHocKy:\s*1|maDonVi:\s*1|xep-lich-thong-minh|caHocCatalog|staffApi\.getSchedules|Gợi ý lịch trống
```

## Runtime Status

Targeted P22.1 runtime smoke was executed through Chrome CDP against the local app.

Artifact: `docs/artifacts/p22-smart-schedule-runtime/p22-runtime-results.json`

## Runtime Smoke Result

| Check | Result |
| --- | --- |
| `/staff/schedule` | PASS |
| `/staff/schedule/published` | PASS |
| Valid POST create schedule | PASS, `POST /api/thoi-khoa-bieu` returned `201` |
| Invalid date blocked before API | PASS |
| Smart suggestion apply date sync | PASS |
| Bulk suggestion collision-aware | PASS |
| Smart whole-term generate | PASS, `POST /api/thoi-khoa-bieu/generate` returned `200` |
| AlertCircle unresolved | PASS, 0 |
| `GET /api/thoi-khoa-bieu` 400 | PASS, 0 |
| Valid `POST /api/thoi-khoa-bieu` 400 | PASS, 0 |
| Fake success | PASS, 0 |
| Page-level spinner blocker | PASS, 0 |

Runtime API evidence included:

- `GET /api/thoi-khoa-bieu`
- `GET /api/courses`
- `GET /api/master-data/academic-terms`
- `GET /api/master-data/rooms`
- `GET /api/ca-hoc`
- `POST /api/thoi-khoa-bieu/check-xung-dot`
- `POST /api/thoi-khoa-bieu`
- `POST /api/thoi-khoa-bieu/generate`

## Final Decision

`P22_PASS`

## P22.2 Pending Draft Visibility Fix

- Smart generate now passes `maDonVi`, `maHocKy`, and `draftId` to `/staff/schedule/pending`.
- Pending page now loads drafts using required backend query params instead of calling `listDrafts()` without context.
- Newly generated draft is auto-selected and highlighted when `draftId` is present in the route query.
- Direct pending route without context now shows an instruction instead of a misleading empty state.
- Pending page wording was adjusted to `Lịch nháp/chờ duyệt` to match backend draft status.

## P22.2 Runtime Smoke Result

| Check | Result |
| --- | --- |
| Smart generate creates draft | PASS |
| Pending route receives `maDonVi`/`maHocKy`/`draftId` | PASS |
| `GET /api/thoi-khoa-bieu/drafts` without context in smart flow | PASS, 0 calls |
| `GET /api/thoi-khoa-bieu/drafts?maDonVi=...&maHocKy=...` | PASS |
| Generated draft visible in pending page | PASS |
| Generated draft auto-selected/highlighted | PASS |

## Git Safety Note

The worktree already contains many modified files from other phases. P22 changes were kept scoped to the two schedule views and this report. Commit must stage only the three P22 files listed in the safe commit checklist.

## Remaining Work

- Commit P22 separately once unrelated worktree changes are either committed or intentionally staged out.
