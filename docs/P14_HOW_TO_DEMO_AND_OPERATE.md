# P14 How to Demo & Operate

> Hướng dẫn vận hành và demo cho buổi bảo vệ.

---

## 1. Cách chạy Backend

```powershell
cd Backend

# Set environment variables (1 lần mỗi session)
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Server=DELL\SQLEXPRESS02;Database=LMS_TEST_P12;Trusted_Connection=true;TrustServerCertificate=true;Encrypt=false"

# Chạy
dotnet run --urls http://localhost:5097
```

**Thời gian start**: ~30-45 giây (EF Core model compilation).
**Kiểm tra**: `curl -X POST http://localhost:5097/api/auth/login -H "Content-Type: application/json" -d "{\"email\":\"superadmin@lms.local\",\"password\":\"123456\"}"`
→ Nếu trả về 200 có accessToken là backend đã chạy và login OK.

## 2. Cách chạy Frontend

```powershell
cd frontend
npm run dev
```

**URL**: https://localhost:5173 (Vite dùng HTTPS với self-signed cert)
**Lưu ý**: Trình duyệt sẽ cảnh báo SSL — click "Advanced" → "Proceed to localhost".

## 3. Database cần dùng

| Database | Nội dung |
|---|---|
| **LMS_TEST_P12** | Database demo chính. Chứa 100 giáo viên + 10.000 sinh viên + 20 khóa học + 10 phòng + 4 ca học |
| Connection string | `Server=DELL\SQLEXPRESS02;Database=LMS_TEST_P12;Trusted_Connection=true;TrustServerCertificate=true;Encrypt=false` |

**Seed data**: Đã chạy `LargeDemoSeed` (100 teachers, 10000 students). Xem `docs/sql/P12_4_SEED_LMS_TEST_P12.sql`.

## 4. Demo accounts

| Role | Email | Password |
|---|---|---|
| Super Admin | `superadmin@lms.local` | `123456` |
| Staff (Giáo vụ) | `p12test_staff01@lms.local` | `Test@123` |
| Teacher CNTT | `p12test_teacher01@lms.local` | `Test@123` |
| Teacher TKĐH | `p12test_teacher07@lms.local` | `Test@123` |
| Student | `p12test_student011@lms.local` | `Test@123` |

## 5. Click flow — Staff (Giáo vụ)

1. `https://localhost:5173` → Portal Landing
2. Click **Giáo vụ** → `https://localhost:5173/login/giao-vu`
3. Login: `p12test_staff01@lms.local` / `Test@123`
4. Dashboard → Xem thống kê
5. **Course Allocation** → `/giao-vu/assignments` → Chọn học kỳ, xem danh sách phân công
6. **Smart Timetable** → `/giao-vu/schedule` → Generate → Preview → Publish
7. **Conflict Check** → `/giao-vu/conflicts` → Kiểm tra xung đột
8. **Rooms** → `/giao-vu/rooms` → Xem danh sách phòng
9. **Shifts** → `/giao-vu/shifts` → Xem ca học
10. **Academic Terms** → `/giao-vu/academic-terms` → Xem học kỳ

## 6. Click flow — Teacher

1. `https://localhost:5173` → Portal Landing
2. Click **Giảng viên** → `https://localhost:5173/login/giang-vien`
3. Login: `p12test_teacher01@lms.local` / `Test@123`
4. Dashboard → Xem thống kê
5. **Courses** → `/giang-vien/courses` → Xem khóa học được phân công
6. **Schedule** → Xem lịch dạy (sau P12 publish)
7. **Assignments** → Xem bài tập
8. **Grading** → Chấm bài
9. **Attendance** → Điểm danh

## 7. Click flow — Student

1. `https://localhost:5173` → Portal Landing
2. Click **Sinh viên** → `https://localhost:5173/login/sinh-vien`
3. Login: `p12test_student011@lms.local` / `Test@123`
4. Dashboard → Xem thống kê
5. **Courses** → `/student/courses` → Xem khóa học
6. **Course Detail** → Click vào khóa học → Xem nội dung
7. **Schedule** → `/student/schedule` → Xem lịch học
8. **Grades** → `/student/grades` → Xem điểm
9. **Attendance** → `/student/attendance` → Xem điểm danh
10. **Notifications** → Xem thông báo

## 8. Cách chạy SQL validation

Mở SQL Server Management Studio, kết nối `DELL\SQLEXPRESS02`, database `LMS_TEST_P12`.

Chạy file: `docs/sql/P12_3_SMART_TIMETABLE_VALIDATION.sql`

10 queries kiểm tra:
1. Không trùng giáo viên
2. Không trùng lớp
3. Không trùng phòng
4. Phòng đủ sức chứa
5. Phòng/ca active
6. Đúng cơ sở
7. Tổng số buổi học
8. Workload giáo viên
9. Số lớp/lịch
10. Capacity usage

Mỗi query trả về 0 rows = PASS.

## 9. Cách kiểm tra lỗi bằng Chrome DevTools

1. Mở Chrome → F12 (hoặc Ctrl+Shift+I)
2. Tab **Console**: Xem error, warning
3. Tab **Network**: Lọc `api` → Kiểm tra status code
4. **Lưu ý**:
   - 401 = chưa đăng nhập / token hết hạn
   - 403 = không có quyền
   - 404 = endpoint chưa có
   - 500 = backend bug
   - CORS = backend thiếu CORS config

## 10. Cách xử lý khi demo lỗi

### Backend không chạy
```
dotnet build → sửa lỗi compile
dotnet run → đợi 30-45s
```
Nếu lỗi port bị chiếm: đổi `--urls http://localhost:5098`

### Frontend trắng
```
npm run build → xem lỗi compile
Xóa node_modules, npm install, npm run dev
```
Nếu lỗi SSL: vào Chrome `chrome://flags/#allow-insecure-localhost` → Enable

### Login fail
Kiểm tra:
- DB đã seed chưa? (LMS_TEST_P12)
- Email/password đúng?
- Backend đã chạy?
- Proxy Vite có target đúng backend URL?

### 401/403
- Token hết hạn → refresh page
- Role không đúng → kiểm tra role trong DB
- Endpoint yêu cầu role cao hơn

### API 500
- Xem log backend trong terminal
- Kiểm tra DB connection
- Kiểm tra seed data

### DB chưa seed
Chạy:
```powershell
cd Backend
$env:ConnectionStrings__DefaultConnection = "Server=DELL\SQLEXPRESS02;Database=LMS_TEST_P12;..."
dotnet run --no-build --SeedProfile=LargeDemo
```

### Port bị chiếm
```powershell
netstat -ano | findstr :5097  # backend
netstat -ano | findstr :5173  # frontend
taskkill /PID <PID> /F
```
