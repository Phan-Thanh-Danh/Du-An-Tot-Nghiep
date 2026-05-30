namespace Backend.Models;

public class Tang
{
    public int MaTang { get; set; }
    public int MaToaNha { get; set; }
    public string TenTang { get; set; } = string.Empty;
    public int ThuTuTang { get; set; }
    public string? MoTa { get; set; }
    public bool ConHoatDong { get; set; }

    public ToaNha? ToaNha { get; set; }
}
