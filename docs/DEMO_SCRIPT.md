# Demo Script Bảo Vệ Đồ Án

## Mục Tiêu Demo

Trình bày hệ thống LMS/Academic Management System theo hướng rõ ràng: kiến trúc, xác thực, trải nghiệm học sinh, dữ liệu học vụ và phần mở rộng cho giáo viên/admin/AI. Khi demo, phân biệt phần đã chạy thật với phần dự kiến.

## Checklist Trước Khi Bảo Vệ

- Backend build thành công.
- Frontend build thành công.
- SQL Server có database và seed data demo.
- Tài khoản demo có role rõ ràng: student, teacher/admin nếu có.
- Không dùng secret/password thật trong slide hoặc code.
- API contract đã cập nhật theo controller thật.
- Chuẩn bị dữ liệu demo: khóa học, bài học, bài tập, điểm, điểm danh, thông báo.
- Kiểm tra màn hình ở độ phân giải máy chiếu.

## Mở Đầu Demo

1. Giới thiệu tên dự án: LMS Academic Management System.
2. Nêu mục tiêu: quản lý học tập, học vụ, người dùng, tổ chức/cơ sở, theo dõi tiến độ và hỗ trợ AI.
3. Trình bày stack:
   - Backend: ASP.NET Core, EF Core, SQL Server, JWT.
   - Frontend: Vue 3, Vite, Pinia, Vue Router, Tailwind.
4. Nêu nguyên tắc: API rõ ràng, phân quyền theo vai trò, dữ liệu theo cơ sở.

## Demo Login

Trạng thái hiện tại:
- Backend đã có `POST /api/auth/login`.
- Frontend có view `/login` nhưng cần kiểm tra mức độ nối API thực tế.

Kịch bản:
1. Mở trang login.
2. Nhập tài khoản demo.
3. Gửi login.
4. Trình bày response backend gồm access token, hạn token, trạng thái đổi mật khẩu lần đầu và thông tin user.
5. Nếu user yêu cầu đổi mật khẩu lần đầu, demo flow đổi mật khẩu nếu frontend đã hỗ trợ; nếu chưa, giải thích backend đã có API `POST /api/auth/change-password`.

## Demo Student Dashboard

Trạng thái hiện tại:
- Router đã có `/student/dashboard`.
- Layout student đã có sidebar/topbar.
- Dữ liệu có thể đang là demo/mock tùy màn.

Kịch bản:
1. Vào `/student/dashboard`.
2. Giới thiệu các khối thông tin chính: khóa học, lịch học, bài tập, điểm, thông báo.
3. Nhấn mạnh dashboard là điểm vào cho học sinh/sinh viên.
4. Nếu dữ liệu chưa nối API, nói rõ phần API dashboard là dự kiến/cần bổ sung.

## Demo Course/Lesson

Trạng thái hiện tại:
- Router đã có `/student/courses` và `/student/courses/:courseId`.
- Database có `KhoaHoc`, `Chuong`, `BaiHoc`, `TienDoBaiHoc`.
- Chưa thấy controller course/lesson.

Kịch bản:
1. Mở danh sách khóa học.
2. Chọn một khóa học.
3. Trình bày cấu trúc chương/bài học, loại bài học video/pdf/văn bản/trắc nghiệm nếu UI có.
4. Giải thích tiến độ học sẽ lưu vào `TienDoBaiHoc`.
5. Ghi rõ API course/lesson hiện là dự kiến nếu chưa triển khai controller.

## Demo Assignment/Submission

Trạng thái hiện tại:
- Router đã có `/student/assignments` và `/student/assignments/:assignmentId`.
- Database có `BaiTap`, `BaiNop`, `CanhBaoDaoVan`.
- Chưa thấy controller assignment/submission.

Kịch bản:
1. Mở danh sách bài tập.
2. Vào chi tiết bài tập.
3. Trình bày hạn nộp, định dạng cho phép, số lần nộp tối đa.
4. Demo thao tác nộp bài nếu UI đã hỗ trợ.
5. Giải thích điểm, nhận xét, đạo văn và điểm AI đề xuất lưu ở `BaiNop`/`CanhBaoDaoVan`.

## Demo Grade/Attendance

Trạng thái hiện tại:
- Router đã có `/student/grades` và `/student/attendance`.
- Database có `DiemSo`, `DiemDanh`, `YeuCauSuaDiem`, `YeuCauMoKhoaDiemDanh`.
- Chưa thấy controller grade/attendance.

Kịch bản:
1. Mở bảng điểm.
2. Giới thiệu điểm quá trình, giữa kỳ, cuối kỳ, GPA môn học và trạng thái đạt/rớt.
3. Mở điểm danh.
4. Giới thiệu trạng thái có mặt/vắng/đi muộn/có phép.
5. Nếu chưa có API, trình bày đây là module database/UI dự kiến hoàn thiện tiếp.

## Demo Notification/Support

Trạng thái hiện tại:
- Router đã có `/student/support-tickets`, `/student/requests`.
- Database có `ThongBao`, `ThongBaoHenGio`, `PhieuHoTro`, `TinNhanHoTro`, `DonTu`.
- Chưa thấy controller notification/support.

Kịch bản:
1. Mở ticket hỗ trợ.
2. Trình bày học sinh gửi yêu cầu kỹ thuật/học vụ/tài chính.
3. Mở đơn từ/yêu cầu.
4. Giải thích workflow duyệt đơn có log trong `NhatKyDuyetDon`.
5. Nêu kế hoạch bổ sung notification realtime/email/push nếu thuộc phạm vi đồ án.

## Demo Teacher/Admin Nếu Có

Trạng thái hiện tại:
- Backend có role/policy và endpoint organizations cho SuperAdmin.
- Frontend chưa thấy route teacher/admin đầy đủ.

Kịch bản backend/API:
1. Demo `GET /api/organizations/tree` với token hợp lệ.
2. Demo tạo/sửa/xóa tổ chức nếu dùng tài khoản `SuperAdmin`.
3. Giải thích campus scope: SuperAdmin thấy toàn bộ, CampusAdmin theo cây cơ sở, user khác theo cơ sở hiện tại.
4. Demo `GET /api/admin/accounts` như endpoint mẫu kiểm tra role, không trình bày như module account management hoàn chỉnh.

## Demo AI Features Nếu Có

Trạng thái hiện tại:
- Database/model có trường phục vụ AI.
- Chưa thấy controller AI.

Kịch bản dự kiến:
1. Tóm tắt bài học từ `BaiHoc.TomTatAi`.
2. Gợi ý điểm bài nộp từ `BaiNop.DiemAiDeXuat`.
3. Cảnh báo đạo văn từ `CanhBaoDaoVan`.
4. Dự báo rủi ro vắng/rớt môn từ `BaoCaoRuiRoVang`, `BaoCaoRuiRoRotMon`.
5. Phân tích cảm xúc đánh giá giáo viên từ `DanhGiaGiaoVien`.

Khi bảo vệ, chỉ demo live nếu đã có controller/service/UI thật; nếu chưa, trình bày là hướng mở rộng có sẵn nền tảng dữ liệu.

## Kết Luận Demo

- Nhấn mạnh hệ thống đã có nền tảng backend, database học vụ rộng và frontend student shell.
- Nêu rõ phần đã triển khai thật: Auth, Organizations, JWT, EF Core schema, student routes/layout.
- Nêu phần cần hoàn thiện: API học tập/bài tập/điểm danh/điểm số, auth frontend, teacher/admin UI, AI service.
- Kết thúc bằng roadmap ngắn và giá trị học thuật của đồ án.
