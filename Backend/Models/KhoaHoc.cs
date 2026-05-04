namespace Backend.Models;

public class KhoaHoc
{
    public int MaKhoaHoc { get; set; }
    public int MaDonVi { get; set; }
    public int MaGiaoVien { get; set; }
    public int MaMonHoc { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? UrlAnhBia { get; set; }
    public DateTime NgayTao { get; set; }

    public DonVi? DonVi { get; set; }
    public NguoiDung? GiaoVien { get; set; }
    public DanhMucMonHoc? MonHoc { get; set; }
}
