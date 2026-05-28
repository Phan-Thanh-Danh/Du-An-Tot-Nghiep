namespace Backend.Models;

public class ChuongTrinhHocKy
{
    public int MaChuongTrinhHocKy { get; set; }
    public int MaChuongTrinh { get; set; }
    public int MaHocKy { get; set; }
    public int ThuTuHocKy { get; set; }

    public ChuongTrinhDaoTao? ChuongTrinhDaoTao { get; set; }
    public HocKy? HocKy { get; set; }
}
