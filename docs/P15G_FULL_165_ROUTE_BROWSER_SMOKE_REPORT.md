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
