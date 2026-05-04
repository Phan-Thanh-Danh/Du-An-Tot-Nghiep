namespace Backend.Models;

public class LopHanhChinh
{
    public int MaLop { get; set; }
    public int MaDonVi { get; set; }
    public string MaCodeLop { get; set; } = string.Empty;
    public string TenLop { get; set; } = string.Empty;
    public int? MaGiaoVienChuNhiem { get; set; }
    public int? NamNhapHoc { get; set; }
    public bool ConHoatDong { get; set; }

    public DonVi? DonVi { get; set; }
    public NguoiDung? GiaoVienChuNhiem { get; set; }
}
