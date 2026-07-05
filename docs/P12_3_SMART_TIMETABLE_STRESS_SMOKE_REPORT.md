# P12.3 Smart Timetable Stress Test & Real Data Smoke Report

> **Branch:** `feature/p12-3-smart-timetable-stress-smoke`
> **Date:** 2026-07-05
> **Author:** AI Agent
> **Status:** ✅ PASS — All 8 API smoke tests + 10 SQL validations pass

---

## 1. Environment

| Item | Value |
|---|---|
| OS | Windows 10+ |
| Backend | ASP.NET Core `net10.0` |
| Database | SQL Server on `DELL\SQLEXPRESS02` |
| DB Name | `LMS_TEST_P12` |
| .NET SDK | 10.0.x |
| EF Core | 10.0.6 |
| EF Tools | 8.0.22 |
| Node/npm | Available |

---

## 2. Database & Migration Status

| Check | Result |
|---|---|
| `dotnet build` | ✅ 0 errors, 12 pre-existing warnings |
| `dotnet ef database update` | ✅ Applied successfully |
| `ScheduleGenerationJob` table | ✅ Created |
| `ScheduleDraftItem` table | ✅ Created |
| `KhoaHoc` table | ✅ Created |
| `PhongHoc` table | ✅ Created |
| `CaHoc` table | ✅ Created |
| `GiaoVienMonHoc` table | ✅ Created |
| **Seed data** | ✅ Seeded via `P12_4_SEED_LMS_TEST_P12.sql` |
| Backend running instance | ✅ Verified |

---

## 3. P12.4.1 Real API Smoke Fix Result

### Bugs fixed
1. **LINQ translation failure in SmartTimetableService**
   - Root cause: GroupJoin + FirstOrDefault could not be translated by EF Core.
   - Fix: split query into smaller EF queries and map DTO in memory.

2. **DbContext concurrency error**
   - Root cause: Task.WhenAll used on the same scoped DbContext.
   - Fix: removed concurrent DbContext operations and executed queries sequentially or with safe preloaded dictionaries.

3. **SQL Server retrying execution strategy transaction error**
   - Root cause: BeginTransactionAsync was used directly under SqlServerRetryingExecutionStrategy.
   - Fix: wrapped publish transaction inside EF execution strategy.

### Files Changed
| File | Change |
|---|---|
| `Backend/Services/ThoiKhoaBieu/SmartTimetableService.cs` | `ToDraftDtoAsync()` replaced GroupJoin with split-query + dictionary; `PublishAsync` wrapped in `CreateExecutionStrategy()` |

### Validation
- P12.4 API Smoke Tests: 8/8 PASS
- SQL Validation Queries: 10/10 PASS
- P12.2 Unit Tests: 10/10 PASS
- P12.3 Stress Tests: 10/10 PASS
- dotnet build: PASS, 0 errors

### Final result
P12 Smart Timetable real-data smoke is PASS for the LMS_TEST_P12 seed dataset.

---

## 4. P12.4 API Smoke Test Results

### Group A — Generate Draft

| ID | Test | Expected | Actual | Result |
|---|---|---|---|---|
| A1 | `Generate_WithSeededData_ShouldProcessAllCourses` | 20/20 courses assigned, score 100% | 20 xepDuoc, 0 khongXepDuoc, score 100.00 | ✅ PASS |
| A2 | `GetDraft_AfterGenerate_ShouldMatchGeneratedData` | Draft returned with same data | Draft matches generate output | ✅ PASS |
| A3 | `ListDrafts_AfterGenerate_ShouldContainOurDraft` | List includes our draft | Draft present in list | ✅ PASS |
| A4 | `Generate_WithCourseFilter_ShouldOnlyScheduleFilteredCourses` | Only filtered courses scheduled | Correct subset | ✅ PASS |

### Group B — Publish

| ID | Test | Expected | Actual | Result |
|---|---|---|---|---|
| B1 | `Publish_ValidDraft_ShouldCreateSessions` | 20 sessions created | created=20, errors=0 | ✅ PASS |
| B2 | `Publish_AlreadyPublishedDraft_ShouldFail` | 400 "đã được xuất bản" | `"Bản nháp này đã được xuất bản."` | ✅ PASS |
| B3 | `DeleteDraft_AfterPublish_ShouldFail` | 400 "Không thể xóa bản nháp đã xuất bản" | `"Không thể xóa bản nháp đã xuất bản."` | ✅ PASS |

### Group C — Conflict Detection

| ID | Test | Expected | Actual | Result |
|---|---|---|---|---|
| C1 | `CheckXungDot_Batch_ShouldDetectConflicts` | Conflicts detected | Correctly detected | ✅ PASS |

**8/8 API smoke tests pass.**

---

## 5. SQL Validation Results

All 10 queries from `docs/sql/P12_3_SMART_TIMETABLE_VALIDATION.sql` executed against `LMS_TEST_P12` after a full generate+publish cycle:

| # | Query | Expected | Actual | Result |
|---|---|---|---|---|
| Q1 | Teacher conflict (TKB) | 0 rows | 0 rows | ✅ PASS |
| Q2 | Class conflict (TKB) | 0 rows | 0 rows | ✅ PASS |
| Q3 | Room conflict (TKB) | 0 rows | 0 rows | ✅ PASS |
| Q4 | Teacher conflict (BuoiHoc) | 0 rows | 0 rows | ✅ PASS |
| Q5 | Class conflict (BuoiHoc) | 0 rows | 0 rows | ✅ PASS |
| Q6 | Room conflict (BuoiHoc) | 0 rows | 0 rows | ✅ PASS |
| Q7 | Capacity violation | 0 rows | 0 rows | ✅ PASS |
| Q8 | Inactive room usage | 0 rows | 0 rows | ✅ PASS |
| Q9 | Inactive shift usage | 0 rows | 0 rows | ✅ PASS |
| Q10 | Duplicate TKB | 0 rows | 0 rows | ✅ PASS |

**10/10 validation queries pass** — the OccupationMap algorithm correctly prevents all conflicts, capacity violations, and duplicates.

---

## 6. Automated Unit Test Results

### OccupationMap Unit Tests (P12_2 test suite)

| Test | Result |
|---|---|
| `OccupyTeacher_ThenIsOccupied_ReturnsTrue` | ✅ PASS |
| `OccupyTeacher_DifferentSlot_NotOccupied` | ✅ PASS |
| `OccupyClass_ThenIsOccupied_ReturnsTrue` | ✅ PASS |
| `OccupyRoom_ThenIsOccupied_ReturnsTrue` | ✅ PASS |
| `Occupy_SameSlotTwice_DoesNotDuplicate` | ✅ PASS |
| `Clear_ResetsAllOccupations` | ✅ PASS |
| `IsTeacherOccupied_EmptyMap_ReturnsFalse` | ✅ PASS |
| `IsClassOccupied_EmptyMap_ReturnsFalse` | ✅ PASS |
| `IsRoomOccupied_EmptyMap_ReturnsFalse` | ✅ PASS |
| `MultipleOccupations_CountsTrackedCorrectly` | ✅ PASS |

### P12_3 Stress Tests (Unit Level)

| Test | Focus | Result |
|---|---|---|
| `Generate_ShouldNotPersistOfficialSchedules` | OccupationMap only | ✅ PASS |
| `Generate_ShouldReturnNoTeacherConflictInDraft` | Teacher dedup check | ✅ PASS |
| `Generate_ShouldReturnNoClassConflictInDraft` | Class dedup check | ✅ PASS |
| `Generate_ShouldReturnNoRoomConflictInDraft` | Room dedup check | ✅ PASS |
| `Publish_WithEmptyDraft_ShouldReturnEmptyResult` | Empty scenario | ✅ PASS |
| `Publish_ShouldRejectDuplicateDraftPublish` | State validation | ✅ PASS |
| `Capacity_ShouldBlockSmallRoom` | Room capacity scoring | ✅ PASS |
| `Sessions_ShouldHaveNoTeacherClassRoomConflict` | Conflict prevention | ✅ PASS |
| `Substitute_ShouldRejectTeacherWithoutCapability` | Capability check | ✅ PASS |
| `LargeBulkGenerate_500Courses_OccupationMap` | 500-course stress | ✅ PASS |

**20/20 unit-level tests pass.**

---

## 7. Performance Results

Not measured quantitatively. Generate completed for 20 courses in ~530ms (smoke test timing includes HTTP + auth overhead). Publish completed in ~110ms. Performance for 50/100/300 course bulk generation is pending dedicated load testing.

| Scenario | Target | Observed |
|---|---|---|
| Generate 20 courses | — | ~530ms (includes HTTP + auth) |
| Publish 20 courses | — | ~110ms (includes HTTP + auth + TKB creation) |

---

## 8. Bugs Found & Fixed

| # | Issue | Location | Severity | Status |
|---|---|---|---|---|
| B1 | LINQ `GroupJoin` + `FirstOrDefault` not translatable | `ToDraftDtoAsync` | **Critical** — 500 error on GetDraft/ListDrafts | ✅ **FIXED** |
| B2 | DbContext concurrency from `Task.WhenAll` in split queries | `ToDraftDtoAsync` | **High** — 500 "second operation started" | ✅ **FIXED** |
| B3 | `SqlServerRetryingExecutionStrategy` rejects explicit `BeginTransactionAsync` | `PublishAsync` line 167 | **High** — 500 on publish | ✅ **FIXED** |

---

## 9. Fixes Applied (P12.4.1)

| Fix | File | Description |
|---|---|---|
| LINQ GroupJoin → split-query + dictionary | `SmartTimetableService.cs` | Replaced untranslatable LINQ with separate `ToListAsync` calls and in-memory `Dictionary` mapping |
| Parallel → sequential DbContext queries | `SmartTimetableService.cs` | Changed `Task.WhenAll(coursesTask, roomsTask, shiftsTask)` to sequential `await` calls to avoid concurrent use of scoped `DbContext` |
| Publish transaction → `CreateExecutionStrategy()` | `SmartTimetableService.cs` | Wrapped `BeginTransactionAsync` in `_context.Database.CreateExecutionStrategy().ExecuteAsync()` for `SqlServerRetryingExecutionStrategy` compatibility |

---

## 10. Remaining Risks

| Risk | Impact | Likelihood |
|---|---|---|
| `ScoreAssignment` uses fixed divisor 30 | May assign rooms poorly if class sizes are extreme | Medium |
| No explicit teacher workload limit (e.g. max 4 shifts/day) | A teacher could be scheduled 6+ consecutive shifts | Low |
| No `GiaoVienChuyenNganh` usage in scoring | Specialization not factored into slot selection | Low |
| `DeleteDraftAsync` doesn't restore `SoXepDuoc`/`SoKhongXepDuoc` | Soft delete alternative: mark as deleted instead of removing | Low |
| Published draft items remain in `ScheduleDraftItem` | Items remain with `xep_duoc` status after publish | Low |

---

## 11. Recommendation

### ✅ PASS — Ready for integration

Justification:
- All 8 P12.4 API smoke tests pass against real database (LMS_TEST_P12)
- All 10 SQL validation queries return 0 conflicts/violations
- All 20 unit-level tests pass (10 P12_2 + 10 P12_3)
- Three bugs found and fixed: LINQ translation failure, DbContext concurrency, publish transaction
- OccupationMap algorithm correctly prevents teacher/class/room conflicts
- Generate produces optimal assignments (20/20 courses, score 100%)
- Publish correctly creates 20 TKB sessions with zero conflicts
- Edge cases covered: delete published draft rejected, re-publish rejected, filtered generate, conflict detection

---

## 12. Files Changed (all time)

| File | Change |
|---|---|
| `Backend/Services/ThoiKhoaBieu/SmartTimetableService.cs` | P12.4.1 fix: GroupJoin → split-query + dictionary; publish transaction → `CreateExecutionStrategy()` |
| `docs/P12_3_SMART_TIMETABLE_STRESS_SMOKE_REPORT.md` | Updated (this file) |
| `docs/sql/P12_3_SMART_TIMETABLE_VALIDATION.sql` | Created |
| `docs/sql/P12_4_SEED_LMS_TEST_P12.sql` | Created |
| `Backend.ApiTests/P12_3_SmartTimetableStressSmokeTests.cs` | Created |
| `Backend.ApiTests/P12_4_SmartTimetableSmokeTests.cs` | Created |

---

## 13. Git Status

```
$ git status --short
 M Backend/Services/ThoiKhoaBieu/SmartTimetableService.cs
 M docs/P12_3_SMART_TIMETABLE_STRESS_SMOKE_REPORT.md
?? docs/sql/P12_3_SMART_TIMETABLE_VALIDATION.sql
?? docs/sql/P12_4_SEED_LMS_TEST_P12.sql
?? Backend.ApiTests/P12_3_SmartTimetableStressSmokeTests.cs
?? Backend.ApiTests/P12_4_SmartTimetableSmokeTests.cs
```
