# EXAM WEBRTC PROCTORING FIX

**Date:** 2026-07-01  
**Branch:** feature/exam-taking-proctoring-fix  
**Status:** Implemented — Backend restart required

---

## 1. SignalR Hub Path

Hub: ExamMonitoringHub at /hubs/exam-monitoring  
Auth: [Authorize] — JWT from Bearer header or query string ?access_token=  
Group naming: exam-{maCaThi}

---

## 2. Frontend Hub Connection

```js
new signalR.HubConnectionBuilder()
  .withUrl(`${API_BASE_URL}/hubs/exam-monitoring`, {
    accessTokenFactory: () => token,
    transport: WebSockets | SSE | LongPolling,
  })
  .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
  .configureLogging(signalR.LogLevel.Information)
  .build()
```

accessTokenFactory: header Authorization for LongPolling/SSE.
OnMessageReceived in backend reads ?access_token= for WebSocket.

---

## 3. Backend Hub Methods

### Room Management
JoinExamRoom(int maCaThi) -> JoinedRoom + ProctorRequestedConnections
LeaveExamRoom(int maCaThi)

### Student
JoinAsStudent(int maCaThi, int maHocSinh) -> StudentConnectionIdBroadcast
ScreenShareStarted(int maCaThi, int maHocSinh) -> ScreenShareStatusChanged
ScreenShareStopped(int maCaThi, int maHocSinh) -> ScreenShareStatusChanged

### WebRTC Signaling (DTO-based)
SendOffer(WebRtcOfferDto dto) -> ReceiveOffer to targetConnectionId (fallback: group)
SendAnswer(WebRtcAnswerDto dto) -> ReceiveAnswer to targetConnectionId (fallback: group)
SendIceCandidate(WebRtcIceCandidateDto dto) -> ReceiveIceCandidate to targetConnectionId

### Proctor
SendViolationLog(...) -> ViolationDetected
UpdateStudentStatus(...) -> StudentStatusUpdated
SendWarningToStudent(connectionId, msg) -> WarningReceived

---

## 4. DTO Payload

C# DTOs added to ExamMonitoringHub.cs:
- WebRtcOfferDto: MaCaThi, MaHocSinh, TargetConnectionId, FromConnectionId, Offer
- WebRtcAnswerDto: same with Answer
- WebRtcIceCandidateDto: same with Candidate

Frontend sends:
connection.invoke('SendOffer', { maCaThi, maHocSinh, targetConnectionId, offer })

Frontend receives:
connection.on('ReceiveOffer', (dto) => { dto.fromConnectionId, dto.offer, ... })

---

## 5. Student Flow

1. getDisplayMedia() -> stream
2. POST /api/exam/taking/start -> session
3. examProctoringHub.connect(token)
4. joinAsStudent(maCaThi, maHocSinh)
5. screenShareStarted(maCaThi, maHocSinh)
6. onProctorRequestedConnections -> re-joinAsStudent
7. onReceiveOffer(dto) -> createStudentPeerConnection -> setRemoteDescription(dto.offer) -> createAnswer -> sendAnswer DTO
8. onReceiveIceCandidate(dto) -> addIceCandidate(dto.candidate)

---

## 6. Teacher Flow

1. examProctoringHub.connect(token)
2. joinExamRoom(maCaThi)
3. onStudentConnectionIdBroadcast(payload) -> createProctorPeerConnection -> createOffer -> sendOffer DTO
4. onReceiveAnswer(dto) -> setRemoteDescription(dto.answer)
5. onReceiveIceCandidate(dto) -> addIceCandidate(dto.candidate)
6. ontrack -> remoteStreams.set(maHocSinh, stream) -> video.srcObject = stream

---

## 7. CORS / JWT / WebSocket Config

CORS (appsettings.Development.json): already has localhost + 192.168.2.4:5173

JWT Hub auth (Program.cs) - ADDED:
OnMessageReceived reads ?access_token= for WebSocket transport

SignalR Detailed Errors - ADDED:
options.EnableDetailedErrors = builder.Environment.IsDevelopment()

---

## 8. Test Steps (2 browsers)

Browser A (Student):
1. localhost:5173 -> Login student
2. Open exam -> Start environment
3. Allow screen share (select full screen)
4. Console: [Hub] Connected, [Student] ReceiveOffer, [Student] Answer sent
5. No more "Failed to invoke SendOffer"

Browser B (Teacher):
1. localhost:5173 -> Login teacher
2. /teacher/proctoring -> select session -> Start proctoring
3. Console: [Hub] Connected, [Proctor] Student connected, [Proctor] Offer sent, [Proctor] Remote stream received
4. Grid shows live student screen video

---

## 9. Files Changed

Backend:
- Backend/Program.cs: OnMessageReceived + EnableDetailedErrors
- Backend/Hubs/ExamMonitoringHub.cs: DTOs + DTO-based signaling methods + logging

Frontend:
- frontend/src/services/examProctoringHub.js: DTO signaling, transport config, reconnect hooks
- frontend/src/views/Student/ExamTakeView.vue: DTO-based onReceiveOffer/ICE
- frontend/src/views/GiangVien/ProctoringView.vue: DTO-based sendOffer/ICE handlers
- frontend/src/components/GiangVien/WebRTCScreen.vue: dumb component receives :stream prop

---

## 10. Build Results

Backend: Compile OK (MSB3027 = file locked by running process, not code error)
Frontend: built in 2.47s, no errors

---

## 11. Transport Status

WebSocket: Fixed - OnMessageReceived now reads ?access_token= for Hub JWT
SSE: Fail on LAN HTTP (normal fallback behavior)
LongPolling: Always works via Authorization header

---

## 12. Known Limitations

1. getDisplayMedia requires Secure Context: http://IP may be blocked by Firefox.
   Solution: use localhost OR configure HTTPS with mkcert for LAN testing.
2. No TURN server needed for local/LAN (same network) - STUN only is sufficient.
3. TURN server needed for Internet/different subnet scenarios.
4. Hub authorization only checks [Authorize] JWT - does not verify PhanCongGiamThi/ThiSinhCaThi.
   TODO: add fine-grained check after basic flow is stable.
