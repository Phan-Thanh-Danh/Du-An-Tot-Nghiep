namespace Backend.Models;

public class CauHinhDiemMonHoc
{
    public int MaCauHinhDiem { get; set; }
    public int MaMonHoc { get; set; }
    public int MaHocKy { get; set; }
    public decimal TrongSoQuaTrinh { get; set; }
    public decimal TrongSoGiuaKy { get; set; }
    public decimal TrongSoCuoiKy { get; set; }
    public decimal NguongDat { get; set; }

    public DanhMucMonHoc? MonHoc { get; set; }
    public HocKy? HocKy { get; set; }
}
