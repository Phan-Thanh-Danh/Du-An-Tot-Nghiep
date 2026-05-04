namespace Backend.Models;

public class CanhBaoDaoVan
{
    public int MaCanhBao { get; set; }
    public int MaBaiNop { get; set; }
    public decimal DiemDaoVan { get; set; }
    public string? ChiTiet { get; set; }
    public DateTime NgayTao { get; set; }

    public BaiNop? BaiNop { get; set; }
}
