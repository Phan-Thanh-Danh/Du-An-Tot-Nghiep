# P17 Smart Feature Demo Seed Plan

## Overview
This document defines the deterministic seed data configuration to safely demonstrate the Smart Course Allocation and Smart Timetable features without polluting production or relying on mock/fake logic.

## Configuration

### Campus / MaDonVi used
- P17_DEMO_CAMPUS (Demo Campus / Cơ sở Demo)

### Academic term / MaHocKy used
- P17_DEMO_TERM (Demo Term / Học kỳ Demo 2026)

### Classes used
- P17_DEMO_CLASS_A
- P17_DEMO_CLASS_B

### Subjects used
- P17_DEMO_SUBJ_MATH (Toán logic)
- P17_DEMO_SUBJ_ENG (Tiếng Anh giao tiếp)

### Teachers used
- P17_DEMO_TEACHER_1 (Giảng viên Demo 1)
- P17_DEMO_TEACHER_2 (Giảng viên Demo 2)

### Rooms used
- P17_DEMO_ROOM_101
- P17_DEMO_ROOM_102

### Shifts used
- Sáng (Ca 1, Ca 2)
- Chiều (Ca 3, Ca 4)

### Courses used
- P17_DEMO_COURSE_MATH_A
- P17_DEMO_COURSE_ENG_B

## Expected Outcomes
- **Expected generated draft count:** 1 draft with 4 scheduled sessions (2 for each class)
- **Expected publish session count:** 4 new BuoiHoc records linked to the ThoiKhoaBieu.

## Rollback Strategy
All created records share the P17_DEMO_ prefix or are attached to the P17_DEMO_TERM. A cleanup script or manual deletion of the term will cascade or logically group all records, making them trivial to clean post-demo.
