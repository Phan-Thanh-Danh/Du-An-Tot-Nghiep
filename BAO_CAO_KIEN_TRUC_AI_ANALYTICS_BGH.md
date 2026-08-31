# BÁO CÁO KIẾN TRÚC & GIẢI PHÁP: HỆ THỐNG TRỢ LÝ AI PHÂN TÍCH HỌC THUẬT CHIẾN LƯỢC CHO BAN GIÁM HIỆU (BGH)

---

## PHẦN 1: Ý TƯỞNG & ĐỊNH HƯỚNG KIẾN TRÚC

### 1.1. Bối cảnh & Mục tiêu
Trong hệ thống Quản lý Đại học / Cao đẳng Đa cơ sở (Multi-Campus LMS), Ban Giám Hiệu (BGH) là cấp lãnh đạo cần những **bản báo cáo chiến lược tổng thể, có chiều sâu, mang tính dự báo và đề xuất giải pháp** thay vì chỉ nhìn vào những con số khô khan.

Ý tưởng cốt lõi của giải pháp:
- **Tận dụng tài nguyên phần cứng cá nhân (Local GPU)**: Chạy mô hình ngôn ngữ lớn cục bộ (**Local LLM với Ollama** như `qwen2.5:7b`, `llama3.1:8b`) để đạt hiệu năng xử lý cao, hoàn toàn miễn phí chi phí API (Zero API cost), bảo mật tuyệt đối dữ liệu nội bộ trường học.
- **Kết nối mạng ảo bảo mật cao (Tailscale Mesh Network)**: Tạo đường hầm mạng riêng (VPN Mesh) giữa Server VPS (chạy hệ thống LMS trên Docker) và Máy tính cá nhân (chạy AI Service) thông qua dải IP Tailscale bảo mật (`100.x.y.z`), không để lộ bất kỳ cổng dịch vụ nội bộ nào ra ngoài Internet.
- **Tự động hóa báo cáo học thuật & Sinh biểu đồ thông minh**: Khi BGH bấm nút *"⚡ Phân tích bằng AI"*, hệ thống tự động tổng hợp toàn bộ dữ liệu bối cảnh của cơ sở đó, gửi sang AI để tư duy phân tích đa chiều và trả về một bản báo cáo hoàn chỉnh gồm **văn bản nhận định chiến lược + cấu hình biểu đồ tương tác trực quan (Chart.js / ApexCharts)**.

---

### 1.2. Lựa chọn Mô hình Kiến trúc: CÁCH 1 (Backend .NET làm Data Gateway Trung Gian)

Sau khi cân nhắc giữa việc để AI kết nối trực tiếp vào CSDL và mô hình Gateway, giải pháp được lựa chọn là **Mô hình Gateway trung gian** vì các ưu điểm vượt trội:
1. **Bảo mật CSDL cấp cao nhất**: Cổng SQL Server (1433) được đóng kín hoàn toàn trong Docker nội bộ trên VPS, không mở ra ngoài.
2. **Đảm bảo tính Đa cơ sở (Multi-Tenant Scoping 100%)**: Backend .NET đã có JWT Token của BGH, tự động trích xuất `currentUser.CampusId` và query chính xác dữ liệu của cơ sở đó, loại bỏ hoàn toàn nguy cơ AI query nhầm cơ sở khác.
3. **Phân tách trách nhiệm sạch sẽ (Separation of Concerns)**:
   - **VPS Backend**: Chuyên lo việc xác thực, truy vấn dữ liệu SQL sạch và bảo vệ nghiệp vụ.
   - **Local Machine (Python + Ollama)**: Chuyên lo việc tư duy lập luận (AI Reasoning), phân tích bối cảnh và tạo cấu hình biểu đồ trực quan.

---

## PHẦN 2: LUỒNG HOẠT ĐỘNG QUA LẠI CHI TIẾT (END-TO-END WORKFLOW)

```
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                    LUỒNG HOẠT ĐỘNG HỆ THỐNG                                      │
└──────────────────────────────────────────────────────────────────────────────────────────────────┘

   [1. NGƯỜI DÙNG]
   Ban Giám Hiệu đăng nhập vào Web (VD: Cơ sở Hà Nội, CampusId = 3)
   Mở trang Báo cáo (GPA / Cảnh báo sớm / Pass-Fail / Đánh giá Giảng viên)
   Bấm nút: ⚡ "TẠO BÁO CÁO PHÂN TÍCH AI"
         │
         ▼
   [2. FRONTEND VUE 3 (Web)]
   Gửi HTTP Request: POST /api/bgh/academic/ai-analytics kèm JWT Token và filter (Học kỳ, Khoa...)
         │
         ▼
   [3. BACKEND ASP.NET CORE (VPS)]
   ├─ 3.1. Giải mã JWT Token -> Xác định chính xác CampusId = 3, Role = Principal (BGH).
   ├─ 3.2. Truy vấn SQL Server nội bộ Docker:
   │       Query đồng thời các bảng: Điểm số, Chuyên cần, LMS Logs, Tải Giảng viên, Cơ sở vật chất...
   ├─ 3.3. Đóng gói dữ liệu thành gói JSON Đa chiều (Multi-Dimensional Context Dataset ~ 15KB).
   └─ 3.4. Gửi HTTP Request sang Máy tính Local qua IP Tailscale:
           POST http://100.x.y.z:8000/api/ai-report
         │
         ▼ (Qua đường hầm mạng Tailscale an toàn)
   [4. AI SERVICE TRÊN MÁY LOCAL (FastAPI + Ollama)]
   ├─ 4.1. FastAPI nhận gói dữ liệu JSON từ Backend VPS.
   ├─ 4.2. Xây dựng System Prompt chuyên gia Cố vấn Giáo dục + Nạp dữ liệu bối cảnh vào Context.
   ├─ 4.3. Gọi Ollama Inference (Model: qwen2.5:7b / llama3.1:8b) ở nhiệt độ thấp (temperature = 0.3)
   │       để phân tích lập luận logic, tìm nguyên nhân gốc rễ và tạo schema biểu đồ JSON.
   └─ 4.4. Trả về kết quả JSON chuẩn hóa cho Backend VPS.
         │
         ▼
   [5. BACKEND VPS -> TRẢ VỀ FRONTEND VUE 3]
   ├─ 5.1. Nhận JSON phân tích từ AI.
   └─ 5.2. Trả HTTP Response về cho trình duyệt của BGH.
         │
         ▼
   [6. TRÌNH DIỄN TRÊN GIAO DIỆN BGH (Web Presentation)]
   ├─ Render bài phân tích Markdown: Đánh giá tổng quan, Điểm mạnh, Điểm yếu, Rủi ro, Khuyến nghị.
   └─ Tự động vẽ Biểu đồ tương tác (Chart.js): Biểu đồ cột phân bố GPA, Biểu đồ tròn Pass/Fail, Biểu đồ thanh ngang so sánh các khoa...
```

---

## PHẦN 3: BẢN ĐỒ DỮ LIỆU CSDL & BỐI CẢNH CHO 4 BÁO CÁO CỐT LÕI

Để AI không đưa ra nhận xét bề nổi mà có thể **tư duy khách quan, đa chiều và công tâm**, dưới đây là chi tiết các Bảng CSDL, Cột dữ liệu và Góc nhìn phân tích của từng báo cáo:

---

### 🎯 1. BÁO CÁO GPA HỆ THỐNG (`/bgh/academic/gpa`)
*Mục đích BGH: Đánh giá mặt bằng chất lượng học thuật toàn cơ sở, đo lường độ đồng đều và xu hướng tăng/giảm qua các kỳ.*

#### A. Các Bảng CSDL & Cột dữ liệu trích xuất:
- `BangDiem`: `diem_trung_binh_hoc_ky`, `diem_tich_luy`, `so_tin_chi_tich_luy`, `xep_loai` (Xuất sắc, Giỏi, Khá, TB, Yếu).
- `SinhVien`: `ma_sinh_vien`, `ma_lop_hanh_chinh`, `ma_chuyen_nganh`, `ma_khoa_hoc`, `nam_nhap_hoc`.
- `LopHanhChinh`, `ChuyenNganh`, `DonVi`: `ten_lop`, `ten_chuyen_nganh`, `ten_don_vi`.
- `LichSuHocTap` (hoặc `BangDiem` các kỳ trước): `diem_trung_binh_ky_truoc`, `hoc_ky_so`.
- `DanhMucMonHoc` + `BangDiemChiTiet`: `so_tin_chi`, `loai_mon` (Đại cương vs Cơ sở ngành vs Chuyên ngành).

#### B. Dữ liệu bối cảnh đa chiều nạp cho AI:
1. **Phân bố phổ điểm chi tiết**: Số lượng và tỷ lệ % ở từng dải điểm (9.0-10, 8.0-8.9, 7.0-7.9, 5.0-6.9, <5.0).
2. **Độ lệch chuẩn (Standard Deviation)**: Đo lường mức độ đồng đều học lực của từng khoa/ngành (lệch chuẩn thấp = học lực đồng đều; lệch chuẩn cao = phân hóa lớn giữa nhóm giỏi và nhóm yếu).
3. **Quỹ đạo phát triển (Trend Trajectory)**: Tỷ lệ sinh viên có GPA tăng trưởng vs thụt lùi so với kỳ trước.
4. **Phân rã theo khối kiến thức**: Khoa CNTT điểm Đại cương thấp nhưng điểm Chuyên ngành rất cao $\rightarrow$ Sinh viên có tố chất thực hành nhưng yếu toán nền.

#### C. Biểu đồ AI tự động cấu hình:
- *Biểu đồ cột nhóm (Grouped Bar)*: So sánh GPA trung bình giữa các Khoa/Ngành trong trường.
- *Biểu đồ tròn (Donut Chart)*: Tỷ lệ phần trăm phân bố xếp loại học lực toàn cơ sở.
- *Biểu đồ đường (Line Chart)*: Quỹ đạo biến động GPA trung bình qua các năm của từng khóa (K21, K22, K23).

---

### ⚠️ 2. BÁO CÁO CẢNH BÁO SỚM SINH VIÊN NGUY CƠ RỚT MÔN (`/bgh/academic/at-risk`)
*Mục đích BGH: Tìm đúng Căn nguyên gốc rễ (Root-Cause) đẩy sinh viên vào rủi ro để chỉ đạo phân luồng can thiệp cứu trợ kịp thời trước kỳ thi kết thúc môn.*

#### A. Các Bảng CSDL & Cột dữ liệu trích xuất:
- `DiemDanh`: `trang_thai` (CoMat, Vang, Tre, CoPhep), `ngay_hoc`, `ti_le_vang_phan_tram`.
- `BangDiemChiTiet` + `DiemThanhPhan`: `diem_chuyen_can`, `diem_giua_ky`, `diem_thuc_hanh_lab`, `diem_quiz`.
- `BaiTap` + `NopBaiTap`: `ngay_nop`, `han_nop`, `trang_thai_nop` (NopDungHan, NopTre, ChuaNop), `diem_so`.
- `TienDoBaiHoc` + `KetQuaQuiz`: `ti_le_hoan_thanh_bai_hoc_lms`, `so_lan_lam_quiz_tu_luyen`.
- `CongNoHocPhi`: `so_tien_con_no`, `trang_thai_thanh_toan`, `so_ngay_qua_han`.
- `DonTu`: `loai_don` (XinNghiPhep, BaoLuu, HoanCanhKhoKhan), `ly_do`.

#### B. Dữ liệu bối cảnh đa chiều nạp cho AI:
1. **Phân loại 3 nhóm căn nguyên rủi ro**:
   - *Nhóm Rủi ro Học lực*: Đi học 100%, nộp bài đầy đủ nhưng điểm kiểm tra/Quiz thấp $\rightarrow$ Sinh viên chăm chỉ nhưng không hiểu bài, cần phụ đạo kiến thức.
   - *Nhóm Rủi ro Thái độ/Hành vi*: Điểm đầu vào cao nhưng nghỉ học liên tục, nộp bài trễ hạn, không tương tác LMS $\rightarrow$ Cần cố vấn học tập và phụ huynh chấn chỉnh thái độ.
   - *Nhóm Rủi ro Ngoại cảnh / Tài chính*: Có đơn xin nghỉ, nợ học phí quá hạn, giờ đăng nhập LMS chỉ vào nửa đêm $\rightarrow$ Sinh viên bị áp lực kinh tế/đi làm thêm, cần hỗ trợ học bổng hoặc giãn tiến độ đóng học phí.
2. **Mức độ rủi ro (Risk Severity Matrix)**: Phân tầng Báo động đỏ (Critical), Cảnh báo vàng (Moderate), Cần theo dõi (Watchlist).

#### C. Biểu đồ AI tự động cấu hình:
- *Biểu đồ Donut*: Tỷ lệ phân bổ nguyên nhân đẩy sinh viên vào rủi ro (Học lực 40%, Nghỉ học 35%, Tài chính/Ngoại cảnh 25%).
- *Biểu đồ thanh ngang (Horizontal Bar)*: Top các lớp hành chính có số lượng sinh viên báo động đỏ nhiều nhất.

---

### 📈 3. BÁO CÁO TỶ LỆ PASS / FAIL MÔN HỌC (`/bgh/academic/pass-fail`)
*Mục đích BGH: Bóc tách nguyên nhân rớt môn do độ khó của môn, do đề thi cuối kỳ lệch chuẩn, do cấu trúc điểm bất hợp lý, do giảng viên dạy, hay do lịch thi bị dồn dập.*

#### A. Các Bảng CSDL & Cột dữ liệu trích xuất:
- `DanhMucMonHoc`: `ma_mon_hoc`, `ten_mon_hoc`, `so_tin_chi`, `so_tiet_ly_thuyet`, `so_tiet_thuc_hanh`, `loai_mon`.
- `CauHinhDiemMonHoc`: `ti_le_chuyen_can`, `ti_le_giua_ky`, `ti_le_cuoi_ky`, `diem_liet`.
- `BangDiemChiTiet`: `diem_qua_trinh`, `diem_thi_cuoi_ky`, `diem_tong_ket`, `ket_qua` (Pass/Fail).
- `LopHocPhan`: `ma_lop_hoc_phan`, `ma_giang_vien`, `ma_phong_hoc`, `so_luong_sinh_vien`.
- `ThoiKhoaBieuChiTiet` + `CaThi`: `ngay_thi`, `ca_thi`, `khoang_cach_ngay_thi_giua_cac_mon` (Tải nhận thức thi).
- `PhongHoc` + `ThietBiPhong`: `loai_phong` (Lab máy tính vs Giảng đường lý thuyết), `tinh_trang_thiet_bi`.

#### B. Dữ liệu bối cảnh đa chiều nạp cho AI:
1. **Mổ xẻ Độ lệch Điểm quá trình vs Điểm thi**: Điểm quá trình trung bình 8.0 nhưng điểm thi cuối kỳ chỉ 3.8 (Tỷ lệ rớt 32%) $\rightarrow$ Đề thi cuối kỳ quá dài/quá khó, không bám sát nội dung giảng dạy.
2. **So sánh chéo giữa các Giảng viên cùng dạy một môn**: Cùng một môn học, lớp của Thầy A rớt 4% nhưng lớp của Thầy B rớt 28% $\rightarrow$ Sự chênh lệch lớn về phương pháp truyền đạt hoặc tiêu chí chấm bài.
3. **Phân tích Tải nhận thức lịch thi (Cognitive Overload)**: Môn Toán A2 thi ngay sau môn Lập trình chỉ cách 3 tiếng $\rightarrow$ Sinh viên bị kiệt sức, giảm hiệu suất làm bài.
4. **Môi trường cơ sở vật chất**: Môn thực hành đồ họa nhưng học tại phòng máy Lab có cấu hình máy yếu, thường xuyên gặp sự cố.

#### C. Biểu đồ AI tự động cấu hình:
- *Biểu đồ cột (Bar Chart)*: Top 5 môn học có tỷ lệ rớt môn cao nhất toàn trường.
- *Biểu đồ phân tán (Scatter / Radar Chart)*: Tương quan giữa Điểm quá trình và Điểm thi cuối kỳ của các môn có tỷ lệ rớt cao.

---

### 🏆 4. BÁO CÁO XẾP HẠNG & ĐÁNH GIÁ GIẢNG VIÊN + AI FEEDBACK (`/bgh/evaluations/*`)
*Mục đích BGH: Đánh giá chất lượng giảng dạy thực chất, loại trừ yếu tố thiên vị hoặc đánh giá cảm tính từ sinh viên, phát hiện tình trạng giảng viên bị quá tải giờ dạy.*

#### A. Các Bảng CSDL & Cột dữ liệu trích xuất:
- `DanhGiaGiangVien` + `PhanHoiDanhGia`: `diem_tieu_chi` (1-5 sao theo từng câu hỏi), `nhan_xet_van_ban`, `ngay_danh_gia`.
- `GiangVien` + `NguoiDung`: `ma_giang_vien`, `hoc_vi` (ThS, TS), `loai_giang_vien` (Cơ hữu/Thỉnh giảng), `so_nam_kinh_nghiem`.
- `PhanCongGiangDay` + `LopHocPhan`: `so_lop_phu_trach`, `tong_so_tiet_day_trong_tuan` (Tải giảng dạy).
- `BangDiemChiTiet` (của các lớp thầy cô dạy): `diem_trung_binh_lop_day`, `ti_le_rot_mon_lop_day` (Độ khắt khe chấm điểm).
- `ThietBiPhong`: `ghi_chu_su_co_thiet_bi` (Mic hỏng, máy chiếu mờ, điều hòa hỏng tại phòng dạy).

#### B. Dữ liệu bối cảnh đa chiều nạp cho AI:
1. **Phân tích Cảm xúc & Chủ đề (NLP Sentiment & Topic Modeling)**:
   - AI đọc toàn bộ lời nhận xét tự do của sinh viên và phân loại:
     - 🌟 *Khen ngợi*: Giảng dạy nhiệt tình, slide trực quan, hướng dẫn bài tập kỹ.
     - ⚠️ *Góp ý*: Nói hơi nhanh, mic giảng đường B201 bị rè, trả điểm bài tập lớn hơi muộn.
2. **Loại trừ "Oan sai" cho Giảng viên nghiêm khắc**:
   - Thầy dạy rất chuẩn mực nhưng chấm điểm nghiêm túc (không cho điểm ảo) bị một nhóm sinh viên lười học vote 1 sao $\rightarrow$ AI đối chiếu dữ liệu chuyên cần và chỉ ra: *Đánh giá tiêu cực này mang tính chất thiên kiến cảm tính từ nhóm sinh viên bị điểm kém.*
3. **Phát hiện Tình trạng Quá tải Giảng dạy (Workload Burnout)**:
   - Giảng viên học kỳ trước đạt 4.8/5.0 nhưng kỳ này tụt xuống 3.9/5.0. AI phát hiện học kỳ này thầy phải dạy tới **38 tiết/tuần (vượt 60% định mức)** $\rightarrow$ Đề xuất BGH giảm tải giờ dạy để giảng viên hồi phục chất lượng.

#### C. Biểu đồ AI tự động cấu hình:
- *Biểu đồ tròn (Sentiment Breakdown)*: Tỷ lệ phản hồi Tích cực (68%) vs Trung lập (22%) vs Tiêu cực (10%).
- *Biểu đồ thanh ngang (Topic Frequency)*: Top các chủ đề sinh viên khen ngợi và phàn nàn nhiều nhất.
- *Biểu đồ phân tán (Workload vs Rating)*: Tương quan giữa số giờ dạy/tuần và điểm số đánh giá chất lượng của giảng viên.

---

## PHẦN 4: ĐỊNH DẠNG JSON CHUẨN AI TRẢ VỀ CHO FRONTEND

Khi Ollama hoàn thành phân tích, nó sẽ trả về một gói JSON chuẩn cấu trúc như sau:

```json
{
  "reportTitle": "Báo Cáo Phân Tích Học Thuật Toàn Diện - Học Kỳ 1 (2026-2027)",
  "campusName": "Cơ sở Hà Nội",
  "generatedAt": "2026-08-31T22:45:00Z",
  "executiveSummary": "Chất lượng học thuật toàn cơ sở duy trì ở mức Khá (GPA 7.38). Khoa CNTT dẫn đầu về chất lượng chuyên môn nhưng có dấu hiệu phân hóa học lực. Môn Cấu trúc dữ liệu có tỷ lệ rớt cao bất thường do độ khó đề thi.",
  "sections": [
    {
      "heading": "1. Đánh Giá Mặt Bằng Học Lực & Xu Hướng",
      "content": "Toàn trường có 68% sinh viên đạt loại Khá - Giỏi. Độ lệch chuẩn GPA toàn khoa CNTT là 1.15 cho thấy sự phân hóa rõ rệt...",
      "keyTakeaways": [
        "GPA trung bình tăng 0.15 điểm so với học kỳ trước.",
        "Nhóm sinh viên xuất sắc tập trung chủ yếu ở các lớp chất lượng cao."
      ]
    },
    {
      "heading": "2. Phân Tích Căn Nguyên & Rủi Ro Tiềm Ẩn",
      "content": "Phát hiện 42 sinh viên thuộc nhóm báo động đỏ. 60% trong số này có nguyên nhân gốc rễ bắt nguồn từ việc nợ học phí dẫn đến đi làm thêm và vắng học quá số tiết quy định...",
      "keyTakeaways": [
        "Môn IT201 có 23.4% sinh viên không đạt chuẩn.",
        "28 sinh viên đã chạm ngưỡng bị cấm thi do vắng > 20% số tiết."
      ]
    },
    {
      "heading": "3. Đề Xuất Giải Pháp Chiến Lược Cho Ban Giám Hiệu",
      "content": "Khuyến nghị BGH chỉ đạo các phòng ban triển khai 3 hành động trọng tâm...",
      "keyTakeaways": [
        "Mở lớp phụ đạo tăng cường vào cuối tuần cho môn IT201.",
        "Giãn cách lịch thi các môn cơ sở ngành tối thiểu 2 ngày giữa các ca.",
        "Phòng Công tác sinh viên rà soát hỗ trợ học bổng cho nhóm sinh viên khó khăn."
      ]
    }
  ],
  "charts": [
    {
      "id": "gpa-by-department",
      "type": "bar",
      "title": "So Sánh GPA Trung Bình Giữa Các Khoa",
      "labels": ["Công Nghệ Thông Tin", "Quản Trị Kinh Doanh", "Ngôn Ngữ Anh", "Thiết Kế Đồ Họa"],
      "datasets": [
        {
          "label": "GPA Trung Bình",
          "data": [7.65, 7.20, 7.15, 7.42],
          "backgroundColor": ["#3b82f6", "#10b981", "#f59e0b", "#8b5cf6"]
        }
      ]
    },
    {
      "id": "risk-root-causes",
      "type": "doughnut",
      "title": "Cơ Cấu Căn Nguyên Đẩy Sinh Viên Vào Rủi Ro",
      "labels": ["Kiến thức nền yếu", "Bỏ học / Điểm danh kém", "Áp lực tài chính / Đi làm thêm", "Khác"],
      "datasets": [
        {
          "data": [40, 35, 20, 5],
          "backgroundColor": ["#ef4444", "#f97316", "#eab308", "#64748b"]
        }
      ]
    }
  ]
}
```

---

## PHẦN 5: KẾT LUẬN & ĐÁNH GIÁ GIÁ TRỊ ĐỒ ÁN

1. **Tính Thực Tiễn & Giá Trị Đồ Án**: Mô hình kết hợp giữa **Hệ thống ERP Học vụ thực tế (ASP.NET Core + Vue 3)** và **Trí tuệ Nhân tạo Local LLM (Ollama) qua mạng Tailscale** là một điểm nhấn công nghệ cực kỳ ấn tượng, giải quyết trọn vẹn bài toán bảo mật, chi phí và chất lượng ra quyết định cho Ban Giám Hiệu.
2. **Tính Khách Quan Tuyệt Đối**: Bằng việc tích hợp 6 lớp dữ liệu bối cảnh (LMS, chuyên cần, độ khó môn, tải giảng viên, cơ sở vật chất, tài chính), hệ thống AI không chỉ dừng lại ở việc đọc số liệu mà thực sự đóng vai trò là một **Ban Cố Vấn Chiến Lược Trí Tuệ Nhân Tạo**.
