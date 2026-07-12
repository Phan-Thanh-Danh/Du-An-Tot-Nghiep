# P15G: Full Browser Smoke Test Report

## Original P15G Baseline
- **Total Routes Tested:** 119
- **Pass Count:** 106
- **Fail Count:** 13
- **Note:** This is the historical baseline before P15G.1 continuation fixes.

## P15G.1 Continuation Status
- **API/build/grep hardening:** PASS
- **Fresh full browser smoke:** Completed in P15G.3
- **Current artifact:** `docs/artifacts/p15g-full-smoke/smoke-results-p15g.json`
- **Latest artifact result:** 166 route entries, 166 pass, 0 fail, 0 skipped.
- **Decision:** `PASS`

## P15G.3 Final Browser Smoke Result

> Date: 2026-07-09
> Chrome/CDP: `Chrome/149.0.7827.201`, `http://127.0.0.1:9222`
> Backend: `http://localhost:5097`
> Frontend: `https://localhost:5173`
> Artifact: `docs/artifacts/p15g-full-smoke/smoke-results-p15g.json`

### Result Summary

| Metric | Count |
| --- | ---: |
| Route entries exercised | 166 |
| PASS | 166 |
| FAIL | 0 |
| SKIPPED_NO_DATA | 0 |
| Console errors | 0 |
| Runtime exceptions | 0 |
| Network 401 | 0 |
| Network 403 | 0 |
| Network 404 | 0 |
| Network 500 | 0 |

P15G.3 seeded deterministic real source data for the four previously skipped detail routes:

- BGH at-risk student history.
- BGH teacher evaluation detail.
- Teacher class detail.
- Teacher class workspace.

Additional fixes completed during the final smoke:

- The runner now unwraps nested `students` list payloads from real API responses.
- BGH evaluation ranking/detail views now map the real API DTO shape before rendering numeric fields.
- Teacher class detail now calls `teacherApi.getTeacherClassDetail()` for class-id routes instead of treating the class id as a course id.
- Removed the BGH evaluation detail placeholder teacher response; invalid teacher ids now return a real not-found response.

Final claim: Full P15G browser smoke is now `166/166 PASS`.

## P15G.2 Fresh Browser Smoke Finalization

> Date: 2026-07-09
> Chrome/CDP: `Chrome/149.0.7827.201`, `http://127.0.0.1:9222`
> Backend: `http://localhost:5097`
> Frontend: `https://localhost:5173`
> Artifact: `docs/artifacts/p15g-full-smoke/smoke-results-p15g.json`

### Result Summary

| Metric | Count |
| --- | ---: |
| Route entries exercised | 166 |
| PASS | 162 |
| FAIL | 0 |
| SKIPPED_NO_DATA | 4 |
| Console errors | 0 |
| Runtime exceptions | 0 |
| Network 401 | 0 |
| Network 403 | 0 |
| Network 404 | 0 |
| Network 500 | 0 |

The API connection matrix remains `165/165` connected. The browser smoke artifact contains `166` route entries because it preserves the existing P15G route inventory including one extra role-route assignment; no route was hidden or removed to improve the result.

### Skipped Routes

These routes were not marked pass because their required detail source record does not exist in the current database. The runner fetched the real list API first and skipped instead of using placeholder IDs.

| Role | Route | Source API | Status |
| --- | --- | --- | --- |
| BGH | `/bgh/academic/at-risk/1/history` | `/api/bgh/academic/at-risk` | `NO_DATA` |
| BGH | `/bgh/evaluations/detail/1` | `/api/bgh/evaluations/ranking` | `NO_DATA` |
| Teacher | `/teacher/classes/1/details` | `/api/teacher/classes?pageSize=20` | `NO_DATA` |
| Teacher | `/teacher/classes/1/workspace` | `/api/teacher/classes?pageSize=20` | `NO_DATA` |

### P15G.2 Fixes

- Added `docs/artifacts/p15g-full-smoke/p15g-browser-smoke.mjs`, a CDP runner that logs in per role, resolves real detail IDs from list APIs, and writes a bounded JSON artifact.
- Updated the runner to poll page readiness until visible content appears, preventing false negatives during route transitions or async content loading.
- Fixed Staff facilities/master-data screens by clamping `pageSize` to backend validation limits and returning real `items` arrays from the relevant service list calls.
- Rewired BGH schedule conflict monitoring to use the BGH read-only schedule API instead of calling an admin-only conflict-check endpoint.
- Fixed Teacher lessons/class-progress sidebar routes so they no longer call `/api/teacher/classes/undefined`; they load the first real teacher class when available, otherwise render an empty state.

### Final Verification

| Check | Result |
| --- | --- |
| Backend `dotnet build` | PASS, 4 warnings, 0 errors |
| Frontend `npm run build` | PASS |
| Strict production mock/fallback grep | PASS, 0 hits |
| Conflict marker grep | PASS, 0 hits |
| `git diff --check` | PASS, line-ending warnings only |
| Student schedule API | Verified via code scan: `studentApi.getSchedule()` calls `/api/student/dashboard/schedule`, not `/api/thoi-khoa-bieu` |

P15G.2 final status was `PASS_WITH_WARNINGS`. This historical section is superseded by the P15G.3 final result above.

## Original P15G Baseline Failures

| Role | Route | Error |
| --- | --- | --- |
| BGH | /bgh/roles | ReferenceError: useAuthStore is not defined |
| BGH | /bgh/academic-programs | 403 https://localhost:5173/api/master-data/training-programs |
| BGH | /bgh/curriculum | 403 https://localhost:5173/api/master-data/training-programs |
| BGH | /bgh/academic-terms | 403 https://localhost:5173/api/master-data/academic-terms |
| BGH | /bgh/schedule/pending | 403 https://localhost:5173/api/thoi-khoa-bieu |
| BGH | /bgh/schedule/published | 403 https://localhost:5173/api/thoi-khoa-bieu?status=published |
| BGH | /bgh/evaluations/detail/1 | TypeError: Cannot read properties of null (reading 'avgRating') |
| BGH | /bgh/facilities | 403 https://localhost:5173/api/master-data/buildings<br>403 https://localhost:5173/api/master-data/floors<br>403 https://localhost:5173/api/master-data/rooms |
| BGH | /bgh/audit-logs | 403 https://localhost:5173/api/audit-logs |
| Student | /student/courses/1 | 404 https://localhost:5173/api/student/courses/1 |
| Student | /student/assignments | 500 https://localhost:5173/api/student/assignments |
| Student | /student/assignments/1 | 500 https://localhost:5173/api/student/assignments/1 |
| Student | /student/schedule | 403 https://localhost:5173/api/thoi-khoa-bieu |

## P15G.1 Fix Report

### 1. BGH /bgh/roles
- **Root Cause**: `useAuthStore` was used but not imported in `RolesView.vue`.
- **Fix**: Added `import { useAuthStore } from '@/stores/auth'` to `RolesView.vue`.

### 2. BGH /bgh/academic-programs, /bgh/curriculum, /bgh/academic-terms, /bgh/facilities, /bgh/audit-logs
- **Root Cause**: BGH role (Principal/Head) lacked read-only permissions on Admin/Staff endpoints, leading to 403s.
- **Fix**: Created `BghFacadeController` in backend to expose BGH-scoped read-only endpoints (e.g. `/api/bgh/master-data/...`). Updated `bghApi.js` and mapped Frontend views to call these new facade endpoints.

### 3. BGH /bgh/schedule/pending, /bgh/schedule/published
- **Root Cause**: BGH called Admin-scoped `/api/thoi-khoa-bieu` endpoint.
- **Fix**: Added BGH-scoped `/api/bgh/schedules` endpoint to `BghFacadeController` and updated frontend `bghApi.getPendingSchedules` to call it.

### 4. BGH /bgh/evaluations/detail/1
- **Root Cause**: Hardcoded teacherId 1 returned null/404 when no evaluation existed, causing a TypeError when reading `avgRating`.
- **Fix**: Added null-checking guard `if (data) { ... }` in `TeacherEvalDetailsView.vue` and implemented graceful fallback in `BghEvaluationController` to return empty wrapper when evaluation doesn't exist.

### 5. Student /student/courses/1
- **Root Cause**: Hardcoded courseId 1 triggered 404 since it didn't belong to the student.
- **Fix**: Frontend `CourseDetailView.vue` updated to handle 404 errors gracefully. Smoke runner `runner.mjs` dynamically fetches a valid courseId before navigating to the detail page.

### 6. Student /student/assignments & /student/assignments/1
- **Root Cause**: `StudentAssignmentsController` generated SQL 500 error due to missing join mapping for `BaiTap.MaMonHoc`.
- **Fix**: Fixed backend query in `StudentAssignmentsController` to correctly join `NguoiDung` -> `MaLop` -> `KhoaHoc` and match with assignments. Returns empty list if no assignments exist. Validated ownership for detail view.

### 7. Student /student/schedule
- **Root Cause**: Student frontend called Admin-scoped `/api/thoi-khoa-bieu`.
- **Fix**: Updated `studentApi.js` to call the correct `/api/student/courses` to derive schedule, completely removing unauthorized 403 calls.

---

### Continuation Verification - 2026-07-09

This section records the API-level P15G.1 continuation checks that preceded the P15G.2 fresh browser artifact above.

API-level verification after the continuation fixes:

| Check | Result |
| --- | --- |
| `GET /api/student/assignments` as Student | PASS, `count=1` |
| `GET /api/student/assignments/{firstId}` as Student | PASS, `id=1` |
| `GET /api/student/courses` as Student | PASS, `count=5`, first course `GEN101` |
| `GET /api/student/courses/{firstId}` as Student | PASS, `id=GEN101` |
| `GET /api/bgh/master-data/training-programs` as BGH | PASS, `count=4` |
| `GET /api/bgh/schedules?status=published` as BGH | PASS, `count=3` |
| `GET /api/admin/classes?PageSize=100` as Staff | PASS, `count=9` |
| `GET /api/admin/users?PageSize=100` as Staff | PASS, `count=100` |

P15G.1 build and grep verification:

| Check | Result |
| --- | --- |
| Backend `dotnet build` | PASS, `19` warnings, `0` errors |
| Frontend `npm run build` | PASS |
| Strict production mock/fallback grep | PASS, `0` hits |

Continuation fixes completed:

- Removed fake assignment detail fallback from `StudentAssignmentsController`; invalid or unauthorized assignment IDs now return a real `404`.
- Updated student assignment ownership lookup to use real subject IDs from the student's administrative class courses plus enrolled class-section registrations.
- Removed fake hardcoded course object fallback from `StudentCoursesController`; invalid course IDs now return a real `404`.
- Removed local placeholder upload result from `R2StorageService`; upload now fails fast if storage is not configured.
- Fixed Staff administrative class listing by rewriting the EF query to filter and order on real entity fields before projecting to DTO.

Current status after P15G.3: API/build/grep hardening is PASS and the fresh browser smoke is `166/166 PASS` with `0` failed routes and `0` skipped routes.
