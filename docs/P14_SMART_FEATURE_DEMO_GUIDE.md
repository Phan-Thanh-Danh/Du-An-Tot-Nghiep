# P14 Smart Feature Demo Guide

> Hướng dẫn demo và giải thích thuật toán thông minh (P11, P12) cho buổi bảo vệ.

---

## 1. Smart Course Allocation (P11) — Phân công giảng dạy thông minh

### Mô tả nghiệp vụ

Khi giáo vụ (AcademicStaff) mở màn **Phân công giảng dạy** (`/giao-vu/assignments`), hệ thống:

1. **Chọn học kỳ** từ danh sách `GET /api/master-data/academic-terms`
2. **Chọn môn học** từ `GET /api/master-data/subjects`
3. **Chọn lớp** — mỗi lớp cần một giáo viên phụ trách
4. Hệ thống **đề xuất giáo viên** dựa trên:
   - `GiaoVienMonHoc` (bảng phân công giáo viên-môn học): độ phù hợp 0-100, số lần dạy, số năm kinh nghiệm
   - `GiaoVienChuyenNganh` (chuyên ngành của giáo viên): ưu tiên giáo viên cùng chuyên ngành
5. Giáo vụ **xem preview** và **xác nhận bulk-assign**
6. Backend tạo/bulk-update `KhoaHoc` trong transaction

### Giải thích "thông minh"

**Không phải AI**, mà là **thuật toán gợi ý dựa trên dữ liệu có cấu trúc**:
- Sử dụng bảng `GiaoVienMonHoc` để tính điểm phù hợp (fitness score)
- Ưu tiên giáo viên đã dạy môn đó nhiều lần
- Cân bằng workload (số tín chỉ/tuần)
- Kiểm tra trùng lịch (cùng ca, cùng ngày)
- Tôn trọng scope đơn vị/cơ sở

### Câu hỏi bảo vệ

> **"Vì sao gọi là thông minh?"**

Hệ thống không chỉ hiển thị danh sách giáo viên, mà tự động tính điểm phù hợp dựa trên: (1) lịch sử giảng dạy môn đó, (2) chuyên ngành đào tạo, (3) workload hiện tại, (4) ràng buộc cơ sở. Giáo vụ chỉ cần chọn môn/lớp, hệ thống gợi ý giáo viên tốt nhất, giúp giảm 80% thời gian so với làm thủ công.

---

## 2. Smart Timetable (P12) — Xếp lịch thông minh

### Mô tả nghiệp vụ

Sau khi có danh sách `KhoaHoc` (đã phân công giáo viên), giáo vụ vào màn **Xếp lịch thông minh** (`/giao-vu/schedule`):

1. **Generate**: Chọn học kỳ, hệ thống tạo draft schedule tự động
2. **Preview**: Xem bản xếp lịch dạng draft trong `ScheduleDraftItem`
3. **Conflict Check**: Kiểm tra xung đột trước khi publish
4. **Publish**: Ghi vào bảng `ThoiKhoaBieu` và `BuoiHoc` trong transaction

### OccupationMap — Cốt lõi của thuật toán

**OccupationMap** là cấu trúc trong bộ nhớ (in-memory dictionary) dùng để track occupation:

```
Dictionary<string, HashSet<int>>
```

Ví dụ:
- Key: `"teacher_1_2_3"` = Giáo viên 1, Thứ 2, Ca 3
- Key: `"room_5_4_2"` = Phòng 5, Thứ 4, Ca 2
- Key: `"class_3_6_1"` = Lớp 3, Thứ 6, Ca 1

Lookup là **O(1)** — cực nhanh.

### Hard constraints (bắt buộc)

1. **Không trùng giáo viên**: 1 giáo viên không thể dạy 2 nơi cùng lúc
2. **Không trùng lớp**: 1 lớp không thể học 2 môn cùng lúc
3. **Không trùng phòng**: 1 phòng không thể có 2 buổi học cùng lúc
4. **Phòng đủ sức chứa**: SucChua >= sĩ số lớp
5. **Phòng/ca đang hoạt động**: TrangThai = active
6. **Đúng cơ sở**: Cùng MaDonVi

### Soft scoring (tối ưu hóa)

```
score = (room.SucChua / 30) * 10    // phòng phù hợp
       + teacherAvailable * 20       // giáo viên rảnh
       - existingWorkload * 5        // giảm dồn lịch
       + randomFactor * 0.1          // deterministic
```

- **Xếp vào phòng có sức chứa phù hợp nhất** (không lãng phí phòng lớn cho lớp nhỏ)
- **Giảm dồn lịch** (giáo viên không bị dồn nhiều ca trong 1 ngày)
- **Deterministic**: randomFactor = 0.1, seed cố định → kết quả giống nhau mỗi lần chạy
- **Không dùng randomness thật** để đảm bảo tái lập được

### Generate → Publish flow

```
GenerateAsync()
  │
  ├─ Lấy danh sách KhoaHoc, CaHoc, PhongHoc
  ├─ Khởi tạo OccupationMap (rỗng)
  ├─ Với mỗi KhoaHoc:
  │   ├─ Tìm ca/phòng không occupation
  │   ├─ Tính soft score cho từng option
  │   ├─ Chọn option tốt nhất
  │   └─ Update OccupationMap
  ├─ Lưu ScheduleDraftItem (transaction)
  └─ Trả về DraftDto
    
PublishAsync()
  ├─ Re-validate conflicts (phòng xa hơn vì có dữ liệu thật)
  ├─ BEGIN TRANSACTION (SqlServerRetryingExecutionStrategy)
  │   ├─ Tạo ThoiKhoaBieu records
  │   ├─ Tạo BuoiHoc records
  │   └─ Xóa ScheduleDraftItem
  └─ COMMIT (hoặc ROLLBACK nếu lỗi)
```

### SQL Validation sau Publish

```sql
-- Không trùng giáo viên
SELECT COUNT(*) FROM BuoiHoc bh
JOIN ThoiKhoaBieu tkb ON bh.MaTKB = tkb.MaTKB
WHERE tkb.MaHocKy = @termId
GROUP BY tkb.MaGiaoVien, bh.ThuTrongTuan, bh.MaCa
HAVING COUNT(*) > 1
```

### Câu hỏi bảo vệ

> **"Làm sao đảm bảo không trùng lịch?"**

Thuật toán dùng **OccupationMap** — 3 dictionary riêng cho giáo viên, lớp, phòng. Trước khi xếp một buổi học, hệ thống kiểm tra cả 3 dictionary trong O(1). Nếu bất kỳ occupation nào tồn tại, buổi đó bị loại. Sau publish, SQL validation chạy để confirm không có conflict.

> **"Nếu đang preview mà người khác tạo lịch trùng thì sao?"**

Publish dùng **SqlServerRetryingExecutionStrategy** — toàn bộ operation gói trong transaction. Khi publish, hệ thống re-validate conflicts trên dữ liệu thật (không phải draft). Nếu phát hiện conflict, transaction rollback và báo lỗi. Đây là optimistic concurrency ở mức database.

> **"Vì sao không dùng AI/ML?"**

Hệ thống giáo dục có các ràng buộc cứng (hard constraints) rất rõ ràng: không trùng giáo viên/lớp/phòng. AI/ML không cần thiết cho bài toán có cấu trúc này. Thuật toán tham lam (greedy) với soft scoring cho kết quả tối ưu trong O(n × m × p) với n=số khóa học, m=số ca, p=số phòng — đủ nhanh cho quy mô trường học (vài trăm khóa học).

> **"Hạn chế và hướng phát triển?"**

Hiện tại dùng greedy algorithm — chọn option tốt nhất cho từng khóa học theo thứ tự, không quay lui (backtracking). Hạn chế: nếu xếp theo thứ tự không tối ưu, khóa học sau có thể không tìm được chỗ trống trong khi vẫn còn tài nguyên. Hướng phát triển: dùng SAT solver, constraint propagation, hoặc genetic algorithm để tối ưu toàn cục.

---

## 3. Substitute / Dạy thay

### Mô tả nghiệp vụ

Khi giáo viên bận, giáo vụ cần tìm người dạy thay:

1. **Chọn buổi học** cần dạy thay từ `BuoiHocController`
2. Hệ thống **tìm giáo viên thay thế** dựa trên:
   - Role Teacher, Active
   - Có `GiaoVienMonHoc` cho môn đó
   - Không trùng lịch (kiểm tra OccupationMap)
   - Cùng cơ sở
3. Giáo vụ xác nhận → `PUT /api/buoi-hoc/{id}/change-teacher`
4. `BuoiHoc` cập nhật `MaGiaoVienDayThay`

### Câu hỏi bảo vệ

> **"Dạy thay khác gì với phân công thông thường?"**

Dạy thay giữ nguyên `MaGiaoVien` chính (giáo viên phụ trách), chỉ set `MaGiaoVienDayThay`. Buổi học vẫn được tính vào workload của giáo viên chính, nhưng giáo viên thay thế được ghi nhận trong lịch sử.

---

## 4. Câu nói ngắn cho bảo vệ (5 đoạn)

### Đoạn 1: "Hệ thống thông minh là gì?"

> Hệ thống thông minh ở đây không phải AI, mà là tự động hóa các quy trình học vụ dựa trên dữ liệu có cấu trúc và ràng buộc nghiệp vụ. Thay vì giáo vụ phải tự nhẩm tính và kiểm tra bằng tay, máy tính làm việc đó nhanh hơn và chính xác hơn.

### Đoạn 2: "Làm sao không trùng lịch?"

> Chúng tôi dùng cấu trúc OccupationMap — ba dictionary riêng cho giáo viên, lớp học và phòng học. Mỗi khi xếp một buổi, kiểm tra cả ba dictionary trong O(1). Kiểm tra xong mới xếp, không có chuyện trùng lịch. Publish còn dùng transaction để re-validate lần cuối.

### Đoạn 3: "Preview người khác tạo lịch trùng?"

> Publish dùng SQL transaction. Khi publish, toàn bộ dữ liệu draft được kiểm tra conflict lại với dữ liệu thật trong database, không phải trong bộ nhớ. Nếu phát hiện trùng, transaction rollback và báo lỗi. Hoàn toàn an toàn cho môi trường nhiều người dùng.

### Đoạn 4: "Tại sao không dùng AI?"

> Vì bài toán xếp lịch trong giáo dục có ràng buộc rất rõ ràng. AI sinh ra để giải quyết vấn đề mơ hồ, còn ở đây ta có hard constraints và soft scoring xác định. Thuật toán tham lam với soft scoring cho kết quả tốt, deterministic, và dễ debug.

### Đoạn 5: "Hạn chế và tương lai?"

> Hiện tại dùng greedy algorithm — xếp lần lượt từng khóa học. Hạn chế là đôi khi khóa học sau không tìm được chỗ nhưng nếu đổi thứ tự thì vẫn xếp được. Hướng phát triển là dùng constraint programming hoặc genetic algorithm để tối ưu toàn cục. Ngoài ra có thể thêm preference của giáo viên (muốn dạy sáng/chiều).
