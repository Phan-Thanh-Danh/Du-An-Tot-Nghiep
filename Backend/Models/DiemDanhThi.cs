namespace Backend.Models;

public class DiemDanhThi
{
    public int MaDiemDanhThi { get; set; }
    public int MaCaThi { get; set; }
    public int MaHocSinh { get; set; }
    public string TrangThaiDiemDanh { get; set; } = "vang_mat";
    public DateTime? ThoiDiemDiemDanh { get; set; }
    public int? MaNguoiDiemDanh { get; set; }
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }

    public CaThi? CaThi { get; set; }
    public NguoiDung? HocSinh { get; set; }
    public NguoiDung? NguoiDiemDanh { get; set; }
}
