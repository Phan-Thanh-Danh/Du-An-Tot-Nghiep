namespace Backend.Models;

public class DiemDanh
{
    public int MaDiemDanh { get; set; }
    public int MaDonVi { get; set; }
    public int MaBuoiHoc { get; set; }
    public int MaHocSinh { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int NguoiGhiNhan { get; set; }
    public DateTime GhiNhanLuc { get; set; }
    public DateTime? KhoaLuc { get; set; }
    public int HeSoVang { get; set; }
    public int? MaYcMoKhoa { get; set; }

    public DonVi? DonVi { get; set; }
    public BuoiHoc? BuoiHoc { get; set; }
    public NguoiDung? HocSinh { get; set; }
    public NguoiDung? NguoiGhiNhanNavigation { get; set; }
    public YeuCauMoKhoaDiemDanh? YcMoKhoa { get; set; }
}
