namespace Backend.Models;

public class QuyDinhChuyenCan
{
    public int MaQuyDinh { get; set; }
    public int MaDonVi { get; set; }
    public DateTime NgayHieuLuc { get; set; }
    public int QuyVangToiDa { get; set; }
    public decimal TiLeCanhBao { get; set; }
    public decimal HeSoVangKhongPhep { get; set; }
    public decimal HeSoVangCoPhep { get; set; }
    public decimal HeSoDiMuon { get; set; }
    public int HanGuiPhut { get; set; }
    public int HanChinhSuaPhut { get; set; }
    public string? GhiChu { get; set; }
    public int NguoiTao { get; set; }
    public DateTime TaoLuc { get; set; }
    public int? NguoiCapNhat { get; set; }
    public DateTime? CapNhatLuc { get; set; }

    public DonVi? DonVi { get; set; }
    public NguoiDung? NguoiTaoNavigation { get; set; }
}
