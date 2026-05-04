namespace Backend.Models;

public class DeKiemTra
{
    public int MaDeKiemTra { get; set; }
    public int? MaMonHoc { get; set; }
    public int? MaHocKy { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public int ThoiGianPhut { get; set; }
    public string CauHinhDeThi { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;

    public DanhMucMonHoc? MonHoc { get; set; }
    public HocKy? HocKy { get; set; }
}
