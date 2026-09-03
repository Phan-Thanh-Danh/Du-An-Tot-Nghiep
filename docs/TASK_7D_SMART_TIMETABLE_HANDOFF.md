# Smart Timetable — Task 7D Handoff

## Mục đích

Tài liệu này là context bắt buộc trước khi tiếp tục bất kỳ task nào của Smart Timetable. Nó chốt cách hệ thống chọn kỳ, chuẩn bị dữ liệu, đánh giá khả thi, tạo draft và xuất bản.

## Luồng nghiệp vụ

`AcademicStaff` gọi `GET /api/academic-scheduling/context` trước. Context là nguồn sự thật: chỉ cho phép chuẩn bị lịch cho học kỳ tương lai gần nhất của chính campus. Với LargeDemo hiện tại, campus 14 chọn `HK1_2027` (ID 15); không ép UI hay backend sang kỳ fixture xa hơn.

Khi người dùng chọn khóa học, frontend gửi `POST /api/thoi-khoa-bieu/generate` với `maHocKy`, `maDonVi` và tùy chọn `maKhoaHocFilter`. Controller ủy quyền cho `SmartTimetableService`. Service kiểm tra quyền/campus, gọi `ValidateSchedulableTermAsync`, tải khóa học, ca học và phòng active, rồi chạy feasibility trước solver. Chỉ khi khả thi mới tạo `ScheduleGenerationJob` trạng thái `draft`, gọi `GeneticTimetableSolver`, lưu `ScheduleDraftItem` và trả draft.

Publish là bước tách biệt `POST /api/thoi-khoa-bieu/publish`; Generate tuyệt đối không tự publish. Draft test phải được xóa qua `DELETE /api/thoi-khoa-bieu/drafts/{draftId}` sau verification, chỉ đúng draft vừa tạo.

## Tiêu chí xếp lịch bắt buộc

- Kỳ phải đúng context, mở và chưa có lịch đã xuất bản cần bảo vệ.
- Khóa học phải có campus, môn, lớp, block thuộc chính kỳ, giảng viên và LHP đúng kỳ.
- `CourseCapacityService` tính sĩ số từ sinh viên active và đăng ký/LHP; thiếu dữ liệu là `DATA_INCOMPLETE`, không coi là 0.
- Phòng phải active, cùng campus và đủ sức chứa; ca học phải active.
- Mapping `QuyDoiTinChi` là chính sách thật; không thêm mapping 1/1/1 để ép pass. 3 tín chỉ trong demo hiện là 3 buổi/tuần.
- Giảng viên phải có năng lực môn active, đạt ngưỡng phù hợp; tổng ca không vượt `WeeklyCapCa` (6). Unavailable slots là hard constraint.
- Solver không được tạo trùng phòng, lớp hoặc giảng viên trong cùng ngày/ca; block và ngày phải nằm trong kỳ.

## Dữ liệu LargeDemo / C2 baseline

Seeder chạy chỉ khi `SeedProfile=LargeDemo`. Sau prerequisite broad seed, `LargeDemoSeeder` ensure dữ liệu Smart Scheduling trong kỳ context hiện hành. Quy trình idempotent và được bọc trong SQL Server execution strategy + transaction.

Baseline đã xác minh cho LMS Docker: `HK1_2027` campus 14 có 30 khóa D0, 30 LHP, 409 đăng ký, 5 Block; Generate với đúng 30 khóa trả 30 xếp được, 0 unassigned và 0 hard conflict. `HK_DEMO_SMART_2029_HCM` được giữ làm fixture lịch sử, không phải target hiện hành.

Fresh database có ID khác với LMS hiện tại; không hardcode ID 15 trong seeder. Fresh one-start tạo room bootstrap khi campus chưa có phòng active, sau đó tạo 30 khóa/30 LHP/300 đăng ký; restart không duplicate.

## Files cần đọc và phạm vi tiếp tục

Đọc các file nêu ở `AGENTS.md` trước. Nếu sửa logic solver/capacity/context/seeder, chạy build backend, test targeted Smart Timetable + Academic Scheduling Context, verify Docker LargeDemo, và gọi Generate qua API AcademicStaff. Không commit/publish khi chưa có yêu cầu mới.
