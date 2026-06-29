namespace Backend.DTOs.RewardDiscipline;

public class StudentRewardQueryParameters
{
    public int? MaHocKy { get; set; }
    public string? LoaiKhenThuong { get; set; }
    public string? TrangThai { get; set; }
    public bool? HasCertificate { get; set; }
    public string? Keyword { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class StudentRewardListItemDto
{
    public int MaKhenThuong { get; set; }
    public int? MaDotKhenThuong { get; set; }
    public string LoaiKhenThuong { get; set; } = string.Empty;
    public string? TenLoaiKhenThuong { get; set; }
    public string? DanhHieuSnapshot { get; set; }
    public string? TenHocKySnapshot { get; set; }
    public int? XepHang { get; set; }
    public decimal? DiemXet { get; set; }
    public decimal? GpaHocKy { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public bool HasCertificate { get; set; }
    public string TrangThai { get; set; } = string.Empty;
}

public class StudentRewardDetailDto : StudentRewardListItemDto
{
    public string? HoTenSnapshot { get; set; }
    public string? MssvSnapshot { get; set; }
    public int MaHocKy { get; set; }
    public int MaDonVi { get; set; }
    public DateTime CapLuc { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}
