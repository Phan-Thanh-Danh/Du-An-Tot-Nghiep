namespace Backend.Models;

public class CauHinhKhenThuong
{
    public int MaCauHinhKt { get; set; }
    public int MaDonVi { get; set; }
    public string LoaiKhenThuong { get; set; } = string.Empty;
    public decimal? GpaToiThieu { get; set; }
    public bool ConHoatDong { get; set; }

    public DonVi? DonVi { get; set; }
}
