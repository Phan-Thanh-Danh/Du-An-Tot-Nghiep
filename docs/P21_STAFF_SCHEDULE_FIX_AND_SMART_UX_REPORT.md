# P21 — Staff Schedule Fix & Smart Schedule UX

## Overview

Fixed `ScheduleManagerView.vue` for GiaoVu's schedule management: redesigned the create/edit form from manual text inputs to smart searchable dropdowns with auto-fill and scheduling heuristics.

## Changes Made

**File:** `frontend/src/views/GiaoVu/Schedule/ScheduleManagerView.vue`

### 1. Course Searchable Dropdown (replaces 7 manual text inputs)
- Removed individual text inputs for: học kỳ (x2), lớp (x2), môn học (x2), giảng viên (x1)
- Added a single searchable course dropdown fetching from `courseApi.getCourses()`
- On course selection, auto-fills: `hocKy`, `lop`, `monHoc`, `giaoVien`, `maKhoaHoc`
- Displays selected course info as semantic pills (subject, class, teacher, term)

### 2. Room Searchable Dropdown (replaces phòng học text input)
- Replaced the `<input>` for phòng học with a searchable dropdown from `roomApi.list()`
- Shows room name, code, building, and capacity in the dropdown

### 3. Smart Schedule Suggestion Algorithm (FE-only)
- Added `suggestSlots()` function that analyzes existing schedule grid
- Builds occupancy map from non-canceled rows, scores all free (thu, ca) combinations
- Scoring: earlier days > later days, morning > afternoon > evening, adjacent free slots bonus
- Returns top 5 suggestions with scores, clicking one applies it immediately

### 4. Fixed Conflict Check Payload
- Updated `checkConflicts()` to send `{ maKhoaHoc, thuTrongTuan, maCaHoc, maPhong, excludeMaTkb }`
- Removed incorrect fields: `maGiaoVien`, `maLop`
- Matches backend `CheckScheduleConflictRequest` DTO exactly

### 5. Fixed Create/Update Payload
- Updated `saveSchedule()` to build minimal payload: `{ maKhoaHoc, thuTrongTuan, maCaHoc, maPhong, ngayBatDau, ngayKetThuc, trangThai }`
- Matches backend `CreateThoiKhoaBieuRequest` DTO exactly
- No longer ships extra fields (`hocKy`, `lop`, `monHoc`, `giaoVien`, `tenPhong`, etc.)

### 6. Validation Updated
- `validateForm()` now checks: khóa học selected, phòng học selected, dates valid
- Removed old checks for manual text fields

## Verification

- `npm run build` passes (2379 modules, 0 errors)
- `npm run lint` passes

## Architecture

```
ScheduleManagerView.vue
├── loadCourses() → courseApi.getCourses({ PageSize: 200, TrangThai: 'dang_day' })
├── loadRooms() → roomApi.list({ PageSize: 100 })
├── selectCourse(course) → auto-fill hocKy, lop, monHoc, giaoVien
├── selectRoom(room) → set maPhong, phongHoc
├── suggestSlots() → FE scoring over (thu, ca) grid → top 5
├── checkConflicts() → POST /api/thoi-khoa-bieu/check-xung-dot
└── saveSchedule() → POST /api/thoi-khoa-bieu (create) | PUT (update)
```
