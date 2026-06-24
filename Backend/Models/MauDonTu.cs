namespace Backend.Models;

public class MauDonTu
{
    public int MaMauDon { get; set; }
    public string LoaiDon { get; set; } = string.Empty;
    public string TenMau { get; set; } = string.Empty;
    public int PhienBan { get; set; }
    public string CauHinhJson { get; set; } = string.Empty;
    public bool BatBuocMinhChung { get; set; }
    public int SoTepToiDa { get; set; }
    public long DungLuongTepToiDaByte { get; set; }
    public long TongDungLuongToiDaByte { get; set; }
    public int? SlaGio { get; set; }
    public bool DangHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
}
