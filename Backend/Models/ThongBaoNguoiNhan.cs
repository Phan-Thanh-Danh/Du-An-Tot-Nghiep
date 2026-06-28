namespace Backend.Models;

public class ThongBaoNguoiNhan
{
    public int MaThongBaoNguoiNhan { get; set; }
    public int MaThongBao { get; set; }
    public int MaNguoiNhan { get; set; }
    public int MaDonVi { get; set; }
    public bool DaDoc { get; set; }
    public DateTime? DocLuc { get; set; }
    public bool DaAn { get; set; }
    public DateTime? AnLuc { get; set; }
    public DateTime NhanLuc { get; set; }
    public DateTime NgayTao { get; set; }

    public ThongBao? ThongBao { get; set; }
    public NguoiDung? NguoiNhan { get; set; }
    public DonVi? DonVi { get; set; }
}
