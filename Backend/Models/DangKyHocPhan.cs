namespace Backend.Models;

public class DangKyHocPhan
{
    public int MaDangKy { get; set; }
    public int MaHocSinh { get; set; }
    public int MaLopHocPhan { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int? ViTriCho { get; set; }
    public bool LaHocLai { get; set; }
    public bool KiemTraTienQuyet { get; set; }
    public DateTime NgayTao { get; set; }
    public bool DaKiemTraTienQuyet { get; set; }

    public NguoiDung? HocSinh { get; set; }
    public LopHocPhan? LopHocPhan { get; set; }
}
