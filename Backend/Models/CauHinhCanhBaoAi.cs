namespace Backend.Models;

public class CauHinhCanhBaoAi
{
    public int MaCauHinh { get; set; }
    public string TenQuyTac { get; set; } = string.Empty;
    public string DieuKienKichHoat { get; set; } = string.Empty;
    public int NguongTriSo { get; set; }
    public string KenhNhan { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
}
