# Duplicate Architecture Report

## 1. Duplicate Layout Logic

### 1.1. Sidebar Components (6 nearly identical implementations)

| File | Role | Lines | Variation |
|------|------|-------|-----------|
| components/SinhVien/AppSidebar.vue | Student | 168 | Brand: GraduationCap, Subtitle: "Sinh viên" |
| components/PhuHuynh/AppSidebar.vue | Parent | 168 | Brand: HeartHandshake, Subtitle: "Phu huynh" |
| components/GiangVien/AppSidebar.vue | Teacher | 168 | Brand: GraduationCap, Subtitle: "Giang vien" |
| components/GiaoVu/AppSidebar.vue | Staff | 168 | Brand: ShieldCheck, Subtitle: "Phong Giao vu" |
| components/BGH/AppSidebar.vue | BGH | 168 | Brand: Landmark, Subtitle: "BGH" |
| components/SuperAdmin/AppSidebar.vue | SuperAdmin | 168 | Brand: ShieldCheck, Subtitle: "Admin" |

**Impact**: ~1000 lines of nearly identical code. Sidebar logic (collapse, flyout, avatar, logout, help) is duplicated 6 times. Any change to sidebar behavior requires editing 6 files.

### 1.2. Topbar Components (3 implementations)

| File | Role | Lines |
|------|------|-------|
| components/SinhVien/AppTopbar.vue | Student | ~300 |
| components/PhuHuynh/AppTopbar.vue | Parent | ~360 |
| components/SuperAdmin/SuperAdminTopbar.vue | SuperAdmin | ~50 |

Teacher and Staff reuse the Student AppTopbar, BGH has no topbar shown in the layout file. The topbar logic (breadcrumbs, search, theme toggle, user menu, notifications) is duplicated.

### 1.3. Page Container (2 implementations)

| File | Lines |
|------|-------|
| components/SinhVien/PageContainer.vue | ~20 |
| components/SuperAdmin/PageContainer.vue | ~25 |

SinhVien/PageContainer is reused by Teacher and Staff. SuperAdmin has its own version.

### 1.4. Layout Shell Structure (6 identical shell patterns)

All 6 role layouts follow the exact same pattern:
```
lg-app-bg > flex h-screen > [Sidebar | Topbar + content > main > PageContainer > router-view]
```

The only differences are:
- CSS variable colors for accent
- Menu data source
- Brand icon in sidebar

**Impact**: High maintenance cost. Any layout improvement (mobile, accessibility, dark mode) must be replicated 6 times.

## 2. Duplicate Menu Data (6 files)

| File | Lines |
|------|-------|
| components/SinhVien/data/menuData.js | ~60 |
| components/PhuHuynh/data/menuData.js | ~50 |
| components/GiangVien/data/menuData.js | ~70 |
| components/GiaoVu/data/menuData.js | ~90 |
| components/BGH/data/menuData.js | ~70 |
| components/SuperAdmin/data/menuData.js | ~120 |

Menu definitions are per-role, which is architecturally correct. However, the **structure** (group with id/label/icon/children) is identical across all 6 files. Menu items for common features (profile, notifications) could be shared.

## 3. Duplicate Status Mapping

### 3.1. Role Normalization (3 sources of truth)

| File | Purpose |
|------|---------|
| stores/auth.js (normalizeRole) | Alias mapping for auth |
| utils/roleRoutes.js (normalizeRole) | Alias mapping for home route |
| constants/roleCatalog.js (normalizeRole) | Alias mapping for role catalog |

Each has slightly different alias lists. Duplicating normalization logic creates risk of inconsistent role resolution.

### 3.2. Home Route Mapping (3 sources)

| File | Purpose |
|------|---------|
| utils/roleRoutes.js (ROLE_HOME_ROUTES) | Route-level home mapping |
| constants/roleCatalog.js (ROLE_HOME_ROUTES) | Catalog-level home mapping |
| data/authPortals.js (homeRoute per portal) | Portal-level home mapping |

These three sources must be kept in sync manually. A change to `/teacher/dashboard` path requires edits in 3 files.

## 4. Duplicate API Service Patterns

### 4.1. CRUD boilerplate repetition

Most service files follow the same pattern:
```
export const [name]Api = {
  list: () => apiRequest('/api/...'),
  get: (id) => apiRequest(`/api/.../${id}`),
  create: (data) => apiRequest('/api/...', { method: 'POST', body: data }),
  update: (id, data) => apiRequest(`/api/.../${id}`, { method: 'PUT', body: data }),
  delete: (id) => apiRequest(`/api/.../${id}`, { method: 'DELETE' }),
}
```

This pattern is repeated across: buildingApi, floorApi, roomApi, shiftApi, subjectApi, academicTermApi, classApi, courseApi, etc.

### 4.2. Student API vs Parent API vs Teacher API overlap

Multiple services define similar endpoints independently:
- `studentApi.schedule` vs `teacherApi.schedule` vs `parentApi.schedule`
- `studentApi.attendance` vs `teacherApi.attendance`
- `studentApi.profile` vs `teacherApi.profile`

While role-scoped APIs are valid, the response normalization logic is duplicated.

## 5. Duplicate CSS

### 5.1. Scrollbar styles (in each layout)

Each layout component defines the same scrollbar styles inline:
```css
::-webkit-scrollbar { width: 5px; height: 5px; }
::-webkit-scrollbar-track { background: transparent; }
::-webkit-scrollbar-thumb { background: var(--border-default); border-radius: 999px; }
```

Found in: Layout_SinhVien.vue, Layout_PhuHuynh.vue, Layout_GiangVien.vue, Layout_GiaoVu.vue, Layout_BGH.vue

### 5.2. Font import (in each layout)

`@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap')` is duplicated in each layout file. Should be in index.html or main.css.

### 5.3. Dark mode Tailwind overrides (in liquid-glass.css)

The dark mode compatibility layer remaps `.dark .bg-white`, `.dark .text-slate-*`, etc. This is necessary for legacy compatibility, but creates a second source of truth for dark mode styling.

## 6. Duplicate Mock/Inline Data

### 6.1. Student data mock (stores/auth.js + data/studentData.mock.js)

The auth store includes dev mock login users that sync with `studentData.mock.js`. This creates a dependency where auth changes must be mirrored in mock data.

### 6.2. Inline data across views

Multiple views define their own inline mock data arrays:
- GiangVien/AttendanceTodayView.vue: `todayClasses` and `students` arrays
- GiangVien/AttendanceHistoryView.vue: history and students per session
- GiangVien/PendingRequestsView.vue: request array
- BGH/Schedule*.vue: schedule data inline
- SuperAdmin/AwardsView.vue: large inline mock
- SuperAdmin/DisciplineView.vue: inline mock

### 6.3. UseSchedule composable vs scheduleApi

`composables/useSchedule.js` defines its own inline event CRUD with auto-increment IDs, independent of `services/scheduleApi.js`. These are not connected - one is a local calendar helper, the other is a real API client.

## 7. Duplicate View Components

### 7.1. Student views with different naming

| English Path | Vietnamese Path | Notes |
|-------------|----------------|-------|
| views/Student/CoursesView.vue | views/SinhVien/HocTap/KhoacHoc.vue | Different implementations |
| views/Student/Dashboard.vue (legacy?) | views/SinhVien/Dashboard.vue | SinhVien version routes to '/student/dashboard' |
| views/Student/* | views/SinhVien/HocTap/* | Mixed routing (some to Student, some to SinhVien) |

### 7.2. Notification view reused across roles

`views/Student/NotificationsView.vue` is used by:
- Student (correct)
- Teacher (`/teacher/notifications`) - **wrong context**
- BGH (`/bgh/notifications`) - **wrong context**

## 8. Summary

| Category | Count | Severity |
|----------|-------|----------|
| Nearly identical sidebar implementations | 6 | P1 - High |
| Layout shell patterns duplicated | 6 | P1 - High |
| Page title sources of truth | 3+ | P1 - High |
| Role normalization sources | 3 | P1 - High |
| Home route mapping sources | 3 | P1 - High |
| Inline mock data arrays in views | 6 | P2 - Medium |
| Duplicate scrollbar CSS locations | 6 | P2 - Medium |
| Duplicate font import locations | 6 | P2 - Medium |
| Legacy dead component files | 9 | P3 - Low |
