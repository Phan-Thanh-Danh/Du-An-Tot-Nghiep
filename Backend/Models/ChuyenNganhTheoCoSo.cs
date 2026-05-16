namespace Backend.Models;

public class ChuyenNganhTheoCoSo
{
    public int MaChuyenNganhCoSo { get; set; }
    public int MaChuyenNganh { get; set; }
    public int MaDonVi { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int? NamBatDau { get; set; }
    public int? ChiTieuDuKien { get; set; }
    public string? GhiChu { get; set; }
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public ChuyenNganh? ChuyenNganh { get; set; }
    public DonVi? DonVi { get; set; }
}
