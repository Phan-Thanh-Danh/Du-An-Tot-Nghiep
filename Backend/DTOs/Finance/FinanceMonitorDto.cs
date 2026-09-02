namespace Backend.DTOs.Finance;

public class FinanceMonitorDto
{
    public decimal TongDoanhThu { get; set; }
    public decimal DaThu { get; set; }
    public decimal ConNo { get; set; }
    public int SoHoaDonChuaThu { get; set; }
    public int SoHoaDonQuaHan { get; set; }
    public List<DailyRevenueDto> DailyRevenue { get; set; } = new();
    public List<TopDebtorsDto> TopDebtors { get; set; } = new();
}

public class DailyRevenueDto
{
    public DateTime Ngay { get; set; }
    public decimal SoTien { get; set; }
}

public class TopDebtorsDto
{
    public int MaHocSinh { get; set; }
    public string HoTenHocSinh { get; set; } = string.Empty;
    public decimal ConNo { get; set; }
    public int SoNgayQuaHan { get; set; }
}
