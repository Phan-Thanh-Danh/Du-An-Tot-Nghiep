namespace Backend.Models;

public class BaoCaoRuiRoVang
{
    public int MaBaoCao { get; set; }
    public int MaHocSinh { get; set; }
    public int? MaMonHoc { get; set; }
    public decimal DiemRuiRo { get; set; }
    public string? DacTrungJson { get; set; }
    public DateTime TaoLuc { get; set; }

    public NguoiDung? HocSinh { get; set; }
    public DanhMucMonHoc? MonHoc { get; set; }
}
