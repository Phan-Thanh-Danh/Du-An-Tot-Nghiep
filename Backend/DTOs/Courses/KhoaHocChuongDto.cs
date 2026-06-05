namespace Backend.DTOs.Courses;

public class KhoaHocChuongDto
{
    public int MaChuong { get; set; }
    public int MaMonHoc { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public int ThuTu { get; set; }
    public bool DaAn { get; set; }
    public IReadOnlyList<KhoaHocBaiHocDto> BaiHocs { get; set; } = [];
}
