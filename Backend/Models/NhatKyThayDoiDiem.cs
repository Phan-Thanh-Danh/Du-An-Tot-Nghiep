namespace Backend.Models;

public class NhatKyThayDoiDiem
{
    public int MaNkThayDoi { get; set; }
    public int MaDiemSo { get; set; }
    public int NguoiThayDoi { get; set; }
    public string GiaTriCu { get; set; } = string.Empty;
    public string GiaTriMoi { get; set; } = string.Empty;
    public string LyDo { get; set; } = string.Empty;
    public int? NguoiDuyet { get; set; }
    public DateTime ThayDoiLuc { get; set; }

    public DiemSo? DiemSo { get; set; }
    public NguoiDung? NguoiThayDoiNavigation { get; set; }
    public NguoiDung? NguoiDuyetNavigation { get; set; }
}
