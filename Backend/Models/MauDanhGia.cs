namespace Backend.Models;

public class MauDanhGia
{
    public int MaMauDanhGia { get; set; }
    public string TenMau { get; set; } = string.Empty;
    public string CauHinhJson { get; set; } = string.Empty;
    public bool DangHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}
