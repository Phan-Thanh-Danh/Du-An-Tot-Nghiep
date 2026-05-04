namespace Backend.Models;

public class TienDoBaiHoc
{
    public int MaTienDo { get; set; }
    public int MaHocSinh { get; set; }
    public int MaBaiHoc { get; set; }
    public decimal PhanTramTienDo { get; set; }
    public DateTime? LanGuiNhipTimCuoi { get; set; }
    public DateTime? HoanThanhLuc { get; set; }

    public NguoiDung? HocSinh { get; set; }
    public BaiHoc? BaiHoc { get; set; }
}
