namespace Backend.Models;

public class MonHocTienQuyet
{
    public int MaMonHoc { get; set; }
    public int MaMonTienQuyet { get; set; }
    public decimal? DiemToiThieu { get; set; }

    public DanhMucMonHoc? MonHoc { get; set; }
    public DanhMucMonHoc? MonTienQuyet { get; set; }
}
