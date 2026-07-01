# EXAM PROCTORING AUDIT

**Audit Date:** 2026-07-01  
**Scope:** Module Thi Cử — Trang làm bài thi có giám sát màn hình online  

---

## 1. Frontend Routes Hiện Có

| Route | Name | Component | Ghi chú |
|---|---|---|---|
| /student/exams/:examId/take | student-exam-take | ExamTakeView.vue | Fullscreen, ngoài layout shell |
| /student/exams | student-exams | ExamsView.vue | Trong Student layout |
| /student/exams/detail/:examId | student-exam-detail | ExamDetailView.vue | Chi tiết bài thi |
| /student/exams/:examResultId | student-exam-result | ExamResultView.vue | Kết quả |
| /teacher/proctoring | teacher-proctoring | ProctoringView.vue | Trang giám sát |

**Nhận xét:** ExamTakeView.vue dùng mockData (mockExams, mockQuestions). ProctoringView.vue có UI nhưng cũng dùng mock.

---

## 2. Backend API Hiện Có

### 2a. Exam Taking (Student) - /api/exam

| Method | Endpoint | Auth | Response |
|---|---|---|---|
| GET | /api/exam/student/list | Student | StudentExamListItemDto[] |
| POST | /api/exam/taking/start | Student | PhienThiDto (có MaPhienThi, MaDeKiemTra) |
| POST | /api/exam/taking/autosave | Student | success |
| POST | /api/exam/taking/submit | Student | PhienThiDto |

**QUAN TRỌNG:** /api/exam/taking/start nhận maCaThi, KHÔNG trả về câu hỏi.

### 2b. Quiz Attempt (câu hỏi) - /api/quiz-attempts

| Method | Endpoint | Auth | Response |
|---|---|---|---|
| GET | /api/quiz-attempts/{quizId}/availability | Student | QuizAvailabilityDto |
| POST | /api/quiz-attempts/{quizId}/start | Student | StartQuizAttemptResponse (có CauHoi[]) |
| PUT | /api/quiz-attempts/sessions/{attemptId}/autosave | Student | success |
| POST | /api/quiz-attempts/sessions/{attemptId}/submit | Student | QuizAttemptResultDto |

### 2c. Exam Admin - /api/exam

| Method | Endpoint |
|---|---|
| GET | /api/exam/ca-thi |
| GET | /api/exam/ca-thi/{id} |
| GET | /api/exam/ca-thi/{maCaThi}/thi-sinh |
| GET | /api/exam/ca-thi/{maCaThi}/giam-thi |
| GET | /api/exam/ca-thi/{maCaThi}/diem-danh |
| POST | /api/exam/ca-thi/diem-danh |
| GET | /api/exam/ca-thi/{maCaThi}/vi-pham |
| POST | /api/exam/vi-pham |

---

## 3. Backend Thiếu Endpoint

| Endpoint dự kiến | Lý do cần |
|---|---|
| GET /api/exam/taking/session/{maPhienThi} | Resume phiên thi đang diễn ra |
| GET /api/exam/taking/questions/{maPhienThi} | Bridge lấy câu hỏi từ phiên thi chính thức |
| POST /api/exam/taking/events | Log sự kiện giám sát |
| GET /api/proctor/exam-sessions | Ca thi giám thị đang phụ trách |
| GET /api/proctor/attempts/{maPhienThi}/events | Log sự kiện thí sinh |

---

## 4. SignalR Hub Hiện Có

Hub: ExamMonitoringHub tại /hubs/exam-monitoring

**Đã có:**
- JoinExamRoom(maCaThi), LeaveExamRoom(maCaThi)
- SendViolationLog(maCaThi, maHocSinh, loaiViPham, chiTiet)
- UpdateStudentStatus(maCaThi, maHocSinh, status)
- SendWarningToStudent(studentConnectionId, message)

**Thiếu (cần bổ sung vào hub):**
- ScreenShareStarted(maCaThi, maHocSinh)
- ScreenShareStopped(maCaThi, maHocSinh)
- SendOffer(maCaThi, targetConnectionId, sdp)
- SendAnswer(maCaThi, targetConnectionId, sdp)
- SendIceCandidate(maCaThi, targetConnectionId, candidate)
- BroadcastStudentConnectionId(maCaThi, maHocSinh, connectionId)
Events thiếu: ScreenShareStatusChanged, ReceiveOffer, ReceiveAnswer, ReceiveIceCandidate

---

## 5. Kế Hoạch WebRTC

`
Browser A (Student)          SignalR Hub             Browser B (Teacher)
    |                            |                           |
    |-- JoinExamRoom(maCaThi) -->|                           |
    |                            |<-- JoinExamRoom(maCaThi) -|
    |                            |                           |
    |-- getDisplayMedia() [USER PERMISSION REQUIRED]        |
    |-- ScreenShareStarted() --->|-- ScreenShareStatusChanged->|
    |-- SendOffer(sdp) -------->|-- ReceiveOffer(sdp) ------>|
    |                           |<-- SendAnswer(sdp) ---------|
    |<-- ReceiveAnswer(sdp) ----|                           |
    |   ICE exchange ...                                    |
    |   P2P stream ============================================>|
`

STUN: stun:stun.l.google.com:19302 (local test: không cần TURN)

---

## 6. Luồng Student

1. /student/exams → chọn bài thi (accessStatus=official)
2. Click bài thi → open /student/exams/:examId/take
3. Trang prepare: thông tin bài, kiểm tra môi trường, screen share
4. Bấm "Chia sẻ màn hình" → getDisplayMedia() (browser prompt)
5. Cấp quyền → connect SignalR → ScreenShareStarted
6. Nút "Bắt đầu làm bài" enabled
7. POST /api/exam/taking/start {maCaThi} → PhienThiDto
8. Dùng MaDeKiemTra từ PhienThiDto → POST /api/quiz-attempts/{maDeKiemTra}/start → câu hỏi
9. Làm bài: autosave mỗi 30s, theo dõi screen track
10. Nộp bài: confirm → POST /api/exam/taking/submit → dừng share

---

## 7. Luồng Teacher/Proctor

1. /teacher/proctoring → danh sách ca thi
2. Chọn ca → GET /api/exam/ca-thi/{id}/thi-sinh
3. Connect SignalR → JoinExamRoom(maCaThi)
4. Dashboard: danh sách thí sinh + status realtime
5. Nhận ScreenShareStatusChanged → cập nhật trạng thái
6. Nhận ReceiveOffer từ học sinh → tạo RTCPeerConnection → SendAnswer
7. ICE exchange → stream video hiển thị <video>
8. Timeline: vi phạm, screen share events

---

## 8. Rủi Ro Kỹ Thuật

| Rủi ro | Mức độ | Giải pháp |
|---|---|---|
| getDisplayMedia yêu cầu HTTPS | High | Dùng localhost (dev) |
| Auth JWT với SignalR | Medium | accessTokenFactory trong @microsoft/signalr |
| Track.onended không đáng tin | Medium | Kết hợp poll stream.active |
| ICE NAT (2 máy khác mạng) | Medium | STUN/TURN, local test OK |
| Hai hệ thống Exam+Quiz không liên thông | High | Bridge: lấy MaDeKiemTra từ PhienThiDto |
| ExamTakeView.vue dùng mock | High | Cần refactor toàn bộ |

---

## 9. Files Cần Tạo/Sửa

### Backend:
- **[MODIFY]** Hubs/ExamMonitoringHub.cs — Bổ sung WebRTC signaling methods
- **[NEW]** DTOs/Exam/ExamProctoringDtos.cs — DTOs cho signaling

### Frontend:
- **[NEW]** services/examApi.js — API client chính thức
- **[NEW]** services/examProctoringHub.js — SignalR hub client
- **[NEW]** services/webrtcScreenShare.js — WebRTC utility
- **[MODIFY]** views/Student/ExamTakeView.vue — Replace mock với API thật + WebRTC
- **[MODIFY]** views/GiangVien/ProctoringView.vue — Bổ sung WebRTC viewer

---

## 10. Phát Hiện Quan Trọng

1. **ExamTakeView.vue** (line 22-28): Import mockExams, mockQuestions — CẦN REFACTOR.
2. **Bridge câu hỏi:** StartExam trả về MaDeKiemTra, dùng nó để gọi /api/quiz-attempts/{maDeKiemTra}/start.
3. **autosave:** Exam module dùng CauTraLoiJson (string JSON), Quiz module dùng Answers[] (array). Cần đồng bộ format.
4. **ExamMonitoringHub** chưa có WebRTC signaling — đây là phần thiếu chính cần bổ sung.
5. **ProctoringView.vue** có UI tốt với iewState: sessions/attendance/dashboard nhưng dùng mock data và không có WebRTC viewer.
