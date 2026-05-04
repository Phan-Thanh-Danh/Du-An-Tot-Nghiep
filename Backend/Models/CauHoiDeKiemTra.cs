namespace Backend.Models;

public class CauHoiDeKiemTra
{
    public int MaDeKiemTra { get; set; }
    public int MaCauHoi { get; set; }
    public decimal DiemSo { get; set; }
    public int? ThuTu { get; set; }

    public DeKiemTra? DeKiemTra { get; set; }
    public CauHoi? CauHoi { get; set; }
}
