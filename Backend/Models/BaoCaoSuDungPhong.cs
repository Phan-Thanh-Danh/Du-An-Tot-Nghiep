namespace Backend.Models;

public class BaoCaoSuDungPhong
{
    public int MaBcSuDungPhong { get; set; }
    public int MaPhong { get; set; }
    public int MaDonVi { get; set; }
    public DateOnly TuNgay { get; set; }
    public DateOnly DenNgay { get; set; }
    public decimal SoGioSuDung { get; set; }
    public decimal? TiLeSuDung { get; set; }
    public DateTime TaoLuc { get; set; }

    public PhongHoc? Phong { get; set; }
    public DonVi? DonVi { get; set; }
}
