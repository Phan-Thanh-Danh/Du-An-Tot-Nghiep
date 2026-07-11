namespace Backend.Models;

public class GiaoVienNguyenVongHocKy
{
    public int Id { get; set; }
    public int MaGiaoVien { get; set; }
    public int MaHocKy { get; set; }
    public int MaDonVi { get; set; }
    public int? SoLopToiDaMongMuon { get; set; }
    public int? SoCaToiDaMoiTuan { get; set; }
    public string? GhiChu { get; set; }
    public string TrangThai { get; set; } = "draft";
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public DateTime? NgayGui { get; set; }
    public byte[]? RowVersion { get; set; }

    public virtual NguoiDung GiaoVien { get; set; } = null!;
    public virtual HocKy HocKy { get; set; } = null!;
    public virtual DonVi DonVi { get; set; } = null!;
    public virtual ICollection<GiaoVienNguyenVongCaDay> ChiTietNguyenVong { get; set; } = new List<GiaoVienNguyenVongCaDay>();
}
