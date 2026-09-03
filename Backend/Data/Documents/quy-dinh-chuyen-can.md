# Quy định Chuyên cần và Điểm danh — AET Academy
> **Nguồn:** Nội quy học tập AET Academy + Quy chế Đào tạo 2026, Điều 10  
> **Cập nhật:** Học kỳ 1, Năm học 2026–2027

---

## 1. Mục đích của quy định chuyên cần

Chuyên cần (attendance) là yếu tố quan trọng phản ánh thái độ học tập và là điều kiện bắt buộc để được dự thi cuối kỳ. Hệ thống AET LMS ghi nhận điểm danh **điện tử theo thời gian thực** trong mỗi buổi học.

---

## 2. Cách thức điểm danh trên AET LMS

### 2.1 Điểm danh bằng Giảng viên
- Giảng viên mở buổi điểm danh trên thiết bị trong **5 phút đầu** của buổi học.
- Sinh viên nhận thông báo đẩy (push notification) trên ứng dụng AET LMS.
- Sinh viên xác nhận có mặt bằng **QR Code** hoặc **mã OTP 6 chữ số** trong cửa sổ 5 phút.

### 2.2 Điểm danh bằng ExamGuard Agent (phòng máy)
- Tại các phòng thực hành máy tính có cài ExamGuard Agent, điểm danh tự động qua nhận diện tài khoản đăng nhập máy.

### 2.3 Trạng thái điểm danh
| Mã trạng thái | Tên hiển thị | Mô tả |
|---|---|---|
| `co_mat` | ✅ Có mặt | Điểm danh thành công trong thời gian quy định |
| `muon` | ⏰ Đi muộn | Điểm danh sau cửa sổ 5 phút nhưng trong 15 phút đầu |
| `vang_co_phep` | 📋 Vắng có phép | Vắng nhưng đã nộp đơn + minh chứng được duyệt |
| `vang_khong_phep` | ❌ Vắng không phép | Vắng không có lý do hợp lệ |

---

## 3. Quy tắc tính tỷ lệ chuyên cần

### 3.1 Công thức
```
Tỷ lệ chuyên cần (%) = (Số buổi Có mặt + 0.5 × Số buổi Muộn) / Tổng số buổi đã học × 100%

Số buổi vắng hiệu lực = Số vắng không phép + Số vắng có phép + 0.5 × Số buổi muộn
```

### 3.2 Ngưỡng vắng tối đa theo số tín chỉ
| Số TC môn học | Tổng buổi học (ước tính) | Vắng tối đa cho phép | Vắng buổi thứ... → Cấm thi |
|---|---|---|---|
| 1 TC | ~8 buổi | 1 buổi | Buổi vắng thứ 2 |
| 2 TC | ~15 buổi | 3 buổi | Buổi vắng thứ 4 |
| 3 TC | ~22 buổi | 4 buổi | Buổi vắng thứ 5 |
| 4 TC | ~30 buổi | 6 buổi | Buổi vắng thứ 7 |

> ⚠️ **Quan trọng:** Số buổi thực tế phụ thuộc vào lịch học được xếp bởi Phòng Giáo vụ. Kiểm tra trực tiếp trên AET LMS → Mục "Chuyên cần" của từng môn.

### 3.3 Ví dụ tính tỷ lệ chuyên cần
**Sinh viên A — Môn COM101 (3 TC), đã học 20 buổi:**
- Có mặt: 15 buổi
- Muộn: 2 buổi
- Vắng có phép: 2 buổi
- Vắng không phép: 1 buổi

```
Số buổi vắng hiệu lực = 2 + 1 + (0.5 × 2) = 4 buổi
Tỷ lệ chuyên cần = (15 + 0.5×2) / 20 × 100% = 16/20 × 100% = 80%
Tỷ lệ vắng = 20% → Đúng ngưỡng cho phép → CÒN đủ điều kiện thi nhưng RẤT NGUY HIỂM
```

---

## 4. Hậu quả khi vi phạm quy định chuyên cần

### 4.1 Bị cấm thi (điểm F tự động)
- Khi tỷ lệ vắng vượt **20%**, hệ thống AET LMS tự động:
  - Đánh dấu sinh viên "KHÔNG ĐỦ ĐIỀU KIỆN DỰ THI" cho môn học đó.
  - Gửi email cảnh báo đến sinh viên và phụ huynh/người giám hộ (nếu đã liên kết).
  - Giảng viên nhận thông báo về danh sách sinh viên bị cấm thi.

### 4.2 Ảnh hưởng đến điểm tổng kết
- Điểm F do bị cấm thi = **0 điểm** cho toàn môn học (không phải chỉ phần thi CK).
- Điểm F được tính vào CPA → Ảnh hưởng đến xếp loại học lực và cảnh báo học vụ.

---

## 5. Quy trình xin phép vắng học

### Bước 1: Nộp đơn xin phép trước (nếu biết trước)
1. Đăng nhập AET LMS → **Đơn từ** → **Đơn xin nghỉ phép tạm thời**
2. Chọn ngày nghỉ, môn học bị ảnh hưởng, nhập lý do
3. Đính kèm minh chứng (nếu có)
4. Gửi đơn → Giảng viên duyệt trong 24h

### Bước 2: Xác nhận sau khi vắng (trường hợp khẩn cấp)
1. Liên hệ **Giảng viên** qua tin nhắn nội bộ AET LMS ngay khi có thể
2. Nộp đơn xin phép hồi tố trên LMS trong **48 giờ** sau buổi vắng
3. Đính kèm minh chứng (giấy khám bệnh, giấy tai nạn, giấy xác nhận công tác...)

### Tài liệu minh chứng được chấp nhận
- ✅ Giấy khám bệnh / Đơn thuốc có chữ ký bác sĩ
- ✅ Giấy nhập viện / Giấy xuất viện
- ✅ Giấy xác nhận tai nạn (công an/bệnh viện)
- ✅ Giấy xác nhận công việc của doanh nghiệp (thực tập)
- ✅ Giấy triệu tập của cơ quan nhà nước
- ❌ Ảnh chụp màn hình chat/tin nhắn KHÔNG được chấp nhận

---

## 6. Tra cứu tình trạng chuyên cần trên AET LMS

**Sinh viên tự kiểm tra:**
1. Đăng nhập → Menu **Khóa học** → Chọn môn học → Tab **Chuyên cần**
2. Xem biểu đồ chuyên cần theo tuần và thống kê tổng kết
3. Màu xanh = Có mặt | Màu vàng = Muộn | Màu cam = Vắng có phép | Màu đỏ = Vắng không phép

**Trợ lý AI AET:**
Hỏi trợ lý chatbot: "Tôi đã vắng bao nhiêu buổi?" để nhận báo cáo chuyên cần tức thì.

---

## 7. Khiếu nại điểm danh sai

Nếu bạn bị ghi nhận vắng nhưng thực tế đã có mặt:

1. **Trong ngày:** Liên hệ trực tiếp Giảng viên sau buổi học để được xác nhận và cập nhật.
2. **Sau ngày học:** Nộp yêu cầu chỉnh sửa điểm danh tại Phòng Giáo vụ (kèm minh chứng như ảnh, xác nhận của bạn học).
3. **Thời hạn khiếu nại:** Tối đa **7 ngày** sau buổi học bị ghi nhận sai.

> Sau 7 ngày, yêu cầu chỉnh sửa sẽ không được xem xét trừ trường hợp đặc biệt.

