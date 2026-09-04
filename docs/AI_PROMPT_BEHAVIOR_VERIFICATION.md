# AI Prompt Behavior Verification

Ngày kiểm tra: 2026-09-04

## Mục tiêu

Tăng độ đúng ý cho các ô prompt AI đang dùng khi demo, đặc biệt:

- BGH chỉnh mẫu giấy khen: prompt như `nền vàng viền đen` phải đổi đúng nền và viền, không trả về preset cố định.
- Giáo vụ xếp lịch: prompt như `lịch có ca tối nào không` phải trả lời có/không/chưa có lịch theo dữ liệu thật, không biến thành template xếp lịch.
- Các ô prompt khác vẫn dùng AI tự nhiên hơn, có lịch sử hội thoại ngắn và không tự kích hoạt action khi người dùng chỉ hỏi.

## Cách xử lý

Backend dùng Ollama `/api/chat` với `format` là JSON schema để buộc output có cấu trúc. Cấu hình này dựa trên tài liệu chính thức của Ollama về structured outputs và Chat API:

- https://docs.ollama.com/capabilities/structured-outputs
- https://docs.ollama.com/api/chat

Với các prompt dễ sai trong lúc bảo vệ, backend thêm lớp kiểm tra deterministic:

- Giấy khen: AI vẫn đề xuất CSS/HTML, sau đó backend chốt lại các yêu cầu rõ trong prompt như nền, màu viền, viền nét đứt/nét đôi bằng CSS override. Mọi token `{{...}}` của mẫu phải còn nguyên. Nội dung nguy hiểm như script, event handler, URL ngoài, `@import` bị từ chối.
- Xếp lịch: backend tự tính facts theo campus/học kỳ/draft thật. Câu hỏi ca tối trả lời trực tiếp từ `ThoiKhoaBieu` hoặc bản nháp được chọn. Yêu cầu `bỏ ca tối` chỉ tạo đề xuất `excludeEvening=true`, không tự generate cho tới khi người dùng xác nhận.
- Chat AI dùng chung: lịch sử hội thoại được gửi ngắn gọn sang AI; câu hỏi tư vấn về ngân hàng câu hỏi không còn tự tạo quiz nếu prompt không nói rõ `tạo`.

## Prompt nên demo

1. BGH giấy khen:

   `Nền vàng, viền đen. Giữ nguyên nội dung và các biến dữ liệu.`

2. Follow-up giấy khen:

   `Giữ nền vàng, chỉ đổi viền sang màu xanh lá và nét đứt.`

3. Giáo vụ xếp lịch:

   `Lịch có ca tối nào không?`

4. Follow-up xếp lịch:

   `Vậy tạo bản nháp mới bỏ toàn bộ ca tối đi.`

## Kết quả kiểm chứng

- Backend prompt tests: 14/14 pass.
- Live Ollama 3B test: pass với model local `qwen2.5:3b`.
- Frontend modal tests: 6/6 pass.

Artifact live test:

- `docs/artifacts/ai-prompt-verification/certificate-yellow-black.html`
- `docs/artifacts/ai-prompt-verification/certificate-followup.html`

## Giới hạn nên nói rõ khi bảo vệ

AI không tự thay thuật toán xếp lịch. AI hiểu yêu cầu tiếng Việt, hỏi/đề xuất cấu hình an toàn, rồi backend và solver hiện có mới xử lý dữ liệu thật. Các phân tích BGH trả từ service analytics hiện có vẫn là phần backend tính facts; AI chỉ diễn giải hoặc hỗ trợ giao tiếp ở những endpoint prompt.
