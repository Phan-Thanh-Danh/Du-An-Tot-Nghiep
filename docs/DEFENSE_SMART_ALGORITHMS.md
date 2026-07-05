# Defense — Smart Algorithms (P11 & P12)

> **Branch:** `feature/p13-final-polish-demo-defense`
> **Date:** 2026-07-05
> **Purpose:** Tài liệu giải thích thuật toán cho buổi bảo vệ đồ án

---

## 1. Smart Course Allocation là gì?

**Định nghĩa:** Hệ thống gợi ý giảng viên phù hợp nhất cho từng môn học dựa trên dữ liệu chuyên môn (GiaoVienMonHoc), kinh nghiệm giảng dạy, mức độ phù hợp.

**Đầu vào:**
- Học kỳ cần phân công
- Môn học cần giảng dạy
- Danh sách lớp hành chính

**Đầu ra:**
- Danh sách giảng viên được đề xuất theo thứ tự ưu tiên
- Giáo vụ preview và xác nhận trước khi tạo KhoaHoc

**Cơ sở dữ liệu:**
- `GiaoVienMonHoc`: lưu mức độ phù hợp (0-100), số lần đã dạy, số năm kinh nghiệm, là môn chính hay phụ
- `GiaoVienChuyenNganh`: chuyên ngành của giảng viên

---

## 2. Smart Timetable là gì?

**Định nghĩa:** Hệ thống tự động xếp lịch học cho tất cả khóa học trong một học kỳ, đảm bảo các ràng buộc cứng (không xung đột) và tối ưu hóa theo điểm mềm (soft score).

**Đầu vào:**
- Học kỳ
- Đơn vị/cơ sở
- Danh sách khóa học (đã được phân công giảng viên)
- Danh sách phòng học, ca học
- OccupationMap hiện tại (lịch đã publish)

**Đầu ra:**
- Bản nháp lịch (draft) với các slot được xếp
- Thông tin số buổi xếp được / không xếp được
- Điểm đánh giá (score)

---

## 3. Vì sao không dùng random?

Random scheduling có thể tạo ra lịch, nhưng:
- **Không đảm bảo tối ưu**: random không ưu tiên slot tốt hơn
- **Không kiểm soát chất lượng**: không có cơ chế soft scoring
- **Không tái lập được**: cùng dữ liệu đầu vào có thể cho kết quả khác nhau

Giải pháp của chúng tôi dùng **greedy algorithm với soft scoring**:
- Duyệt từng khóa học theo thứ tự ưu tiên
- Thử từng slot (thứ + ca + phòng)
- Tính điểm cho từng slot dựa trên soft scoring
- Chọn slot có điểm cao nhất thỏa mãn hard constraints

---

## 4. Hard Constraints

Các ràng buộc cứng được kiểm tra bằng `OccupationMap`:

### 4.1 Không trùng giáo viên
```
Một giáo viên không thể dạy hai lớp khác nhau
trong cùng (Học kỳ, Thứ, Ca)
```
- Kiểm tra: `map.IsTeacherOccupied(maHocKy, thu, ca, maGiaoVien)`

### 4.2 Không trùng lớp
```
Một lớp không thể học hai môn khác nhau
trong cùng (Học kỳ, Thứ, Ca)
```
- Kiểm tra: `map.IsClassOccupied(maHocKy, thu, ca, maLop)`

### 4.3 Không trùng phòng
```
Một phòng không thể được sử dụng bởi hai lớp khác nhau
trong cùng (Học kỳ, Thứ, Ca)
```
- Kiểm tra: `map.IsRoomOccupied(maHocKy, thu, ca, maPhong)`

### 4.4 Phòng đủ sức chứa
```
Sức chứa phòng >= số lượng sinh viên ước tính
```
- Kiểm tra: `room.SucChua >= estimatedClassSize`

### 4.5 Đúng cơ sở/quyền
```
Giáo viên và phòng học phải thuộc cùng cơ sở với khóa học
```
- Kiểm tra: scope validation trong service

### 4.6 Phòng và ca học đang hoạt động
```
Phòng có trạng thái = 'hoat_dong'
Ca học có con_hoat_dong = 1
```

---

## 5. Soft Scoring

Khi nhiều slot cùng thỏa mãn hard constraints, hệ thống chọn slot có điểm cao nhất:

```
Score = 
  + (room.SucChua / 30) * 10        # Phòng vừa sức chứa (hệ số 30 SV)
  + teacherAvailable * 20            # Giáo viên rảnh (luôn true nếu pass hard)
  - existingWorkload * 5             # Tránh dồn lịch cho giáo viên
  + randomFactor * 0.1               # Phá vỡ tie (nếu cùng điểm)
```

**Giải thích:**
- **Phòng vừa sức chứa**: ưu tiên phòng không quá rộng hoặc quá chật so với sĩ số
- **Tránh dồn lịch**: giảm điểm nếu giáo viên đã có nhiều ca trong ngày
- **Random factor**: rất nhỏ (0.1), chỉ dùng để phá tie

---

## 6. OccupationMap hoạt động thế nào?

```
class OccupationMap {
    // Key: (maHocKy, thu, ca) -> Set<maGiaoVien>
    _teacherMap: Dictionary<string, HashSet<int>>
    
    // Key: (maHocKy, thu, ca) -> Set<maLop>
    _classMap: Dictionary<string, HashSet<int>>
    
    // Key: (maHocKy, thu, ca) -> Set<maPhong>
    _roomMap: Dictionary<string, HashSet<int>>
}
```

**Luồng hoạt động:**

1. **Khởi tạo**: Đọc tất cả lịch đã publish từ database, đánh dấu occupied
2. **Generate**: Với mỗi khóa học cần xếp:
   - Duyệt tất cả (thứ, ca, phòng)
   - Kiểm tra hard constraints qua OccupationMap
   - Tính soft score
   - Chọn slot tốt nhất
   - Đánh dấu occupied trong map (để course sau không trùng)
3. **Kết thúc**: Trả về danh sách các slot đã chọn

**Ưu điểm:**
- Toàn bộ kiểm tra trong bộ nhớ (O(1) lookup)
- Không cần truy vấn database trong lúc generate
- Có thể chạy độc lập (unit test không cần DB)

---

## 7. Preview & Publish an toàn

### Preview (Generate)
- Generate chỉ tạo dữ liệu trong bộ nhớ + lưu vào bảng tạm `ScheduleDraftItem`
- **Không ảnh hưởng** đến lịch thật (`ThoiKhoaBieu`)
- Có thể xem, chỉnh sửa, xóa draft trước khi publish
- Thông tin: số buổi xếp được (xepDuoc), không xếp được (khongXepDuoc), điểm

### Publish
- Publish kiểm tra lại toàn bộ xung đột một lần nữa
- Dùng **SQL Server transaction** (EF Core execution strategy) để đảm bảo:
  ```
  BEGIN TRANSACTION
    Kiểm tra xung đột với OccupationMap (từ DB + draft)
    Tạo ThoiKhoaBieu records
    Cập nhật trạng thái job -> 'da_xuat_ban'
  COMMIT  -- hoặc ROLLBACK nếu có lỗi
  ```
- Nếu có bất kỳ xung đột nào, toàn bộ bị **rollback**
- Sau publish, chạy **SQL validation** để xác nhận không có xung đột

---

## 8. Transaction Rollback

```csharp
var strategy = _context.Database.CreateExecutionStrategy();
await strategy.ExecuteAsync(async () =>
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // Validate conflicts with OccupationMap
        // Create ThoiKhoaBieu records
        // Update job status
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
});
```

**Tại sao dùng `CreateExecutionStrategy`?**
- SQL Server có `SqlServerRetryingExecutionStrategy` mặc định
- Execution strategy không hỗ trợ `BeginTransactionAsync` trực tiếp
- Giải pháp: wrap transaction trong `CreateExecutionStrategy().ExecuteAsync()`

---

## 9. SQL Validation chứng minh không trùng

Sau khi publish, chạy 10 queries từ `P12_3_SMART_TIMETABLE_VALIDATION.sql`:

| Query | Kiểm tra | Điều kiện |
|---|---|---|
| Q1 | Teacher conflict (TKB) | 0 rows |
| Q2 | Class conflict (TKB) | 0 rows |
| Q3 | Room conflict (TKB) | 0 rows |
| Q4 | Teacher conflict (BuoiHoc level) | 0 rows |
| Q5 | Class conflict (BuoiHoc level) | 0 rows |
| Q6 | Room conflict (BuoiHoc level) | 0 rows |
| Q7 | Capacity violation | 0 rows |
| Q8 | Inactive room usage | 0 rows |
| Q9 | Inactive shift usage | 0 rows |
| Q10 | Duplicate TKB | 0 rows |

**Kết quả thực tế (LMS_TEST_P12, 20 courses):** 10/10 PASS — zero conflicts, zero violations.

---

## 10. Giới hạn hiện tại

| Giới hạn | Mô tả | Ảnh hưởng |
|---|---|---|
| Score cố định | `ScoreAssignment` dùng divisor 30 | Phòng không chuẩn xác với lớp cực ít/cực đông |
| Không có workload limit | Không giới hạn số ca/ngày cho giáo viên | Giáo viên có thể bị dồn lịch |
| Không dùng GiaoVienChuyenNganh | Chưa dùng specialization trong scoring | Có thể gợi ý thiếu chính xác |
| Greedy, không global optimum | Thuật toán tham lam, không đảm bảo tối ưu toàn cục | Có thể bỏ lỡ giải pháp tốt hơn |

---

## 11. Hướng phát triển

- **OR-Tools / Constraint Solver**: Thay greedy algorithm bằng constraint programming để tìm nghiệm tối ưu toàn cục
- **Google OR-Tools** hoặc **Microsoft Solver Foundation** cho scheduling phức tạp
- **Machine Learning**: Dự đoán workload và tối ưu phân bố dựa trên lịch sử
- **Real-time adjustment**: Cho phép giáo viên đổi lịch trong giới hạn, hệ thống tự động đề xuất slot thay thế
