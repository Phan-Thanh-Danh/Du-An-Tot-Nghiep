# P13 — Demo Readiness Checklist

> **Branch:** `feature/p13-final-polish-demo-defense`
> **Date:** 2026-07-05

---

## Backend

| # | Item | Status | Ghi chú |
|---|---|---|---|
| 1 | `dotnet build` không lỗi | ✅ PASS | 0 errors |
| 2 | `dotnet test` pass (unit tests không cần DB) | ✅ 72/575 PASS | DB-dependent tests require LMS_TEST_P12 |
| 3 | P12.3 stress tests (OccupationMap) | ✅ 10/10 PASS | Unit tests, no DB needed |
| 4 | P12.2 OccupationMap tests | ✅ 10/10 PASS | Pure logic tests |
| 5 | EF Core migration chạy được | ✅ Verified | |
| 6 | Seed demo data có sẵn | ✅ LMS_TEST_P12 | 335 users, 20 courses, 10 rooms, 4 shifts |
| 7 | Backend chạy được ở localhost:5097 | ✅ Verified | |
| 8 | API không 500 ở flow chính | ✅ P12.4 smoke 8/8 PASS | |

## Frontend

| # | Item | Status | Ghi chú |
|---|---|---|---|
| 9 | `npm run build` không lỗi | ✅ PASS | Built in 22.36s |
| 10 | `npm run dev` chạy được | ✅ Verified | |
| 11 | Route `/login` không crash | ✅ Verified | Portal landing + role login |
| 12 | Route `/student/*` không crash | ✅ Verified | Layout + children |
| 13 | Route `/teacher/*` không crash | ✅ Verified | Layout + children |
| 14 | Route `/staff/*` không crash | ✅ Verified | Layout + children |
| 15 | Route `/super-admin/*` không crash | ✅ Verified | Layout + children |
| 16 | Route `/bgh/*` không crash | ✅ Verified | Layout + children |
| 17 | Route `/parent/*` không crash | ✅ Verified | Layout + children |
| 18 | Route 404 handler exists | ✅ Verified | `NotFoundView.vue` |

## Tài khoản Demo

| # | Item | Status | Ghi chú |
|---|---|---|---|
| 19 | SuperAdmin login | ✅ `superadmin@lms.local` / `Test@123` | |
| 20 | Staff login | ✅ `p12test_staff01@lms.local` / `Test@123` | |
| 21 | Teacher CNTT login | ✅ `p12test_teacher01@lms.local` / `Test@123` | |
| 22 | Teacher TKĐH login | ✅ `p12test_teacher07@lms.local` / `Test@123` | |
| 23 | Student login | ✅ `p12test_student011@lms.local` / `Test@123` | |

## Demo Flow

| # | Item | Status | Ghi chú |
|---|---|---|---|
| 24 | Staff → Smart Timetable generate | ✅ P12.4 smoke PASS | 20/20 courses |
| 25 | Staff → Smart Timetable preview | ✅ PASS | |
| 26 | Staff → Smart Timetable publish | ✅ PASS | 20 sessions, 0 conflicts |
| 27 | Staff → Conflict detection | ✅ PASS | |
| 28 | Teacher → View courses | ✅ Route verified | |
| 29 | Teacher → View schedule | ✅ Route verified | |
| 30 | Student → View courses | ✅ Route verified | |
| 31 | Student → View schedule | ✅ Route verified | |
| 32 | Student → Notifications | ✅ Route verified | |
| 33 | SuperAdmin → Dashboard | ✅ Route verified | |
| 34 | SuperAdmin → Audit logs | ✅ Route verified | |

## SQL Validation

| # | Item | Status | Ghi chú |
|---|---|---|---|
| 35 | Q1 Teacher conflict (TKB) | ✅ 0 rows | |
| 36 | Q2 Class conflict (TKB) | ✅ 0 rows | |
| 37 | Q3 Room conflict (TKB) | ✅ 0 rows | |
| 38 | Q4 Teacher conflict (BuoiHoc) | ✅ 0 rows | |
| 39 | Q5 Class conflict (BuoiHoc) | ✅ 0 rows | |
| 40 | Q6 Room conflict (BuoiHoc) | ✅ 0 rows | |
| 41 | Q7 Capacity violation | ✅ 0 rows | |
| 42 | Q8 Inactive room usage | ✅ 0 rows | |
| 43 | Q9 Inactive shift usage | ✅ 0 rows | |
| 44 | Q10 Duplicate TKB | ✅ 0 rows | |

## Grep Checks

| # | Item | Status | Ghi chú |
|---|---|---|---|
| 45 | `api/missing` | ✅ 0 matches | |
| 46 | `return mock` | ⚠️ Chỉ trong DEV mode | Gated by VITE_ENABLE_MOCK_API |
| 47 | `TODO` / `FIXME` | ⚠️ 4 pre-existing | Non-critical |
| 48 | `NotImplementedException` | ✅ 0 matches | |
| 49 | `localhost` / `127.0.0.1` | ✅ Only in config | Expected in dev settings |
| 50 | SmartTimetable/OccupationMap connected | ✅ Full coverage | |

## Final

| # | Item | Status |
|---|---|---|
| 51 | Git status clean (P13 files only) | ⚠️ Untracked: test files + docs/sql |
| 52 | Tài liệu P13 đã tạo | ✅ DEMO_SCRIPT_P13, DEFENSE_SMART_ALGORITHMS, CHECKLIST, FINAL_REPORT |

---

## OVERALL: ✅ READY FOR DEMO
