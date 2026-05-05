using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<AnhChupPhanTich> AnhChupPhanTichs => Set<AnhChupPhanTich>();
    public DbSet<BaiHoc> BaiHocs => Set<BaiHoc>();
    public DbSet<BaiNop> BaiNops => Set<BaiNop>();
    public DbSet<BaiTap> BaiTaps => Set<BaiTap>();
    public DbSet<BaoCaoRuiRoRotMon> BaoCaoRuiRoRotMons => Set<BaoCaoRuiRoRotMon>();
    public DbSet<BaoCaoRuiRoVang> BaoCaoRuiRoVangs => Set<BaoCaoRuiRoVang>();
    public DbSet<BaoCaoSuDungPhong> BaoCaoSuDungPhongs => Set<BaoCaoSuDungPhong>();
    public DbSet<BinhLuan> BinhLuans => Set<BinhLuan>();
    public DbSet<BuoiHoc> BuoiHocs => Set<BuoiHoc>();
    public DbSet<CanhBaoBaoMat> CanhBaoBaoMats => Set<CanhBaoBaoMat>();
    public DbSet<CanhBaoDaoVan> CanhBaoDaoVans => Set<CanhBaoDaoVan>();
    public DbSet<CauHinhDiemMonHoc> CauHinhDiemMonHocs => Set<CauHinhDiemMonHoc>();
    public DbSet<CauHinhKhenThuong> CauHinhKhenThuongs => Set<CauHinhKhenThuong>();
    public DbSet<CauHoi> CauHois => Set<CauHoi>();
    public DbSet<CauHoiDanhGia> CauHoiDanhGias => Set<CauHoiDanhGia>();
    public DbSet<CauHoiDeKiemTra> CauHoiDeKiemTras => Set<CauHoiDeKiemTra>();
    public DbSet<CauHoiThuongGap> CauHoiThuongGaps => Set<CauHoiThuongGap>();
    public DbSet<Chuong> Chuongs => Set<Chuong>();
    public DbSet<DangKyHocPhan> DangKyHocPhans => Set<DangKyHocPhan>();
    public DbSet<DanhGiaGiaoVien> DanhGiaGiaoViens => Set<DanhGiaGiaoVien>();
    public DbSet<DanhMucMonHoc> DanhMucMonHocs => Set<DanhMucMonHoc>();
    public DbSet<DanhSachRuiRoRotMon> DanhSachRuiRoRotMons => Set<DanhSachRuiRoRotMon>();
    public DbSet<DatPhong> DatPhongs => Set<DatPhong>();
    public DbSet<DeKiemTra> DeKiemTras => Set<DeKiemTra>();
    public DbSet<DiemDanh> DiemDanhs => Set<DiemDanh>();
    public DbSet<DiemSo> DiemSos => Set<DiemSo>();
    public DbSet<DonTu> DonTus => Set<DonTu>();
    public DbSet<DonVi> DonVis => Set<DonVi>();
    public DbSet<GiaiDoanDangKy> GiaiDoanDangKys => Set<GiaiDoanDangKy>();
    public DbSet<GiaoDich> GiaoDichs => Set<GiaoDich>();
    public DbSet<HoSoKyLuat> HoSoKyLuats => Set<HoSoKyLuat>();
    public DbSet<HoaDon> HoaDons => Set<HoaDon>();
    public DbSet<HocKy> HocKys => Set<HocKy>();
    public DbSet<KhenThuong> KhenThuongs => Set<KhenThuong>();
    public DbSet<KhoaHoc> KhoaHocs => Set<KhoaHoc>();
    public DbSet<LienKetPhuHuynh> LienKetPhuHuynhs => Set<LienKetPhuHuynh>();
    public DbSet<LopHanhChinh> LopHanhChinhs => Set<LopHanhChinh>();
    public DbSet<LopHocPhan> LopHocPhans => Set<LopHocPhan>();
    public DbSet<MauThongBao> MauThongBaos => Set<MauThongBao>();
    public DbSet<MonHocTienQuyet> MonHocTienQuyets => Set<MonHocTienQuyet>();
    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<NhatKyDuyetDon> NhatKyDuyetDons => Set<NhatKyDuyetDon>();
    public DbSet<NhatKyKiemToan> NhatKyKiemToans => Set<NhatKyKiemToan>();
    public DbSet<NhatKyThayDoiDiem> NhatKyThayDoiDiems => Set<NhatKyThayDoiDiem>();
    public DbSet<NhatKyThongBao> NhatKyThongBaos => Set<NhatKyThongBao>();
    public DbSet<NopBaiDanhGia> NopBaiDanhGias => Set<NopBaiDanhGia>();
    public DbSet<PhanQuyenNguoiDung> PhanQuyenNguoiDungs => Set<PhanQuyenNguoiDung>();
    public DbSet<PhienThiHocSinh> PhienThiHocSinhs => Set<PhienThiHocSinh>();
    public DbSet<PhieuHoTro> PhieuHoTros => Set<PhieuHoTro>();
    public DbSet<PhongHoc> PhongHocs => Set<PhongHoc>();
    public DbSet<ThietBiPhong> ThietBiPhongs => Set<ThietBiPhong>();
    public DbSet<ThoiKhoaBieu> ThoiKhoaBieus => Set<ThoiKhoaBieu>();
    public DbSet<ThongBao> ThongBaos => Set<ThongBao>();
    public DbSet<ThongBaoHenGio> ThongBaoHenGios => Set<ThongBaoHenGio>();
    public DbSet<TienDoBaiHoc> TienDoBaiHocs => Set<TienDoBaiHoc>();
    public DbSet<TinNhanHoTro> TinNhanHoTros => Set<TinNhanHoTro>();
    public DbSet<TokenLamMoi> TokenLamMois => Set<TokenLamMoi>();
    public DbSet<TuyChonThongBao> TuyChonThongBaos => Set<TuyChonThongBao>();
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<XuatBaoCao> XuatBaoCaos => Set<XuatBaoCao>();
    public DbSet<YeuCauDoiLich> YeuCauDoiLichs => Set<YeuCauDoiLich>();
    public DbSet<YeuCauHoanPhi> YeuCauHoanPhis => Set<YeuCauHoanPhi>();
    public DbSet<YeuCauMoKhoaDiemDanh> YeuCauMoKhoaDiemDanhs => Set<YeuCauMoKhoaDiemDanh>();
    public DbSet<YeuCauSuaDiem> YeuCauSuaDiems => Set<YeuCauSuaDiem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AnhChupPhanTich>(entity =>
        {
            entity.ToTable("AnhChupPhanTich", "dbo");
            entity.HasKey(e => e.MaAnhChup).HasName("PK_AnhChupPhanTich");
            entity.Property(e => e.MaAnhChup)
                .HasColumnName("ma_anh_chup");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.NgayAnhChup)
                .HasColumnName("ngay_anh_chup")
                .HasColumnType("date");
            entity.Property(e => e.LoaiChiSo)
                .HasColumnName("loai_chi_so")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.GiaTriChiSo)
                .HasColumnName("gia_tri_chi_so")
                .HasColumnType("decimal(18,4)");
            entity.Property(e => e.ChieuLocJson)
                .HasColumnName("chieu_loc_json")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.MaDonVi, e.MaHocKy, e.NgayAnhChup, e.LoaiChiSo }).IsUnique().HasDatabaseName("UQ_AnhChupPhanTich_1");
            entity.ToTable(t => t.HasCheckConstraint("CK_AnhChupPhanTich_chieu_loc_json_ISJSON", "[chieu_loc_json] IS NULL OR ISJSON([chieu_loc_json]) = 1"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_AnhChupPhanTich_ma_don_vi__DonVi");
            entity.HasOne(e => e.HocKy)
                .WithMany()
                .HasForeignKey(e => e.MaHocKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_AnhChupPhanTich_ma_hoc_ky__HocKy");
        });

        modelBuilder.Entity<BaiHoc>(entity =>
        {
            entity.ToTable("BaiHoc", "dbo");
            entity.HasKey(e => e.MaBaiHoc).HasName("PK_BaiHoc");
            entity.Property(e => e.MaBaiHoc)
                .HasColumnName("ma_bai_hoc");
            entity.Property(e => e.MaChuong)
                .HasColumnName("ma_chuong");
            entity.Property(e => e.TieuDe)
                .HasColumnName("tieu_de")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.LoaiBaiHoc)
                .HasColumnName("loai_bai_hoc")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.UrlTapTin)
                .HasColumnName("url_tap_tin")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ThoiLuongGiay)
                .HasColumnName("thoi_luong_giay");
            entity.Property(e => e.NoiDungVanBan)
                .HasColumnName("noi_dung_van_ban")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.DieuKienMoKhoa)
                .HasColumnName("dieu_kien_mo_khoa")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TomTatAi)
                .HasColumnName("tom_tat_ai")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ThuTu)
                .HasColumnName("thu_tu")
                .HasDefaultValue(0);
            entity.Property(e => e.DaAn)
                .HasColumnName("da_an")
                .HasDefaultValue(false);
            entity.ToTable(t => t.HasCheckConstraint("CK_BaiHoc_loai_bai_hoc_1", "[loai_bai_hoc] IN (N'video', N'pdf', N'van_ban', N'trac_nghiem')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_BaiHoc_thoi_luong_giay_2", "[thoi_luong_giay] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_BaiHoc_dieu_kien_mo_khoa_ISJSON", "[dieu_kien_mo_khoa] IS NULL OR ISJSON([dieu_kien_mo_khoa]) = 1"));
            entity.HasOne(e => e.Chuong)
                .WithMany()
                .HasForeignKey(e => e.MaChuong)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BaiHoc_ma_chuong__Chuong");
        });

        modelBuilder.Entity<BaiNop>(entity =>
        {
            entity.ToTable("BaiNop", "dbo");
            entity.HasKey(e => e.MaBaiNop).HasName("PK_BaiNop");
            entity.Property(e => e.MaBaiNop)
                .HasColumnName("ma_bai_nop");
            entity.Property(e => e.MaBaiTap)
                .HasColumnName("ma_bai_tap");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.UrlTapTin)
                .HasColumnName("url_tap_tin")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.SoLanNop)
                .HasColumnName("so_lan_nop");
            entity.Property(e => e.NopTre)
                .HasColumnName("nop_tre")
                .HasDefaultValue(false);
            entity.Property(e => e.DiemDaoVan)
                .HasColumnName("diem_dao_van")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiemSo)
                .HasColumnName("diem_so")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiemAiDeXuat)
                .HasColumnName("diem_ai_de_xuat")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.NhanXet)
                .HasColumnName("nhan_xet")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ThoiDiemNop)
                .HasColumnName("thoi_diem_nop")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.DaCongBo)
                .HasColumnName("da_cong_bo")
                .HasDefaultValue(false);
            entity.HasIndex(e => new { e.MaBaiTap, e.MaHocSinh, e.SoLanNop }).IsUnique().HasDatabaseName("UQ_BaiNop_1");
            entity.HasIndex(e => new { e.MaBaiTap, e.MaHocSinh, e.SoLanNop }).HasDatabaseName("IX_BaiNop_BaiTap_HocSinh");
            entity.ToTable(t => t.HasCheckConstraint("CK_BaiNop_so_lan_nop_1", "[so_lan_nop] > 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_BaiNop_diem_dao_van_2", "[diem_dao_van] BETWEEN 0 AND 100"));
            entity.ToTable(t => t.HasCheckConstraint("CK_BaiNop_diem_so_3", "[diem_so] BETWEEN 0 AND 10"));
            entity.ToTable(t => t.HasCheckConstraint("CK_BaiNop_diem_ai_de_xuat_4", "[diem_ai_de_xuat] BETWEEN 0 AND 10"));
            entity.HasOne(e => e.BaiTap)
                .WithMany()
                .HasForeignKey(e => e.MaBaiTap)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BaiNop_ma_bai_tap__BaiTap");
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BaiNop_ma_hoc_sinh__NguoiDung");
        });

        modelBuilder.Entity<BaiTap>(entity =>
        {
            entity.ToTable("BaiTap", "dbo");
            entity.HasKey(e => e.MaBaiTap).HasName("PK_BaiTap");
            entity.Property(e => e.MaBaiTap)
                .HasColumnName("ma_bai_tap");
            entity.Property(e => e.MaKhoaHoc)
                .HasColumnName("ma_khoa_hoc");
            entity.Property(e => e.TieuDe)
                .HasColumnName("tieu_de")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.MoTa)
                .HasColumnName("mo_ta")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.HanNop)
                .HasColumnName("han_nop")
                .HasColumnType("datetime2");
            entity.Property(e => e.SoLanNopToiDa)
                .HasColumnName("so_lan_nop_toi_da")
                .HasDefaultValueSql("3");
            entity.Property(e => e.DinhDangChoPhep)
                .HasColumnName("dinh_dang_cho_phep")
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(e => e.HuongDanChamDiem)
                .HasColumnName("huong_dan_cham_diem")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("nhap");
            entity.ToTable(t => t.HasCheckConstraint("CK_BaiTap_so_lan_nop_toi_da_1", "[so_lan_nop_toi_da] > 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_BaiTap_trang_thai_2", "[trang_thai] IN (N'nhap', N'da_xuat_ban', N'da_dong')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_BaiTap_dinh_dang_cho_phep_ISJSON", "[dinh_dang_cho_phep] IS NULL OR ISJSON([dinh_dang_cho_phep]) = 1"));
            entity.HasOne(e => e.KhoaHoc)
                .WithMany()
                .HasForeignKey(e => e.MaKhoaHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BaiTap_ma_khoa_hoc__KhoaHoc");
        });

        modelBuilder.Entity<BaoCaoRuiRoRotMon>(entity =>
        {
            entity.ToTable("BaoCaoRuiRoRotMon", "dbo");
            entity.HasKey(e => e.MaBaoCaoRot).HasName("PK_BaoCaoRuiRoRotMon");
            entity.Property(e => e.MaBaoCaoRot)
                .HasColumnName("ma_bao_cao_rot");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaMonHoc)
                .HasColumnName("ma_mon_hoc");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.XacSuatRotMon)
                .HasColumnName("xac_suat_rot_mon")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.DacTrungJson)
                .HasColumnName("dac_trung_json")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TaoLuc)
                .HasColumnName("tao_luc")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_BaoCaoRuiRoRotMon_xac_suat_rot_mon_1", "[xac_suat_rot_mon] BETWEEN 0 AND 1"));
            entity.ToTable(t => t.HasCheckConstraint("CK_BaoCaoRuiRoRotMon_dac_trung_json_ISJSON", "[dac_trung_json] IS NULL OR ISJSON([dac_trung_json]) = 1"));
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BaoCaoRuiRoRotMon_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MaMonHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BaoCaoRuiRoRotMon_ma_mon_hoc__DanhMucMonHoc");
            entity.HasOne(e => e.HocKy)
                .WithMany()
                .HasForeignKey(e => e.MaHocKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BaoCaoRuiRoRotMon_ma_hoc_ky__HocKy");
        });

        modelBuilder.Entity<BaoCaoRuiRoVang>(entity =>
        {
            entity.ToTable("BaoCaoRuiRoVang", "dbo");
            entity.HasKey(e => e.MaBaoCao).HasName("PK_BaoCaoRuiRoVang");
            entity.Property(e => e.MaBaoCao)
                .HasColumnName("ma_bao_cao");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaMonHoc)
                .HasColumnName("ma_mon_hoc");
            entity.Property(e => e.DiemRuiRo)
                .HasColumnName("diem_rui_ro")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.DacTrungJson)
                .HasColumnName("dac_trung_json")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TaoLuc)
                .HasColumnName("tao_luc")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_BaoCaoRuiRoVang_diem_rui_ro_1", "[diem_rui_ro] BETWEEN 0 AND 1"));
            entity.ToTable(t => t.HasCheckConstraint("CK_BaoCaoRuiRoVang_dac_trung_json_ISJSON", "[dac_trung_json] IS NULL OR ISJSON([dac_trung_json]) = 1"));
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BaoCaoRuiRoVang_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MaMonHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BaoCaoRuiRoVang_ma_mon_hoc__DanhMucMonHoc");
        });

        modelBuilder.Entity<BaoCaoSuDungPhong>(entity =>
        {
            entity.ToTable("BaoCaoSuDungPhong", "dbo");
            entity.HasKey(e => e.MaBcSuDungPhong).HasName("PK_BaoCaoSuDungPhong");
            entity.Property(e => e.MaBcSuDungPhong)
                .HasColumnName("ma_bc_su_dung_phong");
            entity.Property(e => e.MaPhong)
                .HasColumnName("ma_phong");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.TuNgay)
                .HasColumnName("tu_ngay")
                .HasColumnType("date");
            entity.Property(e => e.DenNgay)
                .HasColumnName("den_ngay")
                .HasColumnType("date");
            entity.Property(e => e.SoGioSuDung)
                .HasColumnName("so_gio_su_dung")
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);
            entity.Property(e => e.TiLeSuDung)
                .HasColumnName("ti_le_su_dung")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.TaoLuc)
                .HasColumnName("tao_luc")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_BaoCaoSuDungPhong_ti_le_su_dung_1", "[ti_le_su_dung] BETWEEN 0 AND 100"));
            entity.HasOne(e => e.Phong)
                .WithMany()
                .HasForeignKey(e => e.MaPhong)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BaoCaoSuDungPhong_ma_phong__PhongHoc");
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BaoCaoSuDungPhong_ma_don_vi__DonVi");
        });

        modelBuilder.Entity<BinhLuan>(entity =>
        {
            entity.ToTable("BinhLuan", "dbo");
            entity.HasKey(e => e.MaBinhLuan).HasName("PK_BinhLuan");
            entity.Property(e => e.MaBinhLuan)
                .HasColumnName("ma_binh_luan");
            entity.Property(e => e.MaBaiHoc)
                .HasColumnName("ma_bai_hoc");
            entity.Property(e => e.MaNguoiDung)
                .HasColumnName("ma_nguoi_dung");
            entity.Property(e => e.NoiDung)
                .HasColumnName("noi_dung")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.GiayTrongVideo)
                .HasColumnName("giay_trong_video");
            entity.Property(e => e.SoTrangPdf)
                .HasColumnName("so_trang_pdf");
            entity.Property(e => e.MaBinhLuanCha)
                .HasColumnName("ma_binh_luan_cha");
            entity.Property(e => e.DaGhim)
                .HasColumnName("da_ghim")
                .HasDefaultValue(false);
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_BinhLuan_giay_trong_video_1", "[giay_trong_video] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_BinhLuan_so_trang_pdf_2", "[so_trang_pdf] > 0"));
            entity.HasOne(e => e.BaiHoc)
                .WithMany()
                .HasForeignKey(e => e.MaBaiHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BinhLuan_ma_bai_hoc__BaiHoc");
            entity.HasOne(e => e.NguoiDung)
                .WithMany()
                .HasForeignKey(e => e.MaNguoiDung)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BinhLuan_ma_nguoi_dung__NguoiDung");
            entity.HasOne(e => e.BinhLuanCha)
                .WithMany()
                .HasForeignKey(e => e.MaBinhLuanCha)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BinhLuan_ma_binh_luan_cha__BinhLuan");
        });

        modelBuilder.Entity<BuoiHoc>(entity =>
        {
            entity.ToTable("BuoiHoc", "dbo");
            entity.HasKey(e => e.MaBuoiHoc).HasName("PK_BuoiHoc");
            entity.Property(e => e.MaBuoiHoc)
                .HasColumnName("ma_buoi_hoc");
            entity.Property(e => e.MaTkb)
                .HasColumnName("ma_tkb");
            entity.Property(e => e.NgayHoc)
                .HasColumnName("ngay_hoc")
                .HasColumnType("date");
            entity.Property(e => e.GioBatDau)
                .HasColumnName("gio_bat_dau")
                .HasColumnType("time");
            entity.Property(e => e.GioKetThuc)
                .HasColumnName("gio_ket_thuc")
                .HasColumnType("time");
            entity.Property(e => e.TrangThaiBuoi)
                .HasColumnName("trang_thai_buoi")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("chua_xac_nhan");
            entity.Property(e => e.KhoaLuc)
                .HasColumnName("khoa_luc")
                .HasColumnType("datetime2");
            entity.ToTable(t => t.HasCheckConstraint("CK_BuoiHoc_trang_thai_buoi_1", "[trang_thai_buoi] IN (N'da_xac_nhan', N'chua_xac_nhan', N'yeu_cau_mo_khoa', N'da_huy')"));
            entity.HasOne(e => e.Tkb)
                .WithMany()
                .HasForeignKey(e => e.MaTkb)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_BuoiHoc_ma_tkb__ThoiKhoaBieu");
        });

        modelBuilder.Entity<CanhBaoBaoMat>(entity =>
        {
            entity.ToTable("CanhBaoBaoMat", "dbo");
            entity.HasKey(e => e.MaCanhBao).HasName("PK_CanhBaoBaoMat");
            entity.Property(e => e.MaCanhBao)
                .HasColumnName("ma_canh_bao");
            entity.Property(e => e.MaNguoiDung)
                .HasColumnName("ma_nguoi_dung");
            entity.Property(e => e.DiemRuiRo)
                .HasColumnName("diem_rui_ro")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiaChiIp)
                .HasColumnName("dia_chi_ip")
                .HasMaxLength(45);
            entity.Property(e => e.ThongTinTrinhDuyet)
                .HasColumnName("thong_tin_trinh_duyet")
                .HasMaxLength(500);
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("mo");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_CanhBaoBaoMat_diem_rui_ro_1", "[diem_rui_ro] BETWEEN 0 AND 1"));
            entity.ToTable(t => t.HasCheckConstraint("CK_CanhBaoBaoMat_trang_thai_2", "[trang_thai] IN (N'mo', N'da_xem', N'bo_qua')"));
            entity.HasOne(e => e.NguoiDung)
                .WithMany()
                .HasForeignKey(e => e.MaNguoiDung)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_CanhBaoBaoMat_ma_nguoi_dung__NguoiDung");
        });

        modelBuilder.Entity<CanhBaoDaoVan>(entity =>
        {
            entity.ToTable("CanhBaoDaoVan", "dbo");
            entity.HasKey(e => e.MaCanhBao).HasName("PK_CanhBaoDaoVan");
            entity.Property(e => e.MaCanhBao)
                .HasColumnName("ma_canh_bao");
            entity.Property(e => e.MaBaiNop)
                .HasColumnName("ma_bai_nop");
            entity.Property(e => e.DiemDaoVan)
                .HasColumnName("diem_dao_van")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.ChiTiet)
                .HasColumnName("chi_tiet")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_CanhBaoDaoVan_diem_dao_van_1", "[diem_dao_van] BETWEEN 0 AND 100"));
            entity.ToTable(t => t.HasCheckConstraint("CK_CanhBaoDaoVan_chi_tiet_ISJSON", "[chi_tiet] IS NULL OR ISJSON([chi_tiet]) = 1"));
            entity.HasOne(e => e.BaiNop)
                .WithMany()
                .HasForeignKey(e => e.MaBaiNop)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_CanhBaoDaoVan_ma_bai_nop__BaiNop");
        });

        modelBuilder.Entity<CauHinhDiemMonHoc>(entity =>
        {
            entity.ToTable("CauHinhDiemMonHoc", "dbo");
            entity.HasKey(e => e.MaCauHinhDiem).HasName("PK_CauHinhDiemMonHoc");
            entity.Property(e => e.MaCauHinhDiem)
                .HasColumnName("ma_cau_hinh_diem");
            entity.Property(e => e.MaMonHoc)
                .HasColumnName("ma_mon_hoc");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.TrongSoQuaTrinh)
                .HasColumnName("trong_so_qua_trinh")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.TrongSoGiuaKy)
                .HasColumnName("trong_so_giua_ky")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.TrongSoCuoiKy)
                .HasColumnName("trong_so_cuoi_ky")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.NguongDat)
                .HasColumnName("nguong_dat")
                .HasColumnType("decimal(5,2)")
                .HasDefaultValueSql("5");
            entity.ToTable(t => t.HasCheckConstraint("CK_CauHinhDiemMonHoc_trong_so_qua_trinh_1", "[trong_so_qua_trinh] BETWEEN 0 AND 100"));
            entity.ToTable(t => t.HasCheckConstraint("CK_CauHinhDiemMonHoc_trong_so_giua_ky_2", "[trong_so_giua_ky] BETWEEN 0 AND 100"));
            entity.ToTable(t => t.HasCheckConstraint("CK_CauHinhDiemMonHoc_trong_so_cuoi_ky_3", "[trong_so_cuoi_ky] BETWEEN 0 AND 100"));
            entity.ToTable(t => t.HasCheckConstraint("CK_CauHinhDiemMonHoc_nguong_dat_4", "[nguong_dat] BETWEEN 0 AND 10"));
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MaMonHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_CauHinhDiemMonHoc_ma_mon_hoc__DanhMucMonHoc");
            entity.HasOne(e => e.HocKy)
                .WithMany()
                .HasForeignKey(e => e.MaHocKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_CauHinhDiemMonHoc_ma_hoc_ky__HocKy");
        });

        modelBuilder.Entity<CauHinhKhenThuong>(entity =>
        {
            entity.ToTable("CauHinhKhenThuong", "dbo");
            entity.HasKey(e => e.MaCauHinhKt).HasName("PK_CauHinhKhenThuong");
            entity.Property(e => e.MaCauHinhKt)
                .HasColumnName("ma_cau_hinh_kt");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.LoaiKhenThuong)
                .HasColumnName("loai_khen_thuong")
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(e => e.GpaToiThieu)
                .HasColumnName("gpa_toi_thieu")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.ConHoatDong)
                .HasColumnName("con_hoat_dong")
                .HasDefaultValue(true);
            entity.ToTable(t => t.HasCheckConstraint("CK_CauHinhKhenThuong_gpa_toi_thieu_1", "[gpa_toi_thieu] BETWEEN 0 AND 10"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_CauHinhKhenThuong_ma_don_vi__DonVi");
        });

        modelBuilder.Entity<CauHoi>(entity =>
        {
            entity.ToTable("CauHoi", "dbo");
            entity.HasKey(e => e.MaCauHoi).HasName("PK_CauHoi");
            entity.Property(e => e.MaCauHoi)
                .HasColumnName("ma_cau_hoi");
            entity.Property(e => e.MaMonHoc)
                .HasColumnName("ma_mon_hoc");
            entity.Property(e => e.NguoiTao)
                .HasColumnName("nguoi_tao");
            entity.Property(e => e.LoaiCauHoi)
                .HasColumnName("loai_cau_hoi")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.NoiDung)
                .HasColumnName("noi_dung")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.LuaChon)
                .HasColumnName("lua_chon")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.DapAnDung)
                .HasColumnName("dap_an_dung")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.DoKho)
                .HasColumnName("do_kho")
                .HasMaxLength(10)
                .IsRequired();
            entity.ToTable(t => t.HasCheckConstraint("CK_CauHoi_loai_cau_hoi_1", "[loai_cau_hoi] IN (N'trac_nghiem', N'tu_luan')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_CauHoi_do_kho_2", "[do_kho] IN (N'de', N'trung_binh', N'kho')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_CauHoi_dap_an_dung_ISJSON", "[dap_an_dung] IS NULL OR ISJSON([dap_an_dung]) = 1"));
            entity.ToTable(t => t.HasCheckConstraint("CK_CauHoi_lua_chon_ISJSON", "[lua_chon] IS NULL OR ISJSON([lua_chon]) = 1"));
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MaMonHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_CauHoi_ma_mon_hoc__DanhMucMonHoc");
            entity.HasOne(e => e.NguoiTaoNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiTao)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_CauHoi_nguoi_tao__NguoiDung");
        });

        modelBuilder.Entity<CauHoiDanhGia>(entity =>
        {
            entity.ToTable("CauHoiDanhGia", "dbo");
            entity.HasKey(e => e.MaCauHoiDg).HasName("PK_CauHoiDanhGia");
            entity.Property(e => e.MaCauHoiDg)
                .HasColumnName("ma_cau_hoi_dg");
            entity.Property(e => e.NoiDungCauHoi)
                .HasColumnName("noi_dung_cau_hoi")
                .HasMaxLength(500)
                .IsRequired();
            entity.Property(e => e.ConHoatDong)
                .HasColumnName("con_hoat_dong")
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<CauHoiDeKiemTra>(entity =>
        {
            entity.ToTable("CauHoiDeKiemTra", "dbo");
            entity.HasKey(e => new { e.MaDeKiemTra, e.MaCauHoi }).HasName("PK_CauHoiDeKiemTra");
            entity.Property(e => e.MaDeKiemTra)
                .HasColumnName("ma_de_kiem_tra");
            entity.Property(e => e.MaCauHoi)
                .HasColumnName("ma_cau_hoi");
            entity.Property(e => e.DiemSo)
                .HasColumnName("diem_so")
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(1m);
            entity.Property(e => e.ThuTu)
                .HasColumnName("thu_tu");
            entity.HasOne(e => e.DeKiemTra)
                .WithMany()
                .HasForeignKey(e => e.MaDeKiemTra)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_CauHoiDeKiemTra_ma_de_kiem_tra__DeKiemTra");
            entity.HasOne(e => e.CauHoi)
                .WithMany()
                .HasForeignKey(e => e.MaCauHoi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_CauHoiDeKiemTra_ma_cau_hoi__CauHoi");
        });

        modelBuilder.Entity<CauHoiThuongGap>(entity =>
        {
            entity.ToTable("CauHoiThuongGap", "dbo");
            entity.HasKey(e => e.MaCauHoiFaq).HasName("PK_CauHoiThuongGap");
            entity.Property(e => e.MaCauHoiFaq)
                .HasColumnName("ma_cau_hoi_faq");
            entity.Property(e => e.DanhMuc)
                .HasColumnName("danh_muc")
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(e => e.CauHoi)
                .HasColumnName("cau_hoi")
                .HasMaxLength(500)
                .IsRequired();
            entity.Property(e => e.TraLoi)
                .HasColumnName("tra_loi")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.ConHoatDong)
                .HasColumnName("con_hoat_dong")
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<Chuong>(entity =>
        {
            entity.ToTable("Chuong", "dbo");
            entity.HasKey(e => e.MaChuong).HasName("PK_Chuong");
            entity.Property(e => e.MaChuong)
                .HasColumnName("ma_chuong");
            entity.Property(e => e.MaKhoaHoc)
                .HasColumnName("ma_khoa_hoc");
            entity.Property(e => e.TieuDe)
                .HasColumnName("tieu_de")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.ThuTu)
                .HasColumnName("thu_tu")
                .HasDefaultValue(0);
            entity.Property(e => e.DaAn)
                .HasColumnName("da_an")
                .HasDefaultValue(false);
            entity.HasOne(e => e.KhoaHoc)
                .WithMany()
                .HasForeignKey(e => e.MaKhoaHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Chuong_ma_khoa_hoc__KhoaHoc");
        });

        modelBuilder.Entity<DangKyHocPhan>(entity =>
        {
            entity.ToTable("DangKyHocPhan", "dbo");
            entity.HasKey(e => e.MaDangKy).HasName("PK_DangKyHocPhan");
            entity.Property(e => e.MaDangKy)
                .HasColumnName("ma_dang_ky");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaLopHocPhan)
                .HasColumnName("ma_lop_hoc_phan");
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(e => e.ViTriCho)
                .HasColumnName("vi_tri_cho");
            entity.Property(e => e.LaHocLai)
                .HasColumnName("la_hoc_lai")
                .HasDefaultValue(false);
            entity.Property(e => e.KiemTraTienQuyet)
                .HasColumnName("kiem_tra_tien_quyet");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.DaKiemTraTienQuyet)
                .HasColumnName("da_kiem_tra_tien_quyet")
                .HasDefaultValue(false);
            entity.HasIndex(e => new { e.MaHocSinh, e.MaLopHocPhan }).IsUnique().HasDatabaseName("UQ_DangKyHocPhan_1");
            entity.HasIndex(e => new { e.MaLopHocPhan, e.TrangThai }).HasDatabaseName("IX_DangKyHocPhan_LopHocPhan");
            entity.ToTable(t => t.HasCheckConstraint("CK_DangKyHocPhan_trang_thai_1", "[trang_thai] IN (N'da_dang_ky', N'danh_sach_cho', N'da_rut', N'lop_bi_huy')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DangKyHocPhan_vi_tri_cho_2", "[vi_tri_cho] > 0"));
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DangKyHocPhan_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.LopHocPhan)
                .WithMany()
                .HasForeignKey(e => e.MaLopHocPhan)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DangKyHocPhan_ma_lop_hoc_phan__LopHocPhan");
        });

        modelBuilder.Entity<DanhGiaGiaoVien>(entity =>
        {
            entity.ToTable("DanhGiaGiaoVien", "dbo");
            entity.HasKey(e => e.MaDanhGia).HasName("PK_DanhGiaGiaoVien");
            entity.Property(e => e.MaDanhGia)
                .HasColumnName("ma_danh_gia");
            entity.Property(e => e.MaGiaoVien)
                .HasColumnName("ma_giao_vien");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.MaCauHoiDg)
                .HasColumnName("ma_cau_hoi_dg");
            entity.Property(e => e.DiemSo)
                .HasColumnName("diem_so");
            entity.Property(e => e.NhanXetTuDo)
                .HasColumnName("nhan_xet_tu_do")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.AiCamXuc)
                .HasColumnName("ai_cam_xuc")
                .HasMaxLength(20);
            entity.Property(e => e.AiChuDe)
                .HasColumnName("ai_chu_de")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.CohortHash)
                .HasColumnName("cohort_hash")
                .HasMaxLength(128);
            entity.ToTable(t => t.HasCheckConstraint("CK_DanhGiaGiaoVien_diem_so_1", "[diem_so] BETWEEN 1 AND 5"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DanhGiaGiaoVien_ai_cam_xuc_2", "[ai_cam_xuc] IN (N'tich_cuc', N'trung_tinh', N'tieu_cuc')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DanhGiaGiaoVien_ai_chu_de_ISJSON", "[ai_chu_de] IS NULL OR ISJSON([ai_chu_de]) = 1"));
            entity.HasOne(e => e.GiaoVien)
                .WithMany()
                .HasForeignKey(e => e.MaGiaoVien)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DanhGiaGiaoVien_ma_giao_vien__NguoiDung");
            entity.HasOne(e => e.HocKy)
                .WithMany()
                .HasForeignKey(e => e.MaHocKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DanhGiaGiaoVien_ma_hoc_ky__HocKy");
            entity.HasOne(e => e.CauHoiDg)
                .WithMany()
                .HasForeignKey(e => e.MaCauHoiDg)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DanhGiaGiaoVien_ma_cau_hoi_dg__CauHoiDanhGia");
        });

        modelBuilder.Entity<DanhMucMonHoc>(entity =>
        {
            entity.ToTable("DanhMucMonHoc", "dbo");
            entity.HasKey(e => e.MaMonHoc).HasName("PK_DanhMucMonHoc");
            entity.Property(e => e.MaMonHoc)
                .HasColumnName("ma_mon_hoc");
            entity.Property(e => e.MaCodeMonHoc)
                .HasColumnName("ma_code_mon_hoc")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.TenMonHoc)
                .HasColumnName("ten_mon_hoc")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.SoTinChi)
                .HasColumnName("so_tin_chi");
            entity.Property(e => e.ConHoatDong)
                .HasColumnName("con_hoat_dong")
                .HasDefaultValue(true);
            entity.HasIndex(e => e.MaCodeMonHoc).IsUnique().HasDatabaseName("UQ_DanhMucMonHoc_1");
            entity.ToTable(t => t.HasCheckConstraint("CK_DanhMucMonHoc_so_tin_chi_1", "[so_tin_chi] > 0"));
        });

        modelBuilder.Entity<DanhSachRuiRoRotMon>(entity =>
        {
            entity.ToTable("DanhSachRuiRoRotMon", "dbo");
            entity.HasKey(e => e.MaRuiRoRot).HasName("PK_DanhSachRuiRoRotMon");
            entity.Property(e => e.MaRuiRoRot)
                .HasColumnName("ma_rui_ro_rot");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaMonHoc)
                .HasColumnName("ma_mon_hoc");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.XacSuatRotMon)
                .HasColumnName("xac_suat_rot_mon")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.TaoLuc)
                .HasColumnName("tao_luc")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_DanhSachRuiRoRotMon_xac_suat_rot_mon_1", "[xac_suat_rot_mon] BETWEEN 0 AND 1"));
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DanhSachRuiRoRotMon_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MaMonHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DanhSachRuiRoRotMon_ma_mon_hoc__DanhMucMonHoc");
            entity.HasOne(e => e.HocKy)
                .WithMany()
                .HasForeignKey(e => e.MaHocKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DanhSachRuiRoRotMon_ma_hoc_ky__HocKy");
        });

        modelBuilder.Entity<DatPhong>(entity =>
        {
            entity.ToTable("DatPhong", "dbo");
            entity.HasKey(e => e.MaDatPhong).HasName("PK_DatPhong");
            entity.Property(e => e.MaDatPhong)
                .HasColumnName("ma_dat_phong");
            entity.Property(e => e.MaPhong)
                .HasColumnName("ma_phong");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.NguoiYeuCau)
                .HasColumnName("nguoi_yeu_cau");
            entity.Property(e => e.MucDich)
                .HasColumnName("muc_dich")
                .HasMaxLength(500)
                .IsRequired();
            entity.Property(e => e.BatDauLuc)
                .HasColumnName("bat_dau_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.KetThucLuc)
                .HasColumnName("ket_thuc_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.SoNguoiThamDu)
                .HasColumnName("so_nguoi_tham_du");
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(30)
                .IsRequired()
                .HasDefaultValue("cho_duyet");
            entity.Property(e => e.NguoiDuyet)
                .HasColumnName("nguoi_duyet");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_DatPhong_ket_thuc_luc_1", "[ket_thuc_luc] > [bat_dau_luc]"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DatPhong_so_nguoi_tham_du_2", "[so_nguoi_tham_du] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DatPhong_trang_thai_3", "[trang_thai] IN (N'cho_duyet', N'da_xac_nhan', N'tu_choi', N'da_huy')"));
            entity.HasOne(e => e.Phong)
                .WithMany()
                .HasForeignKey(e => e.MaPhong)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DatPhong_ma_phong__PhongHoc");
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DatPhong_ma_don_vi__DonVi");
            entity.HasOne(e => e.NguoiYeuCauNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiYeuCau)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DatPhong_nguoi_yeu_cau__NguoiDung");
            entity.HasOne(e => e.NguoiDuyetNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiDuyet)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DatPhong_nguoi_duyet__NguoiDung");
        });

        modelBuilder.Entity<DeKiemTra>(entity =>
        {
            entity.ToTable("DeKiemTra", "dbo");
            entity.HasKey(e => e.MaDeKiemTra).HasName("PK_DeKiemTra");
            entity.Property(e => e.MaDeKiemTra)
                .HasColumnName("ma_de_kiem_tra");
            entity.Property(e => e.MaMonHoc)
                .HasColumnName("ma_mon_hoc");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.TieuDe)
                .HasColumnName("tieu_de")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.ThoiGianPhut)
                .HasColumnName("thoi_gian_phut");
            entity.Property(e => e.CauHinhDeThi)
                .HasColumnName("cau_hinh_de_thi")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("nhap");
            entity.ToTable(t => t.HasCheckConstraint("CK_DeKiemTra_thoi_gian_phut_1", "[thoi_gian_phut] BETWEEN 1 AND 240"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DeKiemTra_trang_thai_2", "[trang_thai] IN (N'nhap', N'da_len_lich', N'dang_mo', N'da_dong', N'da_cong_bo')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DeKiemTra_cau_hinh_de_thi_ISJSON", "[cau_hinh_de_thi] IS NULL OR ISJSON([cau_hinh_de_thi]) = 1"));
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MaMonHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DeKiemTra_ma_mon_hoc__DanhMucMonHoc");
            entity.HasOne(e => e.HocKy)
                .WithMany()
                .HasForeignKey(e => e.MaHocKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DeKiemTra_ma_hoc_ky__HocKy");
        });

        modelBuilder.Entity<DiemDanh>(entity =>
        {
            entity.ToTable("DiemDanh", "dbo");
            entity.HasKey(e => e.MaDiemDanh).HasName("PK_DiemDanh");
            entity.Property(e => e.MaDiemDanh)
                .HasColumnName("ma_diem_danh");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.MaBuoiHoc)
                .HasColumnName("ma_buoi_hoc");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.NguoiGhiNhan)
                .HasColumnName("nguoi_ghi_nhan");
            entity.Property(e => e.GhiNhanLuc)
                .HasColumnName("ghi_nhan_luc")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.KhoaLuc)
                .HasColumnName("khoa_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.HeSoVang)
                .HasColumnName("he_so_vang");
            entity.Property(e => e.MaYcMoKhoa)
                .HasColumnName("ma_yc_mo_khoa");
            entity.HasIndex(e => new { e.MaBuoiHoc, e.MaHocSinh }).IsUnique().HasDatabaseName("UQ_DiemDanh_1");
            entity.HasIndex(e => new { e.MaDonVi, e.MaBuoiHoc }).HasDatabaseName("IX_DiemDanh_ma_don_vi_ma_buoi_hoc");
            entity.HasIndex(e => new { e.MaBuoiHoc, e.MaHocSinh, e.TrangThai }).HasDatabaseName("IX_DiemDanh_BuoiHoc_HocSinh");
            entity.HasIndex(e => new { e.MaDonVi, e.MaHocSinh }).HasDatabaseName("IX_DiemDanh_DonVi_HocSinh");
            entity.ToTable(t => t.HasCheckConstraint("CK_DiemDanh_trang_thai_1", "[trang_thai] IN (N'co_mat', N'vang', N'di_muon', N'co_phep')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DiemDanh_he_so_vang_2", "[he_so_vang] >= 0"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DiemDanh_ma_don_vi__DonVi");
            entity.HasOne(e => e.BuoiHoc)
                .WithMany()
                .HasForeignKey(e => e.MaBuoiHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DiemDanh_ma_buoi_hoc__BuoiHoc");
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DiemDanh_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.NguoiGhiNhanNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiGhiNhan)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DiemDanh_nguoi_ghi_nhan__NguoiDung");
            entity.HasOne(e => e.YcMoKhoa)
                .WithMany()
                .HasForeignKey(e => e.MaYcMoKhoa)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DiemDanh_ma_yc_mo_khoa__YeuCauMoKhoaDiemDanh");
        });

        modelBuilder.Entity<DiemSo>(entity =>
        {
            entity.ToTable("DiemSo", "dbo");
            entity.HasKey(e => e.MaDiemSo).HasName("PK_DiemSo");
            entity.Property(e => e.MaDiemSo)
                .HasColumnName("ma_diem_so");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaMonHoc)
                .HasColumnName("ma_mon_hoc");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.DiemQuaTrinh)
                .HasColumnName("diem_qua_trinh")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiemGiuaKy)
                .HasColumnName("diem_giua_ky")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiemCuoiKy)
                .HasColumnName("diem_cuoi_ky")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.GpaMonHoc)
                .HasColumnName("gpa_mon_hoc")
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0m);
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("chua_hoan_thanh");
            entity.Property(e => e.DaKhoa)
                .HasColumnName("da_khoa")
                .HasDefaultValue(false);
            entity.Property(e => e.LyDoRot)
                .HasColumnName("ly_do_rot")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NamNhapHoc)
                .HasColumnName("nam_nhap_hoc");
            entity.HasIndex(e => new { e.MaHocSinh, e.MaMonHoc, e.MaHocKy }).IsUnique().HasDatabaseName("UQ_DiemSo_1");
            entity.HasIndex(e => new { e.MaDonVi, e.MaHocKy }).HasDatabaseName("IX_DiemSo_ma_don_vi_ma_hoc_ky");
            entity.HasIndex(e => new { e.MaHocSinh, e.MaHocKy }).HasDatabaseName("IX_DiemSo_HocSinh_HocKy");
            entity.ToTable(t => t.HasCheckConstraint("CK_DiemSo_diem_qua_trinh_1", "[diem_qua_trinh] BETWEEN 0 AND 10"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DiemSo_diem_giua_ky_2", "[diem_giua_ky] BETWEEN 0 AND 10"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DiemSo_diem_cuoi_ky_3", "[diem_cuoi_ky] BETWEEN 0 AND 10"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DiemSo_gpa_mon_hoc_4", "[gpa_mon_hoc] BETWEEN 0 AND 10"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DiemSo_trang_thai_5", "[trang_thai] IN (N'dat', N'rot', N'chua_hoan_thanh', N'cho_hoan_thanh_bo_sung')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DiemSo_ly_do_rot_ISJSON", "[ly_do_rot] IS NULL OR ISJSON([ly_do_rot]) = 1"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DiemSo_ma_don_vi__DonVi");
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DiemSo_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MaMonHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DiemSo_ma_mon_hoc__DanhMucMonHoc");
            entity.HasOne(e => e.HocKy)
                .WithMany()
                .HasForeignKey(e => e.MaHocKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DiemSo_ma_hoc_ky__HocKy");
        });

        modelBuilder.Entity<DonTu>(entity =>
        {
            entity.ToTable("DonTu", "dbo");
            entity.HasKey(e => e.MaDonTu).HasName("PK_DonTu");
            entity.Property(e => e.MaDonTu)
                .HasColumnName("ma_don_tu");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.LoaiDon)
                .HasColumnName("loai_don")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(30)
                .IsRequired()
                .HasDefaultValue("nhap");
            entity.Property(e => e.NguoiDuyetHienTai)
                .HasColumnName("nguoi_duyet_hien_tai");
            entity.Property(e => e.DuLieuBieuMau)
                .HasColumnName("du_lieu_bieu_mau")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.UrlBangChung)
                .HasColumnName("url_bang_chung")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.LyDoTuChoi)
                .HasColumnName("ly_do_tu_choi")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NhatKyTuDong)
                .HasColumnName("nhat_ky_tu_dong")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_DonTu_loai_don_1", "[loai_don] IN ( N'nghi_phep', N'thi_lai', N'chuyen_truong', N'cap_chung_chi', N'khac', N'phuc_tra_diem', N'bao_luu', N'chuyen_nganh', N'chuyen_co_so', N'xac_nhan', N'rut_hoc' )"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DonTu_trang_thai_2", "[trang_thai] IN (N'nhap', N'da_nop', N'dang_xem_xet', N'da_duyet', N'tu_choi')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DonTu_du_lieu_bieu_mau_ISJSON", "[du_lieu_bieu_mau] IS NULL OR ISJSON([du_lieu_bieu_mau]) = 1"));
            entity.ToTable(t => t.HasCheckConstraint("CK_DonTu_nhat_ky_tu_dong_ISJSON", "[nhat_ky_tu_dong] IS NULL OR ISJSON([nhat_ky_tu_dong]) = 1"));
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DonTu_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.NguoiDuyetHienTaiNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiDuyetHienTai)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DonTu_nguoi_duyet_hien_tai__NguoiDung");
        });

        modelBuilder.Entity<DonVi>(entity =>
        {
            entity.ToTable("DonVi", "dbo");
            entity.HasKey(e => e.MaDonVi).HasName("PK_DonVi");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.MaDonViCha)
                .HasColumnName("ma_don_vi_cha");
            entity.Property(e => e.TenDonVi)
                .HasColumnName("ten_don_vi")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.CapDonVi)
                .HasColumnName("cap_don_vi")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.ConHoatDong)
                .HasColumnName("con_hoat_dong")
                .HasDefaultValue(true);
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.NgayCapNhat)
                .HasColumnName("ngay_cap_nhat")
                .HasColumnType("datetime2");
            entity.HasIndex(e => e.MaDonViCha)
                .HasDatabaseName("IX_DonVi_ma_don_vi_cha");
            entity.HasIndex(e => e.CapDonVi)
                .HasDatabaseName("IX_DonVi_cap_don_vi");
            entity.HasIndex(e => e.ConHoatDong)
                .HasDatabaseName("IX_DonVi_con_hoat_dong");
            entity.ToTable(t => t.HasCheckConstraint("CK_DonVi_cap_don_vi_1", "[cap_don_vi] IN (N'root', N'co_so', N'co_so_con')"));
            entity.HasOne(e => e.DonViCha)
                .WithMany(e => e.DonViCons)
                .HasForeignKey(e => e.MaDonViCha)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DonVi_ma_don_vi_cha__DonVi");
        });

        modelBuilder.Entity<GiaiDoanDangKy>(entity =>
        {
            entity.ToTable("GiaiDoanDangKy", "dbo");
            entity.HasKey(e => e.MaGiaiDoanDk).HasName("PK_GiaiDoanDangKy");
            entity.Property(e => e.MaGiaiDoanDk)
                .HasColumnName("ma_giai_doan_dk");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.BatDauLuc)
                .HasColumnName("bat_dau_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.KetThucLuc)
                .HasColumnName("ket_thuc_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("nhap");
            entity.Property(e => e.SoTinChiToiDa)
                .HasColumnName("so_tin_chi_toi_da");
            entity.ToTable(t => t.HasCheckConstraint("CK_GiaiDoanDangKy_trang_thai_1", "[trang_thai] IN (N'nhap', N'dang_mo', N'da_dong')"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_GiaiDoanDangKy_ma_don_vi__DonVi");
            entity.HasOne(e => e.HocKy)
                .WithMany()
                .HasForeignKey(e => e.MaHocKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_GiaiDoanDangKy_ma_hoc_ky__HocKy");
        });

        modelBuilder.Entity<GiaoDich>(entity =>
        {
            entity.ToTable("GiaoDich", "dbo");
            entity.HasKey(e => e.MaGiaoDich).HasName("PK_GiaoDich");
            entity.Property(e => e.MaGiaoDich)
                .HasColumnName("ma_giao_dich");
            entity.Property(e => e.MaHoaDon)
                .HasColumnName("ma_hoa_don");
            entity.Property(e => e.MaThamChieuCong)
                .HasColumnName("ma_tham_chieu_cong")
                .HasMaxLength(100);
            entity.Property(e => e.SoTien)
                .HasColumnName("so_tien")
                .HasColumnType("decimal(15,2)");
            entity.Property(e => e.LoaiGiaoDich)
                .HasColumnName("loai_giao_dich")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.DuLieuPhanHoi)
                .HasColumnName("du_lieu_phan_hoi")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.MaNguoiThucHien)
                .HasColumnName("ma_nguoi_thuc_hien");
            entity.HasIndex(e => e.MaThamChieuCong).IsUnique().HasDatabaseName("UQ_GiaoDich_1");
            entity.ToTable(t => t.HasCheckConstraint("CK_GiaoDich_so_tien_1", "[so_tien] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_GiaoDich_loai_giao_dich_2", "[loai_giao_dich] IN (N'thanh_toan', N'ghi_co', N'ghi_no', N'hoan_tien')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_GiaoDich_trang_thai_3", "[trang_thai] IN (N'dang_xu_ly', N'thanh_cong', N'that_bai', N'da_huy')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_GiaoDich_du_lieu_phan_hoi_ISJSON", "[du_lieu_phan_hoi] IS NULL OR ISJSON([du_lieu_phan_hoi]) = 1"));
            entity.HasOne(e => e.HoaDon)
                .WithMany()
                .HasForeignKey(e => e.MaHoaDon)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_GiaoDich_ma_hoa_don__HoaDon");
            entity.HasOne(e => e.NguoiThucHien)
                .WithMany()
                .HasForeignKey(e => e.MaNguoiThucHien)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_GiaoDich_ma_nguoi_thuc_hien__NguoiDung");
        });

        modelBuilder.Entity<HoSoKyLuat>(entity =>
        {
            entity.ToTable("HoSoKyLuat", "dbo");
            entity.HasKey(e => e.MaKyLuat).HasName("PK_HoSoKyLuat");
            entity.Property(e => e.MaKyLuat)
                .HasColumnName("ma_ky_luat");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.LoaiKyLuat)
                .HasColumnName("loai_ky_luat")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.MoTa)
                .HasColumnName("mo_ta")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.NguoiTao)
                .HasColumnName("nguoi_tao");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_HoSoKyLuat_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_HoSoKyLuat_ma_don_vi__DonVi");
            entity.HasOne(e => e.NguoiTaoNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiTao)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_HoSoKyLuat_nguoi_tao__NguoiDung");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.ToTable("HoaDon", "dbo");
            entity.HasKey(e => e.MaHoaDon).HasName("PK_HoaDon");
            entity.Property(e => e.MaHoaDon)
                .HasColumnName("ma_hoa_don");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.SoTien)
                .HasColumnName("so_tien")
                .HasColumnType("decimal(15,2)");
            entity.Property(e => e.GiamTru)
                .HasColumnName("giam_tru")
                .HasColumnType("decimal(15,2)")
                .HasDefaultValue(0m);
            entity.Property(e => e.DaThanhToan)
                .HasColumnName("da_thanh_toan")
                .HasColumnType("decimal(15,2)")
                .HasDefaultValue(0m);
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("chua_thanh_toan");
            entity.Property(e => e.PhuongThucTt)
                .HasColumnName("phuong_thuc_tt")
                .HasMaxLength(20);
            entity.Property(e => e.MaGiaoDichCong)
                .HasColumnName("ma_giao_dich_cong")
                .HasMaxLength(100);
            entity.Property(e => e.BatDauTtLuc)
                .HasColumnName("bat_dau_tt_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.HetHanTtLuc)
                .HasColumnName("het_han_tt_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.HanThanhToan)
                .HasColumnName("han_thanh_toan")
                .HasColumnType("date");
            entity.Property(e => e.UrlHoaDonPdf)
                .HasColumnName("url_hoa_don_pdf")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.ThoiDiemKhoiTaoTt)
                .HasColumnName("thoi_diem_khoi_tao_tt")
                .HasColumnType("datetime2");
            entity.Property(e => e.HetHanTt)
                .HasColumnName("het_han_tt")
                .HasColumnType("datetime2");
            entity.HasIndex(e => e.MaGiaoDichCong).IsUnique().HasDatabaseName("UQ_HoaDon_1");
            entity.ToTable(t => t.HasCheckConstraint("CK_HoaDon_so_tien_1", "[so_tien] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_HoaDon_giam_tru_2", "[giam_tru] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_HoaDon_da_thanh_toan_3", "[da_thanh_toan] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_HoaDon_trang_thai_4", "[trang_thai] IN (N'chua_thanh_toan', N'mot_phan', N'da_thanh_toan', N'qua_han', N'dang_xu_ly', N'that_bai')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_HoaDon_phuong_thuc_tt_5", "[phuong_thuc_tt] IN (N'vnpay', N'momo', N'chuyen_khoan')"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_HoaDon_ma_don_vi__DonVi");
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_HoaDon_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.HocKy)
                .WithMany()
                .HasForeignKey(e => e.MaHocKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_HoaDon_ma_hoc_ky__HocKy");
        });

        modelBuilder.Entity<HocKy>(entity =>
        {
            entity.ToTable("HocKy", "dbo");
            entity.HasKey(e => e.MaHocKy).HasName("PK_HocKy");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.MaCodeHocKy)
                .HasColumnName("ma_code_hoc_ky")
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(e => e.TenHocKy)
                .HasColumnName("ten_hoc_ky")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.NgayBatDau)
                .HasColumnName("ngay_bat_dau")
                .HasColumnType("date");
            entity.Property(e => e.NgayKetThuc)
                .HasColumnName("ngay_ket_thuc")
                .HasColumnType("date");
            entity.Property(e => e.DaKhoa)
                .HasColumnName("da_khoa")
                .HasDefaultValue(false);
            entity.Property(e => e.SoTinChiToiDa)
                .HasColumnName("so_tin_chi_toi_da");
            entity.Property(e => e.HanRutMon)
                .HasColumnName("han_rut_mon")
                .HasColumnType("date");
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_HocKy_ma_don_vi__DonVi");
        });

        modelBuilder.Entity<KhenThuong>(entity =>
        {
            entity.ToTable("KhenThuong", "dbo");
            entity.HasKey(e => e.MaKhenThuong).HasName("PK_KhenThuong");
            entity.Property(e => e.MaKhenThuong)
                .HasColumnName("ma_khen_thuong");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.LoaiKhenThuong)
                .HasColumnName("loai_khen_thuong")
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(e => e.GpaDatDuoc)
                .HasColumnName("gpa_dat_duoc")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.UrlChungTu)
                .HasColumnName("url_chung_tu")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.CapLuc)
                .HasColumnName("cap_luc")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.DaHuy)
                .HasColumnName("da_huy")
                .HasDefaultValue(false);
            entity.Property(e => e.GhiChuHuy)
                .HasColumnName("ghi_chu_huy")
                .HasColumnType("nvarchar(max)");
            entity.ToTable(t => t.HasCheckConstraint("CK_KhenThuong_loai_khen_thuong_1", "[loai_khen_thuong] IN (N'hoc_luc', N'dac_biet', N'thi_dau')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_KhenThuong_gpa_dat_duoc_2", "[gpa_dat_duoc] BETWEEN 0 AND 10"));
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_KhenThuong_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.HocKy)
                .WithMany()
                .HasForeignKey(e => e.MaHocKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_KhenThuong_ma_hoc_ky__HocKy");
        });

        modelBuilder.Entity<KhoaHoc>(entity =>
        {
            entity.ToTable("KhoaHoc", "dbo");
            entity.HasKey(e => e.MaKhoaHoc).HasName("PK_KhoaHoc");
            entity.Property(e => e.MaKhoaHoc)
                .HasColumnName("ma_khoa_hoc");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.MaGiaoVien)
                .HasColumnName("ma_giao_vien");
            entity.Property(e => e.MaMonHoc)
                .HasColumnName("ma_mon_hoc");
            entity.Property(e => e.TieuDe)
                .HasColumnName("tieu_de")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.MoTa)
                .HasColumnName("mo_ta")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("nhap");
            entity.Property(e => e.UrlAnhBia)
                .HasColumnName("url_anh_bia")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_KhoaHoc_trang_thai_1", "[trang_thai] IN (N'nhap', N'da_xuat_ban', N'luu_tru')"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_KhoaHoc_ma_don_vi__DonVi");
            entity.HasOne(e => e.GiaoVien)
                .WithMany()
                .HasForeignKey(e => e.MaGiaoVien)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_KhoaHoc_ma_giao_vien__NguoiDung");
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MaMonHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_KhoaHoc_ma_mon_hoc__DanhMucMonHoc");
        });

        modelBuilder.Entity<LienKetPhuHuynh>(entity =>
        {
            entity.ToTable("LienKetPhuHuynh", "dbo");
            entity.HasKey(e => e.MaLienKetPh).HasName("PK_LienKetPhuHuynh");
            entity.Property(e => e.MaLienKetPh)
                .HasColumnName("ma_lien_ket_ph");
            entity.Property(e => e.MaPhuHuynh)
                .HasColumnName("ma_phu_huynh");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.QuyenXem)
                .HasColumnName("quyen_xem")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("cho_duyet");
            entity.Property(e => e.LienKetLuc)
                .HasColumnName("lien_ket_luc")
                .HasColumnType("datetime2");
            entity.HasIndex(e => new { e.MaPhuHuynh, e.MaHocSinh }).IsUnique().HasDatabaseName("UQ_LienKetPhuHuynh_1");
            entity.ToTable(t => t.HasCheckConstraint("CK_LienKetPhuHuynh_trang_thai_1", "[trang_thai] IN (N'cho_duyet', N'hoat_dong', N'da_thu_hoi')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_LienKetPhuHuynh_quyen_xem_ISJSON", "[quyen_xem] IS NULL OR ISJSON([quyen_xem]) = 1"));
            entity.HasOne(e => e.PhuHuynh)
                .WithMany()
                .HasForeignKey(e => e.MaPhuHuynh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_LienKetPhuHuynh_ma_phu_huynh__NguoiDung");
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_LienKetPhuHuynh_ma_hoc_sinh__NguoiDung");
        });

        modelBuilder.Entity<LopHanhChinh>(entity =>
        {
            entity.ToTable("LopHanhChinh", "dbo");
            entity.HasKey(e => e.MaLop).HasName("PK_LopHanhChinh");
            entity.Property(e => e.MaLop)
                .HasColumnName("ma_lop");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.MaCodeLop)
                .HasColumnName("ma_code_lop")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.TenLop)
                .HasColumnName("ten_lop")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.MaGiaoVienChuNhiem)
                .HasColumnName("ma_giao_vien_chu_nhiem");
            entity.Property(e => e.NamNhapHoc)
                .HasColumnName("nam_nhap_hoc");
            entity.Property(e => e.ConHoatDong)
                .HasColumnName("con_hoat_dong")
                .HasDefaultValue(true);
            entity.HasIndex(e => e.MaCodeLop).IsUnique().HasDatabaseName("UQ_LopHanhChinh_1");
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_LopHanhChinh_ma_don_vi__DonVi");
            entity.HasOne(e => e.GiaoVienChuNhiem)
                .WithMany()
                .HasForeignKey(e => e.MaGiaoVienChuNhiem)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_LopHanhChinh_ma_giao_vien_chu_nhiem__NguoiDung");
        });

        modelBuilder.Entity<LopHocPhan>(entity =>
        {
            entity.ToTable("LopHocPhan", "dbo");
            entity.HasKey(e => e.MaLopHocPhan).HasName("PK_LopHocPhan");
            entity.Property(e => e.MaLopHocPhan)
                .HasColumnName("ma_lop_hoc_phan");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.MaMonHoc)
                .HasColumnName("ma_mon_hoc");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.MaCodeLopHocPhan)
                .HasColumnName("ma_code_lop_hoc_phan")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.SucChua)
                .HasColumnName("suc_chua");
            entity.Property(e => e.SoDangKyToiThieu)
                .HasColumnName("so_dang_ky_toi_thieu");
            entity.Property(e => e.SoDaDangKy)
                .HasColumnName("so_da_dang_ky")
                .HasDefaultValue(0);
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(30)
                .IsRequired()
                .HasDefaultValue("mo");
            entity.Property(e => e.QuotaVangToiDa)
                .HasColumnName("quota_vang_toi_da");
            entity.HasIndex(e => e.MaCodeLopHocPhan).IsUnique().HasDatabaseName("UQ_LopHocPhan_1");
            entity.ToTable(t => t.HasCheckConstraint("CK_LopHocPhan_suc_chua_1", "[suc_chua] > 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_LopHocPhan_trang_thai_2", "[trang_thai] IN (N'mo', N'dong', N'cho_huy', N'da_huy')"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_LopHocPhan_ma_don_vi__DonVi");
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MaMonHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_LopHocPhan_ma_mon_hoc__DanhMucMonHoc");
            entity.HasOne(e => e.HocKy)
                .WithMany()
                .HasForeignKey(e => e.MaHocKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_LopHocPhan_ma_hoc_ky__HocKy");
        });

        modelBuilder.Entity<MauThongBao>(entity =>
        {
            entity.ToTable("MauThongBao", "dbo");
            entity.HasKey(e => e.MaMauTb).HasName("PK_MauThongBao");
            entity.Property(e => e.MaMauTb)
                .HasColumnName("ma_mau_tb");
            entity.Property(e => e.LoaiSuKien)
                .HasColumnName("loai_su_kien")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.KenhGui)
                .HasColumnName("kenh_gui")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.MauTieuDe)
                .HasColumnName("mau_tieu_de")
                .HasMaxLength(500);
            entity.Property(e => e.MauNoiDung)
                .HasColumnName("mau_noi_dung")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.HasIndex(e => new { e.LoaiSuKien, e.KenhGui }).IsUnique().HasDatabaseName("UQ_MauThongBao_1");
            entity.ToTable(t => t.HasCheckConstraint("CK_MauThongBao_kenh_gui_1", "[kenh_gui] IN (N'email', N'thong_bao_day', N'sms')"));
        });

        modelBuilder.Entity<MonHocTienQuyet>(entity =>
        {
            entity.ToTable("MonHocTienQuyet", "dbo");
            entity.HasKey(e => new { e.MaMonHoc, e.MaMonTienQuyet }).HasName("PK_MonHocTienQuyet");
            entity.Property(e => e.MaMonHoc)
                .HasColumnName("ma_mon_hoc");
            entity.Property(e => e.MaMonTienQuyet)
                .HasColumnName("ma_mon_tien_quyet");
            entity.Property(e => e.DiemToiThieu)
                .HasColumnName("diem_toi_thieu")
                .HasColumnType("decimal(5,2)");
            entity.ToTable(t => t.HasCheckConstraint("CK_MonHocTienQuyet_diem_toi_thieu_1", "[diem_toi_thieu] BETWEEN 0 AND 10"));
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MaMonHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_MonHocTienQuyet_ma_mon_hoc__DanhMucMonHoc");
            entity.HasOne(e => e.MonTienQuyet)
                .WithMany()
                .HasForeignKey(e => e.MaMonTienQuyet)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_MonHocTienQuyet_ma_mon_tien_quyet__DanhMucMonHoc");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.ToTable("NguoiDung", "dbo");
            entity.HasKey(e => e.MaNguoiDung).HasName("PK_NguoiDung");
            entity.Property(e => e.MaNguoiDung)
                .HasColumnName("ma_nguoi_dung");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.HoTen)
                .HasColumnName("ho_ten")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.VaiTroChinh)
                .HasColumnName("vai_tro_chinh")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.MaLop)
                .HasColumnName("ma_lop");
            entity.Property(e => e.SoDienThoai)
                .HasColumnName("so_dien_thoai")
                .HasMaxLength(15);
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("dang_nhap_lan_dau");
            entity.Property(e => e.NamNhapHoc)
                .HasColumnName("nam_nhap_hoc");
            entity.Property(e => e.MatKhauHash)
                .HasColumnName("mat_khau_hash")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.LanDangNhapCuoi)
                .HasColumnName("lan_dang_nhap_cuoi")
                .HasColumnType("datetime2");
            entity.Property(e => e.SoLanSaiMatKhau)
                .HasColumnName("so_lan_sai_mat_khau")
                .HasDefaultValue(0);
            entity.Property(e => e.DangNhapLanDau)
                .HasColumnName("dang_nhap_lan_dau")
                .HasDefaultValue(true);
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_NguoiDung_1");
            entity.HasIndex(e => e.MaDonVi).HasDatabaseName("IX_NguoiDung_ma_don_vi");
            entity.ToTable(t => t.HasCheckConstraint("CK_NguoiDung_vai_tro_chinh_1", "[vai_tro_chinh] IN (N'quan_tri', N'giao_vien', N'hoc_sinh', N'nhan_vien', N'hieu_truong', N'phu_huynh', N'sieu_quan_tri', N'quan_tri_co_so', N'quan_tri_co_so_con')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_NguoiDung_trang_thai_2", "[trang_thai] IN (N'hoat_dong', N'bi_khoa', N'dang_nhap_lan_dau')"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NguoiDung_ma_don_vi__DonVi");
            entity.HasOne(e => e.Lop)
                .WithMany()
                .HasForeignKey(e => e.MaLop)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NguoiDung_ma_lop__LopHanhChinh");
        });

        modelBuilder.Entity<NhatKyDuyetDon>(entity =>
        {
            entity.ToTable("NhatKyDuyetDon", "dbo");
            entity.HasKey(e => e.MaNkDuyet).HasName("PK_NhatKyDuyetDon");
            entity.Property(e => e.MaNkDuyet)
                .HasColumnName("ma_nk_duyet");
            entity.Property(e => e.MaDonTu)
                .HasColumnName("ma_don_tu");
            entity.Property(e => e.MaNguoiDuyet)
                .HasColumnName("ma_nguoi_duyet");
            entity.Property(e => e.HanhDong)
                .HasColumnName("hanh_dong")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.GhiChu)
                .HasColumnName("ghi_chu")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_NhatKyDuyetDon_hanh_dong_1", "[hanh_dong] IN (N'nop', N'phe_duyet', N'tu_choi', N'phan_cong', N'leo_thang')"));
            entity.HasOne(e => e.DonTu)
                .WithMany()
                .HasForeignKey(e => e.MaDonTu)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NhatKyDuyetDon_ma_don_tu__DonTu");
            entity.HasOne(e => e.NguoiDuyet)
                .WithMany()
                .HasForeignKey(e => e.MaNguoiDuyet)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NhatKyDuyetDon_ma_nguoi_duyet__NguoiDung");
        });

        modelBuilder.Entity<NhatKyKiemToan>(entity =>
        {
            entity.ToTable("NhatKyKiemToan", "dbo");
            entity.HasKey(e => e.MaKiemToan).HasName("PK_NhatKyKiemToan");
            entity.Property(e => e.MaKiemToan)
                .HasColumnName("ma_kiem_toan");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.LoaiDoiTuong)
                .HasColumnName("loai_doi_tuong")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.MaDoiTuong)
                .HasColumnName("ma_doi_tuong");
            entity.Property(e => e.HanhDong)
                .HasColumnName("hanh_dong")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.GiaTriCu)
                .HasColumnName("gia_tri_cu")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.GiaTriMoi)
                .HasColumnName("gia_tri_moi")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NguoiThayDoi)
                .HasColumnName("nguoi_thay_doi");
            entity.Property(e => e.ThoiDiemThayDoi)
                .HasColumnName("thoi_diem_thay_doi")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.MaDonVi, e.ThoiDiemThayDoi }).HasDatabaseName("IX_NhatKyKiemToan_ma_don_vi_thoi_diem");
            entity.ToTable(t => t.HasCheckConstraint("CK_NhatKyKiemToan_gia_tri_cu_ISJSON", "[gia_tri_cu] IS NULL OR ISJSON([gia_tri_cu]) = 1"));
            entity.ToTable(t => t.HasCheckConstraint("CK_NhatKyKiemToan_gia_tri_moi_ISJSON", "[gia_tri_moi] IS NULL OR ISJSON([gia_tri_moi]) = 1"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NhatKyKiemToan_ma_don_vi__DonVi");
            entity.HasOne(e => e.NguoiThayDoiNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiThayDoi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NhatKyKiemToan_nguoi_thay_doi__NguoiDung");
        });

        modelBuilder.Entity<NhatKyThayDoiDiem>(entity =>
        {
            entity.ToTable("NhatKyThayDoiDiem", "dbo");
            entity.HasKey(e => e.MaNkThayDoi).HasName("PK_NhatKyThayDoiDiem");
            entity.Property(e => e.MaNkThayDoi)
                .HasColumnName("ma_nk_thay_doi");
            entity.Property(e => e.MaDiemSo)
                .HasColumnName("ma_diem_so");
            entity.Property(e => e.NguoiThayDoi)
                .HasColumnName("nguoi_thay_doi");
            entity.Property(e => e.GiaTriCu)
                .HasColumnName("gia_tri_cu")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.GiaTriMoi)
                .HasColumnName("gia_tri_moi")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.LyDo)
                .HasColumnName("ly_do")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.NguoiDuyet)
                .HasColumnName("nguoi_duyet");
            entity.Property(e => e.ThayDoiLuc)
                .HasColumnName("thay_doi_luc")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_NhatKyThayDoiDiem_gia_tri_cu_ISJSON", "[gia_tri_cu] IS NULL OR ISJSON([gia_tri_cu]) = 1"));
            entity.ToTable(t => t.HasCheckConstraint("CK_NhatKyThayDoiDiem_gia_tri_moi_ISJSON", "[gia_tri_moi] IS NULL OR ISJSON([gia_tri_moi]) = 1"));
            entity.HasOne(e => e.DiemSo)
                .WithMany()
                .HasForeignKey(e => e.MaDiemSo)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NhatKyThayDoiDiem_ma_diem_so__DiemSo");
            entity.HasOne(e => e.NguoiThayDoiNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiThayDoi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NhatKyThayDoiDiem_nguoi_thay_doi__NguoiDung");
            entity.HasOne(e => e.NguoiDuyetNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiDuyet)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NhatKyThayDoiDiem_nguoi_duyet__NguoiDung");
        });

        modelBuilder.Entity<NhatKyThongBao>(entity =>
        {
            entity.ToTable("NhatKyThongBao", "dbo");
            entity.HasKey(e => e.MaNkThongBao).HasName("PK_NhatKyThongBao");
            entity.Property(e => e.MaNkThongBao)
                .HasColumnName("ma_nk_thong_bao");
            entity.Property(e => e.MaThongBao)
                .HasColumnName("ma_thong_bao");
            entity.Property(e => e.MaNguoiNhan)
                .HasColumnName("ma_nguoi_nhan");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.KenhGui)
                .HasColumnName("kenh_gui")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.GuiLuc)
                .HasColumnName("gui_luc")
                .HasColumnType("datetime2");
            entity.HasIndex(e => new { e.MaDonVi, e.GuiLuc }).HasDatabaseName("IX_NhatKyThongBao_ma_don_vi_gui_luc");
            entity.ToTable(t => t.HasCheckConstraint("CK_NhatKyThongBao_trang_thai_1", "[trang_thai] IN (N'cho_gui', N'da_gui', N'da_nhan', N'that_bai', N'bo_qua')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_NhatKyThongBao_kenh_gui_2", "[kenh_gui] IN (N'email', N'thong_bao_day', N'sms')"));
            entity.HasOne(e => e.ThongBao)
                .WithMany()
                .HasForeignKey(e => e.MaThongBao)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NhatKyThongBao_ma_thong_bao__ThongBao");
            entity.HasOne(e => e.NguoiNhan)
                .WithMany()
                .HasForeignKey(e => e.MaNguoiNhan)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NhatKyThongBao_ma_nguoi_nhan__NguoiDung");
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NhatKyThongBao_ma_don_vi__DonVi");
        });

        modelBuilder.Entity<NopBaiDanhGia>(entity =>
        {
            entity.ToTable("NopBaiDanhGia", "dbo");
            entity.HasKey(e => e.MaNopDg).HasName("PK_NopBaiDanhGia");
            entity.Property(e => e.MaNopDg)
                .HasColumnName("ma_nop_dg");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaGiaoVien)
                .HasColumnName("ma_giao_vien");
            entity.Property(e => e.MaHocKy)
                .HasColumnName("ma_hoc_ky");
            entity.Property(e => e.SoLanNop)
                .HasColumnName("so_lan_nop")
                .HasDefaultValue(1);
            entity.Property(e => e.CapNhatLuc)
                .HasColumnName("cap_nhat_luc")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.SoLanSua)
                .HasColumnName("so_lan_sua")
                .HasDefaultValue(0);
            entity.HasIndex(e => new { e.MaHocSinh, e.MaGiaoVien, e.MaHocKy }).IsUnique().HasDatabaseName("UQ_NopBaiDanhGia_1");
            entity.ToTable(t => t.HasCheckConstraint("CK_NopBaiDanhGia_so_lan_nop_1", "[so_lan_nop] BETWEEN 0 AND 2"));
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NopBaiDanhGia_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.GiaoVien)
                .WithMany()
                .HasForeignKey(e => e.MaGiaoVien)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NopBaiDanhGia_ma_giao_vien__NguoiDung");
            entity.HasOne(e => e.HocKy)
                .WithMany()
                .HasForeignKey(e => e.MaHocKy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_NopBaiDanhGia_ma_hoc_ky__HocKy");
        });

        modelBuilder.Entity<PhanQuyenNguoiDung>(entity =>
        {
            entity.ToTable("PhanQuyenNguoiDung", "dbo");
            entity.HasKey(e => new { e.MaNguoiDung, e.MaVaiTro }).HasName("PK_PhanQuyenNguoiDung");
            entity.Property(e => e.MaNguoiDung)
                .HasColumnName("ma_nguoi_dung");
            entity.Property(e => e.MaVaiTro)
                .HasColumnName("ma_vai_tro");
            entity.Property(e => e.NgayGan)
                .HasColumnName("ngay_gan")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.NguoiDung)
                .WithMany()
                .HasForeignKey(e => e.MaNguoiDung)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_PhanQuyenNguoiDung_ma_nguoi_dung__NguoiDung");
            entity.HasOne(e => e.VaiTro)
                .WithMany()
                .HasForeignKey(e => e.MaVaiTro)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_PhanQuyenNguoiDung_ma_vai_tro__VaiTro");
        });

        modelBuilder.Entity<PhienThiHocSinh>(entity =>
        {
            entity.ToTable("PhienThiHocSinh", "dbo");
            entity.HasKey(e => e.MaPhienThi).HasName("PK_PhienThiHocSinh");
            entity.Property(e => e.MaPhienThi)
                .HasColumnName("ma_phien_thi");
            entity.Property(e => e.MaDeKiemTra)
                .HasColumnName("ma_de_kiem_tra");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.BatDauLuc)
                .HasColumnName("bat_dau_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.NopLuc)
                .HasColumnName("nop_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.CauTraLoiJson)
                .HasColumnName("cau_tra_loi_json")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NhatKyViPham)
                .HasColumnName("nhat_ky_vi_pham")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.SaoLuuCucBo)
                .HasColumnName("sao_luu_cuc_bo")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TrangThaiLuong)
                .HasColumnName("trang_thai_luong")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("dang_hoat_dong");
            entity.Property(e => e.DiemTuDong)
                .HasColumnName("diem_tu_dong")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiemCuoiCung)
                .HasColumnName("diem_cuoi_cung")
                .HasColumnType("decimal(5,2)");
            entity.Property(e => e.DiemTuLuanAiGoiY)
                .HasColumnName("diem_tu_luan_ai_goi_y")
                .HasColumnType("decimal(5,2)");
            entity.HasIndex(e => new { e.MaDeKiemTra, e.MaHocSinh }).IsUnique().HasDatabaseName("UQ_PhienThiHocSinh_1");
            entity.ToTable(t => t.HasCheckConstraint("CK_PhienThiHocSinh_trang_thai_luong_1", "[trang_thai_luong] IN (N'dang_hoat_dong', N'bi_gian_doan', N'da_dung')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PhienThiHocSinh_cau_tra_loi_json_ISJSON", "[cau_tra_loi_json] IS NULL OR ISJSON([cau_tra_loi_json]) = 1"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PhienThiHocSinh_nhat_ky_vi_pham_ISJSON", "[nhat_ky_vi_pham] IS NULL OR ISJSON([nhat_ky_vi_pham]) = 1"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PhienThiHocSinh_sao_luu_cuc_bo_ISJSON", "[sao_luu_cuc_bo] IS NULL OR ISJSON([sao_luu_cuc_bo]) = 1"));
            entity.HasOne(e => e.DeKiemTra)
                .WithMany()
                .HasForeignKey(e => e.MaDeKiemTra)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_PhienThiHocSinh_ma_de_kiem_tra__DeKiemTra");
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_PhienThiHocSinh_ma_hoc_sinh__NguoiDung");
        });

        modelBuilder.Entity<PhieuHoTro>(entity =>
        {
            entity.ToTable("PhieuHoTro", "dbo");
            entity.HasKey(e => e.MaPhieuHt).HasName("PK_PhieuHoTro");
            entity.Property(e => e.MaPhieuHt)
                .HasColumnName("ma_phieu_ht");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.DanhMuc)
                .HasColumnName("danh_muc")
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(e => e.TieuDe)
                .HasColumnName("tieu_de")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.MoTa)
                .HasColumnName("mo_ta")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("mo");
            entity.Property(e => e.PhanCongCho)
                .HasColumnName("phan_cong_cho");
            entity.Property(e => e.HanXuLy)
                .HasColumnName("han_xu_ly")
                .HasColumnType("datetime2");
            entity.Property(e => e.DanhGiaHaiLong)
                .HasColumnName("danh_gia_hai_long");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.DoUuTien)
                .HasColumnName("do_uu_tien")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("medium");
            entity.ToTable(t => t.HasCheckConstraint("CK_PhieuHoTro_danh_muc_1", "[danh_muc] IN (N'ky_thuat', N'hoc_vu', N'tai_chinh', N'khac')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PhieuHoTro_trang_thai_2", "[trang_thai] IN (N'mo', N'dang_xu_ly', N'da_giai_quyet', N'da_dong')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PhieuHoTro_danh_gia_hai_long_3", "[danh_gia_hai_long] BETWEEN 1 AND 5"));
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_PhieuHoTro_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.PhanCongChoNavigation)
                .WithMany()
                .HasForeignKey(e => e.PhanCongCho)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_PhieuHoTro_phan_cong_cho__NguoiDung");
        });

        modelBuilder.Entity<PhongHoc>(entity =>
        {
            entity.ToTable("PhongHoc", "dbo");
            entity.HasKey(e => e.MaPhong).HasName("PK_PhongHoc");
            entity.Property(e => e.MaPhong)
                .HasColumnName("ma_phong");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.MaCodePhong)
                .HasColumnName("ma_code_phong")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.TenPhong)
                .HasColumnName("ten_phong")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.SucChua)
                .HasColumnName("suc_chua");
            entity.Property(e => e.LoaiPhong)
                .HasColumnName("loai_phong")
                .HasMaxLength(30)
                .IsRequired();
            entity.Property(e => e.TrangThaiPhong)
                .HasColumnName("trang_thai_phong")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("hoat_dong");
            entity.HasIndex(e => e.MaCodePhong).IsUnique().HasDatabaseName("UQ_PhongHoc_1");
            entity.ToTable(t => t.HasCheckConstraint("CK_PhongHoc_suc_chua_1", "[suc_chua] > 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PhongHoc_loai_phong_2", "[loai_phong] IN (N'ly_thuyet', N'phong_thi_nghiem', N'hoi_truong', N'truc_tuyen', N'khac')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_PhongHoc_trang_thai_phong_3", "[trang_thai_phong] IN (N'hoat_dong', N'bao_tri', N'ngung_hoat_dong')"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_PhongHoc_ma_don_vi__DonVi");
        });

        modelBuilder.Entity<ThietBiPhong>(entity =>
        {
            entity.ToTable("ThietBiPhong", "dbo");
            entity.HasKey(e => e.MaThietBi).HasName("PK_ThietBiPhong");
            entity.Property(e => e.MaThietBi)
                .HasColumnName("ma_thiet_bi");
            entity.Property(e => e.MaPhong)
                .HasColumnName("ma_phong");
            entity.Property(e => e.TenThietBi)
                .HasColumnName("ten_thiet_bi")
                .HasMaxLength(255)
                .IsRequired();
            entity.Property(e => e.SoLuong)
                .HasColumnName("so_luong")
                .HasDefaultValue(1);
            entity.ToTable(t => t.HasCheckConstraint("CK_ThietBiPhong_so_luong_1", "[so_luong] >= 0"));
            entity.HasOne(e => e.Phong)
                .WithMany()
                .HasForeignKey(e => e.MaPhong)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThietBiPhong_ma_phong__PhongHoc");
        });

        modelBuilder.Entity<ThoiKhoaBieu>(entity =>
        {
            entity.ToTable("ThoiKhoaBieu", "dbo");
            entity.HasKey(e => e.MaTkb).HasName("PK_ThoiKhoaBieu");
            entity.Property(e => e.MaTkb)
                .HasColumnName("ma_tkb");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.MaGiaoVien)
                .HasColumnName("ma_giao_vien");
            entity.Property(e => e.MaGiaoVienDayThay)
                .HasColumnName("ma_giao_vien_day_thay");
            entity.Property(e => e.MaMonHoc)
                .HasColumnName("ma_mon_hoc");
            entity.Property(e => e.MaPhong)
                .HasColumnName("ma_phong");
            entity.Property(e => e.MaLop)
                .HasColumnName("ma_lop");
            entity.Property(e => e.MaLopHocPhan)
                .HasColumnName("ma_lop_hoc_phan");
            entity.Property(e => e.ThuTrongTuan)
                .HasColumnName("thu_trong_tuan");
            entity.Property(e => e.GioBatDau)
                .HasColumnName("gio_bat_dau")
                .HasColumnType("time");
            entity.Property(e => e.GioKetThuc)
                .HasColumnName("gio_ket_thuc")
                .HasColumnType("time");
            entity.Property(e => e.DuongDanHop)
                .HasColumnName("duong_dan_hop")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("nhap");
            entity.Property(e => e.BuChoBuoi)
                .HasColumnName("bu_cho_buoi");
            entity.HasIndex(e => new { e.MaGiaoVien, e.ThuTrongTuan, e.GioBatDau }).IsUnique().HasDatabaseName("UQ_ThoiKhoaBieu_1");
            entity.HasIndex(e => new { e.MaPhong, e.ThuTrongTuan, e.GioBatDau }).IsUnique().HasDatabaseName("UQ_ThoiKhoaBieu_2");
            entity.ToTable(t => t.HasCheckConstraint("CK_ThoiKhoaBieu_thu_trong_tuan_1", "[thu_trong_tuan] BETWEEN 1 AND 7"));
            entity.ToTable(t => t.HasCheckConstraint("CK_ThoiKhoaBieu_gio_ket_thuc_2", "[gio_ket_thuc] > [gio_bat_dau]"));
            entity.ToTable(t => t.HasCheckConstraint("CK_ThoiKhoaBieu_trang_thai_3", "[trang_thai] IN (N'nhap', N'da_duyet', N'da_xuat_ban', N'da_huy')"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThoiKhoaBieu_ma_don_vi__DonVi");
            entity.HasOne(e => e.GiaoVien)
                .WithMany()
                .HasForeignKey(e => e.MaGiaoVien)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThoiKhoaBieu_ma_giao_vien__NguoiDung");
            entity.HasOne(e => e.GiaoVienDayThay)
                .WithMany()
                .HasForeignKey(e => e.MaGiaoVienDayThay)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThoiKhoaBieu_ma_giao_vien_day_thay__NguoiDung");
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MaMonHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThoiKhoaBieu_ma_mon_hoc__DanhMucMonHoc");
            entity.HasOne(e => e.Phong)
                .WithMany()
                .HasForeignKey(e => e.MaPhong)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThoiKhoaBieu_ma_phong__PhongHoc");
            entity.HasOne(e => e.Lop)
                .WithMany()
                .HasForeignKey(e => e.MaLop)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThoiKhoaBieu_ma_lop__LopHanhChinh");
            entity.HasOne(e => e.LopHocPhan)
                .WithMany()
                .HasForeignKey(e => e.MaLopHocPhan)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThoiKhoaBieu_ma_lop_hoc_phan__LopHocPhan");
            entity.HasOne(e => e.BuChoBuoiNavigation)
                .WithMany()
                .HasForeignKey(e => e.BuChoBuoi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThoiKhoaBieu_bu_cho_buoi__ThoiKhoaBieu");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.ToTable("ThongBao", "dbo");
            entity.HasKey(e => e.MaThongBao).HasName("PK_ThongBao");
            entity.Property(e => e.MaThongBao)
                .HasColumnName("ma_thong_bao");
            entity.Property(e => e.MaNguoiNhan)
                .HasColumnName("ma_nguoi_nhan");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.LoaiSuKien)
                .HasColumnName("loai_su_kien")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.TieuDe)
                .HasColumnName("tieu_de")
                .HasMaxLength(500);
            entity.Property(e => e.NoiDung)
                .HasColumnName("noi_dung")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.DaDoc)
                .HasColumnName("da_doc")
                .HasDefaultValue(false);
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => new { e.MaNguoiNhan, e.DaDoc }).HasDatabaseName("IX_ThongBao_NguoiNhan_DaDoc");
            entity.HasOne(e => e.NguoiNhan)
                .WithMany()
                .HasForeignKey(e => e.MaNguoiNhan)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThongBao_ma_nguoi_nhan__NguoiDung");
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThongBao_ma_don_vi__DonVi");
        });

        modelBuilder.Entity<ThongBaoHenGio>(entity =>
        {
            entity.ToTable("ThongBaoHenGio", "dbo");
            entity.HasKey(e => e.MaTbHenGio).HasName("PK_ThongBaoHenGio");
            entity.Property(e => e.MaTbHenGio)
                .HasColumnName("ma_tb_hen_gio");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.NguoiTao)
                .HasColumnName("nguoi_tao");
            entity.Property(e => e.LoaiSuKien)
                .HasColumnName("loai_su_kien")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.BoLocNguoiNhan)
                .HasColumnName("bo_loc_nguoi_nhan")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.GuiLuc)
                .HasColumnName("gui_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("da_len_lich");
            entity.ToTable(t => t.HasCheckConstraint("CK_ThongBaoHenGio_trang_thai_1", "[trang_thai] IN (N'da_len_lich', N'dang_cho', N'da_huy', N'hoan_thanh')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_ThongBaoHenGio_bo_loc_nguoi_nhan_ISJSON", "[bo_loc_nguoi_nhan] IS NULL OR ISJSON([bo_loc_nguoi_nhan]) = 1"));
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThongBaoHenGio_ma_don_vi__DonVi");
            entity.HasOne(e => e.NguoiTaoNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiTao)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThongBaoHenGio_nguoi_tao__NguoiDung");
        });

        modelBuilder.Entity<TienDoBaiHoc>(entity =>
        {
            entity.ToTable("TienDoBaiHoc", "dbo");
            entity.HasKey(e => e.MaTienDo).HasName("PK_TienDoBaiHoc");
            entity.Property(e => e.MaTienDo)
                .HasColumnName("ma_tien_do");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaBaiHoc)
                .HasColumnName("ma_bai_hoc");
            entity.Property(e => e.PhanTramTienDo)
                .HasColumnName("phan_tram_tien_do")
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0m);
            entity.Property(e => e.LanGuiNhipTimCuoi)
                .HasColumnName("lan_gui_nhip_tim_cuoi")
                .HasColumnType("datetime2");
            entity.Property(e => e.HoanThanhLuc)
                .HasColumnName("hoan_thanh_luc")
                .HasColumnType("datetime2");
            entity.HasIndex(e => new { e.MaHocSinh, e.MaBaiHoc }).IsUnique().HasDatabaseName("UQ_TienDoBaiHoc_1");
            entity.ToTable(t => t.HasCheckConstraint("CK_TienDoBaiHoc_phan_tram_tien_do_1", "[phan_tram_tien_do] BETWEEN 0 AND 100"));
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TienDoBaiHoc_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.BaiHoc)
                .WithMany()
                .HasForeignKey(e => e.MaBaiHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TienDoBaiHoc_ma_bai_hoc__BaiHoc");
        });

        modelBuilder.Entity<TinNhanHoTro>(entity =>
        {
            entity.ToTable("TinNhanHoTro", "dbo");
            entity.HasKey(e => e.MaTinNhanHt).HasName("PK_TinNhanHoTro");
            entity.Property(e => e.MaTinNhanHt)
                .HasColumnName("ma_tin_nhan_ht");
            entity.Property(e => e.MaPhieuHt)
                .HasColumnName("ma_phieu_ht");
            entity.Property(e => e.MaNguoiGui)
                .HasColumnName("ma_nguoi_gui");
            entity.Property(e => e.NoiDung)
                .HasColumnName("noi_dung")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.UrlDinhKem)
                .HasColumnName("url_dinh_kem")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.PhieuHt)
                .WithMany()
                .HasForeignKey(e => e.MaPhieuHt)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TinNhanHoTro_ma_phieu_ht__PhieuHoTro");
            entity.HasOne(e => e.NguoiGui)
                .WithMany()
                .HasForeignKey(e => e.MaNguoiGui)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TinNhanHoTro_ma_nguoi_gui__NguoiDung");
        });

        modelBuilder.Entity<TokenLamMoi>(entity =>
        {
            entity.ToTable("TokenLamMoi", "dbo");
            entity.HasKey(e => e.MaTokenLamMoi).HasName("PK_TokenLamMoi");
            entity.Property(e => e.MaTokenLamMoi)
                .HasColumnName("ma_token_lam_moi");
            entity.Property(e => e.MaNguoiDung)
                .HasColumnName("ma_nguoi_dung");
            entity.Property(e => e.TokenHash)
                .HasColumnName("token_hash")
                .HasMaxLength(128)
                .IsRequired();
            entity.Property(e => e.HetHanLuc)
                .HasColumnName("het_han_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.ThuHoiLuc)
                .HasColumnName("thu_hoi_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => e.TokenHash).IsUnique().HasDatabaseName("UQ_TokenLamMoi_1");
            entity.HasOne(e => e.NguoiDung)
                .WithMany()
                .HasForeignKey(e => e.MaNguoiDung)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TokenLamMoi_ma_nguoi_dung__NguoiDung");
        });

        modelBuilder.Entity<TuyChonThongBao>(entity =>
        {
            entity.ToTable("TuyChonThongBao", "dbo");
            entity.HasKey(e => e.MaNguoiDung).HasName("PK_TuyChonThongBao");
            entity.Property(e => e.MaNguoiDung)
                .HasColumnName("ma_nguoi_dung")
                .ValueGeneratedNever();
            entity.Property(e => e.NhanEmail)
                .HasColumnName("nhan_email")
                .HasDefaultValue(true);
            entity.Property(e => e.NhanPush)
                .HasColumnName("nhan_push")
                .HasDefaultValue(true);
            entity.Property(e => e.NhanSms)
                .HasColumnName("nhan_sms")
                .HasDefaultValue(false);
            entity.Property(e => e.CapNhatLuc)
                .HasColumnName("cap_nhat_luc")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(e => e.NguoiDung)
                .WithMany()
                .HasForeignKey(e => e.MaNguoiDung)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TuyChonThongBao_ma_nguoi_dung__NguoiDung");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.ToTable("VaiTro", "dbo");
            entity.HasKey(e => e.MaVaiTro).HasName("PK_VaiTro");
            entity.Property(e => e.MaVaiTro)
                .HasColumnName("ma_vai_tro")
                .ValueGeneratedNever();
            entity.Property(e => e.MaCodeVaiTro)
                .HasColumnName("ma_code_vai_tro")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.TenVaiTro)
                .HasColumnName("ten_vai_tro")
                .HasMaxLength(100)
                .IsRequired();
            entity.HasIndex(e => e.MaCodeVaiTro).IsUnique().HasDatabaseName("UQ_VaiTro_1");
        });

        modelBuilder.Entity<XuatBaoCao>(entity =>
        {
            entity.ToTable("XuatBaoCao", "dbo");
            entity.HasKey(e => e.MaXuatBaoCao).HasName("PK_XuatBaoCao");
            entity.Property(e => e.MaXuatBaoCao)
                .HasColumnName("ma_xuat_bao_cao");
            entity.Property(e => e.NguoiYeuCau)
                .HasColumnName("nguoi_yeu_cau");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.LoaiBaoCao)
                .HasColumnName("loai_bao_cao")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.ThamSoJson)
                .HasColumnName("tham_so_json")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.UrlTapTin)
                .HasColumnName("url_tap_tin")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("cho_xu_ly");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_XuatBaoCao_trang_thai_1", "[trang_thai] IN (N'cho_xu_ly', N'dang_xu_ly', N'hoan_thanh', N'that_bai')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_XuatBaoCao_tham_so_json_ISJSON", "[tham_so_json] IS NULL OR ISJSON([tham_so_json]) = 1"));
            entity.HasOne(e => e.NguoiYeuCauNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiYeuCau)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_XuatBaoCao_nguoi_yeu_cau__NguoiDung");
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_XuatBaoCao_ma_don_vi__DonVi");
        });

        modelBuilder.Entity<YeuCauDoiLich>(entity =>
        {
            entity.ToTable("YeuCauDoiLich", "dbo");
            entity.HasKey(e => e.MaYcDoiLich).HasName("PK_YeuCauDoiLich");
            entity.Property(e => e.MaYcDoiLich)
                .HasColumnName("ma_yc_doi_lich");
            entity.Property(e => e.MaTkb)
                .HasColumnName("ma_tkb");
            entity.Property(e => e.GiaoVienDeXuat)
                .HasColumnName("giao_vien_de_xuat");
            entity.Property(e => e.GiaoVienNhanDoi)
                .HasColumnName("giao_vien_nhan_doi");
            entity.Property(e => e.LyDo)
                .HasColumnName("ly_do")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(30)
                .IsRequired()
                .HasDefaultValue("cho_gv_nhan_dong_y");
            entity.Property(e => e.NguoiDuyet)
                .HasColumnName("nguoi_duyet");
            entity.Property(e => e.GvNhanPhanHoiLuc)
                .HasColumnName("gv_nhan_phan_hoi_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.AdminDuyetLuc)
                .HasColumnName("admin_duyet_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.ToTable(t => t.HasCheckConstraint("CK_YeuCauDoiLich_trang_thai_1", "[trang_thai] IN (N'cho_gv_nhan_dong_y', N'cho_admin_duyet', N'da_hoan_doi', N'tu_choi', N'da_huy')"));
            entity.HasOne(e => e.Tkb)
                .WithMany()
                .HasForeignKey(e => e.MaTkb)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauDoiLich_ma_tkb__ThoiKhoaBieu");
            entity.HasOne(e => e.GiaoVienDeXuatNavigation)
                .WithMany()
                .HasForeignKey(e => e.GiaoVienDeXuat)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauDoiLich_gv_de_xuat__NguoiDung");
            entity.HasOne(e => e.GiaoVienNhanDoiNavigation)
                .WithMany()
                .HasForeignKey(e => e.GiaoVienNhanDoi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauDoiLich_gv_nhan_doi__NguoiDung");
            entity.HasOne(e => e.NguoiDuyetNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiDuyet)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauDoiLich_nguoi_duyet__NguoiDung");
        });

        modelBuilder.Entity<YeuCauHoanPhi>(entity =>
        {
            entity.ToTable("YeuCauHoanPhi", "dbo");
            entity.HasKey(e => e.MaHoanPhi).HasName("PK_YeuCauHoanPhi");
            entity.Property(e => e.MaHoanPhi)
                .HasColumnName("ma_hoan_phi");
            entity.Property(e => e.MaHoaDon)
                .HasColumnName("ma_hoa_don");
            entity.Property(e => e.MaHocSinh)
                .HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaDonVi)
                .HasColumnName("ma_don_vi");
            entity.Property(e => e.SoTienYeuCau)
                .HasColumnName("so_tien_yeu_cau")
                .HasColumnType("decimal(15,2)");
            entity.Property(e => e.LoaiHoanPhi)
                .HasColumnName("loai_hoan_phi")
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("cho_duyet");
            entity.Property(e => e.NguoiDuyet)
                .HasColumnName("nguoi_duyet");
            entity.Property(e => e.XuLyLuc)
                .HasColumnName("xu_ly_luc")
                .HasColumnType("datetime2");
            entity.ToTable(t => t.HasCheckConstraint("CK_YeuCauHoanPhi_so_tien_yeu_cau_1", "[so_tien_yeu_cau] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_YeuCauHoanPhi_loai_hoan_phi_2", "[loai_hoan_phi] IN (N'toan_phan', N'mot_phan', N'ghi_co')"));
            entity.ToTable(t => t.HasCheckConstraint("CK_YeuCauHoanPhi_trang_thai_3", "[trang_thai] IN (N'cho_duyet', N'da_duyet', N'tu_choi', N'da_xu_ly')"));
            entity.HasOne(e => e.HoaDon)
                .WithMany()
                .HasForeignKey(e => e.MaHoaDon)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauHoanPhi_ma_hoa_don__HoaDon");
            entity.HasOne(e => e.HocSinh)
                .WithMany()
                .HasForeignKey(e => e.MaHocSinh)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauHoanPhi_ma_hoc_sinh__NguoiDung");
            entity.HasOne(e => e.DonVi)
                .WithMany()
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauHoanPhi_ma_don_vi__DonVi");
            entity.HasOne(e => e.NguoiDuyetNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiDuyet)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauHoanPhi_nguoi_duyet__NguoiDung");
        });

        modelBuilder.Entity<YeuCauMoKhoaDiemDanh>(entity =>
        {
            entity.ToTable("YeuCauMoKhoaDiemDanh", "dbo");
            entity.HasKey(e => e.MaYcMoKhoa).HasName("PK_YeuCauMoKhoaDiemDanh");
            entity.Property(e => e.MaYcMoKhoa)
                .HasColumnName("ma_yc_mo_khoa");
            entity.Property(e => e.MaBuoiHoc)
                .HasColumnName("ma_buoi_hoc");
            entity.Property(e => e.NguoiYeuCau)
                .HasColumnName("nguoi_yeu_cau");
            entity.Property(e => e.LyDo)
                .HasColumnName("ly_do")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("cho_duyet");
            entity.Property(e => e.NguoiDuyet)
                .HasColumnName("nguoi_duyet");
            entity.Property(e => e.MoKhoaDenLuc)
                .HasColumnName("mo_khoa_den_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.NgayTao)
                .HasColumnName("ngay_tao")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasIndex(e => e.MaBuoiHoc).IsUnique().HasDatabaseName("UX_YeuCauMoKhoaDiemDanh_ChoDuyet").HasFilter("[trang_thai] = N'cho_duyet'");
            entity.ToTable(t => t.HasCheckConstraint("CK_YeuCauMoKhoaDiemDanh_trang_thai_1", "[trang_thai] IN (N'cho_duyet', N'da_duyet', N'tu_choi', N'het_han')"));
            entity.HasOne(e => e.BuoiHoc)
                .WithMany()
                .HasForeignKey(e => e.MaBuoiHoc)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauMoKhoaDiemDanh_ma_buoi_hoc__BuoiHoc");
            entity.HasOne(e => e.NguoiYeuCauNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiYeuCau)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauMoKhoaDiemDanh_nguoi_yeu_cau__NguoiDung");
            entity.HasOne(e => e.NguoiDuyetNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiDuyet)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauMoKhoaDiemDanh_nguoi_duyet__NguoiDung");
        });

        modelBuilder.Entity<YeuCauSuaDiem>(entity =>
        {
            entity.ToTable("YeuCauSuaDiem", "dbo");
            entity.HasKey(e => e.MaYcSuaDiem).HasName("PK_YeuCauSuaDiem");
            entity.Property(e => e.MaYcSuaDiem)
                .HasColumnName("ma_yc_sua_diem");
            entity.Property(e => e.MaDiemSo)
                .HasColumnName("ma_diem_so");
            entity.Property(e => e.NguoiYeuCau)
                .HasColumnName("nguoi_yeu_cau");
            entity.Property(e => e.LyDo)
                .HasColumnName("ly_do")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(e => e.UrlBangChung)
                .HasColumnName("url_bang_chung")
                .HasColumnType("nvarchar(max)");
            entity.Property(e => e.TrangThai)
                .HasColumnName("trang_thai")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("cho_duyet");
            entity.Property(e => e.NguoiDuyet)
                .HasColumnName("nguoi_duyet");
            entity.Property(e => e.MoDenLuc)
                .HasColumnName("mo_den_luc")
                .HasColumnType("datetime2");
            entity.Property(e => e.LoaiYeuCau)
                .HasColumnName("loai_yeu_cau")
                .HasMaxLength(30)
                .IsRequired()
                .HasDefaultValue("sua_sau_submit");
            entity.Property(e => e.UnlockExpiresAt)
                .HasColumnName("unlock_expires_at")
                .HasColumnType("datetime2");
            entity.Property(e => e.CotDiemDuocMo)
                .HasColumnName("cot_diem_duoc_mo")
                .HasMaxLength(30);
            entity.ToTable(t => t.HasCheckConstraint("CK_YeuCauSuaDiem_trang_thai_1", "[trang_thai] IN (N'cho_duyet', N'da_duyet', N'tu_choi', N'het_han')"));
            entity.HasOne(e => e.DiemSo)
                .WithMany()
                .HasForeignKey(e => e.MaDiemSo)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauSuaDiem_ma_diem_so__DiemSo");
            entity.HasOne(e => e.NguoiYeuCauNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiYeuCau)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauSuaDiem_nguoi_yeu_cau__NguoiDung");
            entity.HasOne(e => e.NguoiDuyetNavigation)
                .WithMany()
                .HasForeignKey(e => e.NguoiDuyet)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_YeuCauSuaDiem_nguoi_duyet__NguoiDung");
        });

    }
}
