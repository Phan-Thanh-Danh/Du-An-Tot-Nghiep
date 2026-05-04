namespace Backend.Models;

public class AnhChupPhanTich
{
    public int MaAnhChup { get; set; }
    public int MaDonVi { get; set; }
    public int? MaHocKy { get; set; }
    public DateOnly NgayAnhChup { get; set; }
    public string LoaiChiSo { get; set; } = string.Empty;
    public decimal GiaTriChiSo { get; set; }
    public string? ChieuLocJson { get; set; }
    public DateTime NgayTao { get; set; }

    public DonVi? DonVi { get; set; }
    public HocKy? HocKy { get; set; }
}
