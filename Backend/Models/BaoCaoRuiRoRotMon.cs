namespace Backend.Models;

public class BaoCaoRuiRoRotMon
{
    public int MaBaoCaoRot { get; set; }
    public int MaHocSinh { get; set; }
    public int MaMonHoc { get; set; }
    public int MaHocKy { get; set; }
    public decimal XacSuatRotMon { get; set; }
    public string? DacTrungJson { get; set; }
    public DateTime TaoLuc { get; set; }

    public NguoiDung? HocSinh { get; set; }
    public DanhMucMonHoc? MonHoc { get; set; }
    public HocKy? HocKy { get; set; }
}
