namespace Backend.Models;

public class CauHinhDauDiemQuaTrinh
{
    public int MaCauHinhDauDiem { get; set; }
    public int MaMonHoc { get; set; }
    public int MaHocKy { get; set; }
    public int MaLoaiDauDiem { get; set; }
    public int SoLuongCot { get; set; }
    public decimal TrongSoNoiBo { get; set; }

    public DanhMucMonHoc? MonHoc { get; set; }
    public HocKy? HocKy { get; set; }
    public LoaiDauDiemQuaTrinh? LoaiDauDiem { get; set; }
}
