namespace Backend.Models;

public class BinhLuan
{
    public int MaBinhLuan { get; set; }
    public int MaBaiHoc { get; set; }
    public int MaNguoiDung { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public int? GiayTrongVideo { get; set; }
    public int? SoTrangPdf { get; set; }
    public int? MaBinhLuanCha { get; set; }
    public bool DaGhim { get; set; }
    public DateTime NgayTao { get; set; }

    public BaiHoc? BaiHoc { get; set; }
    public NguoiDung? NguoiDung { get; set; }
    public BinhLuan? BinhLuanCha { get; set; }
}
