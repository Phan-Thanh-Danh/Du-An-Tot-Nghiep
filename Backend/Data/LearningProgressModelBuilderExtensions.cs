using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class LearningProgressModelBuilderExtensions
{
    public static void ConfigureLearningProgressModels(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TienDoNoiDungHocTap>(entity =>
        {
            entity.ToTable("TienDoNoiDungHocTap", "dbo");
            entity.HasKey(e => e.MaTienDoNoiDung).HasName("PK_TienDoNoiDungHocTap");
            entity.Property(e => e.MaTienDoNoiDung).HasColumnName("ma_tien_do_noi_dung");
            entity.Property(e => e.MaHocSinh).HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaNoiDung).HasColumnName("ma_noi_dung");
            entity.Property(e => e.LoaiNoiDung).HasColumnName("loai_noi_dung").HasMaxLength(50).IsRequired();
            entity.Property(e => e.TrangThai).HasColumnName("trang_thai").HasMaxLength(30).IsRequired().HasDefaultValue("chua_bat_dau");
            entity.Property(e => e.PhanTramTienDo).HasColumnName("phan_tram_tien_do").HasColumnType("decimal(5,2)").HasDefaultValue(0m);
            entity.Property(e => e.SoGiayDaXacNhan).HasColumnName("so_giay_da_xac_nhan").HasDefaultValue(0);
            entity.Property(e => e.ViTriVideoCuoiGiay).HasColumnName("vi_tri_video_cuoi_giay");
            entity.Property(e => e.PhanTramCuonLonNhat).HasColumnName("phan_tram_cuon_lon_nhat").HasColumnType("decimal(5,2)");
            entity.Property(e => e.ChiSoMucCuoi).HasColumnName("chi_so_muc_cuoi");
            entity.Property(e => e.SoMucDaXem).HasColumnName("so_muc_da_xem");
            entity.Property(e => e.TongSoMuc).HasColumnName("tong_so_muc");
            entity.Property(e => e.BatDauLuc).HasColumnName("bat_dau_luc").HasColumnType("datetime2");
            entity.Property(e => e.LanTuongTacCuoi).HasColumnName("lan_tuong_tac_cuoi").HasColumnType("datetime2");
            entity.Property(e => e.HoanThanhLuc).HasColumnName("hoan_thanh_luc").HasColumnType("datetime2");
            entity.Property(e => e.NgayTao).HasColumnName("ngay_tao").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(e => e.NgayCapNhat).HasColumnName("ngay_cap_nhat").HasColumnType("datetime2");
            entity.Property(e => e.ChiTietTienDoJson).HasColumnName("chi_tiet_tien_do_json").HasColumnType("nvarchar(max)");

            entity.HasIndex(e => new { e.MaHocSinh, e.MaNoiDung }).IsUnique().HasDatabaseName("UQ_TienDoNoiDungHocTap_HocSinh_NoiDung");
            entity.HasIndex(e => e.MaHocSinh).HasDatabaseName("IX_TienDoNoiDungHocTap_MaHocSinh");
            entity.HasIndex(e => e.MaNoiDung).HasDatabaseName("IX_TienDoNoiDungHocTap_MaNoiDung");
            entity.HasIndex(e => e.TrangThai).HasDatabaseName("IX_TienDoNoiDungHocTap_TrangThai");
            entity.HasIndex(e => e.LanTuongTacCuoi).HasDatabaseName("IX_TienDoNoiDungHocTap_LanTuongTacCuoi");

            entity.ToTable(t => t.HasCheckConstraint("CK_TienDoNoiDungHocTap_PhanTramTienDo", "[phan_tram_tien_do] BETWEEN 0 AND 100"));
            entity.ToTable(t => t.HasCheckConstraint("CK_TienDoNoiDungHocTap_PhanTramCuonLonNhat", "[phan_tram_cuon_lon_nhat] IS NULL OR [phan_tram_cuon_lon_nhat] BETWEEN 0 AND 100"));
            entity.ToTable(t => t.HasCheckConstraint("CK_TienDoNoiDungHocTap_SoGiayDaXacNhan", "[so_giay_da_xac_nhan] >= 0"));
            entity.ToTable(t => t.HasCheckConstraint("CK_TienDoNoiDungHocTap_TrangThai", "[trang_thai] IN (N'chua_bat_dau', N'dang_hoc', N'hoan_thanh')"));

            entity.HasOne(e => e.HocSinh).WithMany().HasForeignKey(e => e.MaHocSinh).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_TienDoNoiDungHocTap_MaHocSinh_NguoiDung");
            entity.HasOne(e => e.NoiDung).WithMany().HasForeignKey(e => e.MaNoiDung).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_TienDoNoiDungHocTap_MaNoiDung_BaiHocNoiDung");
        });

        modelBuilder.Entity<PhienHocNoiDung>(entity =>
        {
            entity.ToTable("PhienHocNoiDung", "dbo");
            entity.HasKey(e => e.MaPhienHoc).HasName("PK_PhienHocNoiDung");
            entity.Property(e => e.MaPhienHoc).HasColumnName("ma_phien_hoc");
            entity.Property(e => e.SessionToken).HasColumnName("session_token").IsRequired();
            entity.Property(e => e.MaHocSinh).HasColumnName("ma_hoc_sinh");
            entity.Property(e => e.MaNoiDung).HasColumnName("ma_noi_dung");
            entity.Property(e => e.BatDauLuc).HasColumnName("bat_dau_luc").HasColumnType("datetime2").IsRequired();
            entity.Property(e => e.NhipTimCuoiLuc).HasColumnName("nhip_tim_cuoi_luc").HasColumnType("datetime2");
            entity.Property(e => e.KetThucLuc).HasColumnName("ket_thuc_luc").HasColumnType("datetime2");
            entity.Property(e => e.SoGiayHoatDongDaXacNhan).HasColumnName("so_giay_hoat_dong_da_xac_nhan").HasDefaultValue(0);
            entity.Property(e => e.ViTriVideoCuoiGiay).HasColumnName("vi_tri_video_cuoi_giay");
            entity.Property(e => e.PhanTramCuonLonNhat).HasColumnName("phan_tram_cuon_lon_nhat").HasColumnType("decimal(5,2)");
            entity.Property(e => e.SoThuTuNhipTimCuoi).HasColumnName("so_thu_tu_nhip_tim_cuoi").HasDefaultValue(0);
            entity.Property(e => e.TrangThai).HasColumnName("trang_thai").HasMaxLength(30).IsRequired().HasDefaultValue("dang_hoat_dong");
            entity.Property(e => e.UserAgentHash).HasColumnName("user_agent_hash").HasMaxLength(255);
            entity.Property(e => e.DiaChiIpHash).HasColumnName("dia_chi_ip_hash").HasMaxLength(255);
            entity.Property(e => e.NgayTao).HasColumnName("ngay_tao").HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(e => e.SessionToken).IsUnique().HasDatabaseName("UQ_PhienHocNoiDung_SessionToken");
            entity.HasIndex(e => new { e.MaHocSinh, e.MaNoiDung, e.TrangThai }).HasDatabaseName("IX_PhienHocNoiDung_HocSinh_NoiDung_TrangThai");
            entity.HasIndex(e => e.NhipTimCuoiLuc).HasDatabaseName("IX_PhienHocNoiDung_NhipTimCuoiLuc");

            entity.ToTable(t => t.HasCheckConstraint("CK_PhienHocNoiDung_TrangThai", "[trang_thai] IN (N'dang_hoat_dong', N'da_ket_thuc', N'het_han', N'bi_thay_the')"));

            entity.HasOne(e => e.HocSinh).WithMany().HasForeignKey(e => e.MaHocSinh).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_PhienHocNoiDung_MaHocSinh_NguoiDung");
            entity.HasOne(e => e.NoiDung).WithMany().HasForeignKey(e => e.MaNoiDung).OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_PhienHocNoiDung_MaNoiDung_BaiHocNoiDung");
        });
    }
}
