# FULL DATABASE REFERENCE — LMS Academic Management System

> **Database:** SQL Server (LMS)
> **Schema:** dbo
> **ORM:** EF Core 10.0.6
> **Source:** Backend/Data/ApplicationDbContext.cs (6115 lines)
> **Last updated:** 2026-08-22
> **Total tables:** ~100 entities

---

## 1. MODULE OVERVIEW

| Module | Tables |
|--------|--------|
| Identity | NguoiDung, VaiTro, PhanQuyenNguoiDung, TokenLamMoi, PasswordResetOtps |
| RBAC | QuyenHan, VaiTroQuyenHan |
| Organization | DonVi |
| Academic Structure | NganhDaoTao, ChuyenNganh, ChuyenNganhTheoCoSo, ChuongTrinhDaoTao, ChuongTrinhHocKy, MonHocTrongChuongTrinh, MonHocTienQuyet, HocKy, Block, KhoaTuyenSinh, LopHanhChinh, LopHocPhan, GiaiDoanDangKy |
| Course | DanhMucMonHoc, KhoaHoc, CourseSyllabus (DeCuongMonHoc), Chuong |
| Schedule | ThoiKhoaBieu, BuoiHoc, CaHoc, ScheduleGenerationJob, ScheduleDraftItem, QuyDoiTinChi, QuyDinhChuyenCan |
| Attendance | DiemDanh, YeuCauMoKhoaDiemDanh |
| Grades | DiemSo, CauHinhDiemMonHoc, NhatKyThayDoiDiem, YeuCauSuaDiem |
| Assignments | BaiTap, BaiNop, CanhBaoDaoVan, TienDoBaiHoc |
| Lessons | BaiHocNoiDung |
| Examinations | DeKiemTra, CauHoi, CauHoiDeKiemTra, PhienThiHocSinh, KyThi, LichThiTong, CaThi, ThiSinhCaThi, DiemDanhThi, PhanCongGiamThi, NhatKyViPhamThi, XuLyViPhamThi, BienBanThi |
| Applications | DonTu, MauDonTu, NhatKyDuyetDon, TepDinhKemDonTu |
| Finance | HoaDon, GiaoDich, TaiKhoanNhanTien, CauHinhHocPhiChuongTrinh, YeuCauHoanPhi |
| Reward | KhenThuong, MauBangKhen, DotKhenThuong, CauHinhKhenThuong |
| Discipline | HoSoKyLuat, KhieuNaiKyLuat |
| Notifications | ThongBao, ThongBaoNguoiNhan, ThongBaoHenGio, MauThongBao, NhatKyThongBao, TuyChonThongBao |
| Support | PhieuHoTro, TinNhanHoTro, CauHoiThuongGap |
| Facilities | ToaNha, Tang, PhongHoc, ThietBiPhong, DatPhong, BaoCaoSuDungPhong |
| AI/Analytics | BaoCaoRuiRoRotMon, BaoCaoRuiRoVang, DanhSachRuiRoRotMon, CanhBaoBaoMat |
| Evaluation | DanhGiaGiaoVien, CauHoiDanhGia, NopBaiDanhGia |
| Audit | NhatKyKiemToan, XuatBaoCao |
| Comments | BinhLuan |
| Forms | MauDanhGia, LienKetPhuHuynh |

---

## 2. IDENTITY & AUTHENTICATION

### NguoiDung (User) — Core

| Column | Type | Nullable | Default | Notes |
|--------|------|----------|---------|-------|
| ma_nguoi_dung | int PK | No | identity | |
| ma_don_vi | int FK | Yes | | -> DonVi |
| email | nvarchar(255) | No | | UQ |
| ho_ten | nvarchar(255) | No | | |
| vai_tro_chinh | nvarchar(50) | No | | CK |
| ma_lop | int FK | Yes | | -> LopHanhChinh |
| so_dien_thoai | nvarchar(15) | Yes | | |
| trang_thai | nvarchar(20) | No | dang_nhap_lan_dau | CK |
| nam_nhap_hoc | int | Yes | | |
| mat_khau_hash | nvarchar(max) | Yes | | |
| ngay_tao | datetime2 | Yes | SYSUTCDATETIME() | |
| lan_dang_nhap_cuoi | datetime2 | Yes | | |
| so_lan_sai_mat_khau | int | Yes | 0 | |
| dang_nhap_lan_dau | bit | Yes | true | |

**vai_tro_chinh CK:** quan_tri, giao_vien, hoc_sinh, nhan_vien, hieu_truong, phu_huynh, sieu_quan_tri, quan_tri_co_so, quan_tri_co_so_con, chu_tich, hoidong_quanly_noidung, admin_tai_chinh, ke_toan_co_so, ke_toan_truong_co_so

**trang_thai CK:** hoat_dong, bi_khoa, dang_nhap_lan_dau

### VaiTro (Role)

| Column | Type | Notes |
|--------|------|-------|
| ma_vai_tro | int PK | ValueGeneratedNever |
| ma_code_vai_tro | nvarchar(50) | UQ |
| ten_vai_tro | nvarchar(100) | |

### PhanQuyenNguoiDung (User-Role)

| Column | Type | Notes |
|--------|------|-------|
| ma_nguoi_dung | int FK PK | composite |
| ma_vai_tro | int FK PK | composite |
| ngay_gan | datetime2 | default |

### TokenLamMoi (Refresh Token)

| Column | Type | Notes |
|--------|------|-------|
| ma_token_lam_moi | int PK | |
| ma_nguoi_dung | int FK | |
| token_hash | nvarchar(128) | UQ |
| het_han_luc | datetime2 | |
| thu_hoi_luc | datetime2 | |
| ngay_tao | datetime2 | |

### PasswordResetOtps

| Column | Type | Notes |
|--------|------|-------|
| Id | int PK | |
| Email | nvarchar(255) | IX |
| OtpCode | nvarchar(512) | |
| ExpiredAt | datetime2 | |
| IsVerified | bit | default false |
| IsUsed | bit | default false |
| CreatedAt | datetime2 | |

### QuyenHan (Permission)

| Column | Type | Notes |
|--------|------|-------|
| ma_quyen_han | int PK | |
| ma_code | nvarchar(100) | UQ |
| ten_quyen_han | nvarchar(200) | |
| module | nvarchar(50) | |
| action | nvarchar(50) | |
| mo_ta | nvarchar(500) | |

### VaiTroQuyenHan (Role-Permission)

| Column | Type | Notes |
|--------|------|-------|
| ma_vai_tro | int FK PK | composite, cascade |
| ma_quyen_han | int FK PK | composite, cascade |
| ngay_cap | datetime2 | default GETUTCDATE() |
| nguoi_cap | int FK | -> NguoiDung, SetNull |

---

## 3. ORGANIZATION

### DonVi (Organization Unit — Tree)

| Column | Type | Nullable | Default | Notes |
|--------|------|----------|---------|-------|
| ma_don_vi | int PK | No | identity | |
| ma_don_vi_cha | int FK | Yes | | -> DonVi (self-ref) |
| ten_don_vi | nvarchar(255) | No | | |
| cap_don_vi | nvarchar(20) | No | | CK: root, co_so, co_so_con |
| con_hoat_dong | bit | Yes | true | |
| ngay_tao | datetime2 | Yes | SYSUTCDATETIME() | |
| ngay_cap_nhat | datetime2 | Yes | | |

**IX:** ma_don_vi_cha, cap_don_vi, con_hoat_dong

---

## 4. ACADEMIC STRUCTURE

### HocKy (Term/Semester)

| Column | Type | Nullable | Default | Notes |
|--------|------|----------|---------|-------|
| ma_hoc_ky | int PK | No | identity | |
| ma_don_vi | int FK | No | | -> DonVi |
| ma_code_hoc_ky | nvarchar(30) | No | | |
| ten_hoc_ky | nvarchar(100) | No | | |
| ngay_bat_dau | date | Yes | | |
| ngay_ket_thuc | date | Yes | | |
| nam_hoc | nvarchar(20) | No | | |
| thu_tu_trong_nam | int | Yes | | CK: 1, 2, 3 |
| ngay_ket_thuc_block5 | date | Yes | | |
| da_khoa | bit | Yes | false | |
| so_tin_chi_toi_da | int | Yes | | |
| han_rut_mon | date | Yes | | |

**UQ:** (ma_don_vi, nam_hoc, thu_tu_trong_nam)

### Block

| Column | Type | Notes |
|--------|------|-------|
| ma_block | int PK | |
| ten_block | nvarchar(100) | |
| ma_hoc_ky | int FK | -> HocKy, Cascade |
| thu_tu_block | int | |
| ngay_bat_dau | date | |
| ngay_ket_thuc | date | |

### NganhDaoTao (Major)

| Column | Type | Notes |
|--------|------|-------|
| ma_nganh | int PK | |
| ma_code_nganh | nvarchar(50) | UQ |
| ten_nganh | nvarchar(255) | |
| mo_ta | nvarchar(max) | |
| con_hoat_dong | bit | default true |
| ngay_tao | datetime2 | |
| ngay_cap_nhat | datetime2 | |

### ChuyenNganh (Specialization)

| Column | Type | Notes |
|--------|------|-------|
| ma_chuyen_nganh | int PK | |
| ma_nganh | int FK | -> NganhDaoTao |
| ten_chuyen_nganh | nvarchar(255) | |
| mo_ta | nvarchar(max) | |
| con_hoat_dong | bit | |
| ngay_tao | datetime2 | |
| ngay_cap_nhat | datetime2 | |

**UQ:** (ma_nganh, ten_chuyen_nganh)

### ChuyenNganhTheoCoSo (Specialization per Campus)

| Column | Type | Notes |
|--------|------|-------|
| ma_chuyen_nganh_co_so | int PK | |
| ma_chuyen_nganh | int FK | -> ChuyenNganh |
| ma_don_vi | int FK | -> DonVi |
| trang_thai | nvarchar(30) | CK: draft, pending_approval, approved, active, inactive, rejected |
| nam_bat_dau | int | |
| chi_tieu_du_kien | int | |
| ghi_chu | nvarchar(max) | |
| con_hoat_dong | bit | |

**UQ:** (ma_chuyen_nganh, ma_don_vi)

### KhoaTuyenSinh (Admission Cohort)

| Column | Type | Notes |
|--------|------|-------|
| ma_khoa_tuyen_sinh | int PK | |
| ma_code_khoa | nvarchar(50) | UQ |
| ten_khoa | nvarchar(255) | |
| nam_bat_dau | int | |
| nam_ket_thuc_du_kien | int | |
| mo_ta | nvarchar(max) | |
| con_hoat_dong | bit | |

### ChuongTrinhDaoTao (Training Program)

| Column | Type | Notes |
|--------|------|-------|
| ma_chuong_trinh | int PK | |
| ma_chuyen_nganh | int FK | -> ChuyenNganh, Restrict |
| ma_khoa_tuyen_sinh | int FK | -> KhoaTuyenSinh, Restrict |
| ma_code_chuong_trinh | nvarchar(100) | UQ |
| ten_chuong_trinh | nvarchar(255) | |
| version | nvarchar(50) | |
| so_hoc_ky | int | CK: > 0 |
| thoi_gian_dao_tao_thang | int | CK: > 0 |
| tong_tin_chi_yeu_cau | int | |
| so_tin_chi_toi_thieu_moi_ky | int | |
| so_tin_chi_toi_da_moi_ky | int | |
| trang_thai | nvarchar(30) | CK: draft, pending_approval, approved, rejected, active, inactive, archived |
| mo_ta | nvarchar(max) | |
| nguon_chuong_trinh_id | int FK | -> ChuongTrinhDaoTao (self-ref), Restrict |
| ghi_chu_thay_doi | nvarchar(max) | |
| ngay_hieu_luc | date | |
| ngay_het_hieu_luc | date | |
| nguoi_gui_duyet_id | int FK | -> NguoiDung |
| thoi_gian_gui_duyet | datetime2 | |
| nguoi_duyet_id | int FK | -> NguoiDung |
| thoi_gian_duyet | datetime2 | |
| ghi_chu_duyet | nvarchar(max) | |
| nguoi_tu_choi_id | int FK | -> NguoiDung |
| thoi_gian_tu_choi | datetime2 | |
| ly_do_tu_choi | nvarchar(max) | |
| con_hoat_dong | bit | |
| ngay_tao | datetime2 | |
| ngay_cap_nhat | datetime2 | |

**UQ:** (ma_chuyen_nganh, ma_khoa_tuyen_sinh, version)

### ChuongTrinhHocKy

| Column | Type | Notes |
|--------|------|-------|
| ma_chuong_trinh_hoc_ky | int PK | |
| ma_chuong_trinh | int FK | -> ChuongTrinhDaoTao |
| ma_hoc_ky | int FK | -> HocKy |
| thu_tu_hoc_ky | int | CK: > 0 |

**UQ:** (ma_chuong_trinh, thu_tu_hoc_ky), (ma_chuong_trinh, ma_hoc_ky)

### MonHocTrongChuongTrinh (Subject in Program)

| Column | Type | Notes |
|--------|------|-------|
| ma_chuong_trinh_mon_hoc | int PK | |
| ma_chuong_trinh | int FK | -> ChuongTrinhDaoTao, Restrict |
| ma_mon_hoc | int FK | -> DanhMucMonHoc, Restrict |
| hoc_ky_du_kien | int | CK: > 0 |
| so_tin_chi | int | CK: > 0 |
| loai_mon_hoc | nvarchar(30) | CK: bat_buoc, tu_chon, thay_the |
| bat_buoc | bit | default true |
| thu_tu | int | |
| ghi_chu | nvarchar(max) | |
| con_hoat_dong | bit | |
| ngay_tao | datetime2 | |
| ngay_cap_nhat | datetime2 | |

**UQ:** (ma_chuong_trinh, ma_mon_hoc)

### MonHocTienQuyet (Prerequisite)

| Column | Type | Notes |
|--------|------|-------|
| ma_mon_hoc | int FK PK | -> DanhMucMonHoc |
| ma_mon_tien_quyet | int FK PK | -> DanhMucMonHoc |
| diem_toi_thieu | decimal(5,2) | CK: 0-10 |

### KhoaHoc (Course/Section)

| Column | Type | Notes |
|--------|------|-------|
| ma_khoa_hoc | int PK | |
| ma_don_vi | int FK | -> DonVi |
| ma_giao_vien | int FK | -> NguoiDung |
| ma_mon_hoc | int FK | -> DanhMucMonHoc |
| ma_hoc_ky | int FK | -> HocKy |
| ma_block_bat_dau | int FK | -> Block |
| ma_lop | int FK | -> LopHanhChinh |
| ma_lop_hoc_phan | int FK | -> LopHocPhan |
| tieu_de | nvarchar(255) | |
| mo_ta | nvarchar(max) | |
| trang_thai | nvarchar(20) | CK: nhap, da_xuat_ban, luu_tru |
| url_anh_bia | nvarchar(max) | |
| ngay_tao | datetime2 | |

**UQ:** (ma_don_vi, ma_mon_hoc, ma_hoc_ky, ma_lop)

### LopHanhChinh (Administrative Class)

| Column | Type | Notes |
|--------|------|-------|
| ma_lop | int PK | |
| ma_don_vi | int FK | -> DonVi |
| ma_code_lop | nvarchar(50) | UQ |
| ten_lop | nvarchar(255) | |
| ma_giao_vien_chu_nhiem | int FK | -> NguoiDung |
| ma_chuong_trinh | int FK | -> ChuongTrinhDaoTao |
| nam_nhap_hoc | int | |
| si_so_du_kien | int | |
| con_hoat_dong | bit | |

### LopHocPhan (Class Section)

| Column | Type | Notes |
|--------|------|-------|
| ma_lop_hoc_phan | int PK | |
| ma_don_vi | int FK | -> DonVi |
| ma_mon_hoc | int FK | -> DanhMucMonHoc |
| ma_hoc_ky | int FK | -> HocKy |
| ma_code_lop_hoc_phan | nvarchar(50) | UQ |
| suc_chua | int | CK: > 0 |
| so_dang_ky_toi_thieu | int | |
| so_da_dang_ky | int | default 0 |
| trang_thai | nvarchar(30) | CK: mo, dong, cho_huy, da_huy; default mo |
| quota_vang_toi_da | int | |

### GiaiDoanDangKy (Registration Period)

| Column | Type | Notes |
|--------|------|-------|
| ma_giai_doan_dk | int PK | |
| ma_don_vi | int FK | -> DonVi |
| ma_hoc_ky | int FK | -> HocKy |
| bat_dau_luc | datetime2 | |
| ket_thuc_luc | datetime2 | |
| trang_thai | nvarchar(20) | CK: nhap, dang_mo, da_dong |
| so_tin_chi_toi_da | int | |


---

## 5. COURSE and SYLLABUS

### DanhMucMonHoc (Subject Catalog)
- ma_mon_hoc (int PK)
- ma_code_mon_hoc (nvarchar(50) UQ)
- ten_mon_hoc (nvarchar(255))
- so_tin_chi (int CK >0)
- con_hoat_dong (bit)
- ma_nganh (int FK -> NganhDaoTao)
- ma_chuyen_nganh (int FK -> ChuyenNganh)

### CourseSyllabus / DeCuongMonHoc
- ma_syllabus (int PK)
- ma_mon_hoc (int FK -> DanhMucMonHoc)
- ma_chuyen_nganh (int FK -> ChuyenNganh)
- ma_don_vi (int FK -> DonVi)
- ma_chuong_trinh_mon_hoc (int FK -> MonHocTrongChuongTrinh)
- ten_syllabus (nvarchar(255))
- version (nvarchar(50))
- hoc_ky_du_kien (int)
- bat_buoc (bit)
- trang_thai (nvarchar(30) CK: draft/pending_approval/approved/active/inactive/archived)
- con_hoat_dong (bit)
- UQ: (ma_mon_hoc, ma_chuyen_nganh, ma_don_vi, version)

### Chuong (Chapter)
- ma_chuong (int PK)
- ma_mon_hoc (int FK -> DanhMucMonHoc)
- tieu_de (nvarchar(255))
- thu_tu (int default 0)
- da_an (bit default false)

---

## 6. SCHEDULE AND TIMETABLE (P12 Smart Timetable)

### CaHoc (Time Slot)
- ma_ca_hoc (int PK)
- ten_ca (nvarchar(50) UQ)
- buoi (nvarchar(20) CK: sang/chieu/toi)
- gio_bat_dau (time)
- gio_ket_thuc (time CK > gio_bat_dau)
- thu_tu (int CK >0)
- con_hoat_dong (bit default true)

### ThoiKhoaBieu (Schedule)
- ma_tkb (int PK)
- ma_khoa_hoc (int FK -> KhoaHoc)
- ma_phong (int FK -> PhongHoc)
- ma_ca_hoc (int FK -> CaHoc)
- thu_trong_tuan (int CK 1-7)
- ngay_bat_dau (date)
- ngay_ket_thuc (date)
- trang_thai (nvarchar(20) CK: nhap/da_xuat_ban/da_huy)
- UQ: (ma_khoa_hoc, thu_trong_tuan, ma_ca_hoc) filtered

### BuoiHoc (Session)
- ma_buoi_hoc (int PK)
- ma_tkb (int FK -> ThoiKhoaBieu)
- ma_khoa_hoc (int FK -> KhoaHoc)
- ngay_hoc (date)
- ma_ca_hoc (int FK -> CaHoc)
- ma_phong (int FK -> PhongHoc)
- ma_giao_vien (int FK -> NguoiDung)
- ma_giao_vien_day_thay (int FK -> NguoiDung)
- trang_thai_buoi (nvarchar(30) CK: du_kien/da_dien_ra/da_huy/doi_lich/day_thay)
- loai_thay_doi (nvarchar(30) CK: doi_giang_vien/doi_phong/doi_ca/huy_buoi/doi_lich)
- trang_thai_diem_danh (nvarchar(30) CK: chua_mo/dang_diem_danh/da_gui/da_khoa default chua_mo)
- UQ: (ma_tkb, ngay_hoc)
- IX: (ngay_hoc, ma_ca_hoc, ma_phong), (ngay_hoc, ma_ca_hoc, ma_giao_vien)

### ScheduleGenerationJob
- ma_job (int PK)
- draft_id (nvarchar UQ)
- ma_don_vi (int FK -> DonVi)
- ma_hoc_ky (int FK -> HocKy)
- nguoi_yeu_cau (int FK -> NguoiDung)
- trang_thai (nvarchar(30) CK: draft/da_xuat_ban)
- tong_course (int), so_xep_duoc (int), so_khong_xep_duoc (int)
- score (float), tom_tat_json (nvarchar(max))

### ScheduleDraftItem
- ma_draft_item (int PK)
- ma_job (int FK -> ScheduleGenerationJob Cascade)
- ma_khoa_hoc (int FK -> KhoaHoc)
- ma_giao_vien (int FK -> NguoiDung)
- muc_do_phu_hop (int)
- thu_trong_tuan (int CK 1-7)
- ma_ca_hoc (int FK -> CaHoc)
- ma_phong (int FK -> PhongHoc)
- trang_thai (nvarchar(30) CK: pending/xep_duoc/khong_xep_duoc)
- score (float), canh_bao_json, loi_json

### QuyDoiTinChi (Credit Conversion)
- ma_quy_doi (int PK)
- so_tin_chi (int UQ)
- so_block_hoc (int), so_buoi_moi_tuan (int), so_ca_moi_buoi (int)
- Seed: 2->1,2,1 | 3->1,3,1 | 4->2,2,1 | 5->2,3,1

### QuyDinhChuyenCan (Attendance Policy)
- ma_quy_dinh (int PK)
- ma_don_vi (int FK -> DonVi)
- ngay_hieu_luc (date)
- quy_vang_toi_da (int), ti_le_canh_bao (decimal 5,2)
- he_so_vang_khong_phep (decimal 5,2), he_so_vang_co_phep (decimal 5,2)
- he_so_di_muon (decimal 5,2)
- han_gui_phut (int), han_chinh_sua_phut (int)
- nguoi_tao (int FK), nguoi_cap_nhat (int FK)

---

## 7. ATTENDANCE

### DiemDanh
- ma_diem_danh (int PK)
- ma_don_vi (int FK -> DonVi)
- ma_buoi_hoc (int FK -> BuoiHoc)
- ma_hoc_sinh (int FK -> NguoiDung)
- trang_thai (nvarchar(20) CK: co_mat/vang/di_muon/co_phep)
- nguoi_ghi_nhan (int FK -> NguoiDung)
- ghi_nhan_luc (datetime2), khoa_luc (datetime2)
- he_so_vang (int), ma_yc_mo_khoa (int FK -> YeuCauMoKhoaDiemDanh)
- UQ: (ma_buoi_hoc, ma_hoc_sinh)

### YeuCauMoKhoaDiemDanh
- ma_yc_mo_khoa (int PK)
- ma_buoi_hoc (int FK -> BuoiHoc)
- nguoi_yeu_cau (int FK -> NguoiDung)
- ly_do (nvarchar(max))
- trang_thai (nvarchar(20) CK: cho_duyet/da_duyet/tu_choi/het_han)
- nguoi_duyet (int FK -> NguoiDung)
- UX filtered: ma_buoi_hoc WHERE trang_thai='cho_duyet'

---

## 8. GRADES

### DiemSo
- ma_diem_so (int PK)
- ma_don_vi (int FK -> DonVi)
- ma_hoc_sinh (int FK -> NguoiDung)
- ma_mon_hoc (int FK -> DanhMucMonHoc)
- ma_hoc_ky (int FK -> HocKy)
- diem_qua_trinh (decimal 5,2 CK 0-10)
- diem_giua_ky (decimal 5,2 CK 0-10)
- diem_cuoi_ky (decimal 5,2 CK 0-10)
- gpa_mon_hoc (decimal 5,2 CK 0-10 default 0)
- trang_thai (nvarchar(20) CK: dat/rot/chua_hoan_thanh/cho_hoan_thanh_bo_sung)
- da_khoa (bit), ly_do_rot (nvarchar(max) JSON)
- nam_nhap_hoc (int)
- UQ: (ma_hoc_sinh, ma_mon_hoc, ma_hoc_ky)

### CauHinhDiemMonHoc
- ma_cau_hinh_diem (int PK)
- ma_mon_hoc (int FK), ma_hoc_ky (int FK)
- trong_so_qua_trinh (decimal 5,2), trong_so_giua_ky (decimal 5,2), trong_so_cuoi_ky (decimal 5,2)
- nguong_dat (decimal 5,2 default 5)
- ti_le_chuyen_can_toi_thieu (decimal 5,2 default 0)
- nguoi_cap_nhat (int FK -> NguoiDung)

### NhatKyThayDoiDiem
- ma_nk_thay_doi (int PK)
- ma_diem_so (int FK -> DiemSo)
- nguoi_thay_doi (int FK -> NguoiDung)
- gia_tri_cu (nvarchar(max) JSON), gia_tri_moi (nvarchar(max) JSON)
- ly_do (nvarchar(max)), nguoi_duyet (int FK)
- thay_doi_luc (datetime2)

### YeuCauSuaDiem
- ma_yc_sua_diem (int PK)
- ma_diem_so (int FK -> DiemSo)
- nguoi_yeu_cau (int FK -> NguoiDung)
- ly_do (nvarchar(max)), url_bang_chung (nvarchar(max))
- trang_thai (nvarchar(20) CK: cho_duyet/da_duyet/tu_choi/het_han)
- nguoi_duyet (int FK), mo_den_luc (datetime2)
- loai_yeu_cau (nvarchar(30) default sua_sau_submit)
- unlock_expires_at (datetime2), cot_diem_duoc_mo (nvarchar(30))

---

## 9. ASSIGNMENTS AND LESSONS

### BaiTap (Assignment)
- ma_bai_tap (int PK)
- ma_mon_hoc (int FK -> DanhMucMonHoc)
- tieu_de (nvarchar(255)), mo_ta (nvarchar(max))
- han_nop (datetime2), so_lan_nop_toi_da (int default 3)
- dinh_dang_cho_phep (nvarchar(200) JSON)
- huong_dan_cham_diem (nvarchar(max))
- trang_thai (nvarchar(20) CK: nhap/da_xuat_ban/da_dong)
- ma_cau_hinh_dau_diem (int FK -> CauHinhDauDiemQuaTrinh)

### BaiNop (Submission)
- ma_bai_nop (int PK)
- ma_bai_tap (int FK -> BaiTap)
- ma_hoc_sinh (int FK -> NguoiDung)
- url_tap_tin (nvarchar(max))
- so_lan_nop (int CK >0), nop_tre (bit)
- diem_dao_van (decimal 5,2 CK 0-100)
- diem_so (decimal 5,2 CK 0-10)
- diem_ai_de_xuat (decimal 5,2 CK 0-10)
- nhan_xet (nvarchar(max)), thoi_diem_nop (datetime2)
- da_cong_bo (bit)
- UQ: (ma_bai_tap, ma_hoc_sinh, so_lan_nop)

### CanhBaoDaoVan (Plagiarism Warning)
- ma_canh_bao (int PK)
- ma_bai_nop (int FK -> BaiNop)
- diem_dao_van (decimal 5,2 CK 0-100)
- chi_tiet (nvarchar(max) JSON)

### BaiHocNoiDung (Lesson Content)
- ma_noi_dung (int PK)
- ma_bai_hoc (int FK -> BaiHoc Cascade)
- loai_noi_dung (nvarchar(20) CK: video/slide_html/tai_lieu/quiz/van_ban)
- noi_dung_html (nvarchar(max)), noi_dung_json (nvarchar(max) JSON)
- url_tap_tin (nvarchar(max)), storage_key (nvarchar(500))
- kich_thuoc_byte (int), thoi_luong_giay (int)
- trang_thai (nvarchar(20) CK: nhap/da_xuat_ban)
- thu_tu (int default 0)
- ma_de_kiem_tra (int FK -> DeKiemTra)

### TienDoBaiHoc (Lesson Progress)
- ma_tien_do (int PK)
- ma_hoc_sinh (int FK -> NguoiDung)
- ma_bai_hoc (int FK -> BaiHoc)
- phan_tram_tien_do (decimal 5,2 CK 0-100)
- UQ: (ma_hoc_sinh, ma_bai_hoc)

---

## 10. EXAMINATIONS

### DeKiemTra (Exam)
- ma_de_kiem_tra (int PK)
- ma_mon_hoc (int FK), ma_hoc_ky (int FK)
- tieu_de (nvarchar(255))
- thoi_gian_phut (int CK 1-240)
- cau_hinh_de_thi (nvarchar(max) JSON)
- trang_thai (nvarchar(20) CK: nhap/da_len_lich/dang_mo/da_dong/da_cong_bo)
- loai_de_thi (nvarchar(50) CK: trac_nghiem/tu_luan/ket_hop/quiz_bai_hoc/progress_test)
- hinh_thuc_thi (nvarchar(50) CK: online_tap_trung/online_tu_do/van_dap)
- ty_le_trac_nghiem (decimal 5,2), ty_le_tu_luan (decimal 5,2)
- ma_nguoi_soan (int FK), ma_nguoi_duyet (int FK)
- trang_thai_duyet (nvarchar(20) CK: nhap/cho_duyet/da_duyet/tu_choi)

### CauHoi (Question)
- ma_cau_hoi (int PK)
- ma_mon_hoc (int FK), nguoi_tao (int FK)
- loai_cau_hoi (nvarchar(20) CK: trac_nghiem/tu_luan)
- noi_dung (nvarchar(max))
- kieu_lua_chon (nvarchar(20) CK: chon_mot/chon_nhieu)
- lua_chon (nvarchar(max) JSON), dap_an_dung (nvarchar(max) JSON)
- do_kho (nvarchar(10) CK: de/trung_binh/kho)
- con_hoat_dong (bit)

### CauHoiDeKiemTra (Question-Exam)
- composite PK: (ma_de_kiem_tra, ma_cau_hoi)
- diem_so (decimal 5,2 default 1)
- thu_tu (int)

### KyThi (Exam Period)
- ma_ky_thi (int PK)
- ten_ky_thi (nvarchar(255))
- ma_hoc_ky (int FK), ma_nganh (int FK)
- loai_ky_thi (nvarchar(20) CK: giua_ky/cuoi_ky default cuoi_ky)
- trang_thai (nvarchar(30) CK: nhap/dang_dien_ra/da_ket_thuc)

### LichThiTong (Exam Schedule)
- ma_lich_thi_tong (int PK)
- ma_ky_thi (int FK -> KyThi)
- ma_mon_hoc (int FK), ma_de_kiem_tra (int FK)
- hinh_thuc_thi (nvarchar(30) CK: online_tap_trung/online_tu_do/van_dap)
- ngay_thi_du_kien (datetime2)
- trang_thai (nvarchar(30) CK: nhap/da_gui_ve_co_so/da_huy)

### CaThi (Exam Session)
- ma_ca_thi (int PK)
- ma_lich_thi_tong (int FK -> LichThiTong)
- ten_ca_thi (nvarchar(255))
- ma_phong (int FK -> PhongHoc)
- thoi_gian_bat_dau (datetime2), thoi_gian_ket_thuc (datetime2)
- ma_don_vi (int FK)
- trang_thai (nvarchar(30) CK: nhap/cho_phan_cong/da_san_sang/dang_diem_danh/dang_thi/da_ket_thuc/da_huy/su_co)
- ghi_chu (nvarchar(max)), ly_do_dieu_chinh (nvarchar(max))

### ThiSinhCaThi (Exam Candidate)
- ma_thi_sinh_ca_thi (int PK)
- ma_ca_thi (int FK -> CaThi)
- ma_hoc_sinh (int FK -> NguoiDung)
- trang_thai_du_thi (nvarchar(30) CK: cho_thi/duoc_thi/khong_duoc_thi/dinh_chi/vang_thi)
- UQ: (ma_ca_thi, ma_hoc_sinh)

### DiemDanhThi (Exam Attendance)
- ma_diem_danh_thi (int PK)
- ma_ca_thi (int FK), ma_hoc_sinh (int FK)
- trang_thai_diem_danh (nvarchar(30) CK: co_mat/vang_mat/di_muon_qua_gio/su_co)
- thoi_diem_diem_danh (datetime2)
- ma_nguoi_diem_danh (int FK -> NguoiDung)
- UQ: (ma_ca_thi, ma_hoc_sinh)

### PhanCongGiamThi (Proctor Assignment)
- ma_phan_cong (int PK)
- ma_ca_thi (int FK -> CaThi)
- ma_giam_thi (int FK -> NguoiDung)
- vai_tro_giam_thi (nvarchar(30) CK: giam_thi_chinh/giam_thi_phu/ho_tro_ky_thuat)
- trang_thai (nvarchar(30) CK: du_kien/da_xac_nhan/thay_the/huy_phan_cong)
- UQ: (ma_ca_thi, ma_giam_thi)

### NhatKyViPhamThi (Exam Violation)
- ma_vi_pham (int PK)
- ma_phien_thi (int FK -> PhienThiHocSinh)
- ma_hoc_sinh (int FK), ma_ca_thi (int FK)
- loai_vi_pham (nvarchar(30) CK: chuyen_tab/mat_focus/mat_camera/tieng_on/khac)
- muc_do (nvarchar(20) CK: nhac_nho/nghiem_trong)
- chi_tiet_json (nvarchar(max) JSON), thoi_diem (datetime2)

### XuLyViPhamThi (Violation Handling)
- ma_xu_ly (int PK)
- ma_vi_pham (int FK -> NhatKyViPhamThi)
- hanh_dong_xu_ly (nvarchar(30) CK: nhac_nho_he_thong/canh_bao_truc_tiep/dinh_chi_thi/bo_qua)
- lan_nhac_nho (int), ma_nguoi_xu_ly (int FK)
- thoi_diem (datetime2), ly_do, ghi_chu

### BienBanThi (Exam Minutes)
- ma_bien_ban (int PK)
- ma_ca_thi (int FK), ma_phien_thi (int FK -> PhienThiHocSinh)
- loai_bien_ban (nvarchar(30) CK: gian_lan/su_co_diem_danh/quen_ky_ten/su_co_he_thong/khac)
- noi_dung (nvarchar(max))
- ma_nguoi_lap (int FK -> NguoiDung)
- thoi_diem_lap (datetime2)
- trang_thai_xu_ly (nvarchar(20) CK: cho_xu_ly/da_xu_ly/huy_bo)

### PhienThiHocSinh (Exam Session Student)
- ma_phien_thi (int PK)
- ma_de_kiem_tra (int FK), ma_hoc_sinh (int FK)
- bat_dau_luc (datetime2), nop_luc (datetime2)
- cau_tra_loi_json (JSON), nhat_ky_vi_pham (JSON), sao_luu_cuc_bo (JSON)
- trang_thai_luong (nvarchar(30) CK: dang_hoat_dong/bi_gian_doan/da_dung)
- diem_tu_dong (decimal 5,2 CK 0-10), diem_cuoi_cung (decimal 5,2 CK 0-10)
- diem_tu_luan_ai_goi_y (decimal 5,2 CK 0-10)
- lan_thu (int default 1 CK >0)
- ma_ca_thi (int FK -> CaThi)
- trang_thai_ky_ten (nvarchar(20) CK: chua_ky/da_ky/quen_ky/su_co)
- trang_thai_cong_bo (nvarchar(30) CK: chua_co_diem/da_cham_xong/da_doc_diem/da_cong_bo)
- UQ: (ma_de_kiem_tra, ma_hoc_sinh, lan_thu)
- UQ: (ma_ca_thi, ma_hoc_sinh) filtered

---

## 11. APPLICATION FORMS (Don Tu)

### MauDonTu (Form Template)
- ma_mau_don (int PK)
- loai_don (nvarchar(50)), ten_mau (nvarchar(200))
- phien_ban (int CK >0)
- cau_hinh_json (nvarchar(max) JSON)
- bat_buoc_minh_chung (bit)
- so_tep_toi_da (int default 5 CK 0-5)
- dung_luong_tep_toi_da_byte (int CK >0)
- tong_dung_luong_toi_da_byte (int CK >= dung_luong)
- sla_gio (int)
- dang_hoat_dong (bit)
- UQ: (loai_don, phien_ban)
- UX filtered: loai_don WHERE dang_hoat_dong=1

### DonTu (Application Form)
- ma_don_tu (int PK)
- ma_don_vi (int FK -> DonVi)
- ma_hoc_sinh (int FK -> NguoiDung)
- ma_mau_don (int FK -> MauDonTu)
- loai_don (nvarchar(50) CK: nghi_phep/thi_lai/chuyen_truong/cap_chung_chi/khac/phuc_tra_diem/bao_luu/chuyen_nganh/chuyen_co_so/xac_nhan/rut_hoc)
- tieu_de (nvarchar(255))
- trang_thai (nvarchar(30) CK: nhap/da_nop/dang_xem_xet/yeu_cau_bo_sung/da_duyet/tu_choi/da_huy)
- trang_thai_xu_ly_nghiep_vu (nvarchar(50) CK: chua_xu_ly/cho_xu_ly/da_ghi_nhan/xu_ly_thanh_cong/xu_ly_that_bai/can_xu_ly_thu_cong)
- nguoi_duyet_hien_tai (int FK), nguoi_xu_ly_cuoi (int FK)
- du_lieu_bieu_mau (JSON), url_bang_chung (nvarchar(max))
- ly_do_tu_choi (nvarchar(max))
- ket_qua_xu_ly_json (JSON), nhat_ky_tu_dong (JSON)
- row_version (rowversion concurrency)

### NhatKyDuyetDon (Approval Log)
- ma_nk_duyet (int PK)
- ma_don_tu (int FK -> DonTu)
- ma_nguoi_duyet (int FK -> NguoiDung)
- nguon_thuc_hien (nvarchar(20) CK: user/system)
- hanh_dong (nvarchar(50) CK: tao_nhap/cap_nhat/nop/nop_lai/phan_cong/tiep_nhan/yeu_cau_bo_sung/bo_sung/phe_duyet/tu_choi/leo_thang/huy/xu_ly_nghiep_vu)
- trang_thai_cu (nvarchar(30)), trang_thai_moi (nvarchar(30))
- ghi_chu (nvarchar(max)), ghi_chu_cong_khai, ghi_chu_noi_bo
- snapshot_json (JSON)

### TepDinhKemDonTu (Attachment)
- ma_tep (int PK)
- ma_don_tu (int FK -> DonTu)
- storage_key (nvarchar(500) UQ)
- ten_file_goc (nvarchar(255)), ten_file_luu (nvarchar(255))
- content_type (nvarchar(150)), kich_thuoc_byte (int)
- file_hash (nvarchar(128))
- nguoi_tai_len (int FK -> NguoiDung)
- da_xoa (bit default false)

---

## 12. FINANCE AND TUITION

### HoaDon (Invoice)
- ma_hoa_don (int PK)
- ma_don_vi (int FK -> DonVi)
- ma_hoc_sinh (int FK -> NguoiDung)
- ma_hoc_ky (int FK -> HocKy)
- ma_hoa_don_code (nvarchar(50) UQ)
- loai_hoa_don (nvarchar(30) CK: hoc_phi/le_phi/tai_lieu/khac default hoc_phi)
- so_tien (decimal 15,2 CK >=0)
- giam_tru (decimal 15,2 CK >=0 default 0)
- da_thanh_toan (decimal 15,2 CK >=0 default 0)
- trang_thai (nvarchar(30) CK: chua_thanh_toan/thanh_toan_mot_phan/da_thanh_toan/qua_han/da_huy)
- han_thanh_toan (date)
- url_hoa_don_pdf (nvarchar(max))
- ghi_chu, ly_do_huy (nvarchar(max))
- nguoi_tao, nguoi_cap_nhat, nguoi_huy (int FK)

### GiaoDich (Transaction)
- ma_giao_dich (int PK)
- ma_hoa_don (int FK -> HoaDon)
- ma_tai_khoan_nhan_tien (int FK -> TaiKhoanNhanTien)
- ma_tham_chieu_noi_bo (nvarchar(100) UQ)
- ma_tham_chieu_cong (nvarchar(100) UQ filtered)
- so_tien (decimal 15,2)
- loai_giao_dich (nvarchar(50) CK: phat_sinh_hoc_phi/thanh_toan_hoc_phi/dieu_chinh_cong_no/hoan_tien/huy_hoa_don)
- trang_thai (nvarchar(50) CK: phat_sinh/cho_thanh_toan/dang_xu_ly/thanh_cong/that_bai/het_han/da_huy/sai_so_tien/cho_xu_ly_thu_cong)
- nha_cung_cap_thanh_toan (nvarchar(30) CK: payos/vietqr)
- qr_payload (nvarchar(max)), qr_url (nvarchar(max)), checkout_url (nvarchar(max))
- request_payload_json, response_payload_json, callback_payload_json (all JSON)
- ngay_tao, ngay_cap_nhat, ngay_het_han, ngay_thanh_toan (datetime2)
- ma_nguoi_thuc_hien (int FK -> NguoiDung)

### TaiKhoanNhanTien (Payment Account)
- ma_tai_khoan_nhan_tien (int PK)
- ma_don_vi (int FK -> DonVi)
- ten_ngan_hang (nvarchar(100)), ma_ngan_hang (nvarchar(30))
- so_tai_khoan (nvarchar(50)), ten_chu_tai_khoan (nvarchar(255))
- chi_nhanh (nvarchar(255))
- nha_cung_cap_thanh_toan (nvarchar(30) CK: payos/vietqr default payos)
- trang_thai_duyet (nvarchar(30) CK: nhap/cho_duyet/da_duyet/tu_choi/ngung_hoat_dong)
- cau_hinh_provider_json (JSON)
- la_mac_dinh (bit default false)
- con_hoat_dong (bit default false)
- UX filtered: DonVi WHERE la_mac_dinh=1 AND con_hoat_dong=1

### CauHinhHocPhiChuongTrinh (Tuition Config)
- ma_cau_hinh_hoc_phi (int PK)
- ma_don_vi (int FK), ma_chuong_trinh_dao_tao (int FK), ma_hoc_ky (int FK)
- nam_hoc_trong_chuong_trinh (int CK >=1)
- hoc_ky_trong_nam (int CK: 1/2/3)
- so_thu_tu_hoc_ky (int CK >=1)
- loai_cach_tinh_hoc_phi (nvarchar(30) CK: co_dinh_theo_hoc_ky/theo_tin_chi/theo_mon_hoc)
- so_tien_hoc_phi (decimal 15,2 CK >=0)
- tien_hoc_lieu (decimal 15,2 CK >=0 default 0)
- tong_tien_du_kien (decimal 15,2 CK = so_tien + hoc_lieu)
- UQ filtered: (ma_don_vi, ma_chuong_trinh, ma_hoc_ky) WHERE con_hoat_dong=1

### YeuCauHoanPhi (Refund Request)
- ma_hoan_phi (int PK)
- ma_hoa_don (int FK -> HoaDon)
- ma_hoc_sinh (int FK -> NguoiDung)
- ma_don_vi (int FK -> DonVi)
- so_tien_yeu_cau (decimal 15,2 CK >=0)
- loai_hoan_phi (nvarchar(20) CK: toan_phan/mot_phan/ghi_co)
- trang_thai (nvarchar(20) CK: cho_duyet/da_duyet/tu_choi/da_xu_ly)
- ly_do_yeu_cau, ly_do_tu_choi, ghi_chu (nvarchar(max))
- nguoi_tao, nguoi_cap_nhat, nguoi_duyet (int FK)

---

## 13. REWARD AND DISCIPLINE

### KhenThuong (Reward)
- ma_khen_thuong (int PK)
- ma_don_vi (int FK), ma_hoc_sinh (int FK -> NguoiDung), ma_hoc_ky (int FK)
- ma_dot_khen_thuong (int FK -> DotKhenThuong)
- ma_mau_bang_khen (int FK -> MauBangKhen)
- loai_khen_thuong (nvarchar(50) CK: hoc_luc/dac_biet/thi_dau/TOP_100_HOC_KY/KHAC)
- trang_thai (nvarchar(30) CK: nhap/cho_duyet/da_duyet/da_cap/da_sinh_pdf/loi_sinh_pdf/da_huy)
- gpa_dat_duoc (decimal 5,2 CK 0-10)
- diem_xet (decimal 10,4), xep_hang (int)
- url_chung_tu (nvarchar(max))
- url_pdf_bang_khen (nvarchar(1000))
- ho_ten_snapshot, mssv_snapshot, ten_hoc_ky_snapshot, danh_hieu_snapshot

### DotKhenThuong (Reward Campaign)
- ma_dot_khen_thuong (int PK)
- ma_hoc_ky (int FK), ma_don_vi (int FK)
- ten_dot (nvarchar(255))
- loai_dot (nvarchar(50) CK: TOP_100_HOC_KY)
- so_luong_toi_da (int default 100)
- tieu_chi_xet_json (JSON)
- trang_thai (nvarchar(30) CK: nhap/dang_xet/cho_duyet/da_duyet/da_cong_bo/da_huy)
- UX filtered: (ma_hoc_ky, ma_don_vi, loai_dot) WHERE trang_thai <> 'da_huy'

### MauBangKhen (Certificate Template)
- ma_mau_bang_khen (int PK)
- ten_mau (nvarchar(200))
- loai_mau (nvarchar(50) CK: TOP_100_HOC_KY)
- file_nen_url (nvarchar(1000))
- chieu_rong (int CK >0), chieu_cao (int CK >0)
- huong_giay (nvarchar(20) CK: A4_NGANG/A4_DOC)
- cau_hinh_json (JSON)
- con_hoat_dong (bit)

### CauHinhKhenThuong
- ma_cau_hinh_kt (int PK)
- ma_don_vi (int FK -> DonVi)
- loai_khen_thuong (nvarchar(30))
- gpa_toi_thieu (decimal 5,2 CK 0-10)
- con_hoat_dong (bit)

### HoSoKyLuat (Discipline Record)
- ma_ky_luat (int PK)
- ma_hoc_sinh (int FK), ma_don_vi (int FK), ma_hoc_ky (int FK)
- tieu_de (nvarchar(255))
- loai_ky_luat (nvarchar(50))
- muc_do_vi_pham (nvarchar(30) CK: nhe/trung_binh/nghiem_trong)
- hinh_thuc_xu_ly (nvarchar(30) CK: nhac_nho/khien_trach/canh_cao/dinh_chi/khac)
- trang_thai (nvarchar(30) CK: nhap/cho_duyet/da_duyet/tu_choi/dang_hieu_luc/het_hieu_luc/da_go_hieu_luc/da_huy)
- mo_ta (nvarchar(max)), can_cu_xu_ly (nvarchar(2000))
- ngay_vi_pham (date), ngay_hieu_luc (date), ngay_het_hieu_luc (date)
- da_go_ky_luat (bit), ly_do_go_ky_luat (nvarchar(max))
- nguoi_tao, nguoi_duyet, nguoi_huy, nguoi_go_ky_luat, nguoi_ap_dung (int FK)

### KhieuNaiKyLuat (Discipline Appeal)
- ma_khieu_nai_ky_luat (int PK)
- ma_ho_so_ky_luat (int FK -> HoSoKyLuat Cascade)
- ma_hoc_sinh (int FK), ma_don_vi (int FK)
- ly_do_khieu_nai (nvarchar(2000))
- chung_tu_json (JSON)
- trang_thai (nvarchar(50))
- nguoi_xu_ly (int FK), ngay_xu_ly (datetime2)

---

## 14. NOTIFICATIONS

### ThongBao (Notification)
- ma_thong_bao (int PK)
- ma_nhom_thong_bao (uniqueidentifier default NEWID())
- ma_nguoi_nhan (int FK -> NguoiDung)
- ma_don_vi (int FK -> DonVi)
- loai_su_kien (nvarchar(100))
- loai_thong_bao (nvarchar(100) CK: thong_bao_chung/hoc_phi/bao_tri/co_so_vat_chat/hoc_vu/khan_cap/system/manual/schedule_changed/session_cancelled/...)
- tieu_de (nvarchar(500)), tom_tat (nvarchar(1000))
- noi_dung (nvarchar(max)), noi_dung_json (JSON)
- muc_do (nvarchar(30) CK: thong_tin/quan_trong/khan_cap/info/warning/important)
- doi_tuong_lien_ket, loai_doi_tuong_lien_ket, ma_doi_tuong_lien_ket
- pham_vi_gui (nvarchar(50) CK: toan_he_thong/don_vi/lop_hanh_chinh/vai_tro/nguoi_dung/khoa_hoc/users/class/course/campus)
- duong_dan (nvarchar(500))
- nguoi_tao (int FK), trang_thai (nvarchar(30) CK: nhap/da_gui/da_huy)
- da_doc (bit default false), doc_luc (datetime2)

### ThongBaoNguoiNhan
- ma_thong_bao_nguoi_nhan (int PK)
- ma_thong_bao (int FK -> ThongBao Cascade)
- ma_nguoi_nhan (int FK -> NguoiDung)
- ma_don_vi (int FK)
- da_doc (bit default false), doc_luc (datetime2)
- da_an (bit default false), an_luc (datetime2)
- nhan_luc (datetime2)
- UQ: (ma_thong_bao, ma_nguoi_nhan)

### ThongBaoHenGio (Scheduled Notification)
- ma_tb_hen_gio (int PK)
- ma_don_vi (int FK), nguoi_tao (int FK)
- loai_su_kien (nvarchar(100))
- bo_loc_nguoi_nhan (nvarchar(max) JSON)
- gui_luc (datetime2)
- trang_thai (nvarchar(20) CK: da_len_lich/dang_cho/da_huy/hoan_thanh)

### MauThongBao (Notification Template)
- ma_mau_tb (int PK)
- loai_su_kien (nvarchar(100)), kenh_gui (nvarchar(20) CK: email/thong_bao_day/sms/in_app)
- mau_tieu_de (nvarchar(500)), mau_noi_dung (nvarchar(max))
- ma_don_vi (int FK), ten_mau (nvarchar(200))
- ma_mau (nvarchar(100)), loai_thong_bao (nvarchar(100))
- muc_do_uu_tien (nvarchar(50)), doi_tuong_mac_dinh (nvarchar(100))
- bien_cho_phep_json (JSON)
- dang_hoat_dong (bit default true)
- la_he_thong (bit default false)
- UQ: (loai_su_kien, kenh_gui)

### NhatKyThongBao (Notification Log)
- ma_nk_thong_bao (int PK)
- ma_thong_bao (int FK -> ThongBao)
- ma_nguoi_nhan (int FK -> NguoiDung)
- ma_don_vi (int FK)
- trang_thai (nvarchar(20) CK: cho_gui/da_gui/da_nhan/that_bai/bo_qua)
- kenh_gui (nvarchar(20) CK: email/thong_bao_day/sms)
- gui_luc (datetime2)

### TuyChonThongBao (Notification Preferences)
- ma_nguoi_dung (int PK, FK -> NguoiDung, ValueGeneratedNever)
- nhan_email (bit default true)
- nhan_push (bit default true)
- nhan_sms (bit default false)
- cap_nhat_luc (datetime2)

---

## 15. SUPPORT

### PhieuHoTro (Support Ticket)
- ma_phieu_ht (int PK)
- ma_hoc_sinh (int FK -> NguoiDung)
- danh_muc (nvarchar(30) CK: ky_thuat/hoc_vu/tai_chinh/khac)
- tieu_de (nvarchar(255)), mo_ta (nvarchar(max))
- trang_thai (nvarchar(20) CK: mo/dang_xu_ly/da_giai_quyet/da_dong)
- phan_cong_cho (int FK -> NguoiDung)
- han_xu_ly (datetime2)
- danh_gia_hai_long (int CK 1-5)
- do_uu_tien (nvarchar(20) default medium)

### TinNhanHoTro (Support Message)
- ma_tin_nhan_ht (int PK)
- ma_phieu_ht (int FK -> PhieuHoTro)
- ma_nguoi_gui (int FK -> NguoiDung)
- noi_dung (nvarchar(max))
- url_dinh_kem (nvarchar(max))

### CauHoiThuongGap (FAQ)
- ma_cau_hoi_faq (int PK)
- danh_muc (nvarchar(30))
- cau_hoi (nvarchar(500))
- tra_loi (nvarchar(max))
- con_hoat_dong (bit)

---

## 16. FACILITIES

### ToaNha (Building)
- ma_toa_nha (int PK)
- ma_don_vi (int FK -> DonVi)
- ma_code_toa_nha (nvarchar(50) UQ per DonVi)
- ten_toa_nha (nvarchar(255))
- dia_chi (nvarchar(500)), so_tang (int CK >0)
- con_hoat_dong (bit)

### Tang (Floor)
- ma_tang (int PK)
- ma_toa_nha (int FK -> ToaNha)
- ten_tang (nvarchar(100))
- thu_tu_tang (int UQ per ToaNha)
- mo_ta (nvarchar(max))
- con_hoat_dong (bit)

### PhongHoc (Room)
- ma_phong (int PK)
- ma_don_vi (int FK), ma_toa_nha (int FK), ma_tang (int FK)
- ma_code_phong (nvarchar(50) UQ per DonVi)
- ten_phong (nvarchar(255))
- suc_chua (int CK >0)
- loai_phong (nvarchar(30) CK: ly_thuyet/phong_thi_nghiem/thuc_hanh/lab/hoi_truong/truc_tuyen/khac)
- trang_thai_phong (nvarchar(20) CK: hoat_dong/bao_tri/ngung_hoat_dong)
- ghi_chu (nvarchar(max))

### ThietBiPhong (Room Equipment)
- ma_thiet_bi (int PK)
- ma_phong (int FK -> PhongHoc)
- ten_thiet_bi (nvarchar(255))
- ma_code_thiet_bi (nvarchar(100))
- chung_loai (nvarchar(100))
- tinh_trang (nvarchar(50))
- ngay_kiem_dinh (date)
- ghi_chu (nvarchar(500))
- so_luong (int default 1 CK >=0)

### DatPhong (Room Booking)
- ma_dat_phong (int PK)
- ma_phong (int FK -> PhongHoc)
- ma_don_vi (int FK -> DonVi)
- nguoi_yeu_cau (int FK -> NguoiDung)
- muc_dich (nvarchar(500))
- bat_dau_luc (datetime2), ket_thuc_luc (datetime2 CK > bat_dau)
- so_nguoi_tham_du (int CK >=0)
- trang_thai (nvarchar(30) CK: cho_duyet/da_xac_nhan/tu_choi/da_huy)
- nguoi_duyet (int FK -> NguoiDung)

### BaoCaoSuDungPhong (Room Usage Report)
- ma_bc_su_dung_phong (int PK)
- ma_phong (int FK -> PhongHoc)
- ma_don_vi (int FK -> DonVi)
- tu_ngay (date), den_ngay (date)
- so_gio_su_dung (decimal 10,2)
- ti_le_su_dung (decimal 5,2 CK 0-100)

---

## 17. AI AND ANALYTICS

### BaoCaoRuiRoRotMon (Drop Risk Report)
- ma_bao_cao_rot (int PK)
- ma_hoc_sinh (int FK), ma_mon_hoc (int FK), ma_hoc_ky (int FK)
- xac_suat_rot_mon (decimal 5,2 CK 0-1)
- dac_trung_json (JSON)

### BaoCaoRuiRoVang (Absence Risk Report)
- ma_bao_cao (int PK)
- ma_hoc_sinh (int FK), ma_mon_hoc (int FK)
- diem_rui_ro (decimal 5,2 CK 0-1)
- dac_trung_json (JSON)

### DanhSachRuiRoRotMon
- ma_rui_ro_rot (int PK)
- ma_hoc_sinh (int FK), ma_mon_hoc (int FK), ma_hoc_ky (int FK)
- xac_suat_rot_mon (decimal 5,2 CK 0-1)

### CanhBaoBaoMat (Security Alert)
- ma_canh_bao (int PK)
- ma_nguoi_dung (int FK -> NguoiDung)
- diem_rui_ro (decimal 5,2 CK 0-1)
- dia_chi_ip (nvarchar(45))
- thong_tin_trinh_duyet (nvarchar(500))
- trang_thai (nvarchar(20) CK: mo/da_xem/bo_qua)

---

## 18. EVALUATION

### DanhGiaGiaoVien (Teacher Evaluation)
- ma_danh_gia (int PK)
- ma_giao_vien (int FK), ma_hoc_ky (int FK)
- ma_cau_hoi_dg (int FK -> CauHoiDanhGia)
- diem_so (int CK 1-5)
- nhan_xet_tu_do (nvarchar(max))
- ai_cam_xuc (nvarchar(20) CK: tich_cuc/trung_tinh/tieu_cuc)
- ai_chu_de (nvarchar(max) JSON)
- cohort_hash (nvarchar(128))

### CauHoiDanhGia
- ma_cau_hoi_dg (int PK)
- noi_dung_cau_hoi (nvarchar(500))
- con_hoat_dong (bit)

### NopBaiDanhGia
- ma_nop_dg (int PK)
- ma_hoc_sinh (int FK), ma_giao_vien (int FK), ma_hoc_ky (int FK)
- so_lan_nop (int default 1 CK 0-2)
- UQ: (ma_hoc_sinh, ma_giao_vien, ma_hoc_ky)

### MauDanhGia (Evaluation Template)
- ma_mau_danh_gia (int PK)
- ten_mau (nvarchar(200))
- cau_hinh_json (nvarchar(max) JSON)
- dang_hoat_dong (bit)

---

## 19. OTHER

### XuatBaoCao (Report Export)
- ma_xuat_bao_cao (int PK)
- nguoi_yeu_cau (int FK), ma_don_vi (int FK)
- loai_bao_cao (nvarchar(50))
- tham_so_json (JSON)
- url_tap_tin (nvarchar(max))
- trang_thai (nvarchar(20) CK: cho_xu_ly/dang_xu_ly/hoan_thanh/that_bai)

### NhatKyKiemToan (Audit Log)
- ma_kiem_toan (int PK)
- ma_don_vi (int FK)
- loai_doi_tuong (nvarchar(100))
- ma_doi_tuong (nvarchar(100))
- hanh_dong (nvarchar(50))
- gia_tri_cu (JSON), gia_tri_moi (JSON)
- nguoi_thay_doi (int FK)
- thoi_diem_thay_doi (datetime2)
- dia_chi_ip (nvarchar(45)), user_agent (nvarchar(512))
- trace_id (nvarchar(100))

### BinhLuan (Comment)
- ma_binh_luan (int PK)
- ma_bai_hoc (int FK -> BaiHoc)
- ma_nguoi_dung (int FK -> NguoiDung)
- noi_dung (nvarchar(max))
- giay_trong_video (int), so_trang_pdf (int)
- ma_binh_luan_cha (int FK self-ref)
- da_ghim (bit default false)

### LienKetPhuHuynh (Parent-Student Link)
- ma_lien_ket_ph (int PK)
- ma_phu_huynh (int FK -> NguoiDung)
- ma_hoc_sinh (int FK -> NguoiDung)
- quyen_xem (nvarchar(max) JSON)
- trang_thai (nvarchar(20) CK: cho_duyet/hoat_dong/da_thu_hoi)
- UQ: (ma_phu_huynh, ma_hoc_sinh)

### QuyDinhChuyenCan
(Covered in Section 6)

---

## RELATIONSHIP HIERARCHY (Text Diagram)

DonVi (root/co_so/co_so_con)
  +-- NguoiDung
  +-- HocKy
  +-- ToaNha -> Tang -> PhongHoc -> ThietBiPhong
  +-- NganhDaoTao
       +-- ChuyenNganh -> ChuyenNganhTheoCoSo
       +-- DanhMucMonHoc -> MonHocTienQuyet
            +-- KhoaHoc -> ThoiKhoaBieu -> BuoiHoc
            +-- DeKiemTra -> CauHoiDeKiemTra
            +-- BaiTap -> BaiNop -> CanhBaoDaoVan
       +-- ChuongTrinhDaoTao -> ChuongTrinhHocKy
            +-- MonHocTrongChuongTrinh
            +-- LopHanhChinh

BuoiHoc -> DiemDanh -> YeuCauMoKhoaDiemDanh
DiemSo -> NhatKyThayDoiDiem, YeuCauSuaDiem

DeKiemTra -> PhienThiHocSinh -> NhatKyViPhamThi -> XuLyViPhamThi
KyThi -> LichThiTong -> CaThi -> ThiSinhCaThi, DiemDanhThi, PhanCongGiamThi, BienBanThi

DonTu -> MauDonTu, NhatKyDuyetDon, TepDinhKemDonTu
HoaDon -> GiaoDich -> TaiKhoanNhanTien

DotKhenThuong -> KhenThuong -> MauBangKhen
HoSoKyLuat -> KhieuNaiKyLuat

ThongBao -> ThongBaoNguoiNhan -> NhatKyThongBao
PhieuHoTro -> TinNhanHoTro

ScheduleGenerationJob -> ScheduleDraftItem
(ThoiKhoaBieu, BuoiHoc: created from draft publish)

---

## CHECK CONSTRAINT SUMMARY

| Table | Column | CK Name | Values |
|-------|--------|---------|--------|
| NguoiDung | vai_tro_chinh | CK_NguoiDung_vai_tro_chinh_1 | 14 roles |
| NguoiDung | trang_thai | CK_NguoiDung_trang_thai_2 | hoat_dong, bi_khoa, dang_nhap_lan_dau |
| DonVi | cap_don_vi | CK_DonVi_cap_don_vi_1 | root, co_so, co_so_con |
| HocKy | thu_tu_trong_nam | CK_HocKy_thu_tu_trong_nam_1 | 1, 2, 3 |
| CaHoc | buoi | CK_CaHoc_buoi | sang, chieu, toi |
| CaHoc | gio | CK_CaHoc_gio | gio_ket_thuc > gio_bat_dau |
| ThoiKhoaBieu | thu_trong_tuan | CK_ThoiKhoaBieu_thu_trong_tuan | 1-7 |
| ThoiKhoaBieu | trang_thai | CK_ThoiKhoaBieu_trang_thai | nhap, da_xuat_ban, da_huy |
| BuoiHoc | trang_thai_buoi | CK_BuoiHoc_trang_thai_buoi | du_kien, da_dien_ra, da_huy, doi_lich, day_thay |
| BuoiHoc | trang_thai_diem_danh | CK_BuoiHoc_trang_thai_diem_danh | chua_mo, dang_diem_danh, da_gui, da_khoa |
| DiemDanh | trang_thai | CK_DiemDanh_trang_thai_1 | co_mat, vang, di_muon, co_phep |
| DiemSo | trang_thai | CK_DiemSo_trang_thai_5 | dat, rot, chua_hoan_thanh, cho_hoan_thanh_bo_sung |
| DonTu | loai_don | CK_DonTu_loai_don_1 | 11 types |
| DonTu | trang_thai | CK_DonTu_trang_thai_2 | nhap, da_nop, dang_xem_xet, yeu_cau_bo_sung, da_duyet, tu_choi, da_huy |
| HoaDon | trang_thai | CK_HoaDon_trang_thai | 5 statuses |
| GiaoDich | loai_giao_dich | CK_GiaoDich_loai_giao_dich | 5 types |
| GiaoDich | trang_thai | CK_GiaoDich_trang_thai | 9 statuses |
| HoSoKyLuat | trang_thai | CK_HoSoKyLuat_trang_thai | 8 statuses |
| DeKiemTra | loai_de_thi | CK_DeKiemTra_loai_de_thi | 5 types |
| CaThi | trang_thai | CK_CaThi_trang_thai | 8 statuses |

---

*Generated from Backend/Data/ApplicationDbContext.cs (6115 lines)*
*Total entities: ~100 tables across 18+ modules*
