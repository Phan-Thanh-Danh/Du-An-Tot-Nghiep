namespace Backend.Models;

public class PhieuHoTro
{
    public int MaPhieuHt { get; set; }
    public int MaHocSinh { get; set; }
    public string DanhMuc { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public int? PhanCongCho { get; set; }
    public DateTime? HanXuLy { get; set; }
    public int? DanhGiaHaiLong { get; set; }
    public DateTime NgayTao { get; set; }
    public string DoUuTien { get; set; } = string.Empty;

    public NguoiDung? HocSinh { get; set; }
    public NguoiDung? PhanCongChoNavigation { get; set; }
}
