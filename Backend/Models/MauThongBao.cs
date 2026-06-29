namespace Backend.Models;

public class MauThongBao
{
    public int MaMauTb { get; set; }
    public string LoaiSuKien { get; set; } = string.Empty;
    public string KenhGui { get; set; } = string.Empty;
    public string? MauTieuDe { get; set; }
    public string MauNoiDung { get; set; } = string.Empty;

    // New NT-TEMPLATE fields
    public int? MaDonVi { get; set; }
    public string? TenMau { get; set; }
    public string? MaMau { get; set; }
    public string? LoaiThongBao { get; set; }
    public string? MucDoUuTien { get; set; }
    public string? DoiTuongMacDinh { get; set; }
    public string? BienChoPhepJson { get; set; }
    public bool DangHoatDong { get; set; } = true;
    public bool LaHeThong { get; set; }
    public DateTime NgayTao { get; set; }
    public int? NguoiTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public int? NguoiCapNhat { get; set; }

    public DonVi? DonVi { get; set; }
    public NguoiDung? NguoiTaoNavigation { get; set; }
    public NguoiDung? NguoiCapNhatNavigation { get; set; }
}
