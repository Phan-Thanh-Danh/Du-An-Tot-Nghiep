namespace Backend.Models;

public class ThiSinhCaThi
{
    public int MaThiSinhCaThi { get; set; }
    public int MaCaThi { get; set; }
    public int MaHocSinh { get; set; }
    public string TrangThaiDuThi { get; set; } = "cho_thi";
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }

    public CaThi? CaThi { get; set; }
    public NguoiDung? HocSinh { get; set; }
}
