namespace Backend.Models;

public class NhatKyDuyetDon
{
    public int MaNkDuyet { get; set; }
    public int MaDonTu { get; set; }
    public int? MaNguoiDuyet { get; set; }
    public string NguonThucHien { get; set; } = string.Empty;
    public string HanhDong { get; set; } = string.Empty;
    public string? TrangThaiCu { get; set; }
    public string? TrangThaiMoi { get; set; }
    public string? GhiChu { get; set; }
    public string? GhiChuCongKhai { get; set; }
    public string? GhiChuNoiBo { get; set; }
    public string? SnapshotJson { get; set; }
    public bool HienThiChoHocSinh { get; set; }
    public DateTime NgayTao { get; set; }

    public DonTu? DonTu { get; set; }
    public NguoiDung? NguoiDuyet { get; set; }
}
