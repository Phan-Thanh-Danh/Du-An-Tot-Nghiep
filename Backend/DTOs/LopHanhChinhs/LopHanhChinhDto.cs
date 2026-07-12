namespace Backend.DTOs.LopHanhChinhs;

public class LopHanhChinhDto
{
    public int MaLop { get; set; }
    public string MaCodeLop { get; set; } = string.Empty;
    public string TenLop { get; set; } = string.Empty;
    public int? NamNhapHoc { get; set; }
}
