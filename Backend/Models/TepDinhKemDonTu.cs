namespace Backend.Models;

public class TepDinhKemDonTu
{
    public int MaTep { get; set; }
    public int MaDonTu { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string TenFileGoc { get; set; } = string.Empty;
    public string TenFileLuu { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long KichThuocByte { get; set; }
    public string? FileHash { get; set; }
    public int NguoiTaiLen { get; set; }
    public DateTime NgayTao { get; set; }
    public bool DaXoa { get; set; }
    public int? NguoiXoa { get; set; }
    public DateTime? NgayXoa { get; set; }

    public DonTu? DonTu { get; set; }
    public NguoiDung? NguoiTaiLenNavigation { get; set; }
    public NguoiDung? NguoiXoaNavigation { get; set; }
}
