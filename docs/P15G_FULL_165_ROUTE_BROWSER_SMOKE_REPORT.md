# P15G: Full Browser Smoke Test Report

## Summary
- **Total Routes Tested:** 119
- **Pass Count:** 106
- **Fail Count:** 13

## Failures

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

### Final Verification
- **Total Routes Tested**: 166 (Updated to include full permutations of all 7 roles).
- **Pass Count**: 166
- **Fail Count**: 0
- **Network Errors (401/403/404/500)**: 0
- **Runtime Exceptions**: 0
- **Strict Mock Grep**: 0 hit.

**Status**: 100% Fully Connected and Stable.
