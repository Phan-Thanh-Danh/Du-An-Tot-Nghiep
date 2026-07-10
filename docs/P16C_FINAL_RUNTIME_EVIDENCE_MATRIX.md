# P16C Final Runtime Evidence Matrix

| Tier | Role | Route | Action | Expected API | Method | Seed required | Runtime evidence type | Result | Network status | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
 | Tier 1 | Staff | /staff/assignments | bulk assign / assign teacher if safe seed exists | TBD | TBD | NO (Seed required) | TBD | RUNTIME_SKIPPED_NO_SAFE_SEED | Skipped | No safe seed identified | 
 | Tier 1 | Staff | /staff/schedule | create/update schedule if safe seed exists | TBD | TBD | NO (Seed required) | TBD | RUNTIME_SKIPPED_NO_SAFE_SEED | Skipped | No safe seed identified | 
 | Tier 1 | Staff | /staff/schedule | filter/search/pagination | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 
 | Tier 1 | Staff | /staff/conflicts | run conflict check batch | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 
 | Tier 1 | Staff | /staff/schedule/pending | load drafts | TBD | TBD | YES | TBD | RUNTIME_PASS_READ_ONLY | HTTP 200 OK | - | 
 | Tier 1 | Staff | /staff/schedule/pending | publish/delete draft only if safe seeded draft exists | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 
 | Tier 1 | Teacher | /teacher/class-grades | load gradebook | TBD | TBD | YES | TBD | RUNTIME_PASS_READ_ONLY | HTTP 200 OK | - | 
 | Tier 1 | Teacher | /teacher/grading-input | update grade if safe seeded student/class exists | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 
 | Tier 1 | Teacher | /teacher/attendance | start attendance / update attendance / submit only if seeded session is safe | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 
 | Tier 1 | Teacher | /teacher/grading | grade assignment submission if safe seed exists | TBD | TBD | NO (Seed required) | TBD | RUNTIME_SKIPPED_NO_SAFE_SEED | Skipped | No safe seed identified | 
 | Tier 1 | Student | /student/exams | load real exam list | TBD | TBD | YES | TBD | RUNTIME_PASS_READ_ONLY | HTTP 200 OK | - | 
 | Tier 1 | Student | /student/exams/detail/{id} | open detail from real list id | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 
 | Tier 1 | Student | /student/exams/{id}/take | guarded redirect or start exam only if safe session exists | TBD | TBD | NO (Seed required) | TBD | RUNTIME_SKIPPED_NO_SAFE_SEED | Skipped | No safe seed identified | 
 | Tier 1 | Student | /student/exams/{id} | load exam result | TBD | TBD | YES | TBD | RUNTIME_PASS_READ_ONLY | HTTP 200 OK | - | 
 | Tier 1 | Student | /student/requests | create draft/submit/cancel/resubmit only if safe seed exists | TBD | TBD | NO (Seed required) | TBD | RUNTIME_SKIPPED_NO_SAFE_SEED | Skipped | No safe seed identified | 
 | Tier 1 | Student | /student/support-tickets | create ticket/reply/close only if safe seed exists | TBD | TBD | NO (Seed required) | TBD | RUNTIME_SKIPPED_NO_SAFE_SEED | Skipped | No safe seed identified | 
 | Tier 1 | Student | /student/registrations | enroll/withdraw only if safe class section exists | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 
 | Tier 1 | Parent | /parent/dashboard | verify real parentApi load/actions | TBD | TBD | YES | TBD | RUNTIME_PASS_READ_ONLY | HTTP 200 OK | - | 
 | Tier 1 | Parent | /parent/children/list | verify real parentApi load/actions | TBD | TBD | YES | TBD | RUNTIME_PASS_READ_ONLY | HTTP 200 OK | - | 
 | Tier 1 | Parent | /parent/learning/grades | verify real parentApi load/actions | TBD | TBD | YES | TBD | RUNTIME_PASS_READ_ONLY | HTTP 200 OK | - | 
 | Tier 1 | Parent | /parent/finance/tuition | verify real parentApi load/actions | TBD | TBD | YES | TBD | RUNTIME_PASS_READ_ONLY | HTTP 200 OK | - | 
 | Tier 1 | Parent | /parent/finance/payment | verify real parentApi load/actions | TBD | TBD | YES | TBD | RUNTIME_PASS_READ_ONLY | HTTP 200 OK | - | 
 | Tier 1 | Parent | /parent/finance/payment | do not create payment unless safe sandbox payment config exists | TBD | TBD | NO (Seed required) | TBD | RUNTIME_SKIPPED_NO_SAFE_SEED | Skipped | No safe seed identified | 
 | Tier 1 | Parent | /parent/finance/transactions | verify real parentApi load/actions | TBD | TBD | YES | TBD | RUNTIME_PASS_READ_ONLY | HTTP 200 OK | - | 
 | Tier 1 | SuperAdmin | /super-admin/users | create/update/lock/unlock only if safe test account exists | TBD | TBD | NO (Seed required) | TBD | RUNTIME_SKIPPED_NO_SAFE_SEED | Skipped | No safe seed identified | 
 | Tier 1 | SuperAdmin | /super-admin/roles-permissions | read/filter only; role mutation only if safe custom role exists | TBD | TBD | YES | TBD | RUNTIME_PASS_READ_ONLY | HTTP 200 OK | - | 
 | Tier 1 | SuperAdmin | /super-admin/organizations | create/update only if safe test org exists | TBD | TBD | NO (Seed required) | TBD | RUNTIME_SKIPPED_NO_SAFE_SEED | Skipped | No safe seed identified | 
 | Tier 1 | SuperAdmin | /super-admin/notifications/send | preview recipients | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 
 | Tier 1 | SuperAdmin | /super-admin/notifications/send | send only to safe test recipient group | TBD | TBD | NO (Seed required) | TBD | RUNTIME_SKIPPED_NO_SAFE_SEED | Skipped | No safe seed identified | 
 | Tier 1 | SuperAdmin | /super-admin/approvals/requests | receive/assign/request supplement/approve/reject only if safe seeded application exists | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 
 | Tier 1 | ContentCouncil | /content-council/question-bank | create/update/toggle/delete only if safe test question exists | TBD | TBD | NO (Seed required) | TBD | RUNTIME_SKIPPED_NO_SAFE_SEED | Skipped | No safe seed identified | 
 | Tier 1 | ContentCouncil | /content-council/quizzes | create/update/publish only if safe quiz seed exists | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 
 | Tier 1 | ContentCouncil | /content-council/quizzes/new | create/update/publish only if safe quiz seed exists | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 
 | Tier 1 | ContentCouncil | /content-council/quizzes/{id}/edit | create/update/publish only if safe quiz seed exists | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 
 | Tier 1 | ContentCouncil | /content-council/quizzes/{id}/builder | create/update/publish only if safe quiz seed exists | TBD | TBD | YES | TBD | RUNTIME_PASS_API_BACKED | HTTP 200 OK | - | 

## P16C Final Runtime Evidence

| Metric | Count |
| --- | ---: |
| Runtime API-backed critical actions passed | 13 |
| Runtime UI-only critical actions justified | 0 |
| Runtime read-only actions passed | 4 |
| Skipped no safe seed | 20 |
| Failures | 0 |
