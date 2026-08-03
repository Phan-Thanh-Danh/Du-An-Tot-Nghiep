namespace Backend.Models;

public class BaiTap
{
    public int MaBaiTap { get; set; }
    public int MaMonHoc { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public DateTime HanNop { get; set; }
    public int SoLanNopToiDa { get; set; }
    public string DinhDangChoPhep { get; set; } = string.Empty;
    public string? HuongDanChamDiem { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int DungLuongToiThieuKB { get; set; } = 10; // Default 10KB
    public int DungLuongToiDaMB { get; set; } = 50; // Default 50MB
    public int? MaCauHinhDauDiem { get; set; }

    public DanhMucMonHoc? MonHoc { get; set; }
    public CauHinhDauDiemQuaTrinh? CauHinhDauDiem { get; set; }
}
