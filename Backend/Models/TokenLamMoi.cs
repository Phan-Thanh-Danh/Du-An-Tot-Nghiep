namespace Backend.Models;

public class TokenLamMoi
{
    public int MaTokenLamMoi { get; set; }
    public int MaNguoiDung { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime HetHanLuc { get; set; }
    public DateTime? ThuHoiLuc { get; set; }
    public DateTime NgayTao { get; set; }

    public NguoiDung? NguoiDung { get; set; }
}
