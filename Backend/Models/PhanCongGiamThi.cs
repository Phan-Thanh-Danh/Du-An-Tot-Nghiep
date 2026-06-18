namespace Backend.Models;

public class PhanCongGiamThi
{
    public int MaPhanCong { get; set; }
    public int MaCaThi { get; set; }
    public int MaGiamThi { get; set; }
    public string VaiTroGiamThi { get; set; } = "giam_thi_phu";
    public string TrangThai { get; set; } = "du_kien";
    public string? LyDoThayDoi { get; set; }
    public DateTime NgayTao { get; set; }

    public CaThi? CaThi { get; set; }
    public NguoiDung? GiamThi { get; set; }
}
