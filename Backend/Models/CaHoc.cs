namespace Backend.Models;

public class CaHoc
{
    public int MaCaHoc { get; set; }
    public string TenCa { get; set; } = string.Empty;
    public string Buoi { get; set; } = string.Empty;
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public int ThuTu { get; set; }
    public bool ConHoatDong { get; set; } = true;
}
