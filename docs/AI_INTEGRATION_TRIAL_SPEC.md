# Đặc tả tích hợp thử AI Local vào AET LMS

## 1. Mục tiêu

Tích hợp thử AI local vào AET LMS theo kiến trúc an toàn:

```text
Frontend AET LMS
    -> Backend ASP.NET Core
    -> Ollama API
    -> qwen3.5:9b-q4_K_M (chat/suy luận)
    -> qwen3-embedding:0.6b (embedding/semantic search)
```

Frontend không được gọi trực tiếp Ollama. Mọi yêu cầu AI phải đi qua Backend để áp dụng JWT, phân quyền, giới hạn tải, logging và xử lý lỗi.

## 2. Trạng thái hạ tầng đã xác nhận

- Ollama chạy trên Windows tại `http://localhost:11434`.
- Backend AET LMS chạy trong Docker và truy cập Ollama qua `http://host.docker.internal:11434`.
- Model chat: `qwen3.5:9b-q4_K_M`.
- Model embedding: `qwen3-embedding:0.6b`.
- Embedding đã thử thành công và trả về vector 1.024 chiều.
- Model được lưu tại `E:\AI\OllamaModels`.
- Máy phát triển: RAM 16 GB, RTX 3060 Laptop 6 GB.

## 3. Phạm vi POC

POC cần hoàn thành các chức năng sau:

1. Backend kiểm tra được trạng thái Ollama và sự tồn tại của hai model.
2. Người dùng đã đăng nhập có thể gửi một câu hỏi tới model chat.
3. Backend có thể tạo embedding 1.024 chiều từ một đoạn văn.
4. Có timeout, hàng đợi/giới hạn đồng thời và thông báo khi AI không khả dụng.
5. Không làm ảnh hưởng các module hiện có của AET LMS.
6. Chuẩn bị cấu trúc để bổ sung Semantic Search và RAG ở giai đoạn sau.

## 4. Ngoài phạm vi POC đầu tiên

- Không fine-tune hoặc huấn luyện lại model.
- Không mở cổng Ollama trực tiếp ra Internet.
- Không cho Frontend gọi `/api/embed` của Ollama.
- Không tự động đưa toàn bộ database hoặc dữ liệu cá nhân vào prompt.
- Chưa triển khai vector database nếu chưa được phê duyệt giải pháp lưu trữ.
- Chưa thay đổi các thuật toán Smart Course Allocation/Smart Timetable hiện có.

## 5. Cấu hình đề xuất

Thêm cấu hình Backend tương đương:

```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "ChatModel": "qwen3.5:9b-q4_K_M",
    "EmbeddingModel": "qwen3-embedding:0.6b",
    "ContextLength": 4096,
    "MaxOutputTokens": 384,
    "TimeoutSeconds": 180,
    "MaxConcurrentChatRequests": 1,
    "MaxQueueSize": 10
  }
}
```

Không hard-code URL theo môi trường. Docker Compose phải ghi đè bằng biến môi trường:

```yaml
environment:
  Ollama__BaseUrl: ${OLLAMA_BASE_URL:-http://host.docker.internal:11434}
  Ollama__ChatModel: qwen3.5:9b-q4_K_M
  Ollama__EmbeddingModel: qwen3-embedding:0.6b
```

Nếu chạy Backend trực tiếp bằng `dotnet run`, dùng:

```env
OLLAMA_BASE_URL=http://localhost:11434
```

API Ollama local không cần API key. Không tạo API key giả trong Backend nếu thư viện HTTP không bắt buộc.

## 6. Cấu trúc Backend dự kiến

Tuân theo cấu trúc và quy ước hiện có của repository:

```text
Backend/
  Controllers/
    AiController.cs
  DTOs/
    AI/
      AiChatRequest.cs
      AiChatResponse.cs
      AiHealthResponse.cs
      AiEmbeddingTestRequest.cs
      AiEmbeddingTestResponse.cs
  Services/
    AI/
      IOllamaService.cs
      OllamaService.cs
      OllamaOptions.cs
      AiRequestGate.cs
```

Tên file có thể điều chỉnh theo quy ước thực tế của repository, nhưng không được tạo cấu trúc mới mâu thuẫn với `AGENTS.md` hoặc các module hiện có.

### Yêu cầu cho `OllamaService`

- Dùng `HttpClient` được đăng ký bằng Dependency Injection.
- Không tạo `new HttpClient()` cho từng request.
- Hỗ trợ `CancellationToken`.
- Áp dụng timeout từ cấu hình.
- Gọi `/api/tags` để kiểm tra model.
- Gọi `/api/chat` cho hội thoại.
- Gọi `/api/embed` cho embedding.
- Parse lỗi Ollama thành lỗi API thống nhất của AET LMS.
- Không ghi toàn bộ prompt chứa dữ liệu cá nhân vào log.
- Kiểm tra vector embedding có đúng 1.024 chiều trong POC.

### Đăng ký trong `Program.cs`

- Bind và validate `OllamaOptions`.
- Đăng ký typed/named `HttpClient`.
- Đăng ký service theo lifetime phù hợp.
- Đăng ký bộ giới hạn/hàng đợi AI dùng chung cho toàn Backend.
- Không thêm package mới nếu `HttpClient` và `System.Text.Json` đã đáp ứng được.

## 7. API POC đề xuất

### `GET /api/ai/health`

Mục đích: kiểm tra Backend -> Ollama và trạng thái hai model.

Phản hồi mẫu:

```json
{
  "available": true,
  "chatModel": "qwen3.5:9b-q4_K_M",
  "chatModelAvailable": true,
  "embeddingModel": "qwen3-embedding:0.6b",
  "embeddingModelAvailable": true
}
```

Endpoint phải có `[Authorize]`. Chỉ role quản trị/kỹ thuật phù hợp mới được xem chi tiết chẩn đoán; người dùng thường chỉ nhận trạng thái tổng quát.

### `POST /api/ai/chat`

Request tối thiểu:

```json
{
  "message": "Hãy hướng dẫn sinh viên lập kế hoạch học tập trong một học kỳ.",
  "conversationId": null
}
```

Response tối thiểu:

```json
{
  "answer": "...",
  "conversationId": "...",
  "model": "qwen3.5:9b-q4_K_M",
  "sources": []
}
```

Quy tắc:

- Bắt buộc `[Authorize]`.
- Lấy người dùng từ cơ chế current-user hiện có của AET LMS.
- Giới hạn độ dài `message`.
- Không tin `userId`, role hoặc đơn vị do Frontend truyền lên.
- Context mặc định 4.096 token.
- Output tối đa 256-384 token cho POC.
- Chỉ một lượt sinh câu trả lời được xử lý tại một thời điểm trên máy hiện tại.
- Khi hàng đợi đầy, trả lỗi nghiệp vụ rõ ràng thay vì treo request.

### `POST /api/ai/embedding-test`

Chỉ dành cho Development hoặc role quản trị kỹ thuật.

Request:

```json
{
  "text": "Sinh viên phải tham gia tối thiểu 80 phần trăm số buổi học."
}
```

Response:

```json
{
  "model": "qwen3-embedding:0.6b",
  "dimensions": 1024
}
```

Không trả toàn bộ 1.024 số về Frontend nếu không phục vụ mục đích chẩn đoán cụ thể.

## 8. System prompt POC

```text
Bạn là trợ lý học tập của hệ thống AET LMS.
Trả lời bằng tiếng Việt, rõ ràng và ngắn gọn.
Không tự bịa dữ liệu về người dùng, điểm số, lịch học hoặc quy định.
Nếu không có đủ dữ liệu, hãy nói rõ rằng chưa đủ thông tin.
Không tiết lộ dữ liệu của người dùng khác.
```

System prompt phải được quản lý phía Backend, không nhận toàn bộ từ Frontend.

## 9. Vị trí tích hợp Frontend — ĐÃ XÁC NHẬN THEO MÃ NGUỒN

> Đã rà soát và đối chiếu 100% với kiến trúc mã nguồn thực tế của hệ thống AET LMS.

- Route/trang cần đặt AI: `Toàn bộ các phân hệ qua Layout Shell: Sinh viên (/student/*), Giảng viên (/teacher/*), Giáo vụ (/staff/*), Ban Giám Hiệu (/bgh/*), Quản trị viên (/super-admin/*), Phụ huynh (/parent/*)`
- File Vue hiện có cần chỉnh sửa: `frontend/src/components/SinhVien/Layout_SinhVien.vue, Layout_GiangVien.vue, Layout_GiaoVu.vue, Layout_BGH.vue, Layout_SuperAdmin.vue, Layout_PhuHuynh.vue, và FocusAiCard.vue`
- Component sẽ chứa giao diện chat: `frontend/src/components/ui/AiAssistant.vue`
- Vị trí hiển thị trên trang: `Nút nổi Floating Action Button ở góc dưới cùng bên phải (fixed bottom-5 right-5 z-[150]), popup mở bảng chat kính mờ Liquid-Glass đa tầng`
- API service hiện có cần mở rộng: `frontend/src/services/aiApi.js (kết nối /api/ai/health, /api/ai/chat, /api/ai/embedding-test)`
- Pinia store cần sử dụng/tạo mới: `frontend/src/stores/auth.js (xác thực & vai trò) và composable frontend/src/composables/useAiAssistant.js (quản lý state mở/đóng, quick prompt & context)`
- Các role được phép sử dụng: `Tất cả người dùng đã đăng nhập: Student (Sinh viên), Teacher (Giảng viên), AcademicStaff (Giáo vụ), Principal (BGH), Admin/SuperAdmin, Parent, HoiDongQuanLyNoiDung`
- Kiểu giao diện mong muốn: `Floating Liquid-Glass Popup Chat Panel, hỗ trợ Thinking Accordion (quá trình suy nghĩ), Markdown rendering, Role Quick Prompts và latency badge`
- Có lưu lịch sử hội thoại không: `Lưu conversationId theo phiên (session memory) trong component và có nút Reset chat (làm mới hội thoại)`
- Yêu cầu bổ sung: `Bảo vệ dữ liệu CSDL cấp độ 1 & 2 (JWT current-user context), Concurrency Gate 1 request đồng thời tránh quá tải phần cứng local, fallback mượt mà khi AI offline`

Sau khi phần trên được điền, Frontend phải:

- Chỉ gọi API Backend của AET LMS.
- Dùng `apiClient` và cơ chế JWT hiện có.
- Có trạng thái loading, queued, success và error.
- Chặn gửi liên tục/double submit.
- Hiển thị thông báo khi AI đang bận hoặc offline.
- Không hard-code màu sắc trái với design tokens/glass classes của dự án.
- Không hiển thị raw stack trace hoặc lỗi nội bộ từ Ollama.

## 10. RAG giai đoạn 2

Chỉ triển khai sau khi Chat POC ổn định.

Luồng dự kiến:

```text
Tài liệu được phép truy cập
    -> trích xuất text
    -> chia chunk
    -> tạo embedding 1.024 chiều
    -> lưu vector + metadata + quyền truy cập
    -> embed câu hỏi
    -> semantic search
    -> lọc theo role/người dùng/đơn vị
    -> đưa top-k đoạn cho Qwen
    -> trả câu trả lời kèm nguồn
```

Metadata tối thiểu của một chunk:

- Document ID.
- Chunk ID.
- Nội dung chunk.
- Tên tài liệu.
- Loại tài liệu.
- Version/checksum.
- Đơn vị sở hữu.
- Role/quyền được phép truy cập.
- Ngày cập nhật.
- Thông tin nguồn để trích dẫn.

Không embedding mật khẩu, token, thông tin xác thực hoặc dữ liệu nhạy cảm không cần thiết.

## 11. Bảo mật, Phân quyền & Cô lập CSDL (Database Isolation & 3 Permission Levels)

### 11.1. Nguyên tắc cô lập CSDL tuyệt đối (`Ollama ↛ SQL Server`)

```text
               Ollama (AI Model)
                      ↛ (Không truy cập trực tiếp SQL Server)
                      ↕ (HTTP JSON - Chỉ nhận DTO đã lọc)
 Người dùng -> Frontend -> Backend ASP.NET Core -> SQL Server (Database)
               (JWT)      (Xác thực & Phân quyền)
```

- **Mô hình Ollama hoàn toàn không có quyền truy cập Database:** AI không nắm connection string, không biết cấu trúc bảng hay mật khẩu SQL.
- **Backend là cổng kiểm soát duy nhất (Gatekeeper & Mediator):**
  1. Người dùng gửi câu hỏi kèm JWT Bearer token.
  2. Backend xác thực JWT, lấy `CurrentUserContext` an toàn.
  3. Kiểm tra vai trò (Role) và phạm vi được phép truy cập.
  4. Backend Service truy vấn CSDL và chuyển đổi sang **DTO thu gọn (Sanitized DTO)**.
  5. Đưa DTO an toàn vào System Prompt/Ngữ cảnh của AI để tổng hợp câu trả lời.
  6. AI tuyệt đối **không tự chạy câu lệnh SQL** (`SELECT * FROM SinhVien;`).

### 11.2. Ba mức quyền áp dụng cho AI Assistant

| Mức Quyền | Phạm vi áp dụng | Hành vi & Cơ chế xử lý |
| :--- | :--- | :--- |
| **Mức 1 — Không truy cập dữ liệu cá nhân** | Hỏi đáp chung, giải thích thuật ngữ, công thức toán học, hướng dẫn quy trình học vụ công khai. | Không nạp dữ liệu cá nhân. RAG chỉ lấy tài liệu/quy chế công khai. |
| **Mức 2 — Chỉ đọc qua Backend Service** | Tra cứu điểm số cá nhân (`GetMyGrades`), Lịch học cá nhân (`GetMySchedule`), Chuyên cần (`GetMyAttendance`), Bài tập chưa nộp (`GetMyPendingAssignments`). | Backend tự động lấy `studentId = userContext.UserId` từ JWT hợp lệ. AI không được truyền `studentId` tùy ý. DTO chỉ chứa thông tin học tập của chính sinh viên đang đăng nhập. |
| **Mức 3 — Thay đổi dữ liệu (Mutations/Writes)** | Kế hoạch học tập, đơn từ, đăng ký môn. | **AI tuyệt đối không tự ghi CSDL.** AI chỉ *đề xuất hành động* -> Người dùng xem trước -> Bấm nút Xác nhận trên UI -> Backend kiểm tra quyền lần nữa -> Thực thi và ghi **Audit Log**. |

### 11.3. Những thông tin TUYỆT ĐỐI KHÔNG gửi cho AI

- Chuỗi kết nối (Connection strings) & Mật khẩu SQL Server.
- JWT Access Token hoặc Refresh Token.
- Mã băm mật khẩu (Password Hash).
- Toàn bộ bảng người dùng hoặc danh sách sinh viên khác.
- Dữ liệu đánh giá nội bộ, lương thưởng hoặc ghi chú riêng tư của Giảng viên/Ban giám hiệu.
- Câu truy vấn SQL tùy ý để model tự thực thi.

### 11.4. Cơ chế hoạt động của Embedding Model (`qwen3-embedding:0.6b`)

- Model embedding là một hàm thuần túy (Pure function): `Văn bản thô -> Vector 1.024 chiều`.
- Model embedding **không truy cập CSDL**.
- Backend chịu trách nhiệm đọc tài liệu, kiểm tra quyền hạn của người dùng, gửi đoạn văn bản (chunk) cho model embedding và quản lý lưu trữ/tìm kiếm vector theo đúng quyền hạn.

## 12. Khả năng chuyển máy/deploy

Không hard-code `host.docker.internal` trong code. Dùng biến `Ollama__BaseUrl`:

```text
Backend Docker + Ollama cùng máy Windows:
http://host.docker.internal:11434

Backend chạy trực tiếp trên máy có Ollama:
http://localhost:11434

Backend và máy AI khác nhau trong mạng riêng/VPN:
http://<private-ai-host>:11434
```

Máy người dùng cuối không cần cài model. Chỉ máy cung cấp dịch vụ AI cần Ollama và hai model.

## 13. Tiêu chí nghiệm thu POC

- [ ] Docker Backend gọi được `/api/tags` của Ollama.
- [ ] `GET /api/ai/health` phản hồi đúng trạng thái hai model.
- [ ] `POST /api/ai/chat` trả lời tiếng Việt thành công.
- [ ] `POST /api/ai/embedding-test` trả `dimensions = 1024`.
- [ ] Người chưa đăng nhập nhận `401`.
- [ ] Role không có quyền nhận `403` tại endpoint hạn chế.
- [ ] Backend xử lý được Ollama offline mà không crash.
- [ ] Backend xử lý timeout bằng lỗi API thống nhất.
- [ ] Không có URL Ollama trong code Frontend.
- [ ] Chỉ một chat generation chạy đồng thời trên máy hiện tại.
- [ ] Build Backend thành công.
- [ ] Build và test Frontend hiện có không bị ảnh hưởng.
- [ ] Docker Compose build và khởi động thành công.

## 14. Thứ tự thực hiện dành cho AI agent

1. Đọc đầy đủ `README.md`, `AGENTS.md`, `CLAUDE.md` và tài liệu liên quan trước khi sửa.
2. Kiểm tra working tree và không ghi đè thay đổi không liên quan.
3. Tạo branch riêng cho POC AI.
4. Kiểm tra cấu trúc Backend, middleware lỗi, JWT/current-user và quy ước DTO/service/controller.
5. Thêm options và cấu hình môi trường Ollama.
6. Thêm `IOllamaService`/`OllamaService` bằng `HttpClient`.
7. Thêm giới hạn đồng thời và hàng đợi.
8. Thêm DTO và các endpoint POC.
9. Viết unit/integration test phù hợp; không mock luồng E2E nếu repository yêu cầu dữ liệu thật.
10. Chạy build/test Backend.
11. Kiểm tra Docker Backend kết nối Ollama qua `host.docker.internal`.
12. Dừng trước phần Frontend nếu mục 9 chưa được người dùng điền.
13. Sau khi mục 9 được điền, tích hợp Frontend đúng file/route được chỉ định.
14. Chạy build, unit test, lint và smoke test liên quan.
15. Báo cáo file đã sửa, API mới, cách chạy, kết quả kiểm thử và các giới hạn còn lại.

## 15. Chỉ dẫn bắt buộc cho AI agent

- Không tự ý đổi framework, database hoặc thêm vector database/package lớn.
- Không sửa module ngoài phạm vi AI nếu không cần thiết.
- Không tạo endpoint trùng hoặc đoán contract hiện có.
- Không bỏ qua authentication/authorization để thử nhanh.
- Không commit secret, API key hoặc cấu hình riêng của máy cá nhân.
- Không xóa dữ liệu, migration hoặc Docker volume hiện có.
- Không tuyên bố RAG đã hoạt động nếu mới chỉ gọi model chat/embedding.
- Nếu có mâu thuẫn với hướng dẫn trong repository, ưu tiên hướng dẫn repository và báo lại người dùng.

## 16. Kết quả bàn giao mong muốn

- Backend kết nối Ollama ổn định trong Docker.
- Có API health, chat và embedding test được bảo vệ đúng quyền.
- Cấu hình có thể thay đổi theo môi trường mà không sửa code.
- Có hàng đợi phù hợp với giới hạn máy local.
- Frontend được tích hợp đúng vị trí do người dùng chỉ định.
- Có tài liệu chạy local và chuyển sang máy AI khác.
- Có kết quả build/test chứng minh thay đổi không phá vỡ AET LMS.
