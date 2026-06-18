using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.BuoiHoc;

public class BuoiHocQueryParameters
{
    public int? MaTkb { get; set; }
    public int? MaKhoaHoc { get; set; }
    public int? MaGiaoVien { get; set; }
    public int? MaPhong { get; set; }
    public int? MaCaHoc { get; set; }
    public string? TrangThaiBuoi { get; set; }
    public DateOnly? NgayTu { get; set; }
    public DateOnly? NgayDen { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "pageIndex phải lớn hơn 0.")]
    public int PageIndex { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize phải từ 1 đến 100.")]
    public int PageSize { get; set; } = 20;
}
