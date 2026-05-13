namespace Backend.DTOs.AdministrativeClasses;

public class AdminClassDto
{
    public int MaLop { get; set; }
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public string MaCodeLop { get; set; } = string.Empty;
    public string TenLop { get; set; } = string.Empty;
    public int? MaGiaoVienChuNhiem { get; set; }
    public string? TenGiaoVienChuNhiem { get; set; }
    public int? NamNhapHoc { get; set; }
    public bool ConHoatDong { get; set; }
}
