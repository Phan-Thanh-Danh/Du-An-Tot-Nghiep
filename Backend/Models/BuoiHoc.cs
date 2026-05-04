namespace Backend.Models;

public class BuoiHoc
{
    public int MaBuoiHoc { get; set; }
    public int MaTkb { get; set; }
    public DateOnly NgayHoc { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public string TrangThaiBuoi { get; set; } = string.Empty;
    public DateTime? KhoaLuc { get; set; }

    public ThoiKhoaBieu? Tkb { get; set; }
}
