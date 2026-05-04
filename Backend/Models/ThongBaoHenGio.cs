namespace Backend.Models;

public class ThongBaoHenGio
{
    public int MaTbHenGio { get; set; }
    public int MaDonVi { get; set; }
    public int NguoiTao { get; set; }
    public string LoaiSuKien { get; set; } = string.Empty;
    public string BoLocNguoiNhan { get; set; } = string.Empty;
    public DateTime GuiLuc { get; set; }
    public string TrangThai { get; set; } = string.Empty;

    public DonVi? DonVi { get; set; }
    public NguoiDung? NguoiTaoNavigation { get; set; }
}
