# P12 — Smart Timetable (Giao Vu): Tài liệu đầy đủ

> Vị trí code: `Backend/Services/ThoiKhoaBieu/*`, `Backend/Controllers/ThoiKhoaBieuController.cs`, `frontend/src/views/GiaoVu/Schedule/*`
> Phạm vi: Explain cách xếp lịch, dữ liệu, tính điểm, thuật toán di truyền, luồng hoạt động của Module Smart Timetable dành cho GiaoVu.

---

## 1. Tổng quan kiến trúc

| Thành phần | Vai trò |
|---|---|
| `ThoiKhoaBieuController` (`Backend/Controllers/ThoiKhoaBieuController.cs`) | REST routing, policy `AcademicOperations`, controller duy nhất cho cả TKB truyền thống lẫn Smart Timetable |
| `SmartTimetableService` (`Backend/Services/ThoiKhoaBieu/SmartTimetableService.cs`, ~1008 dòng) | Service chính: generate, xem/liệt kê draft, progress, publish, check-xung-dot-batch, suggest-slots, xóa draft |
| `GeneticTimetableSolver` (`Backend/Services/ThoiKhoaBieu/GeneticTimetableSolver.cs`, ~912 dòng) | **Thuật toán di truyền** xếp lịch toàn học kỳ |
| `ScheduleCandidateScoringService` (`Backend/Services/ThoiKhoaBieu/Scoring/ScheduleCandidateScoringService.cs`) | Tính điểm soft cho từng slot (dùng cho suggest-slots & decode kết quả GA) |
| `OccupationMap` (`Backend/Services/ThoiKhoaBieu/OccupationMap.cs`) | Bản đồ chiếm dụng (GV/Lớp/Phòng theo Học kỳ-Thứ-Ca) chạy trong memory |
| `ScheduleConflictService` (`Backend/Services/ThoiKhoaBieu/ScheduleConflictService.cs`) | Kiểm tra xung đột đơn lẻ với DB (dùng cho create/update TKB tay) |
| `ThoiKhoaBieuService` (`Backend/Services/ThoiKhoaBieu/ThoiKhoaBieuService.cs`) | CRUD TKB truyền thống (tạo tay, cập nhật, hủy, tiến độ buổi) |
| `SmartTimetableScoringOptions` (`Backend/Configuration/SmartTimetableScoringOptions.cs`) | Cấu hình trọng số/ngưỡng (hiện không có section trong appsettings → chạy giá trị default) |
| `ScheduleManagerView.vue`, `PendingSchedulesView.vue`, `scheduleApi.js` | Màn hình GiaoVu: lịch grid + 3 chế độ tạo; màn rà soát & xuất bản draft |

Đăng ký DI: `Backend/Program.cs:130-131, 213-214`.

---

## 2. Danh sách endpoint (`/api/thoi-khoa-bieu`)

Policy: `[Authorize(Policy = "AcademicOperations")]` → được phép khi role ∈ { SuperAdmin, Admin, CampusAdmin, AcademicStaff } (kiểm tra lại trong `EnsureCanManageSchedule`).

| Method & Route | Chức năng | Service |
|---|---|---|
| `GET /` | Danh sách TKB phân trang theo bộ lọc (`ThoiKhoaBieuQueryParameters`) | ThoiKhoaBieuService |
| `GET /{id}` | Chi tiết TKB theo mã | ThoiKhoaBieuService |
| `POST /` | Tạo TKB tay (bắt `ScheduleConflictException` → 409) | ThoiKhoaBieuService |
| `PUT /{id}` | Cập nhật TKB (kiểm tra xung đột lại, loại trừ chính nó `ExcludeMaTkb`) | ThoiKhoaBieuService |
| `PATCH /{id}/cancel` | Hủy TKB (`TrangThai = da_huy`) | ThoiKhoaBieuService |
| `DELETE /{id}` | Xóa TKB | ThoiKhoaBieuService |
| `POST /check-xung-dot` | Kiểm tra xung đột đơn lẻ (GV/Lớp/Phòng) so với DB | ScheduleConflictService |
| `POST /{id}/generate-sessions` | Sinh `BuoiHoc` theo ngày từ TKB đã xuất bản | BuoiHocService |
| **`POST /generate`** | **Sinh TKB thông minh bằng thuật toán di truyền → tạo draft** | SmartTimetableService |
| **`GET /drafts`** | Danh sách draft theo `maDonVi` + `maHocKy` | SmartTimetableService |
| **`GET /drafts/{draftId}`** | Chi tiết draft (kèm items) | SmartTimetableService |
| **`GET /drafts/{draftId}/progress`** | Tiến độ GA (thế hệ hiện tại, fitness, xếp được...) | SmartTimetableService |
| **`POST /publish`** | Xuất bản draft → tạo hàng loạt `ThoiKhoaBieu` trong transaction | SmartTimetableService |
| **`POST /check-xung-dot-batch`** | Check xung đột hàng loạt trên OccupationMap | SmartTimetableService |
| **`DELETE /drafts/{draftId}`** | Xóa draft chưa xuất bản | SmartTimetableService |
| **`POST /suggest-slots`** | Gợi ý top-N slot cho 1 khóa học (không GA) | SmartTimetableService |
| **`POST /suggest-slots-batch`** | Gợi ý tuần tự cho nhiều khóa, các khóa sau không trùng khóa trước | SmartTimetableService |
| `GET /khoa-hoc/{maKhoaHoc}/tien-do-buoi` | Số buổi đã xếp / yêu cầu của khóa | ThoiKhoaBieuService |

---

## 3. Dữ liệu (bảng & các trường)

### 3.1 Đầu vào `GenerateTimetableRequest`

| Trường | Kiểu | Mặc định | Ý nghĩa |
|---|---|---|---|
| `MaHocKy` | int | — | Học kỳ cần xếp |
| `MaDonVi` | int | — | Cơ sở/đơn vị cần xếp (phải đúng campus của user) |
| `TongTheHe` | int? | 100 | Số thế hệ tối đa của GA |
| `KichThuocQuanThe` | int? | 50 | Số cá thể mỗi thế hệ |
| `TyLeCheo` | double? | 0.5 | Xác suất chéo gene (crossover) + xác suất đột biến slot |
| `DoTuoiThoToiDa` | int? | 10 | Số thế hệ không cải thiện thì dừng sớm |
| `MaKhoaHocFilter` | List<int>? | null | Lọc xếp một số khóa (null = tất cả khóa chưa lưu trữ) |
| `ClientDraftId` | Guid? | sinh mới | ID client tạo sẵn để FE poll progress |

### 3.2 Dữ liệu nạp từ DB khi Generate (`SmartTimetableService.GenerateAsync`)

| Nguồn | Entity/Bảng | Các trường dùng |
|---|---|---|
| Khóa học | `KhoaHoc` | `MaKhoaHoc`, `MaDonVi`, `MaGiaoVien` (GV gốc), `MaMonHoc`, `MaHocKy`, `MaLop`, `TrangThai` (≠ `luu_tru`), `MonHoc.SoTinChi` |
| Số buổi/tuần | `QuyDoiTinChi` | Map `SoTinChi → SoBuoiMoiTuan` (mặc định 1) — đây là `RequiredSlots` của khóa |
| Ca học | `CaHoc` | `MaCaHoc`, `TenCa`, `Buoi`, `GioBatDau`, `GioKetThuc`, `ThuTu`, `ConHoatDong = true` |
| Phòng học | `PhongHoc` | `MaPhong`, `MaDonVi`, `SucChua`, `TrangThaiPhong = "hoat_dong"` |
| Ma trận kỹ năng | `GiaoVienMonHoc` + `NguoiDung` | `MucDoPhuHop` (0-100), `LaMonChinh`, `ConHoatDong`; GV phải `VaiTroChinh = "giao_vien"`, `TrangThai = "hoat_dong"`, cùng `MaDonVi` |
| Sĩ số lớp | `NguoiDung` | Đếm user có `MaLop` + `VaiTroChinh = student` + `TrangThai = hoat_dong` |
| Thời gian rảnh | `GiaoVienNguyenVongHocKy` + `GiaoVienNguyenVongCaDay` | Form `TrangThai = "submitted"`; slot `MucDo ∈ {available, preferred}` → GV chỉ được xếp tại (Thứ, Ca) đó; GV chưa khai form → không giới hạn |

### 3.3 Đầu ra lưu DB

**`ScheduleGenerationJob`** (1 draft = 1 job):

| Trường | Ý nghĩa |
|---|---|
| `DraftId` (Guid, unique) | ID công khai FE dùng để poll/publish |
| `MaDonVi`, `MaHocKy`, `NguoiYeuCau` | Phạm vi + người tạo |
| `TrangThai` | `draft` / `da_xuat_ban` (CHECK constraint, không có trạng thái khác) |
| `TongCourse`, `SoXepDuoc`, `SoKhongXepDuoc` | Số khóa tổng / xếp được / không xếp được |
| `Score` (double?) | Điểm trung bình các slot `xep_duoc` |
| `TomTatJson` | Params GA (thế hệ, quần thể, chéo, tuổi thọ) + kết quả (thế hệ đã chạy, bestFitness, ms) |
| `NgayTao`, `NgayXuatBan` | Thời điểm tạo / xuất bản |

**`ScheduleDraftItem`** (mỗi record = 1 ca học của 1 khóa):

| Trường | Ý nghĩa |
|---|---|
| `MaKhoaHoc`, `MaGiaoVien`, `MucDoPhuHop` | Khóa, GV được GA chọn, % phù hợp môn |
| `ThuTrongTuan`, `MaCaHoc`, `MaPhong` | Slot: Thứ (2-7), Ca, Phòng |
| `TrangThai` | `xep_duoc` / `khong_xep_duoc` (CHECK constraint) |
| `Score` | Điểm tổng của slot |
| `ScoreBreakdownJson` | Bảng điểm thành phần (`ScheduleSlotScoreComponentsDto`) |
| `LyDoGoiYJson` | Danh sách lý do chọn (kèm lý do chọn GV) |
| `CanhBaoJson`, `LoiJson` | Cảnh báo / lỗi |

---

## 4. Thuật toán xếp lịch: Genetic Algorithm (`GeneticTimetableSolver`)

> Lưu ý: `docs/DEFENSE_SMART_ALGORITHMS.md` mô tả greedy, nhưng code hiện tại của P12 là **thuật toán di truyền**; greedy chỉ dùng để khởi tạo 1 cá thể đầu tiên và repair cuối.

### 4.1 Mã hóa nhiễm sắc thể

```
Chromosome
├── Genes[i][k]      : khóa i, buổi thứ k → index vào danh sách Feasible (slot hợp lệ)
└── TeacherGene[i]   : khóa i → index GV candidate (GV không cố định, GA tự chọn)
```

### 4.2 Xây bài toán (`BuildProblem`)

- Ngày xếp: `WeekDays = {2,3,4,5,6,7}` (Thứ 2 → Thứ 7; **không xếp Chủ nhật**).
- Với mỗi khóa × (thứ, ca, phòng):
  - Loại nếu `room.SucChua < sĩ số lớp`.
  - Loại nếu không GV candidate nào rảnh tại (thứ, ca) theo `confirmedAvailabilityByTeacher` (nếu GV đã khai form nguyện vọng).
  - Candidate GV: chỉ giữ người có `MucDoPhuHop >= MinTeacherSkill (70)`. **Không fallback GV gốc** nếu không đạt chuẩn → khóa vào `UnassignableCourseIds` (hard constraint).
  - Tính `StaticScore` → sắp `FeasibleByScore` giảm dần.

### 4.3 StaticScore (điểm slot trước GA)

```
100 (BaseScore)
- 5  nếu Thứ 7        (SaturdayPenalty)
- 8  nếu ca tối       (EveningPenalty — TenCa/Buoi chứa "Tối")
+ 5  nếu 1.0 ≤ SucChua/sĩ số ≤ 2.0   (GoodRoomFitBonus)
- 5  nếu SucChua/sĩ số > 2.0         (OversizedRoomPenalty)
```

### 4.4 Khởi tạo quần thể

1. **1 cá thể greedy**: xếp khóa có ít slot khả dụng trước; chọn GV có skill cao nhất và đủ số buổi rảnh; đặt slot theo `FeasibleByScore`; dùng `OccupancyState` chống trùng (GV/Lớp/Phòng/ca) và ≤ `WeeklyCapCa` (6 ca/tuần/GV).
2. **Các cá thể còn lại random**: `TeacherGene` ngẫu nhiên + mỗi gene slot ngẫu nhiên.

### 4.5 Hàm Fitness (`Evaluate`)

| Thành phần | Trọng số | Ghi chú |
|---|---|---|
| `+ SkillScoreWeight × (MucDoPhuHop/100)` | 150 | Thưởng GV hợp môn |
| `+ StaticScore` mỗi slot gán được | — | Điểm nền 100 + điều chỉnh phòng/thứ/ca |
| `− UnassignedSlotPenalty` | 500 | Mỗi ô buổi bị trống |
| `− HardConflictPenalty` × số xung đột | 1000 | Trùng (thầy / lớp / phòng) tại cùng (thứ, ca) |
| `− SameDayDuplicatePenalty × (số ca cùng ngày của 1 khóa − 1)` | 60 | Tránh học 2 buổi cùng môn trong 1 ngày |
| `− ConsecutiveShiftPenalty` | 30 | Tránh 2 ca liên tiếp (cùng khóa cùng ngày) |
| `− TeacherDailyLoadPenalty` nếu GV ≥ 3 ca/ngày | 15 | Ngưỡng `TeacherDailyLoadThreshold = 3` |
| `− ClassDailyLoadPenalty` nếu lớp ≥ 3 ca/ngày | 15 | Ngưỡng `ClassDailyLoadThreshold = 3` |
| `− WeeklyLoadPenalty × |ca/tuần − 5|` | 15 | Cân bằng định mức `WeeklyTargetCa = 5`; GV rảnh hoàn toàn không bị phạt |

### 4.6 Vòng lặp tiến hóa

```
for generation = 1 → TongTheHe (clamp 1..1000):
  1. Sort quần thể theo fitness; elitism: giữ top 2
  2. Chọn cha mẹ: TournamentSelect(k=3) — random 3, lấy tốt nhất
  3. Crossover uniform: với xác suất tyLeCheo, mỗi gene lấy từ cha/mẹ → 2 con
  4. Mutate:
     - TeacherGene: đổi ngẫu nhiên với xác suất 0.15
     - Mỗi gene slot: đổi ngẫu nhiên với xác suất tyLeCheo
  5. Evaluate con, thêm vào quần thể thế hệ sau
  6. onProgress(...) → ghi vào _progressStore[draftId] (FE poll 500ms)
  7. Early stop: staple ≥ DoTuoiThoToiDa thế hệ không cải thiện → break
```

- Clamp params: quần thể [10..200], tyLeCheo [0..1], tuổi thọ [1..100].
- RNG seed cố định `20260701` → kết quả **deterministic** (cùng dữ liệu cho cùng kết quả).

### 4.7 Repair cuối (`RepairGreedy`)

Sau GA, xếp lại từ đầu greedy bằng `OccupancyState` (bỏ hết gene cũ) để **đảm bảo output không xung đột**:
- Thứ tự khóa: (số candidate GV × số slot khả dụng) tăng dần → số buổi giảm dần → mã khóa tăng.
- Thử từng GV theo skill giảm dần; lấy đủ `RequiredSlots` slot rảnh (mỗi slot 1 ca/ngày khác nhau), tránh trùng thầy/lớp/phòng, ≤ `WeeklyCapCa`.

### 4.8 Decode kết quả

- GV của khóa = `TeacherGene` (kèm `MucDoPhuHop` + lý do chọn GV).
- Mỗi slot tính lại điểm bằng `ScheduleCandidateScoringService` (tính cả tải GV/lớp thực tế trong ngày), kèm `Components` (bảng điểm), `Reasons`, `Warnings`.
- Khóa đủ slot → `xep_duoc`; thiếu → `khong_xep_duoc` (cộng dồn `UnassignableCourseIds`).

---

## 5. Scoring soft (`ScheduleCandidateScoringService`) — dùng cho suggest-slots & decode

```
Score = 100 (Base) + RoomFit + Penalties
```

| Điều kiện | Điểm | Trạng thái |
|---|---|---|
| `TeacherDailyLoad >= 3` | −15 | penalty |
| `ClassDailyLoad >= 3` | −15 | penalty |
| Thứ 7 | −5 | penalty |
| Ca tối | −8 | penalty |
| `SucChua < sĩ số` | — | **hard fail** (`HardConstraintPassed=false`, có warning) |
| `1 ≤ SucChua/sĩ số ≤ 2` | +5 | bonus |
| `SucChua/sĩ số > 2` | −5 | penalty |
| Thiếu sĩ số/phòng | — | warning (không fail) |

Sort deterministic: `Score` giảm dần → ít warning → Thứ → Ca → Phòng; đánh `Rank` 1..N.

---

## 6. Luồng hoạt động chi tiết

### 6.1 Xếp lịch thông minh toàn kỳ (`POST /generate`)

1. Validate: user có quyền (`EnsureCanManageSchedule`), học kỳ hợp lệ (`ValidateSchedulableTermAsync`), có khóa học.
2. Nạp courses/shifts/rooms/skills/sĩ số/thời gian rảnh + map `QuyDoiTinChi` → `requiredSlots`.
3. Tạo `ScheduleGenerationJob` (trạng thái `draft`) + `_progressStore[draftId]`.
4. Gọi `_geneticSolver.Solve(...)` **đồng bộ trong request** (params GA từ request, clamp).
5. Ghi `ScheduleDraftItem` (xep_duoc + khong_xep_duoc), cập nhật job (`SoXepDuoc/SoKhongXepDuoc/Score`), lưu `TomTatJson`, audit log `GENERATE`.
6. Trả `ScheduleDraftDto`; FE hiện link sang màn "Bản nháp thời khóa biểu".

### 6.2 Progress (`GET /drafts/{draftId}/progress`)

- GA đang chạy → đọc từ `_progressStore` (theế hệ hiện tại / tổng / bestFitness / xếp được / ms).
- Đã xong → đọc từ DB job (ghi chú: lúc này `TheHeHienTai` được set bằng `TongCourse`, xem SmartTimetableService.cs:218-226).

### 6.3 Xuất bản (`POST /publish`) — bước quan trọng nhất

1. Validate quyền + học kỳ schedulable + draft tồn tại + `TrangThai = draft` + **không còn khóa không xếp được**.
2. `CreateExecutionStrategy` + **transaction Serializable**:
   - Học kỳ đã có TKB `da_xuat_ban` → **409 từ chối** (không ghi đè lịch đã công bố).
   - Hủy mọi TKB cũ `nhap` cùng (học kỳ, cơ sở).
   - Dựng `OccupationMap` từ DB (loại TKB `da_huy`).
   - Kiểm tra khớp: số khóa = `TongCourse`, mỗi khóa có đủ số ca theo `QuyDoiTinChi`, không GV nào > `WeeklyCapCa` (6) ca/tuần.
   - **Gán đè `KhoaHoc.MaGiaoVien` = GV do GA chọn** (ghi `giaoVienChanges` vào audit).
   - Với mỗi item: chống xung đột qua map (thầy/lớp/phòng) → tạo `ThoiKhoaBieu` `da_xuat_ban` với `NgayBatDau/NgayKetThuc` từ `HocKy`; lỗi bất kỳ → throw → **rollback toàn bộ**.
   - Job → `da_xuat_ban` + `NgayXuatBan`; commit; audit `PUBLISH`.

### 6.4 Check xung đột hàng loạt (`POST /check-xung-dot-batch`)

- Dựng `OccupationMap` từ DB, với từng item: khóa không tồn tại / thầy bận / lớp bận / phòng bận → `HasConflict = true` + danh sách lỗi.

### 6.5 Gợi ý slot đơn & hàng loạt (`suggest-slots`, `suggest-slots-batch`)

- Đơn: duyệt (Thứ 2-7 × ca active × phòng cùng campus + filter theo request) → loại hard (thầy/lớp/phòng bận, GV không rảnh theo form, GV ≥ 6 ca/tuần) → scoring → TopN (default 10, max 50).
- Hàng loạt: sắp khóa theo (MaGiaoVien → MaKhoaHoc) để deterministic; gán slot tốt nhất và **đánh dấu occupied** → khóa sau không trùng khóa trước; trả `Assigned`/`Unassigned(NO_VALID_SLOT)` + Summary.

---

## 7. Frontend (GiaoVu)

### `ScheduleManagerView.vue`
- Grid lịch **Thứ (2-7) × Ca**; card theo trạng thái (`nhap`/`da_xuat_ban`/`da_huy`); **drag & drop** đổi thứ/ca.
- Banner học kỳ schedulable từ store `academicSchedulingContext` (chặn xếp khi học kỳ không hợp lệ).
- 3 chế độ "Tạo lịch thông minh":
  1. **Tạo nhanh**: chọn khóa → nút "Gợi ý slot phù hợp" (`suggest-slots`) → "Kiểm tra xung đột" (`check-xung-dot`) → Lưu nháp/Đã xuất bản.
  2. **Gợi ý nhiều khóa**: tick khóa → `suggest-slots-batch` → review bảng slot (điểm, lý do) → tạo nháp hàng loạt.
  3. **Xếp lịch thông minh toàn kỳ**: chọn cơ sở + phạm vi (`unscheduled` hoặc tick tay) + 4 tham số GA → `generateDraft` với `clientDraftId` → modal progress poll 500ms (`getGenerationProgress`) → link sang màn Pending.
- Nút "Xuất bản (N nháp)": publish từng `ThoiKhoaBieu` `nhap` → `da_xuat_ban` (luồng cũ, không qua `/publish` của draft job).

### `PendingSchedulesView.vue`
- Lọc theo cơ sở + học kỳ → `listDrafts`.
- Chi tiết draft: thông tin chung, điểm TKB, **điểm thành phần** (`scoreBreakdown`), **lý do gợi ý** (`lyDoGoiY`), cảnh báo, lỗi.
- Nút **Xuất bản lịch** → `POST /publish` (confirm dialog + audit log).

---

## 8. Cấu hình (SmartTimetableScoringOptions — giá trị default)

| Key | Default | Ý nghĩa |
|---|---|---|
| `BaseScore` | 100 | Điểm nền mỗi slot |
| `PreferredShiftBonus` / `AvailableShiftBonus` | 15 / 5 | (reserved) |
| `TeacherDailyLoadThreshold` / `TeacherDailyLoadPenalty` | 3 / 15 | Quá tải GV theo ngày |
| `ClassDailyLoadThreshold` / `ClassDailyLoadPenalty` | 3 / 15 | Quá tải lớp theo ngày |
| `SaturdayPenalty` | 5 | Thứ 7 |
| `EveningPenalty` | 8 | Ca tối |
| `GoodRoomFitBonus` | 5 | Phòng vừa sức |
| `OversizedRoomPenalty` / `OversizedRoomRatio` | 5 / 2.0 | Phòng quá rộng |
| `SameDayDuplicatePenalty` | 60 | 2 ca cùng ngày cùng khóa |
| `ConsecutiveShiftPenalty` | 30 | Ca liên tiếp |
| `UnassignedSlotPenalty` | 500 | Slot không xếp được |
| `HardConflictPenalty` | 1000 | Xung đột cứng |
| `SkillScoreWeight` / `PreferMainSubjectTeacher` | 150 / true | Trọng số kỹ năng môn |
| `MinTeacherSkill` | 70 | Ngưỡng kỹ năng tối thiểu (hard) |
| `WeeklyTargetCa` / `WeeklyLoadPenalty` | 5 / 15 | Cân bằng định mức tuần |
| `WeeklyCapCa` | 6 | Cap cứng ca/tuần/GV |
| `DefaultTopN` / `MaximumTopN` | 10 / 50 | Số gợi ý |

> Không tìm thấy section `SmartTimetableScoring` trong appsettings hiện tại → hệ thống chạy toàn bộ bằng default trên.

---

## 9. Hard & Soft constraints tóm tắt

**Hard (không xếp / chặn publish):**
- Không trùng (GV | Lớp | Phòng) trong cùng (Học kỳ, Thứ, Ca).
- `PhongHoc.SucChua >= sĩ số lớp`.
- Phòng hoạt động (`hoat_dong`), ca hoạt động (`ConHoatDong`), cùng cơ sở.
- GV đạt `MucDoPhuHop >= 70` cho môn; chỉ xếp vào slot GV đã khai rảnh (nếu đã khai form).
- ≤ 6 ca/tuần/GV; không xếp Chủ nhật.
- Không publish đè lên TKB đã xuất bản của học kỳ/cơ sở.

**Soft (tối ưu điểm):**
- Phòng vừa sức (tỉ lệ 1..2), tránh phòng quá rộng.
- Ưu tiên ngày thường, tránh Thứ 7 và ca tối.
- Dàn đều tải GV (mục tiêu 5 ca/tuần), tránh >3 ca/ngày.
- Ưu tiên GV có skill cao nhất, môn chính.
- Tránh dồn 2 buổi cùng môn trong ngày, tránh ca liên tiếp.

---

## 10. Ghi chú triển khai & điểm cần lưu ý

- GA chạy **đồng bộ trong HTTP request** (không phải background job thật); progress giữ trong `ConcurrentDictionary<Guid, GenerationProgress>` (mất khi restart backend).
- `GetGenerationProgressAsync` sau khi xong trả `TheHeHienTai = TongCourse` (SmartTimetableService.cs:218-226) — có vẻ dùng nhầm field, không gây lỗi FE vì modal tự đóng khi `generate` trả về.
- Seed RNG cố định → deterministic, dễ test/reproduce.
- Draft chỉ có 2 trạng thái theo CHECK constraint: `draft` / `da_xuat_ban` (không có `da_huy`).
- Tài liệu cũ `DEFENSE_SMART_ALGORITHMS.md` mô tả greedy — **code hiện tại là GA**; cập nhật tài liệu defense nếu cần khớp.
- Bảng SQL kiểm chứng xung đột sau publish: `docs/sql/P12_3_SMART_TIMETABLE_VALIDATION.sql`; seed demo: `docs/sql/P12_4_SEED_LMS_TEST_P12.sql`.

---

*Tài liệu được tổng hợp từ code thật ngày 2026-08-10; khi sửa logic vui lòng cập nhật doc này.*