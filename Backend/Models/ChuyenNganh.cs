namespace Backend.Models;

public class ChuyenNganh
{
    public int MaChuyenNganh { get; set; }
    public int MaNganh { get; set; }
    public string MaCodeChuyenNganh { get; set; } = string.Empty;
    public string TenChuyenNganh { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public NganhDaoTao? NganhDaoTao { get; set; }
    public ICollection<ChuyenNganhTheoCoSo> ChuyenNganhTheoCoSos { get; set; } = new List<ChuyenNganhTheoCoSo>();
}
