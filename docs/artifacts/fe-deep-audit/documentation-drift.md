# Documentation Drift Report

## 1. FE_UI_SYSTEM_AUDIT.md (447 lines) — Outdated Audit

| Claim in Doc | Current Code (2026-07-11) | Drift |
|-------------|--------------------------|-------|
| "Menu chua co /student/rewards va /student/discipline tren main" | Route exists in main router with student-rewards (line 189) and student-discipline (line 197) | **FIXED** - routes were added since audit |
| "Staff requests/approvals route chua co trong router main" | Router has staff-requests (line 392), staff-requests-history (line 393), staff-workflow (line 394) | **FIXED** - routes added |
| "Chua co route /student/rewards va /student/discipline tren main, du co views" | Routes now exist at lines 189-199 | **FIXED** |
| "Route can duoc can nhac them sau blueprint/code phase" | Route already added | Needs update |
| "frontend/src/mocks/rewardDisciplineMockData.js" | Mock files no longer imported in main views (real API used) | **POTENTIALLY STALE** - verify |
| "frontend/src/data/studentData.mock.js" | File still exists but usage limited | Still exists, less used |
| Lists only ~50 routes in router inventory | Actual router has ~165 routes | **LARGE DRIFT** |

## 2. FRONTEND_ARCHITECTURE.md (159 lines) — Outdated

| Claim | Current Code | Drift |
|-------|-------------|-------|
| "Router student only: /login, /about, /student/*" | Router now covers 7 roles with 165 routes | **MAJOR DRIFT** |
| "stores/counter.js, stores/auth.js" | Added 10+ stores: theme, popup, recentFavorites, announcements, academicSchedulingContext, CC stores | **OUTDATED** |
| "components/SinhVien/ - student layouts" | Added Parent, Teacher, Staff, BGH, SuperAdmin, ContentCouncil layouts | **OUTDATED** |
| "views/Student - English, views/SinhVien - Vietnamese" | Both exist, routing mixed between them | Still accurate but incomplete |
| "Layout student: components/SinhVien/Layout_SinhVien.vue" | 6 role layouts now exist | **OUTDATED** |
| "Loading: skeleton/spinner, Error: friendly with retry, Empty: notification" | Skeleton components exist (8 variants), but error/empty not consistently implemented | Partially accurate |

## 3. FRONTEND_STYLE_ALIGNMENT.md (93 lines) — Partially Stale

| Claim | Current Code | Drift |
|-------|-------------|-------|
| "BAT BUOC dung GlassButton cho moi nut bam" | Many views still use raw html button or custom classes | **NOT ENFORCED** |
| "BAT BUOC dung GlassBadge cho moi trang thai" | Many views use raw Tailwind badge classes | **NOT ENFORCED** |
| "BAT BUOC dung GlassPanel lam layout card" | Many views use custom card wrappers | **NOT ENFORCED** |

## 4. DESIGN_LIQUID_GLASS.md (1248 lines) — Comprehensive but Partially Stale

| Claim | Current Code | Drift |
|-------|-------------|-------|
| Describes Tailwind config snippet | Tailwind v4 uses @import "tailwindcss", no config file | **DRIFT** - Tailwind v4 migration |
| Token JSON/YAML examples are aspirational | Tokens live in liquid-glass.css only | **PARTIAL DRIFT** |
| File structure described may not match current | Frontend structure has grown significantly since document | **PARTIAL DRIFT** |
| Token/class documentation for lg-* classes is comprehensive | Most classes exist as documented | **ACCURATE** |

## 5. P14_ROLE_SCREEN_API_MATRIX.md (246 lines) — Claims 165/165 Connected

| Claim | Current Code | Drift |
|-------|-------------|-------|
| "165 total screens, 165 CONNECTED" | Router has ~165 routes; most have real API services | **LIKELY ACCURATE** based on P15G smoke results |
| Lists specific route counts per role | Need verification against current router | **NEEDS VERIFICATION** |

## 6. P15G_FULL_165_ROUTE_BROWSER_SMOKE_REPORT.md (195 lines) — Recent

| Claim | Current Code | Drift |
|-------|-------------|-------|
| "166 routes, 166 PASS, 0 fail" | Router count matches (~165-166) | **LIKELY ACCURATE** (dated 2026-07-09) |

## 7. API_CONTRACT.md (1290 lines) — Comprehensive but Aspirational

| Claim | Current Code | Drift |
|-------|-------------|-------|
| Lists 159+ API endpoints | Many endpoints documented but not yet implemented in backend controllers | **MAJOR DRIFT** - contract is aspirational for many modules |
| Auth APIs include refresh-token, forgot-password, otp, reset-password | Auth store has refreshSession but endpoints may not exist | **PARTIAL DRIFT** |

## 8. docs/API_ROLE_CONNECTION_AUDIT.md (366 lines) — Recent

| Claim | Current Code | Drift |
|-------|-------------|-------|
| "100 connected, 30 frontend-only, 26 backend-only, 3 mismatch" | P16/P17 work may have changed this status | **POTENTIALLY STALE** |

## 9. Recommendations

| Document | Action Required |
|----------|----------------|
| FRONTEND_ARCHITECTURE.md | **REWRITE** - completely outdated, describes only Student role |
| FE_UI_SYSTEM_AUDIT.md | **UPDATE** - route claims are now incorrect |
| FRONTEND_STYLE_ALIGNMENT.md | **REVIEW** - enforcement claims not matched by code |
| DESIGN_LIQUID_GLASS.md | **MINOR UPDATE** - Tailwind v4 section |
| P14_ROLE_SCREEN_API_MATRIX.md | **VERIFY** - against current router |
| API_CONTRACT.md | **TRIAGE** - mark implemented vs aspirational endpoints |
| API_ROLE_CONNECTION_AUDIT.md | **VERIFY** - against current codebase |
