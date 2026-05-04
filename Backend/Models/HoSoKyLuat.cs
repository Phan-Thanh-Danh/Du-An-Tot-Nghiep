namespace Backend.Models;

public class HoSoKyLuat
{
    public int MaKyLuat { get; set; }
    public int MaHocSinh { get; set; }
    public int MaDonVi { get; set; }
    public string LoaiKyLuat { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public int NguoiTao { get; set; }
    public DateTime NgayTao { get; set; }

    public NguoiDung? HocSinh { get; set; }
    public DonVi? DonVi { get; set; }
    public NguoiDung? NguoiTaoNavigation { get; set; }
}
