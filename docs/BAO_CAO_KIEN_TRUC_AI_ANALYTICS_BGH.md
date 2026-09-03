# BÁO CÁO KIẾN TRÚC & GIẢI PHÁP: HỆ THỐNG TRỢ LÝ AI PHÂN TÍCH HỌC THUẬT CHIẾN LƯỢC CHO BAN GIÁM HIỆU (BGH)

---

## PHẦN 1: Ý TƯỞNG & ĐỊNH HƯỚNG KIẾN TRÚC

### 1.1. Bối cảnh & Mục tiêu
Trong hệ thống Quản lý Đại học / Cao đẳng Đa cơ sở (Multi-Campus LMS), Ban Giám Hiệu (BGH) là cấp lãnh đạo cần những **bản báo cáo chiến lược tổng thể, có chiều sâu, mang tính dự báo và đề xuất giải pháp** thay vì chỉ nhìn vào những con số khô khan.

Ý tưởng cốt lõi của giải pháp:
- **Tận dụng tài nguyên phần cứng cá nhân (Local GPU)**: Chạy các mô hình đã cài trên Ollama gồm `qwen2.5:3b` cho phân tích nhanh, `qwen3.5:9b-q4_K_M` cho phân tích sâu và `qwen3-embedding:0.6b` cho RAG tài liệu. Hệ thống không phát sinh chi phí API theo lượt gọi và dữ liệu phân tích được giữ trong hạ tầng do dự án kiểm soát.
- **Kết nối mạng ảo bảo mật cao (Tailscale Mesh Network)**: Tạo đường hầm mạng riêng (VPN Mesh) giữa Server VPS (chạy hệ thống LMS trên Docker) và Máy tính cá nhân (chạy AI Service) thông qua dải IP Tailscale bảo mật (`100.x.y.z`), không để lộ bất kỳ cổng dịch vụ nội bộ nào ra ngoài Internet.
- **Tự động hóa báo cáo học thuật & phân tích thông minh**: Khi BGH bấm nút *"⚡ Phân tích bằng AI"*, Backend tổng hợp dữ liệu thật của đúng cơ sở, tính các chỉ số và cấu hình biểu đồ từ số liệu xác định; AI chỉ nhận gói dữ liệu đã rút gọn để viết nhận định, nêu bằng chứng và đề xuất hành động. Cách phân chia này giúp số liệu chính xác và giảm thời gian xử lý của model.

---

### 1.2. Lựa chọn Mô hình Kiến trúc: CÁCH 1 (Backend .NET làm Data Gateway Trung Gian)

Sau khi cân nhắc giữa việc để AI kết nối trực tiếp vào CSDL và mô hình Gateway, giải pháp được lựa chọn là **Mô hình Gateway trung gian** vì các ưu điểm vượt trội:
1. **Bảo mật CSDL cấp cao nhất**: Cổng SQL Server (1433) được đóng kín hoàn toàn trong Docker nội bộ trên VPS, không mở ra ngoài.
2. **Đảm bảo tính Đa cơ sở (Multi-Tenant Scoping 100%)**: Backend .NET đã có JWT Token của BGH, tự động trích xuất `currentUser.CampusId` và query chính xác dữ liệu của cơ sở đó, loại bỏ hoàn toàn nguy cơ AI query nhầm cơ sở khác.
3. **Phân tách trách nhiệm sạch sẽ (Separation of Concerns)**:
   - **VPS Backend**: Chuyên lo việc xác thực, truy vấn dữ liệu SQL sạch và bảo vệ nghiệp vụ.
   - **Local Machine (Python + Ollama)**: Chuyên nhận JSON tổng hợp, viết nhận định và khuyến nghị dựa trên dữ liệu Backend cung cấp; không truy cập CSDL và không tự tính lại dữ liệu biểu đồ.

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
   ├─ 3.2. Gọi các Service/Repository hiện có để truy vấn SQL Server nội bộ Docker.
   ├─ 3.3. Tính toán tại Backend: tổng số, trung bình, tỷ lệ, phân bố, xu hướng và dữ liệu biểu đồ.
   ├─ 3.4. Đóng gói thành AnalyticsContextDto nhỏ gọn (mục tiêu 5–15KB, không gửi hàng nghìn bản ghi thô).
   └─ 3.5. Gửi HTTP Request sang Máy tính Local qua IP Tailscale:
           POST http://100.x.y.z:8000/api/ai-report
         │
         ▼ (Qua đường hầm mạng Tailscale an toàn)
   [4. AI SERVICE TRÊN MÁY LOCAL (FastAPI + Ollama)]
   ├─ 4.1. FastAPI nhận gói dữ liệu JSON từ Backend VPS.
   ├─ 4.2. Xây dựng System Prompt chuyên gia Cố vấn Giáo dục và nạp AnalyticsContextDto vào Context.
   ├─ 4.3. Chế độ nhanh gọi qwen2.5:3b; chế độ phân tích sâu do người dùng chọn gọi qwen3.5:9b-q4_K_M.
   ├─ 4.4. AI chỉ tạo executiveSummary, findings có evidence và recommendations; không tạo hoặc sửa số liệu.
   └─ 4.5. Validate JSON rồi trả aiAnalysis chuẩn hóa cho Backend VPS.
         │
         ▼
   [5. BACKEND VPS -> TRẢ VỀ FRONTEND VUE 3]
   ├─ 5.1. Nhận aiAnalysis từ AI và ghép với metrics/charts do Backend tạo.
   └─ 5.2. Trả HTTP Response hoàn chỉnh về cho trình duyệt của BGH.
         │
         ▼
   [6. TRÌNH DIỄN TRÊN GIAO DIỆN BGH (Web Presentation)]
   ├─ Render bài phân tích Markdown: Đánh giá tổng quan, Điểm mạnh, Điểm yếu, Rủi ro, Khuyến nghị.
   └─ Vẽ biểu đồ tương tác (Chart.js) từ labels/data do Backend tính: phân bố GPA, Pass/Fail, so sánh khoa...
```

---

## PHẦN 3: BẢN ĐỒ DỮ LIỆU CSDL & BỐI CẢNH CHO 4 BÁO CÁO CỐT LÕI

Để AI không đưa ra nhận xét bề nổi mà có thể **tư duy khách quan, đa chiều và công tâm**, dưới đây là chi tiết các Bảng CSDL, Cột dữ liệu và Góc nhìn phân tích của từng báo cáo:

> **Nguyên tắc triển khai:** Danh sách bảng/cột dưới đây là bản đồ dữ liệu mục tiêu. Khi code, Backend chỉ dùng Entity, Service và Repository có thật trong AET LMS; Backend tự query và tính toán các chỉ số trước khi gửi cho AI. Nếu dự án chưa có dữ liệu tương ứng thì bỏ chỉ số đó khỏi phiên bản hiện tại, không tự tạo dữ liệu giả. AI không được truy cập SQL Server hoặc tự chạy câu SQL.

---

### 🎯 1. BÁO CÁO GPA HỆ THỐNG (`/bgh/academic/gpa`)
*Mục đích BGH: Đánh giá mặt bằng chất lượng học thuật toàn cơ sở, đo lường độ đồng đều và xu hướng tăng/giảm qua các kỳ.*

#### A. Các Bảng CSDL & Cột dữ liệu trích xuất:
- `BangDiem`: `diem_trung_binh_hoc_ky`, `diem_tich_luy`, `so_tin_chi_tich_luy`, `xep_loai` (Xuất sắc, Giỏi, Khá, TB, Yếu).
- `SinhVien`: `ma_sinh_vien`, `ma_lop_hanh_chinh`, `ma_chuyen_nganh`, `ma_khoa_hoc`, `nam_nhap_hoc`.
- `LopHanhChinh`, `ChuyenNganh`, `DonVi`: `ten_lop`, `ten_chuyen_nganh`, `ten_don_vi`.
- `LichSuHocTap` (hoặc `BangDiem` các kỳ trước): `diem_trung_binh_ky_truoc`, `hoc_ky_so`.
- `DanhMucMonHoc` + `BangDiemChiTiet`: `so_tin_chi`, `loai_mon` (Đại cương vs Cơ sở ngành vs Chuyên ngành).

#### B. Chỉ số Backend tổng hợp và gửi cho AI:
1. **Phân bố phổ điểm chi tiết**: Số lượng và tỷ lệ % ở từng dải điểm (9.0-10, 8.0-8.9, 7.0-7.9, 5.0-6.9, <5.0).
2. **Độ lệch chuẩn (Standard Deviation)**: Đo lường mức độ đồng đều học lực của từng khoa/ngành (lệch chuẩn thấp = học lực đồng đều; lệch chuẩn cao = phân hóa lớn giữa nhóm giỏi và nhóm yếu).
3. **Quỹ đạo phát triển (Trend Trajectory)**: Tỷ lệ sinh viên có GPA tăng trưởng vs thụt lùi so với kỳ trước.
4. **Phân rã theo khối kiến thức**: Khoa CNTT điểm Đại cương thấp nhưng điểm Chuyên ngành rất cao $\rightarrow$ Sinh viên có tố chất thực hành nhưng yếu toán nền.

#### C. Biểu đồ Backend cấu hình từ dữ liệu đã tính:
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

#### B. Chỉ số Backend tổng hợp và gửi cho AI:
1. **Phân loại 3 nhóm căn nguyên rủi ro**:
   - *Nhóm Rủi ro Học lực*: Đi học 100%, nộp bài đầy đủ nhưng điểm kiểm tra/Quiz thấp $\rightarrow$ Sinh viên chăm chỉ nhưng không hiểu bài, cần phụ đạo kiến thức.
   - *Nhóm Rủi ro Thái độ/Hành vi*: Điểm đầu vào cao nhưng nghỉ học liên tục, nộp bài trễ hạn, không tương tác LMS $\rightarrow$ Cần cố vấn học tập và phụ huynh chấn chỉnh thái độ.
   - *Nhóm Rủi ro Ngoại cảnh / Tài chính*: Có đơn xin nghỉ, nợ học phí quá hạn, giờ đăng nhập LMS chỉ vào nửa đêm $\rightarrow$ Sinh viên bị áp lực kinh tế/đi làm thêm, cần hỗ trợ học bổng hoặc giãn tiến độ đóng học phí.
2. **Mức độ rủi ro (Risk Severity Matrix)**: Phân tầng Báo động đỏ (Critical), Cảnh báo vàng (Moderate), Cần theo dõi (Watchlist).

#### C. Biểu đồ Backend cấu hình từ dữ liệu đã tính:
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

#### B. Chỉ số Backend tổng hợp và gửi cho AI:
1. **Mổ xẻ Độ lệch Điểm quá trình vs Điểm thi**: Điểm quá trình trung bình 8.0 nhưng điểm thi cuối kỳ chỉ 3.8 (Tỷ lệ rớt 32%) $\rightarrow$ Đề thi cuối kỳ quá dài/quá khó, không bám sát nội dung giảng dạy.
2. **So sánh chéo giữa các Giảng viên cùng dạy một môn**: Cùng một môn học, lớp của Thầy A rớt 4% nhưng lớp của Thầy B rớt 28% $\rightarrow$ Sự chênh lệch lớn về phương pháp truyền đạt hoặc tiêu chí chấm bài.
3. **Phân tích Tải nhận thức lịch thi (Cognitive Overload)**: Môn Toán A2 thi ngay sau môn Lập trình chỉ cách 3 tiếng $\rightarrow$ Sinh viên bị kiệt sức, giảm hiệu suất làm bài.
4. **Môi trường cơ sở vật chất**: Môn thực hành đồ họa nhưng học tại phòng máy Lab có cấu hình máy yếu, thường xuyên gặp sự cố.

#### C. Biểu đồ Backend cấu hình từ dữ liệu đã tính:
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

#### B. Chỉ số Backend tổng hợp và gửi cho AI:
1. **Phân tích Cảm xúc & Chủ đề (NLP Sentiment & Topic Modeling)**:
   - AI đọc toàn bộ lời nhận xét tự do của sinh viên và phân loại:
     - 🌟 *Khen ngợi*: Giảng dạy nhiệt tình, slide trực quan, hướng dẫn bài tập kỹ.
     - ⚠️ *Góp ý*: Nói hơi nhanh, mic giảng đường B201 bị rè, trả điểm bài tập lớn hơi muộn.
2. **Loại trừ "Oan sai" cho Giảng viên nghiêm khắc**:
   - Thầy dạy rất chuẩn mực nhưng chấm điểm nghiêm túc (không cho điểm ảo) bị một nhóm sinh viên lười học vote 1 sao $\rightarrow$ AI đối chiếu dữ liệu chuyên cần và chỉ ra: *Đánh giá tiêu cực này mang tính chất thiên kiến cảm tính từ nhóm sinh viên bị điểm kém.*
3. **Phát hiện Tình trạng Quá tải Giảng dạy (Workload Burnout)**:
   - Giảng viên học kỳ trước đạt 4.8/5.0 nhưng kỳ này tụt xuống 3.9/5.0. AI phát hiện học kỳ này thầy phải dạy tới **38 tiết/tuần (vượt 60% định mức)** $\rightarrow$ Đề xuất BGH giảm tải giờ dạy để giảng viên hồi phục chất lượng.

#### C. Biểu đồ Backend cấu hình từ dữ liệu đã tính:
- *Biểu đồ tròn (Sentiment Breakdown)*: Tỷ lệ phản hồi Tích cực (68%) vs Trung lập (22%) vs Tiêu cực (10%).
- *Biểu đồ thanh ngang (Topic Frequency)*: Top các chủ đề sinh viên khen ngợi và phàn nàn nhiều nhất.
- *Biểu đồ phân tán (Workload vs Rating)*: Tương quan giữa số giờ dạy/tuần và điểm số đánh giá chất lượng của giảng viên.

---

## PHẦN 4: ĐỊNH DẠNG JSON CHUẨN BACKEND TRẢ VỀ CHO FRONTEND

Backend là nguồn sự thật cho phạm vi, số liệu và biểu đồ. Ollama chỉ tạo trường `aiAnalysis`; Backend phải validate JSON từ AI trước khi ghép vào response cuối cùng.

```json
{
  "reportTitle": "Báo Cáo Phân Tích Học Thuật Toàn Diện - Học Kỳ 1 (2026-2027)",
  "campusName": "Cơ sở Hà Nội",
  "generatedAt": "2026-09-03T08:00:00Z",
  "scope": {
    "campusId": 3,
    "semesterId": 12,
    "departmentId": null
  },
  "metrics": {
    "studentCount": 520,
    "averageGpa": 7.38,
    "previousAverageGpa": 7.23,
    "atRiskCount": 42,
    "passRate": 82.4
  },
  "charts": [
    {
      "id": "gpa-by-department",
      "type": "bar",
      "title": "So Sánh GPA Trung Bình Giữa Các Khoa",
      "labels": ["Công Nghệ Thông Tin", "Quản Trị Kinh Doanh"],
      "datasets": [
        {
          "label": "GPA Trung Bình",
          "data": [7.65, 7.20],
          "backgroundColor": ["#3b82f6", "#10b981"]
        }
      ]
    }
  ],
  "aiAnalysis": {
    "executiveSummary": "GPA trung bình tăng nhẹ so với kỳ trước nhưng vẫn có 42 sinh viên thuộc nhóm cần theo dõi.",
    "findings": [
      {
        "observation": "GPA trung bình tăng 0.15 điểm so với kỳ trước.",
        "evidence": ["averageGpa", "previousAverageGpa"],
        "severity": "info"
      }
    ],
    "recommendations": [
      {
        "priority": 1,
        "action": "Rà soát nhóm sinh viên có nguy cơ và lập kế hoạch hỗ trợ.",
        "evidence": ["atRiskCount"],
        "confidence": "medium"
      }
    ]
  },
  "processing": {
    "mode": "fast",
    "model": "qwen2.5:3b",
    "cached": false
  }
}
```

### 4.1. Cấu hình model và tốc độ

| Mục đích | Model | Cấu hình chính |
|---|---|---|
| Phân tích nhanh mặc định | `qwen2.5:3b` | `num_ctx=2048`, `num_predict=256`, `temperature=0.1`, `keep_alive=30m` |
| Phân tích sâu do BGH chủ động chọn | `qwen3.5:9b-q4_K_M` | `think=false`, `num_ctx=2048`, `num_predict=384`, `temperature=0.1`, `keep_alive=10m` |
| RAG tài liệu/quy chế khi cần | `qwen3-embedding:0.6b` | Chỉ dùng để tìm đoạn tài liệu liên quan, không dùng cho số liệu analytics thông thường |

Hệ thống chỉ xử lý **một lượt sinh nội dung cùng lúc** trên máy AI. Backend ưu tiên hiển thị metrics/charts ngay khi query xong; phần `aiAnalysis` có thể hiển thị sau để người dùng không phải chờ model mới xem được số liệu.

---

## PHẦN 5: KẾT LUẬN & ĐÁNH GIÁ GIÁ TRỊ ĐỒ ÁN

1. **Tính Thực Tiễn & Giá Trị Đồ Án**: Mô hình kết hợp giữa **Hệ thống ERP Học vụ thực tế (ASP.NET Core + Vue 3)** và **Trí tuệ Nhân tạo Local LLM (Ollama) qua mạng Tailscale** là điểm nhấn công nghệ giúp kiểm soát dữ liệu, không phát sinh chi phí API theo lượt và hỗ trợ Ban Giám Hiệu đọc báo cáo nhanh hơn.
2. **Tính Minh Bạch & Có Thể Kiểm Chứng**: Backend chịu trách nhiệm tính metrics/charts từ dữ liệu thật; AI chỉ viết nhận định và phải gắn `evidence` vào mỗi phát hiện hoặc khuyến nghị. Các kết luận về nguyên nhân được trình bày dưới dạng dấu hiệu hoặc mối tương quan cần kiểm tra thêm, không được xem là kết luận tuyệt đối về sinh viên hay giảng viên.

---

## PHẦN 6: THIẾT KẾ KỸ THUẬT CHI TIẾT 5 HÀM BACKEND & RAG PIPELINE

Theo sự thống nhất với Ban Dự Án, kiến trúc Backend sẽ được tổ chức thành **4 hàm Context Analytics chuyên biệt + 1 hàm Orchestrator điều phối tổng thể** cùng hệ thống RAG tài liệu quy chế nhẹ (In-Memory Cosine Similarity).

```
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│                             KIẾN TRÚC ĐIỀU PHỐI BACKEND BGH AI                                  │
└──────────────────────────────────────────────────────────────────────────────────────────────────┘

                                    [CLIENT BGH]
                                         │  POST /api/bgh/academic/ai-analytics
                                         ▼
                 ┌────────────────────────────────────────────────────────┐
                 │          5. BGH AI ORCHESTRATOR                        │
                 │  GenerateBghAiReportAsync(BghAiReportRequest, User)   │
                 └───────────────────────┬────────────────────────────────┘
                                         │
                 ┌───────────────────────┼───────────────────────┐
                 │ (1) Kiểm tra Cache    │ (2) Query dữ liệu sạch│
                 ▼ IMemoryCache          ▼                       ▼
          [Trả kết quả ngay]     ┌───────────────┐       ┌────────────────┐
                                 │ 1. GPA        │       │ 3. Pass/Fail   │
                                 ├───────────────┤       ├────────────────┤
                                 │ 2. At-Risk    │       │ 4. Evaluations │
                                 └───────┬───────┘       └────────┬───────┘
                                         │                        │
                                         └───────────┬────────────┘
                                                     ▼
                                           AnalyticsContextDto
                                                     │
                                                     ▼
                         ┌───────────────────────────────────────────────────────┐
                         │ (3) Đối chiếu Quy chế (RAG Pipeline - nếu useRag=true)│
                         │     IRagRetriever.SearchAsync() -> Top 3 chunks       │
                         └───────────────────────────┬───────────────────────────┘
                                                     │
                                                     ▼
                         ┌───────────────────────────────────────────────────────┐
                         │ (4) Gọi OllamaService (Máy Local qua Tailscale 11434)  │
                         │     Fast Mode (3B) / Deep Mode (9B) -> format: "json" │
                         └───────────────────────────┬───────────────────────────┘
                                                     │
                                                     ▼
                         ┌───────────────────────────────────────────────────────┐
                         │ (5) Validate JSON -> Ghép Metrics/Charts/AI -> Cache  │
                         └───────────────────────────────────────────────────────┘
```

### 6.1. Chi tiết 4 Hàm Analytics Lấy Dữ Liệu Sạch (Context Builders)

#### 1. GPA Analytics
```csharp
Task<GpaAnalyticsContextDto> GetGpaAnalyticsContextAsync(
    int campusId,
    int semesterId,
    int? departmentId,
    CancellationToken cancellationToken);
```
- **Backend tính toán:**
  - Tổng số sinh viên, GPA trung bình toàn cơ sở, GPA kỳ trước, mức tăng/giảm (+/- delta).
  - Phân bố 5 dải điểm: Xuất sắc (9.0-10), Giỏi (8.0-8.9), Khá (7.0-7.9), Trung bình (5.0-6.9), Yếu (<5.0).
  - GPA trung bình theo từng khoa/ngành và độ lệch chuẩn (độ phân hóa học lực).
  - Dữ liệu biểu đồ Chart.js: Biểu đồ cột so sánh khoa, biểu đồ tròn xếp loại, biểu đồ đường xu hướng.
- **Vai trò AI:** Nhận context số liệu đã gom gọn, viết nhận xét phân hóa học lực và đề xuất can thiệp.

#### 2. At-Risk Analytics (Cảnh báo sớm sinh viên rớt môn)
```csharp
Task<AtRiskAnalyticsContextDto> GetAtRiskAnalyticsContextAsync(
    int campusId,
    int semesterId,
    int? departmentId,
    CancellationToken cancellationToken);
```
- **Backend tính toán:**
  - Tổng số sinh viên cần theo dõi, phân tầng 3 mức: Báo động đỏ (Critical), Cảnh báo vàng (Moderate), Cần theo dõi (Watchlist).
  - Phân loại 3 căn nguyên rủi ro từ dữ liệu thật: Rủi ro học lực (điểm thấp), Rủi ro chuyên cần/hành vi (vắng nhiều, nợ bài tập), Rủi ro ngoại cảnh/tài chính (nợ học phí quá hạn, có đơn khó khăn).
  - Danh sách top lớp học phần có tỷ lệ rủi ro cao nhất.
- **Vai trò AI:** Diễn giải dấu hiệu và khuyến nghị BGH kiểm tra, không quy kết nguyên nhân tuyệt đối.

#### 3. Pass/Fail Analytics
```csharp
Task<PassFailAnalyticsContextDto> GetPassFailAnalyticsContextAsync(
    int campusId,
    int semesterId,
    int? departmentId,
    CancellationToken cancellationToken);
```
- **Backend tính toán:**
  - Tỷ lệ Pass / Fail toàn trường và theo từng khoa.
  - Top 5 môn có tỷ lệ rớt cao nhất cơ sở.
  - Điểm quá trình trung bình vs Điểm thi kết thúc môn (độ lệch đề thi).
  - So sánh chéo tỷ lệ rớt môn giữa các lớp học phần cùng dạy một môn (phát hiện lệch chuẩn phương pháp giảng dạy hoặc tiêu chí chấm).
- **Vai trò AI:** Phân tích độ lệch chuẩn đề thi và khuyến nghị cân bằng tải nhận thức lịch thi.

#### 4. Teacher Evaluation Analytics (Đánh giá Giảng viên)
```csharp
Task<TeacherEvaluationContextDto> GetTeacherEvaluationContextAsync(
    int campusId,
    int semesterId,
    int? departmentId,
    CancellationToken cancellationToken);
```
- **Backend tính toán:**
  - Điểm đánh giá trung bình toàn trường, số lượng phản hồi sinh viên.
  - Tải giảng dạy (số tiết dạy/tuần) -> Phát hiện giảng viên vượt ngưỡng 35 tiết/tuần (nguy cơ quá tải giờ dạy/burnout).
  - Đối chiếu tỷ lệ Pass/Fail lớp dạy để loại trừ trường hợp giảng viên nghiêm khắc bị sinh viên điểm kém vote 1 sao cảm tính.
  - Tuyệt đối bảo mật: Không gửi tên sinh viên đánh giá hoặc thông tin cá nhân nhạy cảm cho AI.
- **Vai trò AI:** Phân tích cảm xúc nhận xét sinh viên (Khen ngợi vs Góp ý), cảnh báo quá tải giờ dạy.

---

### 6.2. Hàm Điều Phối Tổng Thể (BGH AI Orchestrator)

```csharp
Task<BghAiReportResponse> GenerateBghAiReportAsync(
    BghAiReportRequest request,
    CurrentUser currentUser,
    CancellationToken cancellationToken);
```

**Các bước xử lý tuần tự trong hàm điều phối:**
1. **Scope & Quyền:** Lấy `CampusId` và `Role` từ JWT Token (`currentUser`).
2. **Kiểm tra IMemoryCache:** Nếu không yêu cầu `forceRefresh`, kiểm tra cache key. Nếu có sẵn trong RAM, trả ngay lập tức (< 1ms).
3. **Query & Tính toán Metrics:** Gọi hàm Context Analytics tương ứng (`GetGpaAnalyticsContextAsync`, v.v.).
4. **Đối chiếu RAG Quy chế (Nếu `useRag = true`):**
   - Chỉ kích hoạt khi BGH cần đối chiếu văn bản (ngưỡng cảnh báo, điều kiện thi, chính sách).
   - Dùng `qwen3-embedding:0.6b` sinh vector câu hỏi, tính Cosine Similarity trên RAM và lấy Top 3 đoạn trích dẫn ngắn.
5. **Chọn Model AI:**
   - `mode = "fast"` $\rightarrow$ `qwen2.5:3b` (Phân tích nhanh, ~3–5s).
   - `mode = "deep"` $\rightarrow$ `qwen3.5:9b-q4_K_M` (Phân tích chiến lược sâu, ~15–30s).
6. **Gọi Ollama API qua Tailscale:** Gửi request với `format: "json"` qua `AiRequestGate` (đảm bảo 1 luồng xử lý GPU duy nhất).
7. **Ghép nối & Cache:** Validate JSON trả về, ghép với Metrics + Charts của Backend, lưu vào `IMemoryCache` (TTL 30 phút) và trả về Frontend.

---

### 6.3. Kiến Trúc RAG Tài Liệu Nhẹ (Lightweight In-Memory Cosine Similarity)

Vì Backend chạy trên VPS trong khi Ollama chạy trên máy local Windows, kiến trúc phân bổ tài nguyên chuẩn xác như sau:

| Thành phần | Nơi lưu trữ | Cơ chế xử lý |
|---|---|---|
| **Ba mô hình Ollama** (`3b`, `9b`, `embedding`) | Máy Windows (`E:\AI\OllamaModels`) | Nhận request từ VPS qua IP Tailscale cổng `11434` |
| **Tài liệu quy chế gốc (.txt/.md)** | Backend VPS (thư mục `App_Data/rag_documents/`) | Upload từ Admin Web lên VPS |
| **Vector Chunks & Embeddings** | SQL Server trên VPS (`AiDocuments`, `AiDocumentChunks`) | Lưu vector dưới dạng chuỗi JSON `EmbeddingJson` |
| **Tìm kiếm tương đồng (Search)** | RAM Backend VPS | Đọc vector từ DB vào RAM, tính Cosine Similarity bằng C# (vài chục vector chỉ mất 1–2ms, không cần cài Vector DB nặng nề) |
| **Cache báo cáo BGH** | `IMemoryCache` Backend VPS | Lưu kết quả báo cáo trong RAM, truy xuất tức thì |

**Hai bảng CSDL SQL Server phục vụ RAG:**
```sql
CREATE TABLE [AiDocuments] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Title] NVARCHAR(255) NOT NULL,
    [FileName] NVARCHAR(255) NOT NULL,
    [Source] NVARCHAR(100) NULL,
    [AllowedRoles] NVARCHAR(255) NULL,
    [CampusId] INT NULL,
    [ContentHash] NVARCHAR(64) NULL,
    [Status] NVARCHAR(50) DEFAULT 'active',
    [CreatedAt] DATETIME2 DEFAULT SYSUTCDATETIME(),
    [UpdatedAt] DATETIME2 NULL
);

CREATE TABLE [AiDocumentChunks] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [DocumentId] INT NOT NULL FOREIGN KEY REFERENCES [AiDocuments]([Id]) ON DELETE CASCADE,
    [ChunkIndex] INT NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [EmbeddingJson] NVARCHAR(MAX) NOT NULL, -- Vector float[] lưu dạng JSON
    [TokenCount] INT DEFAULT 0,
    [CreatedAt] DATETIME2 DEFAULT SYSUTCDATETIME()
);
```

---

### 6.4. Giao Diện Người Dùng (Frontend UI/UX)

- **Vị trí nút:** Nút `⚡ Phân tích bằng AI` đặt tại Topbar của 4 trang báo cáo BGH.
- **Dropdown chọn chế độ (Chuẩn các AI lớn, ẩn tên model kỹ thuật):**
  - ⚡ **Phân tích nhanh** *(Phản hồi sau 3–5 giây, tóm lược điểm chính)* $\rightarrow$ ngầm gọi `qwen2.5:3b`.
  - 🧠 **Phân tích chuyên sâu** *(Phân tích đa chiều, dự báo và ma trận hành động)* $\rightarrow$ ngầm gọi `qwen3.5:9b`.
- **Toggle bổ sung:** Switch nhỏ `[x] Đối chiếu quy chế đào tạo (RAG)` (mặc định tắt để chạy nhanh nhất).
- **Trải nghiệm Deferred Loading:** Biểu đồ Chart.js và các thẻ số liệu hiển thị trong 0.3s; khối nhận định AI hiển thị hiệu ứng skeleton loading và xuất hiện mượt mà ngay khi hoàn tất.

---

## PHẦN 7: GIẢI THÍCH KỸ THUẬT & CÁC ĐIỂM ĐÃ THỐNG NHẤT

### 7.1. Giải Thích Cơ Chế `IMemoryCache`

#### `IMemoryCache` là gì?
`IMemoryCache` là dịch vụ lưu trữ bộ nhớ đệm trong RAM (Random Access Memory) được tích hợp sẵn trong nền tảng ASP.NET Core:
- **Tốc độ:** Dữ liệu nằm trực tiếp trong bộ nhớ RAM của tiến trình web, thời gian truy xuất đạt mức **micro-giây ($\mu s$)**, nhanh hơn hàng nghìn lần so với đọc từ ổ cứng SSD hay truy vấn SQL Server.
- **Cấu trúc lưu trữ:** Hoạt động theo mô hình Key-Value Dictionary trong RAM.
  - *Cache Key định danh:* `bgh_ai_report:{campusId}:{reportType}:{semesterId}:{deptId}:{mode}:{useRag}`
  - *Cache Value:* Toàn bộ đối tượng `BghAiReportResponse` (chỉ khoảng 5–15KB).

#### Cơ chế quản lý vòng đời (Expiration Policy):
1. **Absolute Expiration (Hết hạn tuyệt đối):** Đặt thời hạn cố định là **30 phút**. Sau 30 phút, cache tự động bị giải phóng khỏi RAM để đảm bảo nếu điểm số trong DB có cập nhật thì BGH sẽ nhận được phân tích mới.
2. **Nút Làm Mới (`forceRefresh = true`):** Khi BGH bấm nút *"Làm mới phân tích"* trên giao diện, Backend sẽ bỏ qua cache, gọi Ollama tính lại toàn bộ và ghi đè dữ liệu mới vào `IMemoryCache`.
3. **An toàn bộ nhớ RAM:** Do mỗi báo cáo chỉ chiếm khoảng 10KB, kể cả khi cache 50 báo cáo khác nhau cùng lúc thì tổng RAM tiêu thụ cũng chỉ khoảng **0.5MB**, hoàn toàn nhẹ nhàng cho server VPS.
4. **Lưu ý:** Khi server VPS khởi động lại hoặc khi bạn chạy lệnh rebuild Docker container, RAM sẽ được làm mới (cache bị xóa), lần gọi đầu tiên sau đó Backend sẽ tính lại và cache tiếp.

---

### 7.2. Tóm Tắt 5 Quyết Định Đã Được Thống Nhất

| Vấn đề | Quyết định đã chốt | Ghi chú thực thi |
|---|---|---|
| **1. Kết nối Backend -> AI** | **Gọi trực tiếp Ollama REST API** qua cổng 11434 qua IP Tailscale | Tận dụng `OllamaService.cs` và `AiRequestGate` có sẵn trong .NET, bỏ qua FastAPI |
| **2. Giao diện chọn chế độ** | **Dropdown thân thiện:** "Phân tích nhanh ⚡" và "Phân tích chuyên sâu 🧠" | Ẩn hoàn toàn tên kỹ thuật của model (`qwen2.5:3b`, `qwen3.5:9b`), thiết kế đẹp chuẩn ChatGPT/Gemini |
| **3. Lưu trữ báo cáo AI** | **Lưu trong `IMemoryCache` (TTL 30 phút)** kèm nút `forceRefresh` | Phản hồi tức thì, không gây rác CSDL |
| **4. Dữ liệu đánh giá GV** | **Seed 30–50 nhận xét thực tế** vào `DanhGiaGiaoVien.nhan_xet_van_ban` | Đầy đủ chủ đề khen ngợi, góp ý, quá tải giờ dạy để AI phân tích cảm xúc |
| **5. Lộ trình thực hiện** | **Triển khai 5 hàm Backend + RAG** $\rightarrow$ Kết nối UI GPA & At-Risk $\rightarrow$ Pass/Fail & Evaluations | Đảm bảo tính vững chắc của kiến trúc dữ liệu trước khi ghép giao diện |

