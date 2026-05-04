namespace Backend.Models;

public class HocKy
{
    public int MaHocKy { get; set; }
    public int MaDonVi { get; set; }
    public string MaCodeHocKy { get; set; } = string.Empty;
    public string TenHocKy { get; set; } = string.Empty;
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
    public bool DaKhoa { get; set; }
    public int? SoTinChiToiDa { get; set; }
    public DateOnly? HanRutMon { get; set; }

    public DonVi? DonVi { get; set; }
}
