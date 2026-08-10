# P2 — Hướng dẫn Demo Báo Cáo: Xếp Lịch Thông Minh (Smart Timetable GA)

> Soạn ngày 2026-08-10 (tối trước báo cáo). Mọi số liệu dưới đây lấy từ **DB thật đang chạy trên Docker** — chạy lại đúng quy trình là ra kết quả như trong file này.

---

## 1. Chuẩn bị trước khi demo (10 phút)

### 1.1 Bật hệ thống

```powershell
cd "C:\Users\maita\OneDrive\Máy tính\Du-An-Tot-Nghiep"
docker compose up -d --build
docker ps
```

Chờ 3 container lên và `sqlserver-1` hiện `(healthy)` (~30-60 giây).
Lần đầu hoặc sau `down -v`, backend tự seed `SeedProfile=LargeDemo`:
**10.005 học sinh, 110 giáo viên, 989 khóa học** — không cần làm gì thêm.

| Chạy | URL | Ghi chú |
|---|---|---|
| Frontend (Nginx) | `https://localhost` | HTTPS, cert tự ký — browser sẽ cảnh báo, chọn "Advanced → Proceed" |
| Backend API | `http://localhost:5097` | Swagger: `http://localhost:5097/swagger` |
| SQL Server | `localhost,1433` | sa / `Test@123_PassWord!` — DB `LMS` |

### 1.2 Tài khoản demo

| Vai trò | Email | Mật khẩu | Quyền |
|---|---|---|---|
| Giáo vụ (demo chính) | `p12test_staff01@lms.local` | `Test@123` | AcademicStaff, cơ sở 3 (FPT HCM) |
| Giáo vụ (thay thế) | `giaovu.hcm@lms.local` | `123456` | AcademicStaff, cơ sở 3 |

### 1.3 Kiểm tra sức khỏe nhanh (3 câu lệnh)

```powershell
# 1) Backend sống?
Invoke-RestMethod "http://localhost:5097/api/organizations" -TimeoutSec 10 | Out-Null; "OK"

# 2) Draft P2 còn tồn tại? (đã sinh sẵn, dùng để demo "xem kết quả" không cần chờ GA)
docker exec du-an-tot-nghiep-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Test@123_PassWord!" -C -d LMS -Q "SET NOCOUNT ON; SELECT TOP 1 draft_id, so_xep_duoc, so_khong_xep_duoc, CAST(score AS decimal(6,2)) AS score FROM ScheduleGenerationJob ORDER BY ngay_tao DESC"

# 3) Login demo
$b = '{"email":"p12test_staff01@lms.local","password":"Test@123"}'
$r = Invoke-RestMethod -Method Post -Uri "http://localhost:5097/api/auth/login" -ContentType "application/json" -Body $b -TimeoutSec 30
"Login OK - role=$($r.user.role) campus=$($r.user.campusName)"
```

---

## 2. Dữ liệu "đó là gì" — nắm chắc để trả lời hội đồng

Khi hội đồng hỏi "dữ liệu này ở đâu ra / là gì?", trả lời theo 4 lớp sau:

### 2.1 Học kỳ & đơn vị (phạm vi)

| Câu hỏi | Trả lời |
|---|---|
| Xếp lịch cho gì? | Học kỳ **HK3_2026** (ma_hoc_ky = 3) tại cơ sở **FPT Polytechnic Hồ Chí Minh** (ma_don_vi = 3). |
| Vì sao chỉ 1 học kỳ? | Backend chặn: `ValidateSchedulableTermAsync` chỉ cho xếp **học kỳ tương lai gần nhất** — tránh xếp nhầm kỳ đã kết thúc / kỳ xa (SmartTimetableService.cs:61, AcademicSchedulingContextService.cs:210). |
| Vì sao khóa học không bị trùng lịch cũ? | Guard phát hiện lịch đã công bố → **409 từ chối** thay vì ghi đè. Nếu demo generate bị 409, chạy bước "reset" ở mục 6.4. |

### 2.2 Khóa học & sĩ số (đầu vào)

12 khóa học HK3/đơn vị 3 chưa lưu trữ (trạng thái ≠ `luu_tru`). Sĩ số = **đếm học sinh thật** có `ma_lop` + `vai_tro_chinh='hoc_sinh'`:

| Mã khóa | Môn | Lớp | Sĩ số thật |
|---|---|---|---|
| 19 | Lập trình JavaScript | SD1904 | 3 |
| 20 | Lập trình C# | SD1901 | 6 |
| 21 | Lập trình C# | SD1902 | 0 |
| 22 | Lập trình C# | SD1903 | 0 |
| 23 | Lập trình C# | SD1904 | 3 |
| 24 | Lập trình C# | SD1905 | 0 |
| 25 | Cơ sở dữ liệu | SD1901 | 6 |
| 26 | Cơ sở dữ liệu | SD1902 | 0 |
| 27 | Cơ sở dữ liệu | SD1903 | 0 |
| 28 | Cơ sở dữ liệu | SD1906 | 0 |
| 29 | Marketing căn bản | MKT1901 | 3 |
| 30 | Marketing căn bản | MKT1902 | 0 |

Mỗi khóa cần **3 buổi/tuần** (map `MonHoc.SoTinChi → QuyDoiTinChi.so_buoi_moi_tuan`; 3 tín chỉ → 3 buổi).

### 2.3 Phòng học (ràng buộc sức chứa)

10 phòng cùng cơ sở 3, đều `hoat_dong`:

| Phòng | Sức chứa | | Phòng | Sức chứa |
|---|---|---|---|---|
| A101 | 40 | | B101 | 45 |
| A102 | 35 | | B201 | 35 |
| Lab A201 | 30 | | Studio C101 | 25 |
| Lab A202 | 30 | | P301 | 40 |
| Hội trường A301 | 50 | | P302 | 40 |

Ràng buộc cứng: `SucChua >= sĩ số` — GA **loại ngay** phòng thiếu chỗ khi dựng slot (BuildProblem), không chờ tới fitness.

### 2.4 Giảng viên & ma trận kỹ năng (điểm mới của P2)

Bảng `GiaoVienMonHoc.muc_do_phu_hop` (0-100) + `la_mon_chinh`. GA **chỉ chọn GV có `MucDoPhuHop >= 70`** (`MinTeacherSkill`, hard constraint — P2). Đây là thứ phân biệt P2 với P12 cũ (trước đây xếp bừa GV gốc).

Kết quả GA cho 12 khóa (draft thật `6755E83A...`):

| Khóa | GV gốc (KhoaHoc) | GV GA chọn | Skill | Ghi chú |
|---|---|---|---|---|
| 19 | 14 | 14 | 95% | giữ |
| 20 | 18 | 18 | 100% | giữ |
| 21 | 18 | 18 | 100% | giữ |
| 22 | **18** | **19** | 95% | **GA đổi GV** (18 bận/dàn tải) |
| 23 | 19 | 19 | 95% | giữ |
| 24 | **19** | **13** | 80% | **GA đổi GV** (cân bằng tải) |
| 25 | 20 | 20 | 95% | giữ |
| 26 | 20 | 20 | 95% | giữ |
| 27 | 21 | 21 | 85% | giữ |
| 28 | 21 | 21 | 85% | giữ |
| 29 | 22 | 22 | 95% | giữ |
| 30 | 22 | 22 | 95% | giữ |

Cột "GV GA chọn" nằm ở **`ScheduleDraftItem.ma_giao_vien`** (cột mới, migration `20260807094746`), skill ở `muc_do_phu_hop` — KHÔNG nằm ở `KhoaHoc.ma_giao_vien` (chỉ được gán đè khi publish).

---

## 3. Quy trình hoạt động ("nó chạy thế nào") — nói 60 giây

```
Giáo vụ bấm "Xếp lịch thông minh"
   → POST /api/thoi-khoa-bieu/generate { maHocKy:3, maDonVi:3, tongTheHe, kichThuocQuanThe, tyLeCheo, doTuoiThoToiDa, clientDraftId }
   1. Validate quyền (AcademicStaff/SuperAdmin/Admin/CampusAdmin) + học kỳ schedulable
   2. Nạp dữ liệu từ DB: 12 khóa + 3 buổi/tuần + 10 phòng + 10 GV đơn vị 3 (8 GV có skill matrix) + sĩ số + thời gian rảnh GV
   3. GeneticTimetableSolver.Solve():
      - BuildProblem: loại slot vi phạm hard (sức chứa, GV <70 skill, GV bận theo form)
      - Khởi tạo quần thể 50 cá thể (1 greedy + 49 random, seed cố định 20260701 → deterministic)
      - Tiến hóa ≤100 thế hệ: tournament select (k=3) → uniform crossover 0.5 → mutate
      - Fitness = Σ(150×skill% + staticScore) − 500×slot trống − 1000×xung đột − 60×trùng ngày − 30×ca liên tiếp − 15×quá tải
      - Early stop khi 10 thế hệ không cải thiện
      - RepairGreedy cuối: xếp lại greedy đảm bảo 0 xung đột, ≤6 ca/tuần/GV
   4. Lưu ScheduleGenerationJob (draft) + 36 ScheduleDraftItem (xep_duoc/khong_xep_duoc + score + lý do)
   → FE poll GET /drafts/{clientDraftId}/progress mỗi 500ms (modal tiến trình: thế hệ/fitness/số buổi xếp được)
Giáo vụ mở "Lịch chờ duyệt" → xem score breakdown từng buổi → "Phê duyệt & Xuất bản"
   → POST /publish: transaction Serializable
       - chặn nếu học kỳ đã có lịch da_xuat_ban (409)
       - hủy TKB cũ trạng thái nhap
       - kiểm tra lại toàn bộ xung đột → tạo ThoiKhoaBieu da_xuat_ban cho 36 buổi
       - gán đè KhoaHoc.ma_giao_vien = GV GA chọn
       - lỗi bất kỳ → rollback toàn bộ
```

---

## 4. Kịch bản demo trên UI (từng bước)

> Đảm bảo màn hình Fullscreen (F11), mở DevTools → Network để show request API khi chốt kiến trúc.

1. **Login**: mở `https://localhost` → đăng nhập `p12test_staff01@lms.local` / `Test@123` → vào giao diện Giáo vụ.
2. **Quản lý lịch học**: sidebar → **Quản lý lịch học** (ScheduleManagerView). Grid lịch Thứ 2-7 × Ca.
3. **Kiểm tra học kỳ đúng**: banner trên màn hình hiển thị học kỳ schedulable (HK3_2026 / cơ sở HCM). Nếu bị 409 "đã có lịch công bố" → thực hiện mục **6.4 Reset** rồi quay lại.
4. **Xếp lịch thông minh**: bấm nút **Xếp lịch tự động** (biểu tượng đũa phép) → modal hiện 4 tham số GA:
   - Số thế hệ: `100`; Quần thể: `50`; Tỷ lệ chéo: `0.5`; Tuổi thọ tối đa: `10` → bấm **Sinh lịch**.
   - Mở DevTools → Network → thấy `POST /api/thoi-khoa-bieu/generate` + các request `progress` lặp lại mỗi ~500ms → modal hiện: thế hệ hiện tại / tổng, `bestFitness`, số buổi xếp được / không xếp được.
5. **Hoàn tất**: thông báo "Sinh thời khóa biểu thông minh thành công" → bấm link sang **Lịch chờ duyệt** (PendingSchedulesView).
6. **Xem kết quả** (đây là phần thuyết minh quan trọng): mở draft mới (hoặc draft có sẵn `6755E83A...`):
   - Tổng: **12/12 khóa xếp được, 0 không xếp được**, score ~98/100.
   - Mở 1 buổi → **score breakdown**: `base 100 + roomFit +5 (phòng vừa sức)`; `lyDoGoiY` ghi rõ "GV được chọn với mức độ phù hợp 95%".
   - **Điểm "bán" cho hội đồng**: khóa 22/24 — GV gốc lần lượt là GV18/GV19 nhưng GA tự đổi sang GV19/GV13 (95%/80%) vì ràng buộc tải 6 ca/tuần — chứng minh GA **tối ưu hóa chứ không chỉ "nhặt GV cũ"**.
7. **Kiểm tra xung đột (tùy chọn)**: bấm **Kiểm tra xung đột** → `POST /api/thoi-khoa-bieu/check-xung-dot-batch` trả 0 xung đột → minh chứng an toàn trước khi publish.
8. **Phê duyệt & Xuất bản**: bấm **Xuất bản lịch** (có confirm dialog) → `POST /api/thoi-khoa-bieu/publish` thành công → draft chuyển `da_xuat_ban` → mở lại **Quản lý lịch học**, lịch mới hiện trạng thái **Đã xuất bản** đầy đủ 12 khóa × 3 buổi.

---

## 5. SQL xác thực tính đúng (chạy được ngay trên DB thật)

> File: `docs/sql/xac-thuc-ga-xep-lich.sql` — bản cải tiến so với `xac-thuc-xep-lich.sql`: kiểm tra xung đột GV theo **GV GA chọn** (`ScheduleDraftItem.ma_giao_vien`), không theo `KhoaHoc.ma_giao_vien` (chỉ sau publish mới bằng nhau).

Chạy nhanh bằng docker (thay draft id nếu muốn):

```powershell
docker cp docs/sql/xac-thuc-ga-xep-lich.sql du-an-tot-nghiep-sqlserver-1:/tmp/x.sql
docker exec du-an-tot-nghiep-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Test@123_PassWord!" -C -d LMS -i /tmp/x.sql
```

Kỳ vọng (draft `6755E83A-EE5B-494F-B9C1-319EF7A2BCAA` — sinh lúc 15:31, params 20 thế hệ/80 quần thể/0.15 chéo, dùng làm "draft có sẵn"):
- Khối 0: `TongKhoaHoc=12, XepDuoc=12, KhongXepDuoc=0, DiemTB≈97.92`
- Khối 1-4: 0 dòng (không xung đột GV / lớp / phòng / trùng slot) — **đây là nhóm "chứng minh tính đúng"**
- Khối 5: 0 dòng (mỗi khóa đủ 3 buổi)
- Khối 6: 0 dòng (không vượt sức chứa)
- Khối 7: bảng phân bố thứ/ca (draft này: Thứ 2=20, Thứ 3=8, Thứ 5=4, Thứ 6=4 → trải đều, không dồn)
- Khối 8: **có thể có dòng** — đây là SOFT constraint (`SameDayDuplicatePenalty=60`/ca dư, fitness phạt chứ không cấm). Draft này có 12 dòng vì nhiều GV khai lịch rảnh hạn chế (bảng `GiaoVienNguyenVongCaDay`). **Câu trả lời khi hội đồng hỏi**: "trùng 2 buổi/môn/ngày bị phạt điểm chứ không chặn xếp, vì đôi khi GV chỉ rảnh vài ngày — hệ thống ưu tiên điểm tổng; nếu cần chặn cứng có thể nâng `SameDayDuplicatePenalty`."
- Khối 9: 0 dòng (không khóa không xếp được)
- Khối 10: `SoBuoiXep=36, MinSkill=80 (≥ ngưỡng 70), AvgSkill≈92` — là câu trả lời "GA có xếp GV kém chuyên môn không? Không."
- Khối 11: khóa 22 và 24 mang `GA_DOI_GV` (GV gốc 18/19 → GA chọn 19/13) — minh chứng GA **tối ưu chứ không nhặt GV cũ**.

---

## 6. Ứng phó sự cố demo

| Sự cố | Nguyên nhân | Xử lý |
|---|---|---|
| Login sai mật khẩu | Account seed có thể đổi | dùng `p12test_staff01@lms.local / Test@123` (đã xác minh) hoặc `giaovu.hcm@lms.local / 123456` |
| `409 Không thể chuẩn bị lịch vì đã có lịch công bố` | HK3 đã có TKB `da_xuat_ban` (seeder tự publish) | chạy mục 6.4 reset rồi thử lại |
| `Chỉ được chuẩn bị lịch cho học kỳ tương lai gần nhất` | Chọn nhầm học kỳ / cơ sở | chọn đúng HK3_2026 + cơ sở 3 |
| Generate lâu (>10s) | 1000 thế hệ + quần thể 200 (params clamp) | dùng param mặc định 100/50/0.5/10 |
| Draft cũ không còn | Bị reset DB / xóa | chỉ cần generate draft mới, SQL đổi draft_id |
| Hỏi "kết quả có giống nhau mỗi lần chạy không?" | RNG seed cố định `20260701` | Trả lời: **có, deterministic** — cùng dữ liệu cho cùng kết quả |

### 6.4 Reset nhanh cho demo lặp lại (khi bị 409)

```powershell
docker exec du-an-tot-nghiep-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Test@123_PassWord!" -C -d LMS -Q "UPDATE t SET trang_thai='nhap' FROM ThoiKhoaBieu t JOIN KhoaHoc k ON k.ma_khoa_hoc=t.ma_khoa_hoc WHERE k.ma_hoc_ky=3 AND k.ma_don_vi=3 AND t.trang_thai='da_xuat_ban'; DELETE FROM ScheduleDraftItem WHERE ma_job IN (SELECT ma_job FROM ScheduleGenerationJob WHERE ma_hoc_ky=3 AND ma_don_vi=3); DELETE FROM ScheduleGenerationJob WHERE ma_hoc_ky=3 AND ma_don_vi=3;"
```

> Reset hoàn toàn DB (mất draft sẵn, seed lại sạch): `docker compose down -v; docker compose up -d --build` (~3-5 phút, seed tự chạy `LargeDemo`).

---

## 7. Các câu hỏi hội đồng hay gặp & câu trả lời gợi ý

**Q: Thuật toán là gì?**
A: Genetic Algorithm (thuật toán di truyền). Nhiễm sắc thể = danh sách slot (thứ, ca, phòng) + gene GV cho từng khóa. Khởi tạo 50 cá thể (1 greedy + 49 random) → tiến hóa tối đa 100 thế hệ bằng tournament selection, crossover uniform, mutation; giữ elite; dừng sớm khi 10 thế hệ không tiến bộ. Cuối cùng repair greedy để cam kết 0 xung đột.

**Q: Xung đột được xử lý thế nào?**
A: Hai tầng. Hard: lúc dựng slot (OccupationMap trong bộ nhớ — cùng GV/lớp/phòng tại 1 thứ+ca bị loại ngay; phòng thiếu sức chứa bị loại; GV < 70% chuyên môn bị loại). Soft: fitness phạt 1000 điểm/xung đột để GA không để sót. Khi publish chạy lại kiểm tra trong transaction — lỗi → rollback toàn bộ.

**Q: Làm sao biết GA chọn đúng giảng viên?**
A: Kết quả ghi rõ 2 cột `ma_giao_vien` (GV GA chọn) và `muc_do_phu_hop` (95-100% cho hầu hết khóa). Ngưỡng tối thiểu 70% là ràng buộc cứng — câu SQL mục 5 trả min_skill=80 chứng minh. Đặc biệt khóa 22, 24: GA **chủ động đổi GV** chỉ vì cân bằng tải, vẫn đạt ≥95%/80%.

**Q: Nếu không xếp được khóa nào thì sao?**
A: Khóa đó vào `khong_xep_duoc` với lý do trong `LoiJson`, draft vẫn sinh được phần còn lại; publish sẽ **chặn** nếu còn khóa không xếp được → không bao giờ công bố lịch thiếu.

**Q: Dữ liệu test có phải giả không?**
A: Không. Toàn bộ từ SQL Server thật qua seed `LargeDemo` (10.005 HS, 110 GV, 989 khóa). Sĩ số tính từ đếm user thật. Không có mock API (dự án đã loại bỏ toàn bộ mock từ P15F).

**Q: Chạy lại có ra kết quả khác không?**
A: Không — RNG seed cố định `20260701` → deterministic, dễ tái lập khi kiểm thử / báo cáo.

---

## 8. Thông tin gợi ý nói trong phiếu điểm

1. **P2 nâng cấp P12**: GV theo skill matrix (`MinTeacherSkill=70`) + sức chứa phòng + tải tối đa 6 ca/tuần + cân bằng định mức 5 ca/tuần.
2. Migration `20260807094746_AddMaGiaoVienToScheduleDraftItem`: thêm `ma_giao_vien`, `muc_do_phu_hop` vào `ScheduleDraftItem`.
3. Publish trong transaction Serializable: gán đè `KhoaHoc.MaGiaoVien = GV GA`, tạo `ThoiKhoaBieu` + `BuoiHoc`, rollback nếu lỗi, audit log `GENERATE`/`PUBLISH`.
4. FE: modal tiến trình GA poll 500ms qua endpoint mới `GET /drafts/{id}/progress`; score breakdown + lý do gợi ý hiển thị từng buổi.
5. Kiểm thử: 14/14 test tự động (P26 hard constraints + P28 preference-aware + Genetic), build 0 lỗi, smoke draft thật 12/12 khóa / 0 xung đột / 0 vượt sức chứa.