namespace Backend.Models;

public class VaiTro
{
    public int MaVaiTro { get; set; }
    public string MaCodeVaiTro { get; set; } = string.Empty;
    public string TenVaiTro { get; set; } = string.Empty;

    public virtual ICollection<VaiTroQuyenHan> VaiTroQuyenHans { get; set; } = new List<VaiTroQuyenHan>();
}
