namespace Backend.Models;

public class CanhBaoBaoMat
{
    public int MaCanhBao { get; set; }
    public int MaNguoiDung { get; set; }
    public decimal DiemRuiRo { get; set; }
    public string? DiaChiIp { get; set; }
    public string? ThongTinTrinhDuyet { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }

    public NguoiDung? NguoiDung { get; set; }
}
