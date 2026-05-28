namespace Backend.DTOs.Floors;

public class FloorDto
{
    public int MaTang { get; set; }
    public int MaToaNha { get; set; }
    public string MaCodeToaNha { get; set; } = string.Empty;
    public string TenToaNha { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public string TenTang { get; set; } = string.Empty;
    public int ThuTuTang { get; set; }
    public string? MoTa { get; set; }
    public bool ConHoatDong { get; set; }
}
