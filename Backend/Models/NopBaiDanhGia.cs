namespace Backend.Models;

public class NopBaiDanhGia
{
    public int MaNopDg { get; set; }
    public int MaHocSinh { get; set; }
    public int MaGiaoVien { get; set; }
    public int MaHocKy { get; set; }
    public int SoLanNop { get; set; }
    public DateTime CapNhatLuc { get; set; }
    public int SoLanSua { get; set; }

    public NguoiDung? HocSinh { get; set; }
    public NguoiDung? GiaoVien { get; set; }
    public HocKy? HocKy { get; set; }
}
