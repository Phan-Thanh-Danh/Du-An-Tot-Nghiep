# P15F Browser Smoke And Skeleton Report

> Date: 2026-07-08
> Branch: `feature/p15-zero-mock-full-production-connection`
> Decision: `PASS_WITH_WARNINGS`

## Environment

| Item | Value |
|---|---|
| Backend URL | `http://localhost:5097` |
| Backend HTTPS | `https://localhost:7150` |
| Frontend URL | `https://localhost:5173` |
| Chrome | `Chrome/149.0.7827.201` |
| Database | `LMS` on `DELL\SQLEXPRESS02` |
| Seed profile | `SeedProfile=LargeDemo` |

Chrome command used:

```powershell
chrome.exe --remote-debugging-port=9222 --ignore-certificate-errors --user-data-dir="$env:TEMP\lms-p15f-smoke-final" https://localhost:5173
```

P15F.3 expanded smoke used:

```powershell
chrome.exe --remote-debugging-port=9222 --ignore-certificate-errors --user-data-dir="$env:TEMP\lms-p15f-smoke-p15f3" https://localhost:5173
node docs/artifacts/p15f-smoke/p15f3-route-smoke.mjs
```

## P15F.2 Database Reset

The local `LMS` database on `DELL\SQLEXPRESS02` was dropped and recreated through EF Core migrations/code-first startup. Backend startup now always runs base role/demo seeding before optional `LargeDemo` seeding, so P12/P15 deterministic accounts survive a clean DB reset.

DB data check after reset:

| Metric | Count |
|---|---:|
| Total users | 10,131 |
| Students (`hoc_sinh`) | 10,005 |
| Teachers (`giao_vien`) | 110 |
| Parents (`phu_huynh`) | 2 |
| Parent links | 1 |

Seeded account check:

| Account | Role | Status | Login |
|---|---|---|---|
| `p12test_staff01@lms.local` | `nhan_vien` | `hoat_dong` | PASS |
| `p12test_teacher01@lms.local` | `giao_vien` | `hoat_dong` | PASS |
| `p12test_student011@lms.local` | `hoc_sinh` | `hoat_dong` | PASS |
| `p15test_bgh01@lms.local` | `hieu_truong` | `hoat_dong` | PASS |
| `p15test_content01@lms.local` | `hoidong_quanly_noidung` | `hoat_dong` | PASS |
| `p15test_parent01@lms.local` | `phu_huynh` | `hoat_dong` | PASS |

## Preflight

| Check | Result |
|---|---|
| Branch | `feature/p15-zero-mock-full-production-connection` |
| `Backend/appsettings.json` | Protected: generic LocalDB connection string and empty secret placeholders |
| `Backend/appsettings.Development.json` | Local dev override keeps `Server=DELL\SQLEXPRESS02;Database=LMS;Trusted_Connection=True;TrustServerCertificate=True` |
| Backend start | PASS |
| OpenAPI reachable | PASS, `GET /openapi/v1.json` returned `200` |
| Frontend Vite | PASS, existing dev server on `5173` |
| Backend build | PASS, 15 warnings, 0 errors |
| Frontend build | PASS |
| `npm run lint` | FAIL, backlog documented in `docs/P15F_LINT_BACKLOG.md`; not build-blocking |

## Route Coverage

Final smoke focused on the P15F.2 regression areas after DB reset and parent login repair.

| Role | Route | HTTP API Status | Console Error | Runtime Exception | Result | Screenshot |
|---|---|---|---:|---:|---|---|
| Parent | `/parent/dashboard` | No 4xx/5xx | 0 | 0 | PASS | `docs/artifacts/p15f-smoke/parent-dashboard-final.png` |
| Parent | `/parent/children/list` | No 4xx/5xx | 0 | 0 | PASS | `docs/artifacts/p15f-smoke/parent-children-final.png` |
| Parent | `/parent/finance/tuition` | No 4xx/5xx | 0 | 0 | PASS | `docs/artifacts/p15f-smoke/parent-tuition-final.png` |
| SuperAdmin | `/super-admin/dashboard` | No 4xx/5xx | 0 | 0 | PASS | `docs/artifacts/p15f-smoke/superadmin-dashboard-regression-final.png` |
| Student | `/student/dashboard` | No 4xx/5xx | 0 | 0 | PASS | `docs/artifacts/p15f-smoke/student-dashboard-regression-final.png` |

Smoke totals from `smoke-results-final.json`:

| Metric | Count |
|---|---:|
| Routes tested | 5 |
| Routes passed | 5 |
| Console errors | 0 |
| Runtime exceptions | 0 |
| Network 401 | 0 |
| Network 403 | 0 |
| Network 404 | 0 |
| Network 500 | 0 |

P15F.3 expanded browser smoke focused on release-hardening coverage without claiming the full 165-route browser clickthrough. It covered every Parent sidebar route, every SuperAdmin sidebar route, and one representative route for Student, Teacher, Staff, BGH, and ContentCouncil.

| Scope | Routes Tested | Routes Passed | Console Errors | Runtime Exceptions | Network 401 | Network 403 | Network 404 | Network 500 | Result |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---|
| Parent sidebar | 15 | 15 | 0 | 0 | 0 | 0 | 0 | 0 | PASS |
| SuperAdmin sidebar | 45 | 45 | 0 | 0 | 0 | 0 | 0 | 0 | PASS |
| Core role representatives | 5 | 5 | 0 | 0 | 0 | 0 | 0 | 0 | PASS |
| **Total** | **65** | **65** | **0** | **0** | **0** | **0** | **0** | **0** | **PASS** |

Representative role routes:

- Student: `/student/dashboard`
- Teacher: `/teacher/dashboard`
- Staff: `/staff/dashboard`
- BGH: `/bgh/dashboard`
- ContentCouncil: `/content-council/subjects`

P15F.3 route smoke artifact: `docs/artifacts/p15f-smoke/smoke-results-p15f3.json`.

## Fixes Completed

| Issue | Fix |
|---|---|
| P15 parent login returned `401` after clean DB reset. | Added deterministic P15 parent seed and ensured base seed runs before `LargeDemo`; verified `p15test_parent01@lms.local / Test@123` login PASS. |
| Parent finance route used stale child id `1`, causing `403` after DB reset. | Finance screens now load real parent children first, select a valid linked child, then call child-scoped tuition/invoice/transaction APIs. |
| Parent dashboard/children/profile/notifications screens still used local presentation data. | Removed `parentData.js`; rewired Parent dashboard, children list/overview, profile, notification history/system screens to `parentApi`. Remaining Parent local state is limited to UI state such as selected child id in `localStorage`. |
| `Backend/appsettings.json` contained machine-specific/local and secret-like values. | Replaced production/default config with generic LocalDB placeholder plus empty SMTP/R2/PayOS placeholders. `DELL\SQLEXPRESS02` is kept only in `appsettings.Development.json`. |
| Empty R2 placeholders caused backend startup validation failure. | Changed `ApplicationEvidenceStorageOptions` default provider to `Local` with a temp storage root. Production still requires explicit non-local storage configuration. |
| SuperAdmin screens imported `apiRequest as apiClient` and called `.get`. | Updated SuperAdmin dashboard/security/modules/AI screens to call `apiRequest('/api/super-admin/...')`. |
| SuperAdmin Users/Roles pages threw runtime exceptions with current API response shapes during expanded smoke. | Normalized user/role response data defensively and guarded filtered lists. |
| Several FE API services still had `ENABLE_MOCK_API` / `withFallback` branches. | Removed service fallback branches from account, academic term, class, assignment, course, staff, shift, and schedule APIs. |
| Content-council question UI contained hardcoded subject/question preview data. | Rewired subject options to `subjectStore` real API data and removed static question preview payloads. |

## Skeleton Coverage

Added shared skeleton components:

- `frontend/src/components/common/skeleton/SkeletonBlock.vue`
- `frontend/src/components/common/skeleton/SkeletonCard.vue`
- `frontend/src/components/common/skeleton/SkeletonTable.vue`
- `frontend/src/components/common/skeleton/SkeletonChart.vue`
- `frontend/src/components/common/skeleton/SkeletonDashboard.vue`
- `frontend/src/components/common/skeleton/index.js`

These components are build-verified and ready for rollout across remaining heavy screens. Final smoke did not show stuck loading states.

## Mock/Fallback Status

Deleted standalone mock files:

- `frontend/src/data/studentData.mock.js`
- `frontend/src/mocks/*`
- `frontend/src/services/mockDataService.js`
- `frontend/src/services/mockFacilitiesData.js`
- `frontend/src/components/reward-discipline/RewardDisciplineMockBanner.vue`

Strict grep result after cleanup:

```powershell
rg -n "mock|fake|dummy|DEMO_|ENABLE_MOCK_API|withFallback|@/mocks|studentData\.mock|mockFacilitiesData|mockDataService|rewardDisciplineMock" frontend/src Backend/Controllers Backend/Services
```

Result: `0` hits.

Parent-specific release-hardening grep:

```powershell
rg -n "parentData|legacy|local|demo|mock|fake|dummy|DEMO_" frontend/src/views/PhuHuynh frontend/src/components/PhuHuynh
```

Result: `0` production data hits. Remaining `local` hits are UI/client state only, currently `localStorage` for active child selection.

## Artifacts

- `docs/artifacts/p15f-smoke/smoke-results-final.json`
- `docs/artifacts/p15f-smoke/smoke-results-p15f3.json`
- `docs/artifacts/p15f-smoke/p15f3-route-smoke.mjs`
- `docs/artifacts/p15f-smoke/lint-p15f3.txt`
- `docs/artifacts/p15f-smoke/parent-dashboard-final.png`
- `docs/artifacts/p15f-smoke/parent-children-final.png`
- `docs/artifacts/p15f-smoke/parent-tuition-final.png`
- `docs/artifacts/p15f-smoke/superadmin-dashboard-regression-final.png`
- `docs/artifacts/p15f-smoke/student-dashboard-regression-final.png`
- `docs/P15F_LINT_BACKLOG.md`

## Remaining Issues

1. The P15 request for full route-by-route browser coverage across all 165 role assignments is not fully complete. P15F.3 expanded smoke covered `65/65` selected release-hardening routes, including all Parent and all SuperAdmin sidebar routes.
2. `npm run lint` still fails with ESLint parser/unused-variable backlog across unrelated Vue files; `npm run build` passes. See `docs/P15F_LINT_BACKLOG.md`.
3. Backend build passes but still reports 15 warnings, including the existing `Microsoft.OpenApi` NU1903 advisory plus nullable warnings.
4. API connection matrix remains `165/165` connected, strict production mock/fallback grep remains `0` hits, and browser smoke remains `PASS_WITH_WARNINGS` until the complete 165-route browser clickthrough is performed.
