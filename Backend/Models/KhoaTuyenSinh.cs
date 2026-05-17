namespace Backend.Models;

public class KhoaTuyenSinh
{
    public int MaKhoaTuyenSinh { get; set; }
    public string MaCodeKhoa { get; set; } = string.Empty;
    public string TenKhoa { get; set; } = string.Empty;
    public int NamBatDau { get; set; }
    public int? NamKetThucDuKien { get; set; }
    public string? MoTa { get; set; }
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}
