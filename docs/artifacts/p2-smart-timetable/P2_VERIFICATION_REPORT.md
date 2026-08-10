# P2 - Smart Timetable (GA): Verification Report 2B/2C

Ngày: 2026-08-10 — Môi trường: Docker stack (sqlserver + backend 5097) với `SeedProfile=LargeDemo`.

## 2A (đã xong trước đó)
- Fix sĩ số: `SmartTimetableService.cs:1001` dùng `AuthRoles.ToDatabaseCode(AuthRoles.Student)` thay vì chuỗi cũ.
- Verified: draft `cba8f6c3-eb93-4e9f-bc54-2612858683c9`: 12/12 xếp, 0 lỗi, roomFit −5 "Phòng quá lớn" cho lớp có HS thật.
- Di trú: migration `20260807094746_AddMaGiaoVienToScheduleDraftItem` (MaGiaoVien trên ScheduleDraftItem).

## 2B - Docker hóa + capacity test
### docker-compose.yml
- Backend kết nối `Server=sqlserver;Database=LMS;User Id=sa;Password=Test@123_PassWord!;TrustServerCertificate=True;`.
- `SeedProfile=LargeDemo` tự động seed khi khởi động (không cần DB ngoài).
- Healthcheck sqlserver + `depends_on: condition: service_healthy`.

### 2B.3 Capacity test (lớp đông 36 HS)
- Seed verified: 10005 học sinh, 110 GV, 989 khóa học, 10 phòng (đơn vị 3, mỗi phòng 40 chỗ).
- Đã di chuyển 34 học sinh seed (lớp 11 → lớp 1, đơn vị 3) để tạo lớp 36 HS (test data tạm).
- Đã bổ sung skill `GiaoVienMonHoc` cho GV đơn vị 3 để môn 3/4 có ≥5 GV ≥70% (patch SQL, tạm thời — bản sao trong `docs/sql/seed-giao-vien-skill.sql`).
- Thêm 2 phòng nhỏ (A103=25, A104=30) để kiểm tra GA loại phòng thiếu sức chứa (tạm thời).
- Kết quả draft `16f36f7d-b9b9-42fa-ac51-49a6fc4a4598` (file `draft-2b3-capacity-36hs.json`):
  - **12/12 khóa xếp được, 0 không xếp, 0 lỗi.**
  - 0 xung đột phòng / GV / lớp.
  - Min skill ≥ 85% (ngưỡng 70).
  - Khóa 20 & 25 (36 HS): chỉ xếp phòng 40 chỗ (A101/A102/Lab A201); **phòng 25/30 chỗ không được dùng cho bất kỳ ca nào** → capacity constraint hoạt động đúng.
  - roomFit +5 (phòng phù hợp) cho ca lớp đông; −5 "Phòng quá lớn" cho lớp nhỏ (2 HS) — đúng kỳ vọng.
  - 21 ca có cảnh báo "Chưa có dữ liệu sĩ số" — là lớp sĩ số 0 thật (không phải bug).

## 2C - Mô phỏng thành viên mới (reproducibility)
- `docker compose down -v` + `up --build` (reset hoàn toàn, volume mới).
- Seed tự động chạy: 10005 HS / 110 GV / 989 khóa / 10 phòng. Backend khởi động OK.
- Flip 3 row TKB `da_xuat_ban` → `nhap` (khóa 23/24/28, HK3/đv3) để tránh guard 409.
- Login `giaovu.hcm@lms.local`: OK.
- Generate draft `6755e83a-ee5b-494f-b9c1-319ef7a2bcaa`: **36/36 items xếp được (12/12 khóa)** với data seed sạch, min skill 80% — seed base đã đủ GV cho HK3/đv3.

## Test tự động
- `dotnet build`: 0 lỗi.
- `dotnet test --filter P26|P28|Genetic`: 14/14 pass.

## Ghi chú
- Không sửa business logic ngoài fix sĩ số 2A.
- Test data tạm (move HS, phòng nhỏ, skill patch) nằm trên DB docker, sẽ biến mất khi reset — script tái lập trong `docs/sql/`.
- Không commit `appsettings.Development.json` (machine-specific) và `AGENTS.md`.