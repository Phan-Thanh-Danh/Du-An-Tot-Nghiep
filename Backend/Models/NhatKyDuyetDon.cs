namespace Backend.Models;

public class NhatKyDuyetDon
{
    public int MaNkDuyet { get; set; }
    public int MaDonTu { get; set; }
    public int MaNguoiDuyet { get; set; }
    public string HanhDong { get; set; } = string.Empty;
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }

    public DonTu? DonTu { get; set; }
    public NguoiDung? NguoiDuyet { get; set; }
}
