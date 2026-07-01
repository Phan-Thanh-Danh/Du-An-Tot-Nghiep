# Báo Cáo: Sửa Lỗi và Hoàn Thiện Luồng Giám Sát Ca Thi WebRTC

## 1. Route và Flow Hiện Tại
**Student:**
- Học sinh truy cập trang danh sách ca thi (`ExamsView.vue`).
- Vào chi tiết ca thi, sau đó bấm vào làm bài (`ExamTakeView.vue`).
- Trong `ExamTakeView`, hệ thống yêu cầu chia sẻ màn hình, kết nối WebRTC (hiện tại Student đóng vai trò nhận Offer từ Proctor, sau đó gửi Answer).

**Teacher/Giám thị:**
- Giám thị truy cập danh sách ca thi được phân công (`ProctoringView.vue`).
- Chọn ca thi, điểm danh.
- Bắt đầu ca thi.
- Giám thị nhận `StudentConnectionIdBroadcast`, sau đó tự tạo Offer gửi cho Student.
- Nhận luồng video hiển thị trên grid.

## 2. API Hiện Có
Các API đã có sẵn trong `ExamController.cs`:
- Lấy danh sách ca thi của student: `GET /api/exam/student/list`
- Lấy phiên thi chi tiết: `GET /api/exam/taking/session/{maPhienThi}`
- Bắt đầu thi: `POST /api/exam/taking/start`
- Danh sách ca thi cho giám thị: `GET /api/exam/ca-thi`
- Danh sách học sinh trong ca thi: `GET /api/exam/ca-thi/{maCaThi}/thi-sinh`
- Điểm danh: `POST /api/exam/ca-thi/diem-danh`
- Bắt đầu ca thi: `POST /api/exam/ca-thi/{id}/start`

## 3. WebRTC Flow Hiện Tại & Lỗi
**Hiện tại:** 
- Proctor (Giám thị) gửi WebRTC Offer. Student nhận Offer và gửi Answer.
- Khi giám thị chưa kết nối hoặc reload trang, quy trình bị lỗi nếu student đã vào.
- Nếu nhiều giám thị cùng canh một phòng, luồng Offer từ Proctor gây xung đột hoặc trùng lặp (không rõ target).
- ICE Candidate thỉnh thoảng bị lỗi `Failed to execute 'addIceCandidate'` do chưa có `remoteDescription` (đã có queue nhưng logic bất đồng bộ có thể chưa triệt để).
- SignalR connectionId bị mismatch.

**Hướng Sửa Mới (theo yêu cầu):**
- Đảo ngược vai trò: Student là người Share Screen (có track) -> Student phải tạo WebRTC Offer.
- Giám thị nhận WebRTC Offer, trả về Answer.
- Cần map `studentConnectionIds` (từ `StudentConnectionIdBroadcast`) để Student biết gửi Offer cho ai. Tuy nhiên, nếu Student tạo Offer, Student có thể broadcast Offer (không có targetConnectionId) hoặc gửi cho `proctorConnectionId` nếu lưu trữ được.
- Hub Method `SendOffer` nếu chưa có `targetConnectionId` sẽ broadcast cho cả phòng `exam-{MaCaThi}`. Giám thị nhận được sẽ tạo Answer kèm `targetConnectionId` trả về lại cho đúng học sinh.

## 4. Kế Hoạch Sửa Chữa (Chi tiết trong Implementation Plan)
- **Frontend - `examProctoringHub.js`**: Đảm bảo các wrapper gọi đúng Payload.
- **Frontend - `ExamTakeView.vue` (Student)**:
  - Bắt đầu kết nối SignalR, lưu trữ danh sách Proctor connectionIds (từ `ProctorRequestedConnections` hoặc event mới).
  - Khởi tạo RTCPeerConnection, thêm track màn hình.
  - Tạo `Offer`, gọi `sendOffer` gửi tới giám thị.
  - Lắng nghe `ReceiveAnswer` và `ReceiveIceCandidate`.
- **Frontend - `ProctoringView.vue` (Teacher)**:
  - Lắng nghe `ReceiveOffer`. Tạo `RTCPeerConnection` cho học sinh, lưu vào map.
  - Tạo `Answer`, gọi `sendAnswer` gửi về student.
  - Xử lý ICE Candidate queueing.
  - Hiển thị nhiều màn hình (sử dụng video ref map).
- **Backend - Hub**: Hub hiện tại đã hỗ trợ `SendOffer`, `SendAnswer`, `SendIceCandidate` bằng DTO, không cần sửa đổi nhiều nhưng cần review lại hàm broadcast nếu thiếu.

## 5. Test LAN & Vấn Đề Secure Context
Trình duyệt yêu cầu **Secure Context (HTTPS)** để gọi `navigator.mediaDevices.getDisplayMedia()`.
- Khi chạy localhost: `http://localhost:5173` được coi là secure context.
- Khi chạy qua IP LAN (VD: `http://192.168.1.100:5173`), trình duyệt sẽ chặn `getDisplayMedia`.
=> **Cách giải quyết để test LAN**:
- Phải dùng HTTPS cho Vite dev server (sử dụng `@vitejs/plugin-basic-ssl` hoặc `mkcert`).
- Đảm bảo Backend ASP.NET Core cũng cho phép CORS từ origin HTTPS đó.
