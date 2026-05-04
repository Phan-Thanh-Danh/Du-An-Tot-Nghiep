namespace Backend.Models;

public class GiaiDoanDangKy
{
    public int MaGiaiDoanDk { get; set; }
    public int MaDonVi { get; set; }
    public int MaHocKy { get; set; }
    public DateTime BatDauLuc { get; set; }
    public DateTime KetThucLuc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int SoTinChiToiDa { get; set; }

    public DonVi? DonVi { get; set; }
    public HocKy? HocKy { get; set; }
}
