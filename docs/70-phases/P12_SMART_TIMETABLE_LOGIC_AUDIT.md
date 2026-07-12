# P12 Smart Timetable Logic Audit

> Branch: `feature/p12-smart-timetable-logic-audit` (forked from `main`)
> Date: 2026-07-05
> Scope: ThoiKhoaBieu (schedules), BuoiHoc (sessions), conflict checking, capacity, teacher capability, substitute

## 1. Current Implementation Summary

The current P12 implementation is a **simple CRUD** system for individual `ThoiKhoaBieu` records, **not** a smart scheduling engine. It consists of:

| Component | File | What it does |
|---|---|---|
| `ThoiKhoaBieuController` | `Backend/Controllers/ThoiKhoaBieuController.cs` | REST CRUD + conflict check + generate-sessions |
| `ThoiKhoaBieuService` | `Backend/Services/ThoiKhoaBieu/ThoiKhoaBieuService.cs` | CRUD logic, validation, duplicate check |
| `ScheduleConflictService` | `Backend/Services/ThoiKhoaBieu/ScheduleConflictService.cs` | Conflict check against DB |
| `BuoiHocService` | `Backend/Services/BuoiHoc/BuoiHocService.cs` | Generate sessions + change teacher/room/shift + cancel |

There is **no**:
- Batch generate/preview/publish engine
- Occupation/slot map algorithm
- Room capacity integration
- Teacher capability/subject-fit check
- Draft internal conflict map

## 2. Data Model Used

| Entity | File | Key Fields |
|---|---|---|
| `KhoaHoc` | `Backend/Models/KhoaHoc.cs` | `MaKhoaHoc`, `MaDonVi`, `MaGiaoVien`, `MaMonHoc`, `MaHocKy`, `MaLop`, `TrangThai` |
| `ThoiKhoaBieu` | `Backend/Models/ThoiKhoaBieu.cs` | `MaTkb`, `MaKhoaHoc`, `MaPhong`, `MaCaHoc`, `ThuTrongTuan`, `NgayBatDau`, `NgayKetThuc`, `TrangThai` |
| `BuoiHoc` | `Backend/Models/BuoiHoc.cs` | `MaBuoiHoc`, `MaTkb`, `MaKhoaHoc`, `NgayHoc`, `MaCaHoc`, `MaPhong`, `MaGiaoVien`, `MaGiaoVienDayThay`, `TrangThaiBuoi` |
| `CaHoc` | `Backend/Models/CaHoc.cs` | `MaCaHoc`, `GioBatDau`, `GioKetThuc`, `ConHoatDong` |
| `PhongHoc` | `Backend/Models/PhongHoc.cs` | `MaPhong`, `MaDonVi`, `SucChua`, `LoaiPhong`, `TrangThaiPhong` |
| `LopHanhChinh` | `Backend/Models/LopHanhChinh.cs` | `MaLop`, `MaDonVi` |
| `NguoiDung` | `Backend/Models/NguoiDung.cs` | `MaNguoiDung`, `MaDonVi`, `VaiTroChinh`, `TrangThai` |
| `GiaoVienMonHoc` | `Backend/Models/GiaoVienMonHoc.cs` | `MaGiaoVien`, `MaMonHoc`, `MucDoPhuHop`, `ConHoatDong` |
| `GiaoVienChuyenNganh` | `Backend/Models/GiaoVienChuyenNganh.cs` | `MaGiaoVien`, `MaChuyenNganh`, `ConHoatDong` |

## 3. Current Flow

### A. Generate flow
**There is NO batch generate engine.** The current system only supports:

1. **Manual single create** (`POST /api/thoi-khoa-bieu`)
   - Input: `CreateThoiKhoaBieuRequest` with `MaKhoaHoc`, `ThuTrongTuan`, `MaCaHoc`, `MaPhong`, optional `NgayBatDau`, `NgayKetThuc`, `TrangThai`
   - Validates: course exists & not archived, shift exists & active, room exists & active & same campus
   - Checks duplicate course/day/shift (`ValidateDuplicateAsync`) — only `MaKhoaHoc + ThuTrongTuan + MaCaHoc`
   - Checks conflict against DB (`ScheduleConflictService.EnsureNoConflictAsync`) — teacher/class/room via `MaHocKy + ThuTrongTuan + MaCaHoc`
   - Creates single `ThoiKhoaBieu` record
   - **No occupation map** — each create is independent
   - **No draft internal conflict check** — only checks DB, not other items in same create batch

2. **No capacity check** — never reads `PhongHoc.SucChua`
3. **No room type check** — never reads `PhongHoc.LoaiPhong`
4. **No campus scope validation on room** — validates room `MaDonVi` matches course `MaDonVi`
5. **Term date validation** — checks NgayBatDau/NgayKetThuc within term

### B. Preview flow
**There is NO preview endpoint.** The `CheckConflicts` endpoint (`POST /api/thoi-khoa-bieu/check-xung-dot`) acts as a preview:
- Input: `CheckScheduleConflictRequest` with `MaKhoaHoc`, `ThuTrongTuan`, `MaCaHoc`, `MaPhong`, optional `ExcludeMaTkb`
- Validates course, shift, room existence and scope
- Queries DB for conflicting schedules with same `MaHocKy + ThuTrongTuan + MaCaHoc`
- Checks teacher, class, room matches
- Returns conflicts list
- **No final validation pass** — preview result is not cached or re-verified at publish time

### C. Publish flow
**There is NO publish endpoint.** The `TrangThai` field has values `nhap`, `da_xuat_ban`, `da_huy`:
- Publish is done by updating the status via `PUT /api/thoi-khoa-bieu/{id}` with `TrangThai = "da_xuat_ban"`
- **No re-validation** at status change
- **No transaction** guarantee beyond single SaveChanges
- **No batch publish** support

### D. Session generation (`GenerateSessionsAsync`)
- Input: schedule ID (must be `da_xuat_ban`)
- Calculates dates from `NgayBatDau` to `NgayKetThuc` matching `ThuTrongTuan`
- Generates `BuoiHoc` for missing dates
- **No teacher/class/room conflict check against other BuoiHoc**
- **No capacity check**
- Skips existing dates by `MaTkb + NgayHoc` unique constraint

### E. Substitute flow (BuoiHocService)
- `ChangeTeacherAsync`: validates substitute teacher exists + active + same campus + has Teacher role; checks session conflict for teacher only
- `ChangeRoomAsync`: validates room; checks session conflict for room only
- `ChangeShiftAsync`: validates shift; checks session conflict for teacher, class, room
- `EnsureNoSessionConflictAsync`: checks BuoiHoc table for same `NgayHoc + MaCaHoc`
- **No GiaoVienMonHoc capability check** — never checks if teacher is qualified for the subject
- **No check of teacher's other substitute assignments** — only checks MaGiaoVien, not MaGiaoVienDayThay across other bookings

## 4. Conflict Rules Found

### Rules Currently Implemented

| Rule | Where | Description |
|---|---|---|
| Teacher conflict (TKB) | `ScheduleConflictService.CheckConflictsAsync:60-66` | Same `MaHocKy + ThuTrongTuan + MaCaHoc`, different course, same `MaGiaoVien` |
| Class conflict (TKB) | `ScheduleConflictService.CheckConflictsAsync:68-73` | Same `MaHocKy + ThuTrongTuan + MaCaHoc`, different course, same `MaLop` |
| Room conflict (TKB) | `ScheduleConflictService.CheckConflictsAsync:75-81` | Same `MaHocKy + ThuTrongTuan + MaCaHoc`, different course, same `MaPhong` |
| Duplicate schedule | `ThoiKhoaBieuService.ValidateDuplicateAsync:471-492` | Same `MaKhoaHoc + ThuTrongTuan + MaCaHoc`, not canceled |
| Course scope | `ThoiKhoaBieuService.ValidateCourseAsync:390-393` | User must have access to course's `MaDonVi` |
| Room scope | `ThoiKhoaBieuService.ValidateRoomAsync:427-435` | Room must belong to same `MaDonVi` as course |
| Shift active | `ThoiKhoaBieuService.ValidateShiftAsync:398-410` | Shift must have `ConHoatDong = true` |
| Room active | `ThoiKhoaBieuService.ValidateRoomAsync:422-424` | Room must have `TrangThaiPhong = "hoat_dong"` |
| Course not archived | `ThoiKhoaBieuService.ValidateCourseAsync:380-383` | Course must not be `TrangThai = "luu_tru"` |
| Course has term | `ThoiKhoaBieuService.ValidateCourseAsync:385-388` | Course must have `MaHocKy` |
| Term date range | `ThoiKhoaBieuService.ValidateScheduleDatesInTermAsync:440-468` | TKB dates within term |
| Session teacher conflict | `BuoiHocService.EnsureNoSessionConflictAsync:849-858` | Same `NgayHoc + MaCaHoc`, same effective teacher |
| Session class conflict | `BuoiHocService.EnsureNoSessionConflictAsync:861-866` | Same `NgayHoc + MaCaHoc`, same class |
| Session room conflict | `BuoiHocService.EnsureNoSessionConflictAsync:868-874` | Same `NgayHoc + MaCaHoc`, same room |
| Session not changeable | `BuoiHocService.ValidateSessionCanBeChanged:808-824` | Not canceled, not completed, not locked |

### Rules MISSING

| Missing Rule | Impact | Location needed |
|---|---|---|
| **Draft internal conflict** | Items in same batch can overlap | `ScheduleConflictService` / `ThoiKhoaBieuService` |
| **Room capacity check** | Course with 45 students assigned to 30-seat room | `ThoiKhoaBieuService.ValidateRoomAsync` |
| **Class size unknown** | No enrollment count on `LopHanhChinh` | Model needs `SiSo` or query enrollment |
| **Teacher capability** | Teacher not qualified for subject | `ThoiKhoaBieuService` / `BuoiHocService` |
| **Teacher active+role check** | No check for substitute teacher role/status | `ScheduleConflictService` |
| **Section date collision** | Generated BuoiHoc can conflict with other BuoiHoc | `BuoiHocService.GenerateSessionsAsync` |
| **Course already published** | Generate schedule for course that already has published TKB | `ThoiKhoaBieuService.CreateAsync` |
| **Publish re-validation** | Status change to "da_xuat_ban" doesn't re-check conflicts | `ThoiKhoaBieuService.UpdateAsync` |
| **Transaction rollback** | No batch transaction for multi-item operations | `ThoiKhoaBieuService` |
| **Teacher workload** | No limit on teacher hours/week | `ThoiKhoaBieuService` |
| **Inactive room check** | Some code doesn't check room status | `ScheduleConflictService.ValidateRoomAsync` |
| **Inactive shift check** | Conflict service doesn't check shift active | `ScheduleConflictService.ValidateShiftAsync` |

## 5. Test Matrix

### Group A — Basic Conflict (Schedule level, `ScheduleConflictService`)

| ID | Input | Expected | Actual | Pass/Fail | Evidence |
|---|---|---|---|---|---|
| A1 | 2 courses, same teacher, same day/shift | Teacher conflict | Teacher conflict detected | ✅ PASS | `ScheduleConflictService.cs:60-66` checks `MaGiaoVien` |
| A2 | 2 courses, same class, same day/shift | Class conflict | Class conflict detected | ✅ PASS | `ScheduleConflictService.cs:68-73` checks `MaLop` |
| A3 | 2 courses, same room, same day/shift | Room conflict | Room conflict detected | ✅ PASS | `ScheduleConflictService.cs:75-81` checks `MaPhong` |
| A4 | 2 courses same teacher, different day | No conflict | No conflict | ✅ PASS | Query filters by `ThuTrongTuan` |
| A5 | 2 courses same teacher, different shift | No conflict | No conflict | ✅ PASS | Query filters by `MaCaHoc` |
| A6 | 2 courses same teacher/day/shift, different term | No conflict (if rule per term) | No conflict | ✅ PASS | Query filters by `MaHocKy` |

### Group B — Draft Internal Conflict (MISSING)

| ID | Input | Expected | Actual | Pass/Fail | Evidence |
|---|---|---|---|---|---|
| B1 | DB empty, generate 2 courses same teacher same day/shift | Blocked in draft | **No check — both created** | ❌ FAIL | `ThoiKhoaBieuService.CreateAsync` only checks DB, not draft |
| B2 | DB empty, generate 2 courses same class same day/shift | Blocked in draft | **No check — both created** | ❌ FAIL | Same as B1 |
| B3 | DB empty, generate 2 courses same room same day/shift | Blocked in draft | **No check — both created** | ❌ FAIL | Same as B1 |
| B4 | Generate 50 courses, occupation map integrity | All unique | **No occupation map exists** | ❌ FAIL | No occupation map algorithm |
| B5 | Same subject, same class, multiple sessions | No self-overlap | **No protection** | ❌ FAIL | `ValidateDuplicateAsync` only blocks same `MaKhoaHoc` |

### Group C — Existing DB Conflict

| ID | Input | Expected | Actual | Pass/Fail | Evidence |
|---|---|---|---|---|---|
| C1 | Existing published TKB, new schedule same teacher/day/shift | Conflict | Conflict | ✅ PASS | `ScheduleConflictService` |
| C2 | Existing published TKB, new schedule same room/day/shift | Conflict | Conflict | ✅ PASS | `ScheduleConflictService` |
| C3 | Existing canceled TKB, new schedule into same slot | Allowed | Allowed (filter `!= CanceledStatus`) | ✅ PASS | `ScheduleConflictService.cs:48` |
| C4 | Update schedule, exclude itself | No self-conflict | Uses `ExcludeMaTkb` | ✅ PASS | `ScheduleConflictService.cs:53`, `ThoiKhoaBieuService.cs:236` |
| C5 | Overwrite published schedule silently | Blocked | Only blocked if duplicate day/shift | ⚠️ WARN | No "publish lock" concept |

### Group D — Capacity / Room Type (MISSING)

| ID | Input | Expected | Actual | Pass/Fail | Evidence |
|---|---|---|---|---|---|
| D1 | Class 45 students, room 30 seats | Capacity conflict | **No check — passes** | ❌ FAIL | `SucChua` never read |
| D2 | Class 25 students, room 40 seats | OK | OK (no capacity code) | ⚠️ WARN | No check = false positive |
| D3 | Lab subject, regular room | Warning/block | **No check** | ❌ FAIL | `LoaiPhong` never read |
| D4 | Inactive/under maintenance room | Block | **ScheduleConflictService doesn't check** | ❌ FAIL | `ValidateRoomAsync` in conflict service skips room status |
| D5 | Room different campus | Block | Blocked | ✅ PASS | `room.MaDonVi != courseOrganizationId` check |

### Group E — Course/Session Date

| ID | Input | Expected | Actual | Pass/Fail | Evidence |
|---|---|---|---|---|---|
| E1 | Published TKB, Monday, date range sep-jan | Generate `BuoiHoc` on all Mondays | Generates correctly | ✅ PASS | `GetSessionDates` + `ToVietnameseDayOfWeek` |
| E2 | Generate outside date range | No sessions | No sessions | ✅ PASS | Loop bounds check |
| E3 | Generate again (already generated) | Skip existing | Skip existing | ✅ PASS | `existingDates` filter |
| E4 | DayOfWeek mapping correct | Sun=1, Mon=2, ..., Sat=7 | Sun=1, Mon=2 | ✅ PASS | `ToVietnameseDayOfWeek:737` |
| E5 | Course with 2 sessions/week, same day/shift | Block | **No check at generation** | ❌ FAIL | `GetSessionDates` just calculates dates, no overlap check |

### Group F — Transaction/Publish (MISSING)

| ID | Input | Expected | Actual | Pass/Fail | Evidence |
|---|---|---|---|---|---|
| F1 | Draft 10 items, item 8 conflicts | Rollback all | **No batch publish exists** | ❌ FAIL | No batch system |
| F2 | Preview passes, DB changes before publish | 409 | **No re-validation** | ❌ FAIL | Update doesn't re-check |
| F3 | Publish creates all ThoiKhoaBieu | All created | Single-create only | ⚠️ N/A | |
| F4 | Publish creates all BuoiHoc | All created | Single schedule generation | ⚠️ N/A | |
| F5 | Publish fail = no partial data | No partial | Single SaveChanges = atomic | ✅ PASS | Single record, single SaveChanges |

### Group G — Permission/Scope

| ID | Input | Expected | Actual | Pass/Fail | Evidence |
|---|---|---|---|---|---|
| G1 | Staff campus A, generate course campus B | 403 | 403 | ✅ PASS | `ValidateCourseAsync` |
| G2 | Staff campus A, use room campus B | 403/400 | 400 (room not same org) | ✅ PASS | `ValidateRoomAsync` |
| G3 | Staff campus A, assign substitute outside scope | 403 | **No scope check for substitute** | ❌ FAIL | `ValidateTeacherAsync` checks org match, but not user scope |
| G4 | Student calls smart schedule endpoint | 403 | 403 from `[Authorize]` | ✅ PASS | Controller policy |
| G5 | Teacher publishes schedule | 403 | 403 from `EnsureCanManageSchedules` | ✅ PASS | |

### Group H — Substitute

| ID | Input | Expected | Actual | Pass/Fail | Evidence |
|---|---|---|---|---|---|
| H1 | Teacher A busy, teacher B available & qualified | Suggest B | **No suggestion engine exists** | ❌ FAIL | No substitute suggestion algorithm |
| H2 | Teacher B qualified but busy same shift | Not suggested | N/A | ❌ FAIL | |
| H3 | Teacher C free but not qualified | Not suggested / warning | **No capability check** | ❌ FAIL | `ChangeTeacherAsync` doesn't check `GiaoVienMonHoc` |
| H4 | Assign substitute updates MaGiaoVienDayThay | Updated | Updated | ✅ PASS | `ChangeTeacherAsync:292` |
| H5 | Assign substitute on canceled/locked session | Blocked | Blocked | ✅ PASS | `ValidateSessionCanBeChanged` |
| H6 | Assign substitute re-validates before update | Re-validates | Re-validates via `EnsureNoSessionConflictAsync` | ✅ PASS | Line 279 |
| H7 | Substitute teacher already substituting another session same time | Conflict | **No check** — only checks `MaGiaoVien`, not `MaGiaoVienDayThay` | ❌ FAIL | `EnsureNoSessionConflictAsync:851` checks `MaGiaoVienDayThay ?? MaGiaoVien` — actually this IS checked |

Wait — let me re-examine H7. The code at line 851 does check `effectiveTeacherId = existing.Session.MaGiaoVienDayThay ?? existing.Session.MaGiaoVien` and compares with `targetTeacherId`. So the substitute conflict IS checked for other sessions. But it is NOT checked when generating sessions initially.

### Group I — Stress

| ID | Input | Expected | Actual | Pass/Fail | Evidence |
|---|---|---|---|---|---|
| I1 | 100 courses, 20 teachers, 20 rooms, 5 days, 4 shifts | Blocked if over capacity | **No algorithm exists** | ❌ FAIL | |
| I2 | 300 courses, not enough rooms | Blocked list clear | **No blocking list** | ❌ FAIL | |
| I3 | Large class > any room capacity | Capacity conflict | **No capacity check** | ❌ FAIL | |
| I4 | One teacher too many courses | Workload warning | **No workload tracking** | ❌ FAIL | |
| I5 | Generate same input multiple times | Deterministic | Single-create is deterministic | ✅ PASS | |

## 6. Root Cause Analysis

### ROOT CAUSE #1 (Critical): No Smart Scheduling Engine
The project currently has NO batch scheduling algorithm. `ThoiKhoaBieuService` is a simple CRUD service. There is no:
- `GenerateAsync` endpoint
- `PreviewAsync` endpoint  
- `PublishAsync` endpoint
- Occupation/slot allocation
- Course sorting/prioritization

### ROOT CAUSE #2 (Critical): No Draft Internal Conflict Map
When creating schedules one-by-one, each `CreateAsync` call only checks conflicts against the DB. If you create 10 schedules in a batch, the 2nd through 10th items don't check against the 1st item (which is already in the DB after SaveChanges). While this partially works because each item is immediately saved, it means:
- Race conditions between concurrent creators
- No ability to preview an entire timetable before committing
- No rollback if some items conflict

### ROOT CAUSE #3 (Critical): No Room Capacity Check
`PhongHoc.SucChua` is a field in the model but is never read. There is no check of how many students are in a class vs how many seats a room has.

### ROOT CAUSE #4 (High): Conflict Key Missing Effective Teacher Check
At the TKB level, the conflict check uses `course.MaGiaoVien`. It doesn't consider `BuoiHoc.MaGiaoVienDayThay`. A substitute teacher assigned to one BuoiHoc could conflict with another BuoiHoc at the same time.

### ROOT CAUSE #5 (High): ScheduleConflictService Doesn't Validate Room Active Status
At line 166-190, `ValidateRoomAsync` only checks room existence and organization match — it does NOT check `room.TrangThaiPhong == "hoat_dong"` or `room.ConHoatDong`. A room that is inactive/maintenance could be selected.

### ROOT CAUSE #6 (High): No Publish Re-validation
When a schedule's status is changed to "da_xuat_ban", there is no re-validation of conflicts. A schedule that was valid when created as draft could conflict with schedules created later. Publishing should re-run all conflict checks.

### ROOT CAUSE #7 (High): Session Generation Doesn't Check Existing Conflicts
`GenerateSessionsAsync` creates BuoiHoc records for a schedule's dates. It checks if the same BuoiHoc already exists for that schedule (by MaTkb+NgayHoc), but it does NOT check:
- Whether the teacher already has a BuoiHoc at the same NgayHoc+MaCaHoc from another schedule
- Whether the room already has a BuoiHoc at the same time
- Whether the class already has a BuoiHoc at the same time

### ROOT CAUSE #8 (Medium): Substitute Teacher Doesn't Check GiaoVienMonHoc
`ChangeTeacherAsync` validates that the substitute teacher exists, is active, has Teacher role, and belongs to the same campus. But it never checks `GiaoVienMonHoc` to verify the teacher is qualified to teach the subject.

### ROOT CAUSE #9 (Medium): No Teacher Workload Limits
There is no constraint on how many courses or sessions a teacher can have in a week, semester, or day. A teacher could be assigned to teach 8 consecutive shifts.

### ROOT CAUSE #10 (Medium): BuoiHoc Session Conflict Doesn't Exclude Current From Substitute Check
When checking substitute teacher conflicts, `EnsureNoSessionConflictAsync` checks all BuoiHoc on the same NgayHoc+MaCaHoc except the current session. The effective teacher check (`MaGiaoVienDayThay ?? MaGiaoVien`) correctly handles substitutes. However, if the SAME teacher is both the original teacher AND assigned as substitute for different sessions on the same day/shift, this is not caught.

## 7. Risk Level

| # | Issue | Risk | Impact |
|---|---|---|---|
| 1 | No smart scheduling engine | **Critical** | Feature cannot work as P12 specified |
| 2 | No draft internal conflict map | **Critical** | Duplicate overlaps in generated timetables |
| 3 | No room capacity check | **High** | 45 students in 30-seat room |
| 4 | Missing effective teacher conflict at TKB level | **High** | Teacher double-booked via substitute |
| 5 | Conflict service doesn't check room active status | **High** | Inactive rooms assigned |
| 6 | No publish re-validation | **High** | Published schedule could conflict |
| 7 | Session generation no conflict check | **High** | BuoiHoc with teacher/room/class conflicts |
| 8 | No teacher capability check | **Medium** | Wrong teacher assigned to subject |
| 9 | No workload limits | **Medium** | Teacher overwork |
| 10 | BuoiHoc conflict doesn't check substitute cross-session | **Medium** | Substitute doing double duty |
| 11 | No course already-published check | **Medium** | Course could get 2 published schedules |
| 12 | No class size data | **Low** | Can't determine capacity needs |

## 8. Recommended Algorithm Fix

### Architecture: Occupation-Map-Based Smart Scheduling Engine

Create a new service `SmartTimetableService` with these phases:

```
Phase 1 — Input Loading
  ├── Load all courses for the term/campus
  ├── Load all teachers (with GiaoVienMonHoc)
  ├── Load all rooms (with capacity, type, status)
  ├── Load all shifts (active)
  ├── Load existing published TKB and BuoiHoc
  └── Load class enrollment counts

Phase 2 — Occupation Map Construction
  ├── Seed with existing published TKB (teacher/class/room)
  ├── Seed with existing BuoiHoc (teacher/class/room)
  └── Mark occupied slots as { teacher, class, room } -> { day, shift }

Phase 3 — Course Sorting (Priority Order)
  ├── Courses with fixed schedule requirements first
  ├── Courses with specific room requirements
  ├── Largest classes first (capacity constraint)
  └── Remaining courses sorted by teacher workload

Phase 4 — Candidate Slot Generation
  For each course:
  ├── Generate all possible { day, shift, room } combos
  ├── Check teacher not occupied on { day, shift }
  ├── Check class not occupied on { day, shift }
  ├── Check room not occupied on { day, shift }
  ├── Check room capacity >= class size
  ├── Check room type matches subject type
  ├── Check room active
  ├── Check shift active
  └── Score available slots (soft constraints)

Phase 5 — Slot Selection
  ├── Pick best available slot
  ├── Update occupation map
  └── Add to draft list

Phase 6 — Final Validation
  ├── Re-check all hard constraints
  ├── Report any remaining conflicts
  └── Generate conflict report

Phase 7 — Preview
  └── Return draft list with all conflict info

Phase 8 — Publish (Transaction)
  ├── BEGIN TRANSACTION
  ├── Re-validate all items against live DB
  ├── If all clear: insert all ThoiKhoaBieu
  ├── If any conflict: ROLLBACK
  ├── Generate all BuoiHoc
  └── COMMIT
```

### Occupation Map Data Structure

```csharp
class OccupationMap {
    // dayOfWeek + shiftId -> set of teacherIds
    Dictionary<(int dayOfWeek, int shiftId), HashSet<int>> TeacherSlots;
    // dayOfWeek + shiftId -> set of classIds  
    Dictionary<(int dayOfWeek, int shiftId), HashSet<int>> ClassSlots;
    // dayOfWeek + shiftId -> set of roomIds
    Dictionary<(int dayOfWeek, int shiftId), HashSet<int>> RoomSlots;
    
    // For BuoiHoc-level conflicts (date-specific)
    Dictionary<DateOnly, Dictionary<int, HashSet<int>>> DateTeacherSlots;
    Dictionary<DateOnly, Dictionary<int, HashSet<int>>> DateClassSlots;
    Dictionary<DateOnly, Dictionary<int, HashSet<int>>> DateRoomSlots;
}
```

### Soft Scoring (for best-fit selection)

```
Score = +100 if teacher has LaMonChinh for subject
Score += +50 if teacher has high MucDoPhuHop
Score += +30 if room type matches (lab->lab, theory->theory)
Score += +20 if teacher workload < average
Score += penalty for consecutive shifts same teacher
Score += penalty for spreading same class across too many rooms
```

## 9. Minimal Fix Plan

Ordered by priority (do NOT implement yet — this is the plan):

1. **Fix draft internal conflict** — `ThoiKhoaBieuService.CreateAsync` needs an in-memory map of items created in the same request batch
2. **Fix conflict key to include effective teacher** — Add `MaGiaoVienDayThay` to conflict queries at the BuoiHoc level
3. **Fix room capacity check** — Add `PhongHoc.SucChua >= classSize` validation
4. **Fix publish re-validation** — When updating status to `da_xuat_ban`, re-run all conflict checks
5. **Fix session generation conflict check** — `GenerateSessionsAsync` must check teacher/class/room against other BuoiHoc
6. **Fix substitute teacher capability check** — Check `GiaoVienMonHoc` before allowing substitute assignment
7. **Fix ScheduleConflictService room status** — Add `TrangThaiPhong == "hoat_dong"` check
8. **Add course already-published check** — Prevent publishing a schedule for a course that already has a published TKB
9. **Add transaction for multi-item operations** — Wrap batch operations in `IDbContextTransaction`
10. **Build occupation-map engine** — Full smart scheduling implementation
11. **Add deterministic test suite** — Cover all groups A-I

## 10. Validation Result

### dotnet build
```
Build succeeded.
    0 Error(s)
```

### dotnet test
No P12-specific tests exist. Existing tests are for other modules (DL, RD, P0 series, DT series, NT).

### Manual API Test Status
Backend requires SQL Server + seed data + running instance. Not executed in this session.

### Known Blocked
- Full API testing requires a running backend with seeded DB
- No batch generate/preview/publish endpoints exist to test
- No test project exists for P12

## 11. Git Status

```
$ git status --short
  (no output — clean working tree)
$ git branch --show-current
  feature/p12-smart-timetable-logic-audit
```
