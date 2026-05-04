namespace Backend.Models;

public class PhanQuyenNguoiDung
{
    public int MaNguoiDung { get; set; }
    public int MaVaiTro { get; set; }
    public DateTime NgayGan { get; set; }

    public NguoiDung? NguoiDung { get; set; }
    public VaiTro? VaiTro { get; set; }
}
