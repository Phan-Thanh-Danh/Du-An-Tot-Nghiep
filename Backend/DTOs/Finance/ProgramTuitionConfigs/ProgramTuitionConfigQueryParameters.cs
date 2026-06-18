using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Finance.ProgramTuitionConfigs;

public class ProgramTuitionConfigQueryParameters
{
    public string? Keyword { get; set; }
    public int? MaDonVi { get; set; }
    public int? MaChuongTrinhDaoTao { get; set; }
    public int? MaHocKy { get; set; }
    public int? NamHocTrongChuongTrinh { get; set; }
    public int? HocKyTrongNam { get; set; }
    public int? SoThuTuHocKy { get; set; }
    public string? LoaiCachTinhHocPhi { get; set; }
    public bool? ConHoatDong { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "pageNumber phải lớn hơn 0.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize phải từ 1 đến 100.")]
    public int PageSize { get; set; } = 20;
}
