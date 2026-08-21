namespace Backend.Models;

public class DanhMucMonHoc
{
    public int MaMonHoc { get; set; }
    public string MaCodeMonHoc { get; set; } = string.Empty;
    public string TenMonHoc { get; set; } = string.Empty;
    public int SoTinChi { get; set; }
    public bool ConHoatDong { get; set; }
    public int? MaNganh { get; set; }
    public int? MaChuyenNganh { get; set; }

    public NganhDaoTao? Nganh { get; set; }
    public ChuyenNganh? ChuyenNganh { get; set; }
}
