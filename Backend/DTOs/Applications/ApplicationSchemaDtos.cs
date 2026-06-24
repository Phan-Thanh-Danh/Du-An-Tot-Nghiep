namespace Backend.DTOs.Applications;

public class ApplicationTypeDto
{
    public string LoaiDon { get; set; } = string.Empty;
    public string TenHienThi { get; set; } = string.Empty;
}

public class ApplicationStatusDto
{
    public string TrangThai { get; set; } = string.Empty;
    public string TenHienThi { get; set; } = string.Empty;
    public bool LaTrangThaiKetThuc { get; set; }
    public IReadOnlyCollection<string> TrangThaiTiepTheo { get; set; } = [];
}

public class ApplicationTemplateDto
{
    public int MaMauDon { get; set; }
    public string LoaiDon { get; set; } = string.Empty;
    public string TenLoaiDon { get; set; } = string.Empty;
    public string TenMau { get; set; } = string.Empty;
    public int PhienBan { get; set; }
    public string CauHinhJson { get; set; } = string.Empty;
    public bool BatBuocMinhChung { get; set; }
    public int SoTepToiDa { get; set; }
    public long DungLuongTepToiDaByte { get; set; }
    public long TongDungLuongToiDaByte { get; set; }
    public int? SlaGio { get; set; }
}
