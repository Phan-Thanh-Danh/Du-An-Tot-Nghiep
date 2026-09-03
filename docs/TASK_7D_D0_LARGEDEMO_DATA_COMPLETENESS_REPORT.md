# Task 7D-D0 — LargeDemo Smart Scheduling Data Completeness

Ngày kiểm chứng: 2026-09-02. Phạm vi là Docker profile `LargeDemo`; không tạo Draft, TKB hay Publish.

## Baseline và an toàn dữ liệu

- Baseline Git khi bắt đầu: `f1da8f45be3dd404a429a36f19d64eeed5bd5ef1` (`main`). Worktree đã bẩn từ các task trước; không stage, commit hay push thay đổi nào.
- Backup SQL Server trước khi nạp: `LMS_LargeDemo_Task7D_R0_20260902_190000.bak` trong volume `du-an-tot-nghiep_sqlserver_data`, đã `RESTORE VERIFYONLY` thành công.
- Không sửa dữ liệu của `HK Large V10`. Học kỳ lịch sử và học kỳ đang có lịch không bị sửa ngày hoặc lịch.

## Học kỳ mục tiêu

Không có học kỳ tương lai chưa xếp lịch tại cơ sở 14 đáp ứng đồng thời 25–40 khóa học và 20–35 lớp. Seeder vì vậy tạo mới, bằng mã ổn định:

| Thuộc tính | Giá trị |
|---|---|
| Cơ sở | FPT Polytechnic Hồ Chí Minh (campus 14) |
| Học kỳ | `HK_DEMO_SMART_2029_HCM` |
| Khoảng thời gian | 2029-01-01 đến 2029-04-30 |
| Trạng thái | mở, không khóa, chưa xếp lịch |
| Block | 5 |

## Ma trận đầy đủ dữ liệu

| Chuỗi đầu vào | Kết quả Docker |
|---|---:|
| Khóa học / lớp hành chính | 30 / 30 |
| Môn học và giảng viên phụ trách | 30 / 30 |
| Năng lực GV | Mỗi môn được chọn có ít nhất 2 GV hoạt động, mức phù hợp >= 70 |
| Lớp học phần | 30 |
| Đăng ký học phần thực | 409 (9–30 mỗi học phần) |
| Nguyện vọng giảng dạy GV | 9, có chi tiết ca ưu tiên |
| Ca học / phòng | dùng danh mục hoạt động sẵn có của campus; sức chứa section là 40 |
| Khóa học thiếu GV/môn/lớp/block | 0 |
| Draft / TKB published | 0 / 0 |

Lớp được lấy từ sinh viên hoạt động thật của campus và chỉ nhận vào seed khi sĩ số không vượt phòng hoạt động lớn nhất. Điều này xử lý nguyên nhân gốc ở dữ liệu mới thay vì tạo lớp trống hoặc giả định sĩ số đồng đều.

## Cách triển khai

- `LargeDemoSeeder` chạy phần D0 trước guard dữ liệu điểm lớn, còn seed LargeDemo tổng thể vẫn bỏ qua như trước.
- Mã học kỳ, mã lớp học phần và kiểm tra bản ghi đều cố định/idempotent; restart backend không nhân bản dữ liệu.
- `Program` chỉ gọi seed khi `SeedProfile=LargeDemo`; môi trường mặc định vẫn không tự seed.
- Migration cột đánh giá GV được đổi sang DDL có guard để Docker đã có cột vẫn khởi động và ghi nhận migration bình thường.

## Xác minh

- `dotnet build Backend/Backend.csproj --no-restore -p:OutputPath=C:\\codex-tmp\\d0-build\\`: thành công (2 cảnh báo dependency có sẵn).
- `docker compose up -d --build backend`: thành công; backend lắng nghe tại cổng 5597.
- `GET http://127.0.0.1:5597/openapi/v1.json`: HTTP 200.
- Restart backend rồi truy vấn lại giữ nguyên 30 khóa học, 30 lớp học phần, 409 đăng ký và 9 nguyện vọng.

## Nợ dữ liệu đã ghi nhận

Kiểm kê trước D0 cho thấy 20 vi phạm sức chứa lịch sử: 2 ở HK1 2026 và 18 ở `HK Large V10`. Chúng không bị tự động sửa vì thuộc dữ liệu lịch sử/stress không nằm trong học kỳ D0. Học kỳ D0 mới không có TKB/Draft nên không phát sinh vi phạm lịch; dữ liệu sĩ số–phòng được bảo đảm ngay tại seeder.

## C2 closure — actual schedulable term (2026-09-03)

- AcademicStaff campus 14 context chọn đúng `HK1_2027` / ID 15 và `canPrepareSchedule=true`; không dùng override frontend hay SuperAdmin.
- COPY_ONLY backup đã verify: `LMS_LargeDemo_Task7D_D0_C2_20260903_000000.bak`.
- Dữ liệu D0 tại kỳ thật: 30 khóa, 30 LHP, 409 đăng ký, 5 Block, không duplicate course/enrollment/preference. Mapping 3 tín chỉ dùng chính sách hiện hữu 3 buổi/tuần, không thêm mapping 1/1/1.
- Đã bổ sung capacity GV demo theo năng lực môn học để 90 ca/tuần được phân cho 15 GV, tối đa 6 ca/GV.
- Generate API thật bởi AcademicStaff với filter 30 khóa: HTTP 200, 30/30 xếp được, 0 chưa xếp, 0 hard conflict, score 97. Không publish. Draft `3b2979d3-4705-4054-a3a9-1b741b7c56e4` và Job 128 đã cleanup; Job/Draft baseline còn lại không bị đụng.
- Fresh Docker database: startup đầu tạo 10 phòng active, 30 khóa, 30 LHP, 300 đăng ký; restart thứ hai giữ nguyên 30/30/300, không duplicate. Database test đã bị drop an toàn.
- Targeted tests `P12_2_SmartTimetableEngineTests` và `P25_AcademicSchedulingContextTests`: 5/5 pass. Backend build pass. Không stage, commit, push, publish; không chạm `bghExport.js`, secrets/R2/PayOS.
