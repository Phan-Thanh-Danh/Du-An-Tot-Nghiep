# Kế hoạch phân bổ dữ liệu Seed (bản đầy đủ) — Trường AET

> Trạng thái: **BẢN NHÁP ĐỂ BÀN — CHƯA VIẾT SQL**
> Nguyên tắc xuyên suốt: mọi bảng đều được **rải đều theo cơ sở** (và theo ngành/lớp/tài khoản bên trong cơ sở đó), không tập trung dữ liệu vào 1 cơ sở/1 tài khoản.

---

## 0. Tham số gốc (chưa chốt N)
- N = số cơ sở con (chờ bạn chốt số).
- Mỗi cơ sở: 3 ngành cố định (CNTT, TKDH, MKT) × 1.000 SV/ngành = 3.000 SV/cơ sở.
- 1.000 giảng viên/cơ sở, 10 giáo vụ/cơ sở, 1 CampusAdmin (ban giám hiệu)/cơ sở.
- Phụ huynh: tính theo tỉ lệ 1 PH : ~1.5 con → ~2.000 PH/cơ sở.

---

## 1. Tổ chức & danh mục học thuật *(đã chốt ở lượt trước)*
`DonVi`, `NganhDaoTao`, `ChuyenNganh`, `ChuyenNganhTheoCoSo`, `DanhMucMonHoc`, `ChuongTrinhDaoTao`, `MonHocTrongChuongTrinh`, `MonHocTienQuyet`, `HocKy`, `Block`, `QuyDoiTinChi`, `KhoaTuyenSinh` — dùng lại nguyên cấu trúc CNTT/TKDH/MKT, đổi tên cơ sở thành "Trường AET ...".

## 2. Nội dung môn học (LMS)
| Bảng | Cách phân bổ |
|---|---|
| `Chuong` | Mỗi môn (trong ~30 môn thuộc 3 ngành) có **4–6 chương**, đánh số thứ tự |
| `BaiHoc` | Mỗi chương có **2–4 bài học** |
| `BaiHocNoiDung` | Mỗi bài học có **1–3 nội dung** (video/text/tài liệu) |
| `CourseSyllabus` | 1 đề cương/môn học (dùng chung, không lặp theo cơ sở) |
| `TienDoBaiHoc`, `TienDoNoiDungHocTap`, `PhienHocNoiDung` | Sinh theo **SV đã đăng ký môn đó**, tỉ lệ hoàn thành 40–100% ngẫu nhiên có trọng số (không phải toàn bộ 100%, để dữ liệu thực tế hơn) |
| `BinhLuan` | Rải rác trên khoảng 10–20% bài học, vài bình luận/bài |

## 3. Lớp hành chính & lớp học phần
| Bảng | Cách phân bổ |
|---|---|
| `LopHanhChinh` | Mỗi ngành/cơ sở: 1.000 SV ÷ ~35 SV/lớp ≈ **~28 lớp hành chính/ngành** → ~85 lớp/cơ sở. Đặt mã theo mẫu `<MãCS>-<MãNgành><khóa><stt>` (vd `HCM-SD190112`) để tránh trùng giữa các cơ sở |
| `GiaoVienChuyenNganh`, `GiaoVienMonHoc` | 1.000 GV/cơ sở chia đều theo 3 ngành (~333 GV/ngành), mỗi GV dạy 2–4 môn thuộc chuyên ngành mình |
| `LopHocPhan` | Mỗi môn/học kỳ/cơ sở mở nhiều lớp học phần (sức chứa ~40 SV), số lớp = tổng SV đăng ký môn đó ÷ 40, phân giáo viên phụ trách theo `GiaoVienMonHoc` |
| `DangKyHocPhan`, `GiaiDoanDangKy` | SV đăng ký đủ môn theo chương trình học kỳ hiện tại, tôn trọng sức chứa lớp (không vượt `SucChua`) và không trùng lịch |

## 4. Cơ sở vật chất & lịch học
| Bảng | Cách phân bổ |
|---|---|
| `ToaNha`, `Tang`, `PhongHoc`, `ThietBiPhong` | Mỗi cơ sở: 2–3 tòa nhà × 3–4 tầng × ~8 phòng/tầng ≈ 60–90 phòng/cơ sở — đủ đáp ứng số lớp học phần đang mở |
| `CaHoc` | Dùng chung 1 bộ ca học (sáng/chiều/tối) cho toàn hệ thống |
| `ThoiKhoaBieu`, `BuoiHoc` | Sinh theo từng lớp học phần trong suốt học kỳ, gán phòng + ca học không trùng giờ/phòng |
| `DatPhong`, `BaoCaoSuDungPhong` | Một số lượt đặt phòng ngoài lịch cố định (họp, hoạt động ngoại khóa), rải đều theo cơ sở |

## 5. Điểm danh, bài tập, điểm số
| Bảng | Cách phân bổ |
|---|---|
| `DiemDanh` | Với mỗi buổi học đã diễn ra (`TrangThaiBuoi = da_dien_ra`), điểm danh toàn bộ SV trong lớp học phần đó, tỉ lệ ~85% `co_mat`, ~10% `vang`, ~5% `di_muon` (ngẫu nhiên có trọng số, không đều tuyệt đối để giống thật) |
| `BaiTap`, `BaiNop` | Mỗi lớp học phần có 2–4 bài tập; tỉ lệ nộp bài ~80–95% SV (không phải 100%) |
| `CauHinhDauDiemQuaTrinh`, `CauHinhDiemMonHoc` | Cấu hình 1 lần/môn/học kỳ, đảm bảo tổng trọng số = 100 (theo đúng ràng buộc đã phát hiện) |
| `DeKiemTra`, `CauHoi`, `CauHoiDeKiemTra` | Ngân hàng câu hỏi theo môn (~15–20 câu/môn), đề kiểm tra quiz/giữa kỳ theo lớp học phần |
| `DiemSo` | **Tính theo đúng công thức `GradeAggregationService`** (điểm quá trình từ điểm danh + bài tập + quiz, GPA môn = trọng số quá trình/giữa kỳ/cuối kỳ, `TrangThai = dat/rot` theo `NguongDat`) — không random để tránh mâu thuẫn logic |
| `NhatKyThayDoiDiem`, `YeuCauSuaDiem` | Một số ít bản ghi minh họa (không cần phủ hết SV) |

## 6. Kỳ thi
| Bảng | Cách phân bổ |
|---|---|
| `KyThi`, `LichThiTong` | 1 kỳ thi cuối kỳ/học kỳ/cơ sở |
| `CaThi` | Theo môn thi × phòng thi, sức chứa theo `PhongHoc` |
| `ThiSinhCaThi`, `PhanCongGiamThi` | SV đã đăng ký môn → gán vào ca thi tương ứng; giám thị lấy từ GV cùng cơ sở (không nhất thiết cùng chuyên ngành) |
| `DiemDanhThi`, `BienBanThi`, `NhatKyViPhamThi`, `XuLyViPhamThi` | Rải rác một tỉ lệ nhỏ (2–5%) có vi phạm/biên bản để minh họa tính năng |

## 7. Tài chính
| Bảng | Cách phân bổ |
|---|---|
| `CauHinhHocPhiChuongTrinh` | 1 cấu hình/chương trình đào tạo |
| `HoaDon` | Mỗi SV có hóa đơn học phí/học kỳ đang theo học |
| `GiaoDich` | Phần lớn hóa đơn có giao dịch thanh toán tương ứng (tỉ lệ ~90% đã thanh toán, còn lại `qua_han`/`chua_thanh_toan` để đa dạng trạng thái) |
| `TaiKhoanNhanTien` | 1 tài khoản nhận tiền/cơ sở |
| `YeuCauHoanPhi` | Số ít bản ghi minh họa |

## 8. Đơn từ, thông báo, hỗ trợ
| Bảng | Cách phân bổ |
|---|---|
| `MauDonTu` | Copy 8 mẫu (sửa lỗi trùng `LoaiDon`) |
| `DonTu`, `TepDinhKemDonTu`, `NhatKyDuyetDon` | Rải một tỉ lệ nhỏ SV/cơ sở (~3–5%) gửi đơn, đa dạng loại đơn + trạng thái |
| `QuyTrinhDonTu`, `BuocQuyTrinh` | 1 quy trình chuẩn dùng chung |
| `ThongBao`, `ThongBaoNguoiNhan`, `MauThongBao` | Một số thông báo toàn hệ thống (`AllSystem`) + một số theo từng cơ sở (`Organization`) — rải đều, không dồn hết vào 1 cơ sở |
| `PhieuHoTro`, `TinNhanHoTro` | Vài phiếu hỗ trợ/cơ sở, trạng thái đa dạng |
| `CauHoiThuongGap` | Dùng chung 1 bộ FAQ cho toàn hệ thống |

## 9. Khen thưởng — Kỷ luật
| Bảng | Cách phân bổ |
|---|---|
| `MauBangKhen` | **Tự thiết kế mới** (không có sẵn để copy) — 1–2 mẫu chuẩn dùng chung |
| `DotKhenThuong`, `CauHinhKhenThuong`, `UngVienKhenThuong`, `KhenThuong` | Mỗi học kỳ/cơ sở có 1 đợt khen thưởng, đề cử ~5–10% SV có GPA cao |
| `HoSoKyLuat`, `KhieuNaiKyLuat` | Tỉ lệ nhỏ (~1–2%) SV có hồ sơ kỷ luật, rải đều các cơ sở |

## 10. Đánh giá & phản hồi
| Bảng | Cách phân bổ |
|---|---|
| `MauDanhGia`, `CauHoiDanhGia` | 1 bộ mẫu đánh giá giáo viên dùng chung |
| `DanhGiaGiaoVien`, `NopBaiDanhGia` | SV đánh giá GV dạy lớp học phần mình học (tỉ lệ phản hồi ~50–70%, không bắt buộc 100%) |

## 11. Người dùng — tổng hợp phân bổ (nhắc lại, để nhất quán với các mục trên)
| Vai trò | Số lượng/cơ sở | Ghi chú |
|---|---|---|
| CampusAdmin (ban giám hiệu) | 1 | |
| AcademicStaff (giáo vụ) | 10 | |
| Teacher (giảng viên) | 1.000 | chia đều 3 ngành → ~333/ngành |
| Student (sinh viên) | 3.000 | chia đều 3 ngành → 1.000/ngành, rồi chia tiếp theo 3 chuyên ngành/ngành → ~333/chuyên ngành |
| Parent (phụ huynh) | ~2.000 | 1 PH : 1–2 con |
| SuperAdmin, HoiDongQuanLyNoiDung | chỉ ở ROOT, không lặp theo cơ sở | vài tài khoản cố định |

---

## Câu hỏi còn mở
1. **Số cơ sở N** — vẫn đang chờ bạn.
2. File sẽ rất lớn (hàng trăm nghìn dòng dữ liệu nếu N=10) — mình dự định dùng **vòng lặp T-SQL (WHILE / số học sinh sinh)** để sinh hàng loạt thay vì gõ tay từng dòng `INSERT`, vẫn gói gọn trong 1 file `.sql` như bạn yêu cầu. Bạn có đồng ý với cách này không, hay muốn mỗi dòng dữ liệu là 1 câu `INSERT` tường minh (dễ đọc hơn nhưng file sẽ cực lớn, khó mở)?
